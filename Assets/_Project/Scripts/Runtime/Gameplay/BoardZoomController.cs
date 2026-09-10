using UnityEngine;

namespace ReleaseTheArrow.Gameplay
{
    /// Two-finger pinch zooms the board's content transform. One-finger panning is handled
    /// separately by the ScrollRect already on this same GameObject — Unity's EventSystem
    /// distinguishes a small tap (still reaches ArrowView.OnPointerClick) from a drag (captured
    /// by the ScrollRect instead), so this component only ever needs to look at the two-finger
    /// case, which the ScrollRect never sees. Needed once boards got big enough that not every
    /// cell fits on screen at a legible size — without this, a large board would have no way to
    /// actually reach or read its far side.
    public class BoardZoomController : MonoBehaviour
    {
        private const float MinZoom = 0.4f;
        private const float MaxZoom = 3f;

        private RectTransform _content;
        private float _zoom = 1f;
        private float _prevPinchDistance;
        private bool _pinching;

        public void Initialize(RectTransform content)
        {
            _content = content;
            _zoom = 1f;
            _content.localScale = Vector3.one;
        }

        private void Update()
        {
            if (_content == null || Input.touchCount != 2)
            {
                _pinching = false;
                return;
            }

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            float distance = Vector2.Distance(t0.position, t1.position);

            if (!_pinching || t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _prevPinchDistance = distance;
                _pinching = true;
                return;
            }

            if (_prevPinchDistance > 1f)
            {
                float ratio = distance / _prevPinchDistance;
                _zoom = Mathf.Clamp(_zoom * ratio, MinZoom, MaxZoom);
                _content.localScale = new Vector3(_zoom, _zoom, 1f);
            }
            _prevPinchDistance = distance;
        }
    }
}
