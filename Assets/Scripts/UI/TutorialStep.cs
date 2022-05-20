using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class TutorialStep : MonoBehaviour
    {
        public EventHandler<System.EventArgs> OnCompleted;
        public List<RectTransform> _highlightAnchors;
        public List<float> _anchorRotations;
        public Dictionary<RectTransform, float> _test;
        public GameObject _highlightArrowPrefab;
        public TutorialStepType _step;
        private CameraAdjustment _cameraAdjustment;
        private RegionViewManager _sceneManager;
        private bool _isComplete = false;
        private List<GameObject> _arrows = new List<GameObject>();
        private void Awake()
        {
            if (_anchorRotations.Count != _highlightAnchors.Count)
            {
                Debug.LogError("Highlight anchor list and anchor rotation list lengths do not match");
                return;
            }
            switch (_step)
            {
                case TutorialStepType.General:
                {
                    //the general steps don't need custom callbacks
                    //they are just to read by user, but default complete
                    _isComplete = true;
                    break;
                }
                case TutorialStepType.UnityEditorConfig:
                {
                    //The completion function will be mapped to an event in the Unity Editor (certain buttons, etc)
                    break;
                }
                case TutorialStepType.AwaitPanView:
                {
                    if (GetCameraAdjustment())
                    {
                        _cameraAdjustment.OnEndPan += CompletedRequirementCallback;
                    }
                    break;
                }
                case TutorialStepType.AwaitTiltView:
                {
                    if (GetCameraAdjustment())
                    {
                        _cameraAdjustment.OnEndTilt += CompletedRequirementCallback;
                    }
                    break;
                }
                case TutorialStepType.AwaitZoomView:
                {
                    if (GetCameraAdjustment())
                    {
                        _cameraAdjustment.OnZoom += CompletedRequirementCallback;
                    }
                    break;
                }
                case TutorialStepType.Await3DronePorts:
                case TutorialStepType.Await1Parking:
                case TutorialStepType.Await1Restriction:
                case TutorialStepType.Await4DronePorts:
                {
                    if (GetSceneManager())
                    {
                        _sceneManager.OnElementAdded += CheckCompletionCallback;
                        _sceneManager.OnElementRemoved += CheckCompletionCallback;
                    }
                    break;
                }
                case TutorialStepType.AwaitZoomToRestriction:
                {
                    if (GetCameraAdjustment())
                    {
                        _cameraAdjustment.OnZoomToPos += CompletedRequirementCallback;
                    }
                    break;
                }
                case TutorialStepType.AwaitModify:
                {
                    if (GetSceneManager())
                    {
                        _sceneManager.OnElementChanged += CompletedRequirementCallback;
                    }
                    break;
                }
                case TutorialStepType.AwaitZoomToBuildingLevel:
                {
                    if (GetSceneManager())
                    {
                        _sceneManager.OnZoomToBuildingLevel += CompletedRequirementCallback;
                    }
                    break;
                }
                case TutorialStepType.AwaitBuildingsEnabled:
                {
                    if (GetSceneManager())
                    {
                        _sceneManager.OnBuildingsEnabled += CompletedRequirementCallback;
                    }
                    break;
                }
            }
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            for (int i = 0; i < _highlightAnchors.Count; i++)
            {
                var c = Instantiate(_highlightArrowPrefab, _highlightAnchors[i]);
                c.transform.RotateAround(_highlightAnchors[i].position, new Vector3(0, 0, 1), _anchorRotations[i]);
                _arrows.Add(c);
            }
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
            foreach (var a in _arrows)
            {
                a.Destroy();
            }
        }

        private void CompletedRequirementCallback(object sender, System.EventArgs args)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            CompletedRequirement();
        }
        public void CompletedRequirement()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            _isComplete = true;
            OnCompleted?.Invoke(this, System.EventArgs.Empty);
        }

        private void CheckCompletionCallback(object sender, System.EventArgs args)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (CheckCompletion())
            {
                CompletedRequirement();
            }
        }

        /// <summary>
        /// Checks to see if all requirements met for continuing the tutorial, if so, returns true. Also updates the "IsComplete" property of component.
        /// </summary>
        public bool CheckCompletion()
        {
            switch (_step)
            {
                case TutorialStepType.Await3DronePorts:
                {
                    _isComplete = EnvironManager.Instance.Environ.DronePorts.Count >= 3;
                    break;
                }
                case TutorialStepType.Await1Parking:
                {
                    _isComplete = false;
                    foreach (var pS in EnvironManager.Instance.Environ.ParkingStructures)
                    {
                        if (!pS.Value.Type.Contains("LowAltitude"))
                        {
                            _isComplete = true;
                            break;
                        }
                    }
                    break;
                }
                case TutorialStepType.Await1Restriction:
                {
                    _isComplete = EnvironManager.Instance.Environ.RestrictionZones.Count >= 1;
                    break;
                }
                case TutorialStepType.Await4DronePorts:
                {
                    _isComplete = EnvironManager.Instance.Environ.DronePorts.Count >= 4;
                    break;
                }
                case TutorialStepType.AwaitBuildingsEnabled:
                {
                    _isComplete = _sceneManager.AllowBuildings;
                    break;
                }
            }

            return _isComplete;
        }

        /// <summary>
        /// Finds the camera adjustment script, returning true if found.
        /// </summary>
        private bool GetCameraAdjustment()
        {
            _cameraAdjustment = FindObjectOfType<CameraAdjustment>(true);
            if (_cameraAdjustment == null)
            {
                Debug.LogError("Could not find camera adjustment script to initialize tutorial step");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds the scene manager base, returning true if found.
        /// </summary>
        private bool GetSceneManager()
        {
            _sceneManager = FindObjectOfType<RegionViewManager>(true);
            if (_sceneManager == null)
            {
                Debug.LogError("Could not find scene manager base script to initialize tutorial step");
                return false;
            }

            return true;
        }
    }
}
