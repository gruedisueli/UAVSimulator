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

        protected Image _image;
        protected TextPanel _infoPanel;
        protected ElementFollower _elementFollower;

        protected Dictionary<ElementPropertyType, string> preModifiedValues = new Dictionary<ElementPropertyType, string>();

        protected override void Start()
        {
            base.Start();

            ModifyPanel = FindCompInChildren<ModifyPanel>();
            ModifyTools = GetComponentsInChildren<ModifyTool>(true);
            if (ModifyTools == null || ModifyTools.Length == 0)
            {
                Debug.LogError("Modify tools not found in children of element info panel");
            }
            _image = FindCompInChildren<Image>();
            _infoPanel = FindCompInChildren<TextPanel>();
            _elementFollower = FindCompInChildren<ElementFollower>();
            StartModifyTool = FindCompInChildren<StartModifyTool>();

            foreach(var m in ModifyTools)
            {
                m.OnElementModified += ModifyTextValues;
            }

            if (_closeTool != null)
            {
                _closeTool.OnClose += Close;
            }

            if (StartModifyTool != null)
            {
                StartModifyTool.OnStartModify += StartModify;
            }

            if (ModifyPanel != null)
            {
                ModifyPanel._closeTool.OnClose += CancelModify;
                ModifyPanel.CommitTool.OnCommit += CommitModify;
            }

        }

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

            _closeTool.gameObject.SetActive(false);
            StartModifyTool.gameObject.SetActive(false);
            ModifyPanel.SetActive(true);
        }


        protected virtual void CommitModify(object sender, System.EventArgs args)
        {
            _closeTool.gameObject.SetActive(true);
            StartModifyTool.gameObject.SetActive(true);
        }


        protected virtual void CancelModify(object sender, System.EventArgs args)
        {
            _closeTool.gameObject.SetActive(true);
            StartModifyTool.gameObject.SetActive(true);

            //reset text to original premodified version.
            foreach(var kvp in preModifiedValues)
            {
                _infoPanel.TextElements[kvp.Key]._text.text = kvp.Value;
            }
        }

        /// <summary>
        /// Sets the value of a text element on this panel.
        /// </summary>
        protected void SetTextElement(ElementPropertyType pT, string value)
        {
            try
            {
                _infoPanel.TextElements[pT].SetTextAsValue(value);
            }
            catch
            {
                Debug.LogError("Did not find property type in text panel to populate with provided value");
            }
        }

        public virtual void Initialize(SceneElementBase sceneElement)
        {
            _elementFollower.Initialize(sceneElement);
        }

        protected abstract void ModifyTextValues(IModifyElementArgs args);
    }
}
