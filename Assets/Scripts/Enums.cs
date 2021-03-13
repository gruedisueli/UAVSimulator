using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public enum ElementFamily
    {
        City,
        DronePort,
        ParkingStruct,
        RestrictionZone
    }

    public enum DronePortCategory
    {
        Rect,
        Custom
    }

    public enum ParkingStructCategory
    {
        Rect,
        Custom
    }

    public enum RestrictionZoneCategory
    {
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
}
