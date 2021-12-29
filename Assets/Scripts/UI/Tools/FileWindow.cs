using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class FileWindow : MonoBehaviour
    {
        public GameObject _scrollWindowContent;
        public EventHandler<System.EventArgs> OnFileSelect;
        public EventHandler<System.EventArgs> OnFileDeselect;
        private List<FileItem> _files;

        public void Init()
        {
            _files = new List<FileItem>();
            var names = EnvironManager.Instance.GetAllSaveFiles();
            if (names == null)
            {
                Debug.Log("No saved files in directory");
                return;
            }
            var prefab = Resources.Load<GameObject>("GUI/FileButton");
            for (int i = 0; i < names.Length; i++)
            {
                var button = Instantiate(prefab);
                button.transform.SetParent(_scrollWindowContent.transform, false);
                FileItem item = button.GetComponent<FileItem>();
                item.Init(names[i]);
                item.OnSelected += SelectFile;
                item.OnDeselected += DeselectFile;
                _files.Add(item);
            }
        }

        private void OnDestroy()
        {
            if (_files != null)
            {
                foreach (var f in _files)
                {
                    f.OnSelected -= SelectFile;
                    f.OnDeselected -= DeselectFile;
                }
            }
        }

        private void SelectFile(object sender, System.EventArgs args)
        {
            OnFileSelect?.Invoke(sender, args);
        }

        private void DeselectFile(object sender, System.EventArgs args)
        {
            OnFileDeselect?.Invoke(sender, args);
        }
    }
}
