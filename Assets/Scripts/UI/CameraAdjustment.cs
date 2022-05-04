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

namespace Assets.Scripts.UI
{

    //Adapted from: https://wiki.unity3d.com/index.php/MouseOrbitZoom
    //But heavily modified...
    public class CameraAdjustment : MonoBehaviour
    {
        public EventHandler<System.EventArgs> OnZoom;
        public EventHandler<System.EventArgs> OnStartPan;
        public EventHandler<System.EventArgs> OnEndPan;
        public Camera _camera;

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

            if (_isPerspective) SetFog();
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

        /*
         * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
         */
        void LateUpdate()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            bool changed = false;
            if (_allowPan && Input.GetMouseButtonDown(0))
            {
                OnStartPan?.Invoke(this, new System.EventArgs());
                _lastActionWasSetView = false;
            }
            else if (_allowPan && Input.GetMouseButtonUp(0))
            {
                OnEndPan?.Invoke(this, new System.EventArgs());
                _lastActionWasSetView = false;
            }

            // If middle mouse ORBIT
            if (_allowTilt && Input.GetMouseButton(1))
            {
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
                changed = true;
            }
            else if (_allowPan && Input.GetMouseButton(0))
            {
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
                //_camera.transform.Translate(tX);
                //_camera.transform.Translate(tY, Space.World);
                changed = true;
            }


            //zoom
            if (_allowZoom)
            {
                _lastActionWasSetView = false;
                var wheel = Input.GetAxis("Mouse ScrollWheel");
                if (_isPerspective && wheel != 0)
                {
                    float forwardDist = _camera.transform.position.y / _zoomRate;
                    if (wheel < 0)
                    {
                        forwardDist *= -1;
                    }

                    var transZoom = Vector3.forward * forwardDist;
                    _camera.transform.Translate(transZoom);
                    _camera.farClipPlane = _camera.transform.position.y * _farClipMultPersp;
                    SetFog();
                    changed = true;
                    OnZoom?.Invoke(this, new System.EventArgs());
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
                    OnZoom?.Invoke(this, new System.EventArgs());

                }
            }
            if (changed)
            {
                EnvironManager.Instance.LastCamXZS = new float[] { _camera.transform.position.x, _camera.transform.position.z, _camera.orthographicSize };
            }
        }
        /// <summary>
        /// Adjusts fog factor on camera
        /// </summary>
        private void SetFog()
        {
            RenderSettings.fogEndDistance = _camera.farClipPlane;
            RenderSettings.fogStartDistance = _camera.farClipPlane - _camera.farClipPlane / _fogRangeFactor;
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
        }

        public void GoHomePos()
        {
            _camera.transform.position = _homePos;
            _camera.transform.LookAt(new Vector3(_homePos.x, 0, _homePos.z));
        }
    }
}
