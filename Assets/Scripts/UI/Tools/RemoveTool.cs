using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    /// <summary>
    /// Behavior for removing objects from scene
    /// </summary>
    public class RemoveTool : ToolBase
    {
        public EventHandler<System.EventArgs> OnSelectedElementRemoved;
        private ConfirmRemoveTool _removeConfirmTool;

        protected override void Awake()
        {
            base.Awake();
            _removeConfirmTool = FindObjectOfType<ConfirmRemoveTool>(true);
            if (_removeConfirmTool == null)
            {
                Debug.LogError("Could not find remove confirm tool in scene");
                return;
            }
            _removeConfirmTool.SetActive(false);
        }
        
        public virtual void Activate()
        {
            gameObject.SetActive(true);
        }

        public virtual void Cancel()
        {
            gameObject.SetActive(false);
        }

        public virtual void Remove()
        {
            _removeConfirmTool.SetActive(true);
            _removeConfirmTool.OnYes += OnRemoveConfirmed;
            _removeConfirmTool.OnNo += OnRemoveCancelled;
        }

        public void OnRemoveConfirmed(object sender, System.EventArgs args)
        {
            OnSelectedElementRemoved.Invoke(this, new System.EventArgs());
            CloseConfirmationWindow();
        }

        public void OnRemoveCancelled(object sender, System.EventArgs args)
        {
            CloseConfirmationWindow();
        }

        private void CloseConfirmationWindow()
        {
            _removeConfirmTool.OnYes -= OnRemoveConfirmed;
            _removeConfirmTool.OnNo -= OnRemoveCancelled;
            _removeConfirmTool.SetActive(false);
        }
    }
}
