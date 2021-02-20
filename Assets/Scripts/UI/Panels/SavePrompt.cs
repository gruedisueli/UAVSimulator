using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using Assets.Scripts.Environment;
using UnityEngine.UI;


namespace Assets.Scripts.UI.Panels
{
    public class SavePrompt : MonoBehaviour
    {
        public Text _saveText;
        private bool _isQuitOperation;

        /// <summary>
        /// Initiates process of going to main menu
        /// </summary>
        public void GoMain()
        {
            _saveText.text = "Exiting to main menu. Save the current region?";
            _isQuitOperation = false;
            Activate();
        }

        /// <summary>
        /// Initiates process of quitting
        /// </summary>
        public void Quit()
        {
            _saveText.text = "Quitting application. Save the current region?";
            _isQuitOperation = true;
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
        /// Sets save bool value
        /// </summary>
        public void SaveOrNot(bool save)
        {
            Deactivate();
            if (_isQuitOperation)
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
            EnvironController.Instance.CreateNew();
            SceneManager.LoadScene(UISettings.MAINMENU_SCENEPATH, LoadSceneMode.Single);
        }

        /// <summary>
        /// Saves the environment to file and goes to main menu
        /// </summary>
        private void SaveAndGoMain()
        {
            EnvironController.Instance.SaveFile();
            EnvironController.Instance.CreateNew();
            SceneManager.LoadScene(UISettings.MAINMENU_SCENEPATH, LoadSceneMode.Single);
        }

        /// <summary>
        /// Saves the environment to file and quits application
        /// </summary>
        private void SaveAndQuit()
        {
            EnvironController.Instance.SaveFile();
            Application.Quit();
        }

        /// <summary>
        /// Quits the application without saving current environment
        /// </summary>
        private void QuitWithoutSave()
        {
            Application.Quit();
        }
    }
}
