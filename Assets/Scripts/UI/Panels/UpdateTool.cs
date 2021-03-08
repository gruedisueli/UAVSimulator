using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.Fields;
using Assets.Scripts.UI.Events;

namespace Assets.Scripts.UI.Panels
{
    public delegate void ElementUpdated(IUpdateElementArgs args);
    
    /// <summary>
    /// A behavior that allows the user to modify existing simulation elements
    /// </summary>
    public abstract class UpdateTool : MonoBehaviour
    {
        public abstract EditableField[] Fields { get; }
        public event ElementUpdated ElementUpdatedEvent;

        public virtual void Activate()
        {
            gameObject.SetActive(true);
        }

        public virtual void Cancel()
        {
            gameObject.SetActive(false);
        }

        public virtual void Complete()
        {
            ElementUpdatedEvent.Invoke(GatherInformation());
        }

        protected abstract IUpdateElementArgs GatherInformation();
    }
}
