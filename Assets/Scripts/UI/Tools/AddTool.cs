using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.UI.EventArgs;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public delegate void ElementAdded(IAddElementArgs args);

    /// <summary>
    /// Behavior for adding elements to scene
    /// </summary>
    public abstract class AddTool : MonoBehaviour
    {
        public event ElementAdded ElementAddedEvent;

        private void Start()
        {
            Initialize();
        }

        public virtual void Activate()
        {
            gameObject.SetActive(true);
        }

        public virtual void Cancel()
        {
            gameObject.SetActive(false);
        }

        public virtual void Add()
        {
            ElementAddedEvent.Invoke(GatherInformation());
        }

        protected abstract void Initialize();
        protected abstract IAddElementArgs GatherInformation();
    }
}
