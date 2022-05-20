using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.UI.EventArgs;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class TutorialManager : MonoBehaviour
    {
        
        //parent contains layout group with:
        //main buttons: skip tutorial, next step, prev step
        //list of children, containing text boxes for each step, only active step is visible
            //each child may contain elements that highlight part of UI, with animated opacity

        //subscribe to events from children

        //each step may have requirements, when completed, you can move to next step

        public Button _nextButton;
        public Button _prevButton;
        public Button _skipTutorialButton;
        public ProgressBar _progress;
        private TutorialStep[] _tutorialSteps;
        private int _currentStep;

        private void Awake()
        {
            _tutorialSteps = GetComponentsInChildren<TutorialStep>(true);
            if (_tutorialSteps == null || _tutorialSteps.Length == 0)
            {
                Debug.LogError("No tutorial steps found in tutorial");
                return;
            }

            for (int i = 0; i < _tutorialSteps.Length; i++)
            {
                _tutorialSteps[i].OnCompleted += StepCompletedCallback;
            }

            StartTutorial();
        }

        public void StartTutorial()
        {
            gameObject.SetActive(true);
            _currentStep = 0;
            _tutorialSteps[_currentStep].Activate();
            _nextButton.interactable = _tutorialSteps[_currentStep].CheckCompletion();
            StartCoroutine(ProgressInitCoroutine());
        }

        private IEnumerator ProgressInitCoroutine()
        {
            while (!_progress.isActiveAndEnabled)
            {
                yield return new WaitForEndOfFrame();
            }
            _progress.Init("Tutorial Progress");
        }

        public void CloseTutorial()
        {
            _tutorialSteps[_currentStep].Deactivate();
            gameObject.SetActive(false);
        }

        public void PrevStep()
        {
            _tutorialSteps[_currentStep].Deactivate();
            _currentStep--;
            _tutorialSteps[_currentStep].Activate();
            if (_currentStep == 0)
            {
                _prevButton.interactable = false;
            }
            _nextButton.interactable = true;
            _progress.SetCompletion((float)_currentStep / (float)_tutorialSteps.Length);
        }

        public void NextStep()
        {
            if (_currentStep == _tutorialSteps.Length - 1)
            {
                CloseTutorial();
            }
            _tutorialSteps[_currentStep].Deactivate();
            _currentStep++;
            _tutorialSteps[_currentStep].Activate();
            _nextButton.interactable = _tutorialSteps[_currentStep].CheckCompletion();
            _progress.SetCompletion((float)_currentStep / (float)_tutorialSteps.Length);
        }

        private void StepCompletedCallback(object sender, System.EventArgs args)
        {
            _nextButton.interactable = true;
        }
    }
}
