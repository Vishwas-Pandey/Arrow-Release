# Play Store Listing — Release the Arrow

Paste these directly into Play Console → Grow → Store presence → Main store listing.
Edit freely — these are drafts, not final copy.

## App name (30 char max)
Release the Arrow

## Short description (80 char max)
Tap arrows, clear the path, release them all. A puzzle for your commute.

(Character count: 71 — fits with room to spare.)

## Full description (4000 char max)

Release the Arrow is a calm, brain-teasing puzzle about clearing the way.

Every level is a board packed with arrows pointing in four directions. Tap
one to send it flying off the board — but only if its path is actually
clear. Block an arrow's route and it stays put, so you'll need to find the
right order to release every arrow and clear the board.

HOW TO PLAY
- Tap any arrow whose path off the board is unobstructed to release it.
- Tap a blocked arrow and nothing happens for free — figure out what needs
  to move first.
- Clear every arrow to complete the level and unlock the next one.

1,500 HAND-TUNED LEVELS
Every single level — from the first gentle introduction to the final,
punishing puzzle — is generated and verified solvable before it ever
reaches your device. No dead ends, no impossible boards, ever. The
difficulty ramps gradually across all 1,500 levels, so there's always a
next challenge waiting.

THREE LIVES, YOUR CHOICE HOW TO CONTINUE
Tap a blocked arrow and you lose a life. Run out of lives and you can
choose to keep going by watching a short ad — completely optional, and your
exact puzzle state is always preserved, so you never lose progress or
restart from scratch just because you asked for a hand.

DESIGNED FOR SHORT SESSIONS
Clean, original visuals and a calm audio backdrop make this an easy game to
pick up for two minutes or two hours. No forced ads interrupt your puzzle —
you'll only ever see one between levels, and never mid-thought.

- 1,500 levels, every one solvable
- Earn 1-3 stars per level based on how clean your clear was — go back and
  chase a perfect 3-star run on any level you've already beaten
- Simple one-tap controls, easy to learn, hard to master
- Original arrow-based visual identity and audio
- Optional rewarded continues that never punish you for a failed ad
- Progress saved automatically, even mid-level

## Category
Games > Puzzle

## Tags / keywords (for reference, not a Play Console field)
puzzle, arrow, casual, brain teaser, logic, offline puzzle, tap puzzle

## Contact details
- Developer name: Virevia
- Support email: workwithvishwaspandey@gmail.com
- Website: (optional)
- Privacy policy URL: https://vishwas-pandey.github.io/Arrow-Release/ (hosted via GitHub Pages from this repo's `docs/` folder)

## Graphic assets (already generated, in Assets/_Project/Icons/)
- App icon: `AppIcon.png` (1024x1024) — already assigned in Player Settings
- Feature graphic: `FeatureGraphic.png` (1024x500) — upload as-is or refine
- Phone screenshots: captured on-device (emulator) and cleaned up in
  `StoreListing/screenshots/` — main menu, a small early level, the 80x80
  endgame board, the level-complete/star-rating screen, and the level select
  grid. Upload all 5 as-is.

## Content rating questionnaire (Play Console)
Answer honestly based on actual content — this game has no violence, no
user-generated content, no chat, and no gambling mechanics. There IS
advertising (answer "yes" to the ads question) via Google AdMob. Expect an
"Everyone" rating.

## Data safety form (Play Console)
The real Google Mobile Ads SDK is now compiled into the app (currently
running on Google's TEST ad unit IDs for development — see AdMobConfig.cs).
Once shipped with `UseTestAds = false` and real ad unit IDs, AdMob is what
actually collects data, so the Data Safety form must reflect AdMob's
collection, not "no data collected". As a baseline (verify against Play
Console's current categories at submission time, since Google revises this
form periodically):
- **Data collected**: Device or other IDs (advertising ID) — collected via
  AdMob for ad delivery and measurement.
- **Purpose**: Advertising or marketing.
- **Shared with third parties**: Yes — Google/AdMob and its ad network
  partners.
- **Encrypted in transit**: Yes (AdMob requests use HTTPS).
- **Optional / can users request deletion**: users can reset their
  advertising ID or opt out of personalized ads via their device's Google
  Settings; the app itself stores no personal data server-side.
- Everything else (level progress, star ratings, settings) stays local-only,
  never transmitted — answer those categories "not collected".
- The UMP consent flow (ConsentManager.cs) already gates ad requests behind
  user consent where required (EEA/UK/similar), so "ads are optional/
  consent-gated" is accurate to state if the form asks.
