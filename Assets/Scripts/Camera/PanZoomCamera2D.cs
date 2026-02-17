using UnityEngine;
using UnityEngine.EventSystems;

namespace GatchaGamePlay.CameraControls
{
    [DisallowMultipleComponent]
    public sealed class PanZoomCamera2D : MonoBehaviour
    {
        [Header("Pan")]
        [Tooltip("Keyboard pan speed in world units / second.")]
        public float keyboardPanSpeed = 12f;

        [Tooltip("Drag pan speed multiplier.")]
        public float dragPanSpeed = 1f;

        [Tooltip("Hold this mouse button to pan (0=left, 1=right, 2=middle).")]
        public int mouseDragButton = 1;

        [Header("Zoom")]
        public float zoomSpeedMouseWheel = 8f;
        public float zoomSpeedPinch = 0.02f;
        public float minOrthoSize = 2.5f;
        public float maxOrthoSize = 12f;

        [Header("Optional Bounds")]
        public bool clampToBounds;
        public Vector2 boundsMin = new Vector2(-50, -50);
        public Vector2 boundsMax = new Vector2(50, 50);

        private Camera _cam;
        private Vector3 _lastMousePos;
        private bool _dragging;
        private float _fixedZ;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;
            if (_cam == null)
            {
                enabled = false;
                return;
            }

            _fixedZ = _cam.transform.position.z;
        }

        private void Update()
        {
            if (_cam == null) return;

            HandleZoom();
            HandlePan();

            // Keep camera on a fixed Z plane.
            var pos = _cam.transform.position;
            pos.z = _fixedZ;
            _cam.transform.position = pos;

            if (clampToBounds)
            {
                ClampPosition();
            }
        }

        private void HandlePan()
        {
            if (IsPointerOverUI())
            {
                _dragging = false;
                return;
            }

            // Mouse drag pan.
            if (Input.GetMouseButtonDown(mouseDragButton))
            {
                _dragging = true;
                _lastMousePos = Input.mousePosition;
            }

            if (_dragging && Input.GetMouseButton(mouseDragButton))
            {
                var current = Input.mousePosition;
                var deltaWorld = _cam.ScreenToWorldPoint(_lastMousePos) - _cam.ScreenToWorldPoint(current);
                deltaWorld.z = 0;
                _cam.transform.position += deltaWorld * dragPanSpeed;
                _lastMousePos = current;
            }

            if (Input.GetMouseButtonUp(mouseDragButton))
            {
                _dragging = false;
            }

            // Touch drag pan (1 finger).
            if (Input.touchCount == 1)
            {
                var t = Input.GetTouch(0);
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                {
                    return;
                }

                if (t.phase == TouchPhase.Moved)
                {
                    // Convert delta pixels into world delta.
                    var p0 = t.position;
                    var p1 = t.position - t.deltaPosition;
                    var world0 = _cam.ScreenToWorldPoint(new Vector3(p0.x, p0.y, 0));
                    var world1 = _cam.ScreenToWorldPoint(new Vector3(p1.x, p1.y, 0));
                    var delta = world1 - world0;
                    delta.z = 0;
                    _cam.transform.position += delta * dragPanSpeed;
                }
            }

            // Keyboard pan (WASD / arrows).
            var move = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"));

            if (move.sqrMagnitude > 0.001f)
            {
                var speed = keyboardPanSpeed * Time.unscaledDeltaTime;
                _cam.transform.position += new Vector3(move.x, move.y, 0) * speed;
            }
        }

        private void HandleZoom()
        {
            if (!_cam.orthographic) return;

            // Mouse wheel.
            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.0001f && !IsPointerOverUI())
            {
                _cam.orthographicSize = Mathf.Clamp(
                    _cam.orthographicSize - scroll * (zoomSpeedMouseWheel * 0.1f),
                    minOrthoSize,
                    maxOrthoSize);
            }

            // Pinch zoom.
            if (Input.touchCount == 2)
            {
                var t0 = Input.GetTouch(0);
                var t1 = Input.GetTouch(1);

                // If either finger is on UI, ignore.
                if (EventSystem.current != null &&
                    (EventSystem.current.IsPointerOverGameObject(t0.fingerId) || EventSystem.current.IsPointerOverGameObject(t1.fingerId)))
                {
                    return;
                }

                var prev0 = t0.position - t0.deltaPosition;
                var prev1 = t1.position - t1.deltaPosition;

                var prevDist = Vector2.Distance(prev0, prev1);
                var currDist = Vector2.Distance(t0.position, t1.position);
                var diff = currDist - prevDist;

                _cam.orthographicSize = Mathf.Clamp(
                    _cam.orthographicSize - diff * zoomSpeedPinch,
                    minOrthoSize,
                    maxOrthoSize);
            }
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void ClampPosition()
        {
            var p = _cam.transform.position;
            p.x = Mathf.Clamp(p.x, boundsMin.x, boundsMax.x);
            p.y = Mathf.Clamp(p.y, boundsMin.y, boundsMax.y);
            _cam.transform.position = p;
        }
    }
}
