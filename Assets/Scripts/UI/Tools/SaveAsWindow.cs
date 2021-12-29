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
    public class SaveAsWindow : MonoBehaviour
    {
        public Text _saveText;
        public Text _secondaryText;
        public InputField _inputField;
        public GameObject _saveAsButton;
        public GameObject _cancelButton;
        public GameObject _yesButton;
        public GameObject _noButton;
        public Color _successColor;
        public Color _failColor;

        private FileWindow _fileWindow;
        private string _saveAsText = "Save As...";
        private string _overwriteText = "File exists, overwrite?";
        private SaveMode _saveMode = SaveMode.JustSave;

        /// <summary>
        /// Attempts to save the file as name specified in input field
        /// </summary>
        public void SaveAs()
        {
            _secondaryText.text = "";
            string name = _inputField.text;
            if (EnvironManager.Instance.IsFilenameValid(name))
            {
                if (!EnvironManager.Instance.DoesFileExist(name))
                {
                    EnvironManager.Instance.SaveFile(name);
                    StartCoroutine(Success());
                }
                else
                {
                    SetMainDialog(false);
                }
            }
            else
            {
                _secondaryText.text = "Filename contains invalid characters";
                _secondaryText.color = _failColor;
            }
        }

        /// <summary>
        /// Called when succesfully saved-as to display some confirmation text for a few seconds, and then close out.
        /// </summary>
        private IEnumerator Success()
        {
            _secondaryText.text = "Succesfully saved file";
            _secondaryText.color = _successColor;
            yield return new WaitForSeconds(2);
            switch (_saveMode)
            {
                case SaveMode.JustSave:
                    {
                        Deactivate();
                        break;
                    }
                case SaveMode.AndMain:
                    {
                        EnvironManager.Instance.CreateNew();
                        SceneManager.LoadScene(UISettings.MAINMENU_SCENEPATH, LoadSceneMode.Single);
                        break;
                    }
                case SaveMode.AndQuit:
                    {
                        Application.Quit();
                        break;
                    }
            }
        }

        /// <summary>
        /// Overwrites existing save file
        /// </summary>
        public void Overwrite()
        {
            EnvironManager.Instance.SaveFile(_inputField.text);
            _yesButton.SetActive(false);
            _noButton.SetActive(false);
            StartCoroutine(Success());
        }

        /// <summary>
        /// Returns from overwrite window to main save as
        /// </summary>
        public void BackToSaveAs()
        {
            SetMainDialog(true);
        }

        /// <summary>
        /// Cancels current operation
        /// </summary>
        public void Cancel()
        {
            Deactivate();
        }

        public void Activate(SaveMode saveMode)
        {
            _saveMode = saveMode;
            _inputField.text = "";
            var prefab = Resources.Load<GameObject>("GUI/FileWindow");
            _fileWindow = Instantiate(prefab, gameObject.transform).GetComponent<FileWindow>();
            _fileWindow.OnFileSelect += SelectFile;
            _fileWindow.Init();
            SetMainDialog(true);
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            SetMainDialog(true);
            _fileWindow.OnFileSelect -= SelectFile;
            _fileWindow.gameObject.Destroy();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Toggles main save-as dialog and secondary overwrite dialog.
        /// </summary>
        private void SetMainDialog(bool isMain)
        {
            _secondaryText.text = "";
            _saveText.text = isMain ? _saveAsText : _overwriteText;
            _inputField.gameObject.SetActive(isMain);
            _saveAsButton.SetActive(isMain);
            _cancelButton.SetActive(isMain);
            _fileWindow.gameObject.SetActive(isMain);
            _yesButton.SetActive(!isMain);
            _noButton.SetActive(!isMain);
        }

        private void SelectFile(object sender, System.EventArgs args)
        {
            _inputField.text = (sender as FileItem)?.GetName();
        }
    }
}
