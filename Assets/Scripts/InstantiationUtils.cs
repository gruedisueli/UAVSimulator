using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class InstantiationUtils
    {
        /// <summary>
        /// Builds polyline in space.
        /// </summary>
        public static LineRenderer MakePolyline(Vector3[] wayPointsArray, Material material, float width, bool enable, string name = "line")
        {
            GameObject line = new GameObject();
            line.name = name;
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.positionCount = wayPointsArray.Length;
            lineRenderer.SetPositions(wayPointsArray);
            lineRenderer.material = material;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.enabled = enable;
            return lineRenderer;
        }
    }
}
