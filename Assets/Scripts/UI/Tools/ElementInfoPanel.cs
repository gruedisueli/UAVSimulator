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
    public abstract class ElementInfoPanel : PanelBase
    {
        public ModifyPanel ModifyPanel { get; private set; } = null;
        public ModifyTool[] ModifyTools { get; private set; } = null;
        public StartModifyTool StartModifyTool { get; private set; } = null;
        public RemoveTool RemoveTool { get; private set; } = null;
        public DeselectTool DeselectTool { get; private set; } = null;

        protected Image _image;
        protected TextPanel _infoPanel;
        protected ElementFollower _elementFollower;

        protected Dictionary<ElementPropertyType, string> preModifiedValues = new Dictionary<ElementPropertyType, string>();

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var m in ModifyTools)
            {
                m.OnElementModified -= ModifyTextValues;
            }

            if (StartModifyTool != null)
            {
                StartModifyTool.OnStartModify -= StartModify;
            }

            if (ModifyPanel != null)
            {
                ModifyPanel._closeTool.OnClose -= CancelModify;
                ModifyPanel.CommitTool.OnCommit -= CommitModify;
            }

            if (RemoveTool != null)
            {
                RemoveTool.OnSelectedElementRemoved -= Close;
            }
        }


        protected virtual void StartModify(object sender, System.EventArgs args)
        {
            preModifiedValues = new Dictionary<ElementPropertyType, string>();
            if (_infoPanel != null)
            {
                foreach (var e in _infoPanel.TextElements)
                {
                    preModifiedValues.Add(e.Key, e.Value._text.text);
                }
            }

            _closeTool.SetInteractable(false);
            StartModifyTool.SetInteractable(false);
            RemoveTool.SetInteractable(false);

            ModifyPanel.SetActive(true);
        }


        protected virtual void CommitModify(object sender, System.EventArgs args)
        {
            _closeTool.SetInteractable(true);
            StartModifyTool.SetInteractable(true);
            RemoveTool.SetInteractable(true);
        }


        protected virtual void CancelModify(object sender, System.EventArgs args)
        {
            _closeTool.SetInteractable(true);
            StartModifyTool.SetInteractable(true);
            RemoveTool.SetInteractable(true);

            //reset text to original premodified version.
            foreach (var kvp in preModifiedValues)
            {
                _infoPanel.TextElements[kvp.Key]._text.text = kvp.Value;
            }
        }

        /// <summary>
        /// Initializes for use with special scene elements.
        /// </summary>
        public virtual void Initialize(SceneElementBase sceneElement)
        {
            Initialize(sceneElement.gameObject);
        }

        /// <summary>
        /// Initializes for use with generic game objects that are not scene elements.
        /// </summary>
        public virtual void Initialize(GameObject sceneElement)
        {

            ModifyPanel = FindCompInChildren<ModifyPanel>();
            ModifyTools = GetComponentsInChildren<ModifyTool>(true);
            if (ModifyTools == null || ModifyTools.Length == 0)
            {
                Debug.Log("Modify tools not found in children of element info panel");
            }
            var imgs = GetComponentsInChildren<Image>(true);
            if (imgs == null || imgs.Length == 0)
            {
                Debug.Log("No image components found on info panel");
            }
            foreach (var im in imgs)
            {
                if (im.gameObject.name == "InfoImage")
                {
                    _image = im;
                    break;
                }
            }
            if (_image == null)
            {
                Debug.Log("Info image not found on info panel");
            }
            _infoPanel = FindCompInChildren<TextPanel>();
            if (_infoPanel != null)
            {
                _infoPanel.Initialize();
            }

            _elementFollower = FindCompInChildren<ElementFollower>();
            StartModifyTool = FindCompInChildren<StartModifyTool>();
            RemoveTool = FindCompInChildren<RemoveTool>();
            DeselectTool = FindCompInChildren<DeselectTool>();

            foreach (var m in ModifyTools)
            {
                m.OnElementModified += ModifyTextValues;
            }

            if (StartModifyTool != null)
            {
                StartModifyTool.OnStartModify += StartModify;
            }

            if (ModifyPanel != null)
            {
                ModifyPanel.Initialize();
                ModifyPanel._closeTool.OnClose += CancelModify;
                ModifyPanel.CommitTool.OnCommit += CommitModify;
                ModifyPanel.SetActive(false);
            }

            if (RemoveTool != null)
            {
                RemoveTool.OnSelectedElementRemoved += Close;
            }


            _elementFollower?.Initialize(sceneElement.gameObject);
        }

        protected abstract void ModifyTextValues(IModifyElementArgs args);
    }
}
