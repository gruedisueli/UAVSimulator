using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.SceneManagement;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class SelectableGameObject : MonoBehaviour
    {
        public EventHandler<SelectGameObjectArgs> OnSelected;
        private bool _primed = false;//used in case user clicks a close/commit button over another object so you don't immediately select the thing behind button.

        void OnMouseDown()
        {
            if (!EventSystem.current.IsPointerOverGameObject())//prevent selection of objects behind GUI
            {
                _primed = true;
                StartCoroutine(TimeOut());
            }
        }

        public void OnMouseUp()
        {
            if (_primed && !EventSystem.current.IsPointerOverGameObject())//prevent selection of objects behind GUI
            {
                Debug.Log("Clicked selectable game object");
                if ( OnSelected != null) OnSelected.Invoke(this, new SelectGameObjectArgs());
            }
            else
            {
                Debug.Log("Game object behind UI");
            }
            _primed = false;
        }

        /// <summary>
        /// Reverts flag in case user clicks on game object and moves away or something unexpected happens.
        /// </summary>
        private IEnumerator TimeOut()
        {
            yield return new WaitForSeconds(5);
            _primed = false;
        }
    }
}
