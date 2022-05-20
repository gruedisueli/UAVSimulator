using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class HelpManager : MonoBehaviour
    {
        public Texture2D _cursorNoHighlight;
        public Texture2D _cursorHighlight;
        public Vector2 _hotSpot = new Vector2(25.0f, 25.0f);
        public bool HelpEnabled { get; private set; } = false;

        public void ToggleHelp()
        {
            HelpEnabled = !HelpEnabled;
            if (HelpEnabled)
            {
                Cursor.SetCursor(_cursorNoHighlight, _hotSpot, CursorMode.Auto);
                StartCoroutine(HelpCoroutine());
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
        public void ToggleCursorHighlight(bool highlight)
        {
            if (highlight)
            {
                Cursor.SetCursor(_cursorHighlight, _hotSpot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(_cursorNoHighlight, _hotSpot, CursorMode.Auto);
            }
        }

        private IEnumerator HelpCoroutine()
        {
            while (HelpEnabled)
            {
                if (Input.GetKey(KeyCode.Escape))
                {
                    ToggleHelp();
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
