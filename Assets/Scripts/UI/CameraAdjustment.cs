using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Mapbox.Examples;
using Mapbox.Geocoding;
using Mapbox.Unity.Map;

using Assets.Scripts.Environment;

namespace Assets.Scripts.UI
{
    //Adapted from: https://wiki.unity3d.com/index.php/MouseOrbitZoom
    //But heavily modified...
    public class CameraAdjustment : MonoBehaviour
    {
        public float _xSpeed = 200.0f;
        public float _ySpeed = 200.0f;
        public int _yMinLimit = -80;
        public int _yMaxLimit = 80;
        public int _zoomRate = 40;
        public float _panSpeed = 0.3f;
        public float _zoomDampening = 5.0f;
        public bool _allowZoom = true;
        public bool _allowTilt = false;
        public bool _allowPan = true;
        public float _minOrthoZoom = 6000.0f;

        private float _xDeg = 0.0f;
        private float _yDeg = 0.0f;
        private Quaternion _currentRotation;
        private Quaternion _desiredRotation;
        private Quaternion _rotation;
        private Camera _camera;
        private bool _isPerspective;

        void Start() { Init(); }
        void OnEnable() { Init(); }

        public void Init()
        {
            _camera = GetComponent<Camera>();
            _rotation = _camera.transform.rotation;
            _currentRotation = _camera.transform.rotation;
            _desiredRotation = _camera.transform.rotation;
            _isPerspective = !_camera.orthographic;

            _xDeg = Vector3.Angle(Vector3.right, _camera.transform.right);
            _yDeg = Vector3.Angle(Vector3.up, _camera.transform.up);
        }

        /*
         * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
         */
        void LateUpdate()
        {
            //set to top view if certain macro pressed:
            if (Input.GetKeyUp(KeyCode.T) && (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)))//set view to top
            {
                _desiredRotation = Quaternion.Euler(90, 0, 0);
                //currentRotation = transform.rotation;
                _rotation = _desiredRotation;
                _camera.transform.rotation = _rotation;
            }

            // If middle mouse ORBIT
            else if (_allowTilt && Input.GetMouseButton(2))
            {
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
            }
            // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
            else if (_allowPan && Input.GetMouseButton(1))
            {
                //grab the rotation of the camera so we can move in a psuedo local XY space
                var tX = Vector3.right * -Input.GetAxis("Mouse X") * _panSpeed;
                var tY = _camera.transform.up * -Input.GetAxis("Mouse Y") * _panSpeed;
                _camera.transform.Translate(tX);
                _camera.transform.Translate(tY, Space.World);
            }
            //zoom
            if (_allowZoom)
            {
                if (_isPerspective)
                {
                    // calculate position based on the new currentDistance 
                    var transZoom = Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * _zoomRate;
                    _camera.transform.Translate(transZoom);
                }
                else
                {
                    float s = _camera.orthographicSize + Input.GetAxis("Mouse ScrollWheel") * _zoomRate * -1.0f;
                    _camera.orthographicSize = s > _minOrthoZoom ? s : _minOrthoZoom;
                }
            }
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }
}
