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
        /// See https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Resolution_and_Scale 
        /// </summary>
        public static double GetCityTileSideLength()
        {
            double lat = EnvironManager.Instance.Environ.CenterLatLong.x;
            int zoom = EnvironSettings.CITY_ZOOM_LEVEL;
            int res = EnvironSettings.TILE_RESOLUTION;
            double c = EnvironSettings.SCALE_CONSTANT;
            return c * Math.Cos(lat) / Math.Pow(2, zoom) * res;
        }

        /// <summary>
        /// Finds the local tile coordinates (x,y) within the larger region tile where provided world coordinate is located. X,Y coords are relative to region tile center.
        /// </summary>
        public static int[] GetLocalCityTileCoords(Vector3 sampleWorldPos, Vector3 regionTileWorldCenter, float cityTileSide)
        {

            //since side factor is a power of two, the center of the region tile will be on the border of four city tiles
            Vector3 toSample = sampleWorldPos - regionTileWorldCenter;
            int xSteps = (int)Math.Ceiling(toSample.x / cityTileSide);
            int ySteps = (int)Math.Ceiling(toSample.z / cityTileSide);
            return new int[] { xSteps, ySteps };
        }

        /// <summary>
        /// Converts city tile coordinates within a region into actual world coordinates: x-range, z-range
        /// </summary>
        /// <returns></returns>
        public static float[][] TileCoordsToWorldExtents(int[] coordsXY, Vector3 regionTileWorldCenter, float cityTileSideLength, int eExt, int wExt, int nExt, int sExt)
        {
            int x1 = coordsXY[0] -1 - wExt;
            int x0 = coordsXY[0] + eExt;
            int y1 = coordsXY[1] -1 - sExt;
            int y0 = coordsXY[1] + nExt;

            float xR0 = CoordToWorldCoord(regionTileWorldCenter.x, x0, cityTileSideLength);
            float xR1 = CoordToWorldCoord(regionTileWorldCenter.x, x1, cityTileSideLength);
            float zR0 = CoordToWorldCoord(regionTileWorldCenter.z, y0, cityTileSideLength);
            float zR1 = CoordToWorldCoord(regionTileWorldCenter.z, y1, cityTileSideLength);

            return new float[][] { new float[] { xR0, xR1 }, new float[] { zR0, zR1 } };

        }

        /// <summary>
        /// Converts a single tile division line coord (int) into position in world space based on relevant region center tile coordinate compoent.
        /// </summary>
        public static float CoordToWorldCoord(float worldCenterCoord, int coord, float cityTileSideLength)
        {
            return worldCenterCoord + coord * cityTileSideLength;
        }
    }
}
