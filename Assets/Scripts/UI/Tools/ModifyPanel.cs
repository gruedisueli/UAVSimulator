using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public delegate void StartModify();
    public delegate void CommitChange(bool commitChange);

    /// <summary>
    /// This is a panel JUST for containing things that modify an element.
    /// It SHOULD NOT be used to to add/remove/ etc.
    /// It SHOULD NOT contain other buttoms, etc.
    /// </summary>
    public class ModifyPanel : MonoBehaviour
    {
        public event StartModify OnStartModify;
        public event CommitChange OnCommitChange;

        protected ModifyTool[] _childTools;

        private void Start()
        {

        }

        private void OnDestroy()
        {

        }

        /// <summary>
        /// Called to activate/deactivate this entire game object and children.
        /// </summary>
        public virtual void SetActive()
        {
            gameObject.SetActive(true);
            OnStartModify.Invoke();
        }

        /// <summary>
        /// Called when cancelling changes. Do not confuse this with deactivating this panel. Do that from the outside.
        /// </summary>
        public virtual void CancelModification()
        {
            OnCommitChange.Invoke(false);
        }

        /// <summary>
        /// Called when committing the changes. Do not confuse this with deactivating this panel. Do that from the outside.
        /// </summary>
        public virtual void CommitModification()
        {
            OnCommitChange.Invoke(true);
        }

    }
}
