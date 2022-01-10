using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.EventSystems;
using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class SelectableGameObject : MonoBehaviour
    {
        public EventHandler<SelectGameObjectArgs> OnSelected;

        public void OnMouseUp()
        {
            if (!EventSystem.current.IsPointerOverGameObject())//prevent selection of objects behind GUI
            {
                Debug.Log("Clicked selectable game object");
                if ( OnSelected != null) OnSelected.Invoke(this, new SelectGameObjectArgs());
            }
            else
            {
                Debug.Log("Game object behind UI");
            }
        }
    }
}
