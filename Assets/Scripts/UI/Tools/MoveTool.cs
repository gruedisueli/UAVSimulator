using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class MoveTool : MonoBehaviour
    {
        public System.EventHandler<System.EventArgs> OnClicked;
        public void Click()
        {
            OnClicked?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
