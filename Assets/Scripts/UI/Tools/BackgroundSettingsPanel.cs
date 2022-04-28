using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public class BackgroundSettingsPanel : SimulationSettingsPanelBase
    {
        private int _droneCt;
        private float _upperElevM, _lowerElevM;
        private bool _isMetric;
        public void Initialize(SimulationSettingsMainPanel mainPanel, int droneCt, float upperElev, float lowerElev, bool isMetric)
        {
            _mainPanel = mainPanel;
            GetModifyTools();
            _isMetric = isMetric;
            _droneCt = droneCt;
            _upperElevM = upperElev;
            _lowerElevM = lowerElev;

            UpdateIsMetric(isMetric);
            SetModifyToolValue(ElementPropertyType.DroneCount, droneCt);
            if (isMetric)
            {
                SetModifyToolValue(ElementPropertyType.UpperElevation, upperElev);
                SetModifyToolValue(ElementPropertyType.LowerElevation, lowerElev);
            }
            else
            {
                SetModifyToolValue(ElementPropertyType.UpperElevation, UnitUtils.MetersToFeet(upperElev));
                SetModifyToolValue(ElementPropertyType.LowerElevation, UnitUtils.MetersToFeet(lowerElev));
            }
        }

        protected override void OnModification(IModifyElementArgs args)
        {
            base.OnModification(args);
            switch (args.Update.ElementPropertyType)
            {
                case ElementPropertyType.DroneCount:
                {
                    if (args.Update is ModifyIntPropertyArg i)
                    {
                        _droneCt = i.Value;
                    }
                    break;
                }
                case ElementPropertyType.UpperElevation:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        _upperElevM = _isMetric ? f.Value : UnitUtils.FeetToMeters(f.Value);
                    }
                    break;
                }
                case ElementPropertyType.LowerElevation:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        _lowerElevM = _isMetric ? f.Value : UnitUtils.FeetToMeters(f.Value);
                    }
                    break;
                }
            }
            _mainPanel.UpdateBackgroundDroneSettings(_droneCt, _upperElevM, _lowerElevM);
        }
    }
}
