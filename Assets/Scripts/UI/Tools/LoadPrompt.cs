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
    public class LoadPrompt : MonoBehaviour
    {
        public Text _secondaryText;
        public Button _loadButton;
        private FileWindow _fileWindow;
        private MainMenuManager _mainMenuManager;

        private void Awake()
        {
            _mainMenuManager = FindObjectOfType<MainMenuManager>(true);
            _fileWindow.OnFileSelect += SelectFile;
        }

        private void OnDestroy()
        {
            _fileWindow.OnFileSelect -= SelectFile;
        }

        public void Load()
        {
            ////note: if we add feature to load from simulation scenes, must save simulation before loading
            ////currently we assume loading only happens from main menu/startup screen
            //if (EnvironManager.Instance.DoesFileExist(_secondaryText.text))
            //{
            //    _mainMenuManager.LoadSaved(_secondaryText.text);
            //    Deactivate();
            //}
            //else
            //{
            //    _secondaryText.text = "File load error";
            //}
        }

        public void Cancel()
        {
            Deactivate();
        }

        public void Activate()
        {
            _loadButton.interactable = false;
            var prefab = Resources.Load<GameObject>("GUI/FileWindow");
            _fileWindow = Instantiate(prefab, gameObject.transform).GetComponent<FileWindow>();
            _fileWindow.OnFileSelect += SelectFile;
            _fileWindow.Init();
            _secondaryText.text = "select a file to continue";
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            _fileWindow.OnFileSelect -= SelectFile;
            _fileWindow.gameObject.Destroy();
            gameObject.SetActive(false);
        }

        private void SelectFile(object sender, System.EventArgs args)
        {
            _secondaryText.text = (sender as FileItem)?.GetName();
            _loadButton.interactable = true;
        }
    }
}
