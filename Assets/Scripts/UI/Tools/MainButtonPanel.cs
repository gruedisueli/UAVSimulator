using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    /// <summary>
    /// A button panel at the highest level of the GUI, that may contain sub panels or buttons
    /// </summary>
    public class MainButtonPanel : ButtonPanel
    {
        /// <summary>
        /// Main children of this panel, not grandchildren, populated manually in editor.
        /// We need this list to keep track of click events.
        /// </summary>
        public Button[] _mainButtons;

        /// <summary>
        /// If any button has a panel on it, it will populate here.
        /// </summary>
        protected ButtonPanel[] _buttonPanels;

        /// <summary>
        /// Keeps track of currently open panel.
        /// </summary>S
        protected int _activePanel = -1;

        protected override void Awake()
        {
            base.Awake();

            _buttonPanels = new ButtonPanel[_mainButtons.Length];
            for (int i = 0; i < _mainButtons.Length; i++)
            {
                //may be null for some buttons, but that's ok.
                _buttonPanels[i] = _mainButtons[i].gameObject.GetComponentInChildren<ButtonPanel>(true);
                int idx = i;
                _mainButtons[i].onClick.AddListener(delegate { ButtonClick(idx); });
            }

            //it's ok if no button panels are found. Some menus may not have them.
        }

        protected void ButtonClick(int index)
        {
            _activePanel = index;
            for(int i = 0; i < _buttonPanels.Length; i++)
            {
                if (i == _activePanel)
                {
                    continue;
                }
                _buttonPanels[i]?.SetActive(false);
            }
        }
    }
}
