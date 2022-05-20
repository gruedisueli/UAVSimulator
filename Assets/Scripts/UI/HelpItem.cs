using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HelpItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject _helpWindow;
        private HelpManager _helpManager;
        private Button _closeButton;
        private void Awake()
        {
            _helpManager = FindObjectOfType<HelpManager>(true);
            if (_helpManager == null)
            {
                Debug.LogError("Could not find help manager component");
                return;
            }
            if (_helpWindow == null)
            {
                Debug.LogError("Help window not specified");
                return;
            }

            _closeButton = _helpWindow.GetComponentInChildren<Button>(true);
            if (_closeButton == null)
            {
                Debug.LogError("Could not find close button on help window");
                return;
            }
            _closeButton.onClick.AddListener(Close);
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            if (!_helpManager.HelpEnabled)
            {
                return;
            }
            _helpManager.ToggleHelp();
            _helpWindow.SetActive(true);
        }

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if (!_helpManager.HelpEnabled)
            {
                return;
            }
            _helpManager.ToggleCursorHighlight(true);
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
            if (!_helpManager.HelpEnabled)
            {
                return;
            }
            _helpManager.ToggleCursorHighlight(false);
        }

        public void Close()
        {
            _helpWindow.SetActive(false);
        }
    }
}
