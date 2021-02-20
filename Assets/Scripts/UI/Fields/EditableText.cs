using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Fields
{
    public class EditableText : EditableField
    {
        public string textValue;

        public void SetText(string t)
        {
            statValueText.text = t;
            textValue = t;
        }

        public override void InputEntered()
        {
            statValueText.text = inputField.text;
            textValue = inputField.text;
            OnValueChanged();
            inputField.text = "";
        }

    }
}
