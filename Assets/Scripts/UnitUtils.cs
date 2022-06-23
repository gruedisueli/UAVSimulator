using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.Environment;

using UnityEngine;

namespace Assets.Scripts
{
    public static class UnitUtils
    {
        /// <summary>
        /// Gets the length of a region tile side.
        /// </summary>
        public static float GetRegionTileSideLength()
        {
            double lat = EnvironManager.Instance.Environ.CenterLatLong.x;
            int zoom = EnvironSettings.REGION_ZOOM_LEVEL;
            int res = EnvironSettings.TILE_RESOLUTION;
            double c = EnvironSettings.SCALE_CONSTANT;
            return GetTileSideLength(lat, zoom, res, c);
        }

        /// <summary>
        /// Gets length of tile side.
        /// See https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Resolution_and_Scale 
        /// </summary>
        private static float GetTileSideLength(double latitude, int zoom, int tileRes, double scaleConstant)
        {
            double radians = latitude * Math.PI / 180;
            return (float)(Math.Abs(scaleConstant * Math.Cos(radians) / Math.Pow(2, zoom)) * tileRes);
        }

        /// <summary>
        /// Gets 2d extents of region view. { {minX, maxX}, {minZ, maxZ} }
        /// </summary>
        /// <returns></returns>
        public static float[][] GetRegionExtents()
        {
            //tiles are placed from a center tile
            //center tile's transform has xz coords measured from an origin in lower right corner.
            //therefore, center transform xz = {-0.5 * (region tile side), 0.5 * (region tile side)}
            //then increment to get extents to all sides.
            float regionTileSide = GetRegionTileSideLength();
            float halfTile = regionTileSide * 0.5f;
            float centerX = halfTile * -1.0f;
            float centerZ = halfTile;
            float ext = regionTileSide * EnvironSettings.REGION_TILE_EXTENTS;
            float maxX = centerX + halfTile + ext;
            float minX = centerX - halfTile - ext;
            float minZ = centerZ - halfTile - ext;
            float maxZ = centerZ + halfTile + ext;
            return new float[][] { new float[] { minX, maxX }, new float[] { minZ, maxZ } };

        }

        public static float MilesPerHourToMetersPerSecond(float milesPerHour)
        {
            return milesPerHour * EnvironSettings.FEET_PER_MILE / EnvironSettings.SECONDS_PER_HOUR / EnvironSettings.FEET_PER_METERS;
        }

        public static float MetersPerSecondToMilesPerHour(float metersPerSecond)
        {
            return metersPerSecond * EnvironSettings.FEET_PER_METERS / EnvironSettings.FEET_PER_MILE * EnvironSettings.SECONDS_PER_HOUR;
        }

        public static float MetersPerSecondToKilometersPerHour(float metersPerSecond)
        {
            return metersPerSecond * EnvironSettings.SECONDS_PER_HOUR / 1000.0f;
        }

        public static float KilometersPerHourToMetersPerSecond(float kilometersPerHour)
        {
            return kilometersPerHour / EnvironSettings.SECONDS_PER_HOUR * 1000.0f;
        }

        public static float FeetToMeters(float feet)
        {
            return feet / EnvironSettings.FEET_PER_METERS;
        }

        public static float MetersToFeet(float meters)
        {
            return meters * EnvironSettings.FEET_PER_METERS;
        }
    }
}
