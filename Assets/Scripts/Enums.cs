using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public enum ElementFamily
    {
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
        IsScalable
    }
}
