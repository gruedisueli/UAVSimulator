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

    public enum ToolMessageCategory
    {
        Unset,
        ElementModification,
        VisibilityModification
    }

    //NOTE***  explicitly numbering some of these so we don't break things in Unity editor when we add/remove items from this list.
    public enum ElementPropertyType
    {
        Settings = -1,
        Unset = 0,
        Type = 1,
        Description = 2,
        XZPosition = 3,
        Position = 4,
        Rotation = 5,
        XScale = 6,
        ZScale = 7,
        Scale = 8,
        Height = 9,
        Radius = 10,
        Bottom = 11,
        Top = 12,
        StandByPos = 13,
        LandingQueueHead = 14,
        LandingQueueDirection = 15,
        LandingPoint = 16,
        ParkingSpots = 17,
        MaxVehicleSize = 18,
        IsMountable = 19,
        IsOnTheGround = 20,
        IsScalable = 21,
        Name = 22,
        //Population = 23,
        //Jobs = 24,
        EastExt = 25,
        WestExt = 26,
        NorthExt = 27,
        SouthExt = 28,
        Id = 29,
        Capacity = 30,
        MaxSpeed = 31,
        YawSpeed = 32,
        TakeOffSpeed = 33,
        LandingSpeed = 34,
        Range = 35,
        Emission = 36,
        Noise = 37,
        TargetPosition = 38,
        CurrentSpeed = 39,
        Elevation = 40,
        Origin = 41,
        Destination = 42,
        DestinationList = 43,
        Separation = 44,
        State = 45,
        PlaceInQueue = 46,
        ToPark = 47,
        MoveForward = 48,
        IsUTM = 49,
        IsBackgroundDrone = 50,
        WaitTimer = 51,
        WaitTime = 52,
        AverageSpeed = 53,
        Throughput = 54,
        GrossEnergyConsumption = 55,
        GrossEmission = 56,
        HighNoiseBuildings = 57,
        MediumNoiseBuildings = 58,
        LowNoiseBuildings = 59,
        FlyingDrones = 60,
        CongestedCorridors = 61,
        CongestedParkingStructures = 62,
        CongestedDronePorts = 63,
        CallGenInterval = 64,
        AcceptableNoiseThreshold = 65,
        SimulationSpeedMultiplier = 66,
        SoundLevelAtSource = 67,
        DroneCount = 68,
        UpperElevation = 69,
        LowerElevation = 70,
        LowAltDroneTravelRadius = 71
    }

    public enum VisibilityType
    {
        Unset= 0,
        DroneCount = 1,
        Noise = 2,
        Privacy = 3,
        Routes = 4,
        FlightTrails = 5,
        LandingCorridors = 6,
        Demographics = 7,
        NoiseSpheres = 8,
        VehicleMeshSimple = 9,
        RestrictionZones = 10,
        Buildings = 11
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

    public enum AirspaceClass
    {
        Unset,
        A,
        B,
        C,
        D,
        E,
        G
    }

    public enum DroneType
    {
        Corridor,
        LowAltitude,
        Background
    }

    public enum SaveMode
    {
        JustSave,
        AndMain,
        AndQuit
    }

    public enum RestrictionPanelType
    {
        Rect = 0,
        Cyl = 1,
        CylStacked = 2
    }

    public enum ParkingPanelType
    {
        Rect = 0,
        Custom = 1
    }

    public enum UnitType
    {
        None = 0,
        Speed = 1,
        Distance = 2
    }

    public enum ViewDirection
    {
        Top = 0,
        North = 1,
        Northeast = 2,
        East = 3,
        Southeast = 4,
        South = 5,
        Southwest = 6,
        West = 7,
        Northwest = 8
    }

    public enum TutorialStepType
    {
        General = 0,
        UnityEditorConfig = 1,
        AwaitPanView = 2,
        AwaitTiltView = 3,
        AwaitZoomView = 4,
        Await3DronePorts = 5,
        Await1Parking = 6,
        Await1Restriction = 7,
        AwaitZoomToRestriction = 8,
        AwaitModify = 9,
        AwaitZoomToBuildingLevel = 10,
        Await4DronePorts = 11,
        AwaitBuildingsEnabled = 12
        
    }

}
