using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Mapbox.Examples;
using Mapbox.Geocoding;
using Mapbox.Unity.Map;

using Assets.Scripts.Environment;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{

    //Adapted from: https://wiki.unity3d.com/index.php/MouseOrbitZoom
    //But heavily modified...
    public class CameraAdjustment : MonoBehaviour
    {
        public EventHandler<System.EventArgs> OnZoom;
        public EventHandler<System.EventArgs> OnStartPan;
        public EventHandler<System.EventArgs> OnEndPan;
        public EventHandler<System.EventArgs> OnStartTilt;
        public EventHandler<System.EventArgs> OnEndTilt;
        public EventHandler<System.EventArgs> OnZoomToPos;
        public EventHandler<System.EventArgs> OnSetView;
        public EventHandler<System.EventArgs> OnSetViewHome;
        public Camera _camera;
        public bool IsPanningCamera { get; private set; }
        public bool IsTiltingCamera { get; private set; }

        public float _xSpeed = 200.0f;
        public float _ySpeed = 200.0f;
        public int _yMinLimit = -80;
        public int _yMaxLimit = 80;
        public float _zoomRate = 40;
        public float _panSpeed = 0.3f;
        public float _zoomDampening = 5.0f;
        public bool _allowZoom = true;
        public bool _allowTilt = false;
        public bool _allowPan = true;
        public float _minOrthoZoom = 6000.0f;

        public float _farClipMultPersp = 2.5f;
        public float _fogRangeFactor = 2.5f;
        public float _zoomSelectedHeight = 350.0f;

        private float _xDeg = 0.0f;
        private float _yDeg = 0.0f;
        private Quaternion _currentRotation;
        private Quaternion _desiredRotation;
        private Quaternion _rotation;
        private bool _isPerspective;

        private float _minX = float.MinValue;
        private float _maxX = float.MaxValue;
        private float _minZ = float.MinValue;
        private float _maxZ = float.MaxValue;

        private Vector3 _lastCenter = new Vector3();
        private float _lastD = 0;
        private bool _lastActionWasSetView = false;
        private Vector3 _homePos = new Vector3();

        private float _panStartTime = 0.0f;//when clicked down mouse for panning
        private float _panRegistrationTime = 0.1f;//how long you must hold mouse before this component registers itself as "IsPanningCamera"

        void Start() { Init(); }
        void OnEnable() { Init(); }

        public void Init()
        {
            _camera = GetComponent<Camera>();
            _rotation = _camera.transform.rotation;
            _currentRotation = _camera.transform.rotation;
            _desiredRotation = _camera.transform.rotation;
            _isPerspective = !_camera.orthographic;
            _homePos = _camera.transform.position;

            _xDeg = Vector3.Angle(Vector3.right, _camera.transform.right);
            _yDeg = Vector3.Angle(Vector3.up, _camera.transform.up);
        }

        /// <summary>
        /// Sets max extents camera is allowed to travel.
        /// </summary>
        public void SetExtents(float minX, float maxX, float minZ, float maxZ)
        {
            _minX = minX;
            _maxX = maxX;
            _minZ = minZ;
            _maxZ = maxZ;
        }

        void OnGUI()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            var evt = Event.current;

            if (evt.type == EventType.MouseDown)
            {
                if (evt.button == 0)
                {
                    StartPan();
                }
                else if (evt.button == 1)
                {
                    StartTilt();
                }
            }
            if (evt.type == EventType.MouseDrag)
            {
                if (evt.button == 0)
                {
                    UpdatePan();
                }
                else if (evt.button == 1)
                {
                    UpdateTilt();
                }
            }
            if (evt.type == EventType.MouseUp)
            {
                if (IsTiltingCamera)
                {
                    EndTilt();
                }

                if (IsPanningCamera)
                {
                    EndPan();
                }
            }
            if (evt.type == EventType.ScrollWheel)
            {
                Zoom();
            }
        }

        private void StartPan()
        {
            if (!_allowPan)
            {
                return;
            }
            OnStartPan?.Invoke(this, System.EventArgs.Empty);
            _panStartTime = Time.unscaledTime;
            _lastActionWasSetView = false;
        }

        private void StartTilt()
        {
            if (!_allowTilt)
            {
                return;
            }
            IsTiltingCamera = true;
            OnStartTilt?.Invoke(this, System.EventArgs.Empty);
        }

        private void EndPan()
        {
            OnEndPan?.Invoke(this, System.EventArgs.Empty);
            _lastActionWasSetView = false;
            StartCoroutine(EndPanTiltCoroutine());
        }

        private void EndTilt()
        {
            OnEndTilt?.Invoke(this, System.EventArgs.Empty);
            StartCoroutine(EndPanTiltCoroutine());
        }

        /// <summary>
        /// Waits for other objects in scene to execute things. Preventing unnecessary mouse-up actions.
        /// </summary>
        private IEnumerator EndPanTiltCoroutine()
        {
            yield return new WaitForEndOfFrame();
            IsTiltingCamera = false;
            IsPanningCamera = false;
        }

        private void Zoom()
        {
            if (!_allowZoom)
            {
                return;
            }

            _lastActionWasSetView = false;
            var wheel = Input.GetAxis("Mouse ScrollWheel");
            bool changed = false;
            if (_isPerspective && wheel != 0)
            {
                float forwardDist = _camera.transform.position.y / _zoomRate;
                if (wheel < 0)
                {
                    forwardDist *= -1;
                }

                var transZoom = Vector3.forward * forwardDist;
                _camera.transform.Translate(transZoom);
                SetNearClipPlane();
                changed = true;
                OnZoom?.Invoke(this, System.EventArgs.Empty);
            }
            else if (wheel != 0)
            {
                float s;
                if (wheel > 0)
                {
                    s = _camera.orthographicSize / _zoomRate;
                }
                else
                {
                    s = _camera.orthographicSize * _zoomRate;
                }
                _camera.orthographicSize = s > _minOrthoZoom ? s : _minOrthoZoom;
                changed = true;
                SetNearClipPlane();
                OnZoom?.Invoke(this, System.EventArgs.Empty);
            }

            if (changed)
            {
                UpdateEnvironLastCam();
            }
        }

        private void UpdatePan()
        {
            if (!_allowPan || Time.unscaledTime - _panStartTime < _panRegistrationTime)
            {
                return;
            }

            IsPanningCamera = true;
            _lastActionWasSetView = false;
            var tX = _camera.transform.right * -Input.GetAxis("Mouse X") * (_camera.transform.position.y / _panSpeed);
            var tY = _camera.transform.up * -Input.GetAxis("Mouse Y") * (_camera.transform.position.y / _panSpeed);
            var combined = tX + tY;
            var newPos = _camera.transform.position + combined;
            var x0 = newPos.x;
            var y = newPos.y;
            var z0 = newPos.z;
            float x1, z1;
            if (x0 > _maxX) x1 = _maxX;
            else if (x0 < _minX) x1 = _minX;
            else x1 = x0;

            if (z0 > _maxZ) z1 = _maxZ;
            else if (z0 < _minZ) z1 = _minZ;
            else z1 = z0;

            _camera.transform.position = new Vector3(x1, y, z1);

            UpdateEnvironLastCam();
        }

        private void UpdateTilt()
        {
            if (!_allowTilt)
            {
                return;
            }

            _lastActionWasSetView = false;
            _xDeg += Input.GetAxis("Mouse X") * _xSpeed * 0.02f;
            _yDeg -= Input.GetAxis("Mouse Y") * _ySpeed * 0.02f;

            ////////OrbitAngle

            //Clamp the vertical axis for the orbit
            _yDeg = ClampAngle(_yDeg, _yMinLimit, _yMaxLimit);
            // set camera rotation 
            _desiredRotation = Quaternion.Euler(_yDeg, _xDeg, 0);
            _currentRotation = _camera.transform.rotation;

            _rotation = Quaternion.Lerp(_currentRotation, _desiredRotation, Time.deltaTime * _zoomDampening);
            _camera.transform.rotation = _rotation;
            
            UpdateEnvironLastCam();
        }

        private void UpdateEnvironLastCam()
        {
            EnvironManager.Instance.LastCamXZS = new float[] { _camera.transform.position.x, _camera.transform.position.z, _camera.orthographicSize };
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }

        /// <summary>
        /// Zoom to the specified point.
        /// </summary>
        public void ZoomToPosition(Vector3 pos)
        {
            _camera.transform.position = new Vector3(pos.x, _zoomSelectedHeight, pos.z);
            _camera.transform.LookAt(pos);

            _lastCenter = pos;
            _lastD = _zoomSelectedHeight;
            SetNearClipPlane();
            _lastActionWasSetView = true;

            OnZoomToPos?.Invoke(this, System.EventArgs.Empty);
        }

        /// <summary>
        /// Sets camera to view specified.
        /// </summary>
        public void GoToView(int direction)
        {
            ViewDirection dir = (ViewDirection) direction;
            //camera "target" will be the spot on the ground below the camera, not the current target.
            Vector3 t;
            float d;//distance to this target.
            if (_lastActionWasSetView)
            {
                t = _lastCenter;
                d = _lastD;
            }
            else
            {
                t = new Vector3(_camera.transform.position.x, 0, _camera.transform.position.z);
                d = _camera.transform.position.y;
            }
            float a = d * (float) Math.Cos(Math.PI / 4);//distance along ground to camera positions not directly above the target
            float o = d * (float) Math.Sin(Math.PI / 4);//vertical distance from ground to camera positions not directly above the target
            var u = new Vector3(0, o, 0);//vector up to the position if not directly above target
            Vector3 p = new Vector3();
            switch (dir)
            {
                case ViewDirection.Top:
                {
                    p = new Vector3(t.x, d, t.z);
                    break;
                }
                case ViewDirection.North:
                {
                    p = t + new Vector3(0, 0, 1) * a + u;
                    break;
                }
                case ViewDirection.Northeast:
                {
                    p = t + new Vector3(1, 0, 1) * a + u;
                    break;
                }
                case ViewDirection.East:
                {
                    p = t + new Vector3(1, 0, 0) * a + u;
                    break;
                }
                case ViewDirection.Southeast:
                {
                    p = t + new Vector3(1, 0, -1) * a + u;
                    break;
                }
                case ViewDirection.South:
                {
                    p = t + new Vector3(0, 0, -1) * a + u;
                    break;
                }
                case ViewDirection.Southwest:
                {
                    p = t + new Vector3(-1, 0, -1) * a + u;
                    break;
                }
                case ViewDirection.West:
                {
                    p = t + new Vector3(-1, 0, 0) * a + u;
                    break; 
                }
                case ViewDirection.Northwest:
                {
                    p = t + new Vector3(-1, 0, 1) * a + u;
                    break;
                }
            }

            _camera.transform.position = p;
            _camera.transform.LookAt(t);

            _lastCenter = t;
            _lastD = d;
            _lastActionWasSetView = true;

            SetNearClipPlane();

            OnSetView?.Invoke(this, System.EventArgs.Empty);
        }

        public void GoHomePos()
        {
            _camera.transform.position = _homePos;
            _camera.transform.LookAt(new Vector3(_homePos.x, 0, _homePos.z));
            SetNearClipPlane();

            OnSetViewHome?.Invoke(this, System.EventArgs.Empty);
        }

        private void SetNearClipPlane()
        {
            _camera.nearClipPlane = _camera.transform.position.y < 30000.0f || _camera.transform.rotation.x < 80 ? 10.0f : _camera.transform.position.y - _camera.transform.position.y / 10.0f;
        }
    }
}
