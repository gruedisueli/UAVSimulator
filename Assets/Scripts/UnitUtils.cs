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
        /// Gets the length of a city tile side.
        /// </summary>
        public static float GetCityTileSideLength()
        {
            double lat = EnvironManager.Instance.Environ.CenterLatLong.x;
            int zoom = EnvironSettings.CITY_ZOOM_LEVEL;
            int res = EnvironSettings.TILE_RESOLUTION;
            double c = EnvironSettings.SCALE_CONSTANT;
            return GetTileSideLength(lat, zoom, res, c);
        }

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
        /// Converts city options into city extents in world coords
        /// </summary>
        public static float[][] GetCityExtents(CityOptions c)
        {
            float rTHalfSide = GetRegionTileSideLength() / 2;
            float cTSide = GetCityTileSideLength();
            Vector3 minCorner = new Vector3(c.RegionTileWorldCenter.x - rTHalfSide, 0, c.RegionTileWorldCenter.z - rTHalfSide);

            var tile = GetLocalCityTileCoords(c.WorldPos, minCorner, cTSide);

            return TileCoordsToWorldExtents(tile, minCorner, cTSide, c.EastExt, c.WestExt, c.NorthExt, c.SouthExt);
        }

        /// <summary>
        /// Finds the local tile coordinates (x,y) within the larger region tile where provided world coordinate is located. X,Y coords are relative to region tile min pt.
        /// </summary>
        private static int[] GetLocalCityTileCoords(Vector3 sampleWorldPos, Vector3 regionTileMinCorner, float cityTileSideLength)
        {
            Vector3 toSample = sampleWorldPos - regionTileMinCorner;
            int xSteps = (int)Math.Floor(toSample.x / cityTileSideLength);
            int ySteps = (int)Math.Floor(toSample.z / cityTileSideLength);
            return new int[] { xSteps, ySteps };
        }

        /// <summary>
        /// Converts city tile coordinates within a region into actual world coordinates: x-range, z-range
        /// </summary>
        /// <returns></returns>
        private static float[][] TileCoordsToWorldExtents(int[] coordsXY, Vector3 minCorner, float cityTileSideLength, int eExt, int wExt, int nExt, int sExt)
        {
            int x0 = coordsXY[0] - wExt;
            int x1 = coordsXY[0] + 1 + eExt; //+1 because tile has a dimension!
            int y0 = coordsXY[1] - sExt;
            int y1 = coordsXY[1] + 1 + nExt; //+1 because tile has a dimension!

            float xR0 = CoordToWorldCoord(minCorner.x, x0, cityTileSideLength);
            float xR1 = CoordToWorldCoord(minCorner.x, x1, cityTileSideLength);
            float zR0 = CoordToWorldCoord(minCorner.z, y0, cityTileSideLength);
            float zR1 = CoordToWorldCoord(minCorner.z, y1, cityTileSideLength);

            return new float[][] { new float[] { xR0, xR1 }, new float[] { zR0, zR1 } };

        }

        /// <summary>
        /// Converts a single tile division line coord (int) into position in world space based on relevant region min tile coordinate compoent.
        /// </summary>
        private static float CoordToWorldCoord(float worldMinCoord, int coord, float cityTileSideLength)
        {
            return worldMinCoord + coord * cityTileSideLength;
        }
    }
}
