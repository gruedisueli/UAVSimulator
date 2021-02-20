using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    //MARK AS SERIALIZABLE???
    public class Path
    {
        public GameObject origin;
        public GameObject destination;
        public float elevation;
        public List<Vector3> wayPoints;

        public bool Equals(Path p1, Path p2)
        {
            if (p1.origin.transform.position.Equals(p2.origin.transform.position) && p1.destination.transform.position.Equals(p2.destination.transform.position) && Mathf.Abs(p1.elevation - p2.elevation) < 0.001) return true;
            else return false;
        }
    }
}
