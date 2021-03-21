using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class StartModifyTool : ToolBase
    {
        public EventHandler<System.EventArgs> OnStartModify;

        public void StartModify()
        {
            OnStartModify.Invoke(this, new System.EventArgs());
        }
    }
}
