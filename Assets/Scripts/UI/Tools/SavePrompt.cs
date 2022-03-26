using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using Assets.Scripts.Environment;
using UnityEngine.UI;


namespace Assets.Scripts.UI.Tools
{
    public class SavePrompt : MonoBehaviour
    {
        public Text _saveText;
        public Text _secondaryText;
        public GameObject _save;
        //public GameObject _saveAs;
        public GameObject _cancel;
        public GameObject _dontSave;
        public Color _successColor;
        public Color _failColor;
        private SaveAsWindow _saveAsWindow;
        private SaveMode _mode = SaveMode.JustSave;

        private void Awake()
        {
            _saveAsWindow = FindObjectOfType<SaveAsWindow>(true);
            if (_saveAsWindow == null) Debug.LogError("Could not find save as window component in scene");
        }

        /// <summary>
        /// Initiates process of going to main menu
        /// </summary>
        public void GoMain()
        {
            _saveText.text = "Exiting to main menu. Save the current region?";
            _dontSave.SetActive(true);
            _mode = SaveMode.AndMain;
            Activate();
        }

        /// <summary>
        /// Initiates process of quitting
        /// </summary>
        public void Quit()
        {
            _saveText.text = "Quitting application. Save the current region?";
            _dontSave.SetActive(true);
            _mode = SaveMode.AndQuit;
            Activate();
        }

        /// <summary>
        /// Initiates process of saving without going anywhere else.
        /// </summary>
        public void IntermediateSave()
        {
            _saveText.text = "Save File...";
            _dontSave.SetActive(false);
            _mode = SaveMode.JustSave;
            Activate();
        }

        /// <summary>
        /// Cancels current operation
        /// </summary>
        public void Cancel()
        {
            Deactivate();
        }

        /// <summary>
        /// Changes over to Save As Window
        /// </summary>
        public void SaveAs()
        {
            Deactivate();
            _saveAsWindow.Activate(_mode);
        }

        /// <summary>
        /// Sets save bool value
        /// </summary>
        public void SaveOrNot(bool save)
        {
            //Deactivate();
            if (_mode == SaveMode.JustSave && save)
            {
                SaveAndReturn();
            }
            else if (_mode == SaveMode.AndQuit)
            {
                if (save)
                {
                    SaveAndQuit();
                }
                else
                {
                    QuitWithoutSave();
                }
            }
            else
            {
                if (save)
                {
                    SaveAndGoMain();
                }
                else
                {
                    GoMainWithoutSave();
                }
            }
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            _save.SetActive(true);
            //_saveAs.SetActive(true);
            _cancel.SetActive(true);
            _secondaryText.text = "";
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }


        /// <summary>
        /// Goes to main menu without saving
        /// </summary>
        private void GoMainWithoutSave()
        {
            EnvironManager.Instance.CreateNew();
            SceneManager.LoadScene(UISettings.MAINMENU_SCENEPATH, LoadSceneMode.Single);
        }

        /// <summary>
        /// Saves the environment to file and goes to main menu
        /// </summary>
        private void SaveAndGoMain()
        {
            string name = EnvironManager.Instance.OpenedFile;
            if (EnvironManager.Instance.IsFilenameValid(name))
            {
                EnvironManager.Instance.SaveFile(name);
                StartCoroutine(Success());
            }
            else
            {
                Deactivate();
                _saveAsWindow.Activate(SaveMode.AndMain);
            }
        }

        /// <summary>
        /// Saves the environment to file and quits application
        /// </summary>
        private void SaveAndQuit()
        {
            string name = EnvironManager.Instance.OpenedFile;
            if (EnvironManager.Instance.IsFilenameValid(name))
            {
                EnvironManager.Instance.SaveFile(name);
                StartCoroutine(Success());
            }
            else
            {
                Deactivate();
                _saveAsWindow.Activate(SaveMode.AndQuit);
            }
        }

        /// <summary>
        /// Saves the environment to file and returns to simulation
        /// </summary>
        private void SaveAndReturn()
        {
            string name = EnvironManager.Instance.OpenedFile;
            if (EnvironManager.Instance.IsFilenameValid(name))
            {
                EnvironManager.Instance.SaveFile(name);
                StartCoroutine(Success());
            }
            else
            {
                Deactivate();
                _saveAsWindow.Activate(SaveMode.JustSave);
            }
        }

        /// <summary>
        /// Quits the application without saving current environment
        /// </summary>
        private void QuitWithoutSave()
        {
            Application.Quit();
        }


        /// <summary>
        /// Called when succesfully saved-as to display some confirmation text for a few seconds, and then close out.
        /// </summary>
        private IEnumerator Success()
        {
            _secondaryText.text = "Succesfully saved file";
            _secondaryText.color = _successColor;
            _save.SetActive(false);
            //_saveAs.SetActive(false);
            _cancel.SetActive(false);
            _dontSave.SetActive(false);
            yield return new WaitForSeconds(2);
            switch (_mode)
            {
                case SaveMode.JustSave:
                    {
                        Deactivate();
                        break;
                    }
                case SaveMode.AndMain:
                    {
                        GoMainWithoutSave();
                        break;
                    }
                case SaveMode.AndQuit:
                    {
                        Application.Quit();
                        break;
                    }
            }
        }
    }
}
