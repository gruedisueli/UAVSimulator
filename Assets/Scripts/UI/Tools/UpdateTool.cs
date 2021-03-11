using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.Fields;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public delegate void ElementUpdated(IUpdateElementArgs args);
    public delegate void StartUpdating();
    public delegate void CommitChange(bool commitChange);
    
    /// <summary>
    /// A behavior that allows the user to modify existing simulation elements
    /// </summary>
    public abstract class UpdateTool : MonoBehaviour
    {
        public abstract EditableField[] Fields { get; }
        public event ElementUpdated ElementUpdatedEvent;
        public event CommitChange CommitChangeEvent;
        public event StartUpdating StartUpdatingEvent;

        /// <summary>
        /// Called when turning on the modiifer.
        /// </summary>
        public virtual void Activate()
        {
            gameObject.SetActive(true);
            StartUpdatingEvent.Invoke();
        }

        /// <summary>
        /// Called when cancelling changes.
        /// </summary>
        public virtual void Cancel()
        {
            CommitChangeEvent.Invoke(false);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Called whenever a property is changed.
        /// </summary>
        public virtual void RegisterUpdate()
        {
            ElementUpdatedEvent.Invoke(GatherInformation());
        }
        
        /// <summary>
        /// Called when committing the changes.
        /// </summary>
        public virtual void Commit()
        {
            CommitChangeEvent.Invoke(true);
        }

        protected abstract IUpdateElementArgs GatherInformation();
    }
}
