using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    /// <summary>
    /// A panel that contains an array of butttons.
    /// </summary>
    public abstract class ButtonPanel : PanelBase
    {
        protected List<GameObject> _buttons = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
