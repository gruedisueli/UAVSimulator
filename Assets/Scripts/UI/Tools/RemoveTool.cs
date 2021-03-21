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
            OnSelectedElementRemoved.Invoke(this, new System.EventArgs());
        }
    }
}
