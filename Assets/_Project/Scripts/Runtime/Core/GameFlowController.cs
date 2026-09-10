using ReleaseTheArrow.Ads;
using ReleaseTheArrow.Audio;
using ReleaseTheArrow.Gameplay;
using ReleaseTheArrow.Generation;
using ReleaseTheArrow.UI;
using ReleaseTheArrow.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ReleaseTheArrow.Core
{
    /// The single scene's bootstrap + top-level wiring: builds the entire UI procedurally,
    /// creates the core singletons, and translates GameManager state changes into which
    /// screen(s) are visible. This is intentionally the only "God object" in the project — it
    /// does no gameplay or rules logic itself, it only routes events between the systems that do.
    public class GameFlowController : MonoBehaviour
    {
        private GameManager _gameManager;
        private BoardController _board;
        private HudView _hud;
        private RectTransform _gameplayArea;

        private MainMenuScreen _mainMenu;
        private LevelSelectScreen _levelSelect;
        private SettingsScreen _settings;
        private CreditsScreen _credits;
        private PauseScreen _pause;
        private GameOverScreen _gameOver;
        private LevelCompleteScreen _levelComplete;
        private FinalCompletionScreen _finalCompletion;

        private AppState _stateBeforeSettings = AppState.MainMenu;

        private void Awake()
        {
            _gameManager = gameObject.AddComponent<GameManager>();
            gameObject.AddComponent<AudioManager>();
            gameObject.AddComponent<AdManager>();

            HapticManager.VibrationOn = true; // refreshed to the saved value once SaveData loads, below

            var canvas = UIFactory.CreateRootCanvas("RootCanvas");
            var canvasRect = (RectTransform)canvas.transform;
            UIFactory.CreateFullStretchPanel(canvasRect, "Background", Theme.BackgroundBottom);

            BuildGameplayArea(canvasRect);
            BuildMenuScreens(canvasRect);
            BuildOverlayScreens(canvasRect);
            WireEvents();
        }

        private void Start()
        {
            var save = _gameManager.SaveData;
            AudioManager.Instance.ApplySettings(save.soundOn, save.musicOn);
            HapticManager.VibrationOn = save.vibrationOn;
            _settings.SetInitialValues(save.musicOn, save.soundOn, save.vibrationOn);

            _gameManager.SetState(AppState.MainMenu);
        }

        // ---------------------------------------------------------------- construction

        private void BuildGameplayArea(RectTransform parent)
        {
            _gameplayArea = UIFactory.CreateFullStretchPanel(parent, "GameplayArea", new Color(0, 0, 0, 0));

            var scrollGo = new GameObject("BoardScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            var scrollRect = (RectTransform)scrollGo.transform;
            scrollRect.SetParent(_gameplayArea, false);
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = new Vector2(20f, 20f);
            scrollRect.offsetMax = new Vector2(-20f, -180f); // leave room for the HUD

            var contentGo = new GameObject("BoardContent", typeof(RectTransform));
            var contentRect = (RectTransform)contentGo.transform;
            contentRect.SetParent(scrollRect, false);
            contentRect.anchorMin = new Vector2(0.5f, 1f);
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Unrestricted;
            scroll.content = contentRect;
            scroll.viewport = scrollRect;

            // Pinch-to-zoom for boards too large to read at 1x — pans via the ScrollRect above,
            // which already tells a tap on an arrow apart from a drag on the board itself.
            var zoom = scrollGo.AddComponent<BoardZoomController>();
            zoom.Initialize(contentRect);

            var boardGo = new GameObject("BoardController", typeof(RectTransform));
            boardGo.transform.SetParent(_gameplayArea, false);
            _board = boardGo.AddComponent<BoardController>();
            _board.Initialize(scrollRect, contentRect);

            _hud = HudView.Create(_gameplayArea);
            _hud.PauseRequested += () => _gameManager.SetState(AppState.Paused);
        }

        private void BuildMenuScreens(RectTransform parent)
        {
            _mainMenu = MainMenuScreen.Create(parent);
            _levelSelect = LevelSelectScreen.Create(parent);
            _settings = SettingsScreen.Create(parent);
            _credits = CreditsScreen.Create(parent);
        }

        private void BuildOverlayScreens(RectTransform parent)
        {
            _pause = PauseScreen.Create(parent);
            _gameOver = GameOverScreen.Create(parent);
            _levelComplete = LevelCompleteScreen.Create(parent);
            _finalCompletion = FinalCompletionScreen.Create(parent);
        }

        // ---------------------------------------------------------------- wiring

        private void WireEvents()
        {
            _gameManager.StateChanged += OnStateChanged;

            _mainMenu.PlayRequested += () =>
            {
                if (_gameManager.HasResumableLevel) _gameManager.ResumeInProgressLevel();
                else _gameManager.StartLevel(_gameManager.SaveData.highestUnlockedLevel);
                AnalyticsManager.GameStarted(_gameManager.CurrentSession.Layout.levelId);
            };
            _mainMenu.LevelSelectRequested += () => _gameManager.SetState(AppState.LevelSelect);
            _mainMenu.SettingsRequested += () => OpenSettings(AppState.MainMenu);
            _mainMenu.CreditsRequested += () => _gameManager.SetState(AppState.Credits);

            _levelSelect.BackRequested += () => _gameManager.SetState(AppState.MainMenu);
            _levelSelect.LevelChosen += levelId =>
            {
                _gameManager.StartLevel(levelId);
                AnalyticsManager.GameStarted(levelId);
            };

            _credits.BackRequested += () => _gameManager.SetState(AppState.MainMenu);

            _settings.BackRequested += () => _gameManager.SetState(_stateBeforeSettings);
            _settings.SettingsChanged += (music, sound, vibration) => _gameManager.SetSettings(music, sound, vibration);

            _pause.ResumeRequested += () => _gameManager.SetState(AppState.Playing);
            _pause.RestartRequested += () =>
            {
                AnalyticsManager.Retry(_gameManager.CurrentSession.Layout.levelId);
                _gameManager.RestartLevel();
            };
            _pause.SettingsRequested += () => OpenSettings(AppState.Paused);
            _pause.MainMenuRequested += () => _gameManager.SetState(AppState.MainMenu);

            _gameOver.RestartRequested += () =>
            {
                AnalyticsManager.Retry(_gameManager.CurrentSession.Layout.levelId);
                _gameManager.RestartLevel();
            };
            _gameOver.ContinueGranted += () => _gameManager.GrantContinue();

            _levelComplete.NextLevelRequested += () =>
            {
                int justCompleted = _gameManager.CurrentSession.Layout.levelId;
                AdManager.Instance.MaybeShowLevelCompleteInterstitial(justCompleted, () => _gameManager.GoToNextLevel());
            };

            _finalCompletion.PlayAgainRequested += () => _gameManager.StartLevel(1);
            _finalCompletion.MainMenuRequested += () => _gameManager.SetState(AppState.MainMenu);
        }

        private void OpenSettings(AppState returnTo)
        {
            _stateBeforeSettings = returnTo;
            _gameManager.SetState(AppState.Settings);
        }

        // ---------------------------------------------------------------- state -> visuals

        private void OnStateChanged(AppState state)
        {
            bool gameplayVisible = state is AppState.Playing or AppState.Paused or AppState.GameOver or AppState.LevelComplete;
            _gameplayArea.gameObject.SetActive(gameplayVisible);

            _mainMenu.gameObject.SetActive(state == AppState.MainMenu);
            _levelSelect.gameObject.SetActive(state == AppState.LevelSelect);
            _settings.gameObject.SetActive(state == AppState.Settings);
            _credits.gameObject.SetActive(state == AppState.Credits);
            _pause.gameObject.SetActive(state == AppState.Paused);
            _gameOver.gameObject.SetActive(state == AppState.GameOver);
            _levelComplete.gameObject.SetActive(state == AppState.LevelComplete);
            _finalCompletion.gameObject.SetActive(state == AppState.FinalCompletion);

            Time.timeScale = state == AppState.Paused ? 0f : 1f;

            switch (state)
            {
                case AppState.MainMenu:
                    _mainMenu.SetProgress(_gameManager.SaveData.highestUnlockedLevel);
                    _hud.Unbind();
                    break;

                case AppState.LevelSelect:
                    _levelSelect.Open(_gameManager.SaveData.highestUnlockedLevel);
                    break;

                case AppState.Playing:
                    _board.LoadSession(_gameManager.CurrentSession);
                    _hud.Bind(_gameManager);
                    break;

                case AppState.GameOver:
                    _hud.Bind(_gameManager);
                    AudioManager.Instance.Play(Sfx.GameOver);
                    AnalyticsManager.GameOver(_gameManager.CurrentSession.Layout.levelId, _gameManager.CurrentSession.ContinuesUsed);
                    _gameOver.Configure(
                        _gameManager.CurrentSession.AdsRequiredForNextContinue(),
                        _gameManager.CurrentSession.State == LevelSessionState.GameOverFinal);
                    break;

                case AppState.LevelComplete:
                    AudioManager.Instance.Play(Sfx.LevelComplete);
                    AnalyticsManager.LevelCompleted(_gameManager.CurrentSession.Layout.levelId);
                    break;

                case AppState.FinalCompletion:
                    break;
            }
        }
    }
}
