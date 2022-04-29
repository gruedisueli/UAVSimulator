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
        private FloatRange _upperElevRange, _lowerElevRange;
        private int[] _countRange;
        private bool _isMetric;
        public void Initialize(SimulationSettingsMainPanel mainPanel, int droneCt, float upperElev, float lowerElev, FloatRange upperElevRange, FloatRange lowerElevRange, int[] countRange, bool isMetric)
        {
            _mainPanel = mainPanel;
            GetModifyTools();
            _isMetric = isMetric;
            _droneCt = droneCt;
            _upperElevM = upperElev;
            _lowerElevM = lowerElev;
            _upperElevRange = upperElevRange;
            _lowerElevRange = lowerElevRange;
            _countRange = countRange;

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
                        var c = i.Value;
                        var min = _countRange[0];
                        var max = _countRange[1];
                        if (c <= max && c >= min)
                        {
                            _droneCt = c;
                        }
                        else
                        {
                            _droneCt = c > max ? max : min;
                        }
                    }
                    break;
                }
                case ElementPropertyType.UpperElevation:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        var e = _isMetric ? f.Value : UnitUtils.FeetToMeters(f.Value);
                        _upperElevM = _upperElevRange.ClampToRange(e);
                        if (_upperElevM <= _lowerElevM)
                        {
                            _upperElevM = _lowerElevM + 1;
                        }
                    }
                    break;
                }
                case ElementPropertyType.LowerElevation:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        var e = _isMetric ? f.Value : UnitUtils.FeetToMeters(f.Value);
                        _lowerElevM = _lowerElevRange.ClampToRange(e);
                        if (_lowerElevM >= _upperElevM)
                        {
                            _lowerElevM = _upperElevM - 1;
                        }
                    }
                    break;
                }
            }
            _mainPanel.UpdateBackgroundDroneSettings(_droneCt, _upperElevM, _lowerElevM);
        }
    }
}
