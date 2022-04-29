using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Environment;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public abstract class DroneSettingsPanel : SimulationSettingsPanelBase
    {
        protected DroneSettings _droneSettingsCopy;
        protected float _flightElevM;
        protected FloatRange _flightElevRange;
        protected bool _isMetric;
        protected void InitializeDroneSettings(DroneSettings droneSettings, float flightElevM, FloatRange flightElevRange, bool isMetric)
        {
            _droneSettingsCopy = new DroneSettings(droneSettings);
            var s = _droneSettingsCopy;
            _flightElevM = flightElevM;
            _flightElevRange = flightElevRange;
            _isMetric = isMetric;
            SetModifyToolValue(ElementPropertyType.Capacity, s.Capacity);
            SetModifyToolValue(ElementPropertyType.SoundLevelAtSource, s.SoundAtSource_Decibels);
            if (isMetric)
            {
                SetModifyToolValue(ElementPropertyType.Elevation, _flightElevM);
                SetModifyToolValue(ElementPropertyType.MaxSpeed, UnitUtils.MetersPerSecondToKilometersPerHour(s.MaxSpeed_MPS));
                SetModifyToolValue(ElementPropertyType.TakeOffSpeed, UnitUtils.MetersPerSecondToKilometersPerHour(s.TakeOffSpeed_MPS));
                SetModifyToolValue(ElementPropertyType.LandingSpeed, UnitUtils.MetersPerSecondToKilometersPerHour(s.LandingSpeed_MPS));
            }
            else
            {
                SetModifyToolValue(ElementPropertyType.Elevation, UnitUtils.MetersToFeet(_flightElevM));
                SetModifyToolValue(ElementPropertyType.MaxSpeed, UnitUtils.MetersPerSecondToMilesPerHour(s.MaxSpeed_MPS));
                SetModifyToolValue(ElementPropertyType.TakeOffSpeed, UnitUtils.MetersPerSecondToMilesPerHour(s.TakeOffSpeed_MPS));
                SetModifyToolValue(ElementPropertyType.LandingSpeed, UnitUtils.MetersPerSecondToMilesPerHour(s.LandingSpeed_MPS));
            }
        }

        protected override void OnModification(IModifyElementArgs args)
        {
            base.OnModification(args);
            var s = _droneSettingsCopy;
            switch (args.Update.ElementPropertyType)
            {
                case ElementPropertyType.Elevation:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        var e = _isMetric ? f.Value : UnitUtils.FeetToMeters(f.Value);
                        _flightElevM = _flightElevRange.ClampToRange(e);
                    }
                    break;
                }
                case ElementPropertyType.Capacity:
                {
                    if (args.Update is ModifyIntPropertyArg i)
                    {
                        s.Capacity = i.Value;
                    }
                    break;
                }
                case ElementPropertyType.SoundLevelAtSource:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        s.SoundAtSource_Decibels = _droneSettingsCopy.SoundAtSourceRange_Db.ClampToRange(f.Value);
                    }
                    break;
                }
                case ElementPropertyType.MaxSpeed:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        var spd = _isMetric ? UnitUtils.KilometersPerHourToMetersPerSecond(f.Value) : UnitUtils.MilesPerHourToMetersPerSecond(f.Value);
                        s.MaxSpeed_MPS = _droneSettingsCopy.MaxSpeedRange_MPS.ClampToRange(spd);
                    }
                    break;
                }
                case ElementPropertyType.TakeOffSpeed:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        var spd = _isMetric ? UnitUtils.KilometersPerHourToMetersPerSecond(f.Value) : UnitUtils.MilesPerHourToMetersPerSecond(f.Value);
                        s.TakeOffSpeed_MPS = _droneSettingsCopy.TakeOffSpeedRange_MPS.ClampToRange(spd);
                    }
                    break;
                }
                case ElementPropertyType.LandingSpeed:
                {
                    if (args.Update is ModifyFloatPropertyArg f)
                    {
                        var spd = _isMetric ? UnitUtils.KilometersPerHourToMetersPerSecond(f.Value) : UnitUtils.MilesPerHourToMetersPerSecond(f.Value);
                        s.LandingSpeed_MPS = _droneSettingsCopy.LandingOffSpeedRange_MPS.ClampToRange(spd);
                    }
                    break;
                }
            }
        }
    }
}
