using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class CloseTool : MonoBehaviour
    {
        public EventHandler<System.EventArgs> OnClose;

        public void Close()
        {
            OnClose.Invoke(this, new System.EventArgs());
        }
    }
}
