using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class ViewportUtils
    {

         /// <summary>
         /// Converts world point to point in webgl canvas.
         /// </summary>
         public static Vector3 WorldPtCanvasPoint(Vector3 worldPt, Camera camera)
         {
            var vPt = camera.WorldToViewportPoint(worldPt);
            var width = (float)camera.scaledPixelWidth;
            var height = (float)camera.scaledPixelHeight;
            var x = width * vPt.x;
            var y = height * vPt.y;
            return new Vector3(x, y, 0);
         }

         public static RaycastHit GetClosestHit(Vector3 current, RaycastHit[] hit)
         {
             float minDist = Mathf.Infinity;
             RaycastHit minHit = hit[0];
             foreach (RaycastHit h in hit)
             {
                 if (Vector3.Distance(current, h.point) < minDist)
                 {
                     minHit = h;
                     minDist = Vector3.Distance(current, h.point);
                 }
             }
             return minHit;
         }
    }
}
