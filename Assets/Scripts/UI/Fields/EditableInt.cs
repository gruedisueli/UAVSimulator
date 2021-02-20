using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Fields
{
    public class EditableInt : EditableField
    {
        public int intValue;
        public int maxValue;
        public int minValue = 0;

        public void SetValue(int i)
        {
            statValueText.text = i.ToString();
            intValue = i;
        }

        public override void InputEntered()
        {
            if (int.TryParse(inputField.text, out int i)) //must separately output i so it doesn't overwrite existing in case of empty input
            {
                i = i > maxValue ? maxValue : i;
                i = i < minValue ? 0 : i;
                intValue = i;
                statValueText.text = i.ToString();
                OnValueChanged();
            }
            inputField.text = "";
        }

    }
}
