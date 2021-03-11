using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public delegate void ElementRemoved(IRemoveElementArgs args);

    /// <summary>
    /// Behavior for removing objects from scene
    /// </summary>
    public abstract class RemoveTool : MonoBehaviour
    {
        public event ElementRemoved ElementRemovedEvent;
        
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
            ElementRemovedEvent.Invoke(GatherInformation());
        }

        protected abstract IRemoveElementArgs GatherInformation();
    }
}
