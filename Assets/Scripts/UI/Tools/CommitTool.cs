using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class CommitTool : ToolBase
    {
        public EventHandler<System.EventArgs> OnCommit;

        public void Commit()
        {
            OnCommit.Invoke(this, new System.EventArgs());
        }
    }
}
