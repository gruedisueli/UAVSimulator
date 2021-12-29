using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.Environment;

namespace Assets.Scripts.UI.Tools
{
    /// <summary>
    /// User interface element for managing files
    /// </summary>
    public class FileManager : MonoBehaviour
    {
        public Text _mainText;
        public Text _secondaryText;
        public Text _selectedFile;
        public InputField _inputField;
        public Button _renameButton;
        public Button _deleteButton;
        public GameObject _confirmDeleteButton;
        public GameObject _cancelDeleteButton;
        public GameObject _confirmRenameButton;
        public GameObject _cancelRenameButton;
        public GameObject _closeButton;
        public Color _successColor;
        public Color _failColor;

        private FileWindow _fileWindow;

        /// <summary>
        /// Starts rename process
        /// </summary>
        public void Rename()
        {
            DisableFileWindow();
            _mainText.text = "Rename File...";
            _inputField.text = "";
            _inputField.gameObject.SetActive(true);
            _confirmRenameButton.SetActive(true);
            _cancelRenameButton.SetActive(true);
            _renameButton.gameObject.SetActive(false);
            _deleteButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Confirms we actually want to rename file
        /// </summary>
        public void ConfirmRename()
        {
            if (EnvironManager.Instance.RenameFile(_selectedFile.text, _inputField.text))
            {
                GoToMain();
            }
            else
            {
                _secondaryText.text = "Failed to rename file";
                _secondaryText.color = _failColor;
            }
        }

        /// <summary>
        /// Cancels rename process
        /// </summary>
        public void CancelRename()
        {
            GoToMain();
        }

        /// <summary>
        /// Starts delete process
        /// </summary>
        public void Delete()
        {
            DisableFileWindow();
            _mainText.text = "DELETE FILE?";
            _confirmDeleteButton.SetActive(true);
            _cancelDeleteButton.SetActive(true);
            _renameButton.gameObject.SetActive(false);
            _deleteButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Confirms we actually want to delete file
        /// </summary>
        public void ConfirmDelete()
        {
            if (EnvironManager.Instance.DeleteFile(_selectedFile.text))
            {
                GoToMain();
            }
            else
            {
                _secondaryText.text = "Failed to delete file";
                _secondaryText.color = _failColor;
            }
        }

        /// <summary>
        /// Cancels delete process
        /// </summary>
        public void CancelDelete()
        {
            GoToMain();
        }

        /// <summary>
        /// Turns on the manager
        /// </summary>
        public void Activate()
        {
            GoToMain();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Closes the manager
        /// </summary>
        public void Close()
        {
            DisableFileWindow();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Returns to main level of manager
        /// </summary>
        private void GoToMain()
        {
            EnableFileWindow();
            _mainText.text = "Select a File...";
            _selectedFile.text = "";
            _secondaryText.text = "";
            _inputField.text = "";
            _inputField.gameObject.SetActive(false);
            _renameButton.gameObject.SetActive(true);
            _deleteButton.gameObject.SetActive(true);
            _renameButton.interactable = false;
            _deleteButton.interactable = false;
            _confirmDeleteButton.SetActive(false);
            _cancelDeleteButton.SetActive(false);
            _confirmRenameButton.SetActive(false);
            _cancelRenameButton.SetActive(false);
        }

        /// <summary>
        /// Called when a file is selected
        /// </summary>
        private void SelectFile(object sender, System.EventArgs args)
        {
            _selectedFile.text = (sender as FileItem)?.GetName();
            _renameButton.interactable = true;
            _deleteButton.interactable = true;
        }

        /// <summary>
        /// Called when a file is deselected
        /// </summary>
        private void DeselectFile(object sender, System.EventArgs args)
        {

        }

        /// <summary>
        /// Enables the file browser
        /// </summary>
        private void EnableFileWindow()
        {
            var prefab = Resources.Load<GameObject>("GUI/FileWindow");
            _fileWindow = Instantiate(prefab, gameObject.transform).GetComponent<FileWindow>();
            _fileWindow.OnFileSelect += SelectFile;
            _fileWindow.OnFileDeselect += DeselectFile;
            _fileWindow.Init();
        }

        /// <summary>
        /// Disables the file browser
        /// </summary>
        private void DisableFileWindow()
        {
            if (_fileWindow == null) return;
            _fileWindow.OnFileSelect -= SelectFile;
            _fileWindow.OnFileDeselect -= DeselectFile;
            _fileWindow.gameObject.Destroy();
        }

    }
}
