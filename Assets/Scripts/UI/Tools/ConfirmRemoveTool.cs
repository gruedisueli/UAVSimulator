using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class ConfirmRemoveTool : MonoBehaviour
    {
        public EventHandler<System.EventArgs> OnYes;
        public EventHandler<System.EventArgs> OnNo;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            if (active)
            {
                gameObject.transform.SetAsLastSibling();
            }
        }
        public void Yes()
        {
            OnYes?.Invoke(this, System.EventArgs.Empty);
        }

        public void No()
        {
            OnNo?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
