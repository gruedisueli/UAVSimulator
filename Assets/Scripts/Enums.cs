using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public enum ElementFamily
    {
        Unset,
        City,
        DronePort,
        ParkingStruct,
        RestrictionZone
    }

    public enum DronePortCategory
    {
        Unset,
        Rect,
        Custom
    }

    public enum ParkingStructCategory
    {
        Unset,
        Rect,
        Custom
    }

    public enum RestrictionZoneCategory
    {
        Unset,
        Rect,
        Cylindrical,
        CylindricalStacked
    }

    public enum ElementPropertyType
    {
        Unset,
        Type,
        XZPosition,
        Position,
        Rotation,
        XScale,
        ZScale,
        Scale,
        Height,
        Radius,
        Bottom,
        Top,
        StandByPos,
        LandingQueueHead,
        LandingQueueDirection,
        LandingPoint,
        ParkingSpots,
        MaxVehicleSize,
        IsMountable,
        IsOnTheGround,
        IsScalable,
        Name,
        Population,
        Jobs,
        EastExt,
        WestExt,
        NorthExt,
        SouthExt
    }

    public enum InputFieldType
    {
        Unset,
        String_,
        Float_,
        Integer_
    }

    public enum SceneType
    {
        Unset,
        Main,
        FindLoc,
        Region,
        City,
        Quit
    }
}
