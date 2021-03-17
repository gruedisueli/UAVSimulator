using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class DeselectTool : MonoBehaviour
    {
        public EventHandler<DeselectArgs> OnDeselect;

        public void Deselect()
        {
            OnDeselect.Invoke(this, new DeselectArgs());
        }
    }
}
