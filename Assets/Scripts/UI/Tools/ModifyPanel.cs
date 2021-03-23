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
    /// This is a panel JUST for containing things that modify an element.
    /// It SHOULD NOT be used to to add/remove/ etc.
    /// It SHOULD NOT contain other buttoms, etc.
    /// </summary>
    public class ModifyPanel : PanelBase
    {
        public CommitTool CommitTool { get; private set; } = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (CommitTool != null)
            {
                CommitTool.OnCommit -= CommitModification;
            }
        }

        //NOTE: cancellation of modification can be handled by the "close tool" attached to this panel. We can even label it "cancel"

        /// <summary>
        /// Called when committing the changes. Do not confuse this with deactivating this panel. Do that from the outside.
        /// </summary>
        public virtual void CommitModification(object sender, System.EventArgs args)
        {
            gameObject.SetActive(false);
            OnPanelClosed?.Invoke(this, new System.EventArgs());
        }

        public virtual void Initialize()
        {
            CommitTool = FindCompInChildren<CommitTool>();

            if (CommitTool != null)
            {
                CommitTool.OnCommit += CommitModification;
            }
        }

    }
}
