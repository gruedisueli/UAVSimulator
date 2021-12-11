using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.Tools;

namespace Assets.Scripts.Environment
{
    public class SceneRestrictionZone : SceneElementBase
    {
        public override string Guid { get; protected set; }
        public override bool Is2D { get; protected set; } = false;
        public override GameObject Sprite2d { get; protected set; } = null;
        public override Canvas SceneCanvas { get; protected set; } = null;
        public RestrictionZoneBase RestrictionZoneSpecs { get; private set; }
        public List<GameObject> SubElements { get; private set; } = new List<GameObject>();

        public void Initialize(string guid, RestrictionZoneBase rZ, bool isSelectable)
        {
            Guid = guid;
            RestrictionZoneSpecs = rZ;
            _defaultMaterial = Instantiate(EnvironManager.Instance.RestrictionZoneMaterial);


            string type = "";
            if (rZ is RestrictionZoneRect)
            {
                type = "Rect";
                var rRect = rZ as RestrictionZoneRect;
                transform.position = rRect.Position;
                InstantiateRect(rRect);

            }
            else if (rZ is RestrictionZoneCyl)
            {
                type = "Cyl";
                var rCyl = rZ as RestrictionZoneCyl;
                transform.position = rCyl.Position;
                InstantiateCyl(rCyl);
            }
            else if (rZ is RestrictionZoneCylStack)
            {
                type = "CylStacked";
                var tmp = rZ as RestrictionZoneCylStack;
                transform.position = tmp.Position;
                foreach(var t  in tmp.Elements)
                {
                    InstantiateCyl(t);
                }
            }

            gameObject.name = "RestrictionZone_" + type;
            gameObject.tag = "RestrictionZone";
            gameObject.layer = rZ.Layer;

            foreach (var c in SubElements)
            {
                c.transform.parent = transform;
                var mR = c.GetComponent<MeshRenderer>();
                mR.material = _defaultMaterial;
                c.name = gameObject.name;
                c.tag = gameObject.tag;
                c.layer = gameObject.layer;
            }
            if (isSelectable)
            {
                foreach (var c in SubElements)
                {
                    c.AddComponent<SelectableGameObject>();
                }
                MakeSelectable();
            }
        }

        /// <summary>
        /// Sets colors of all game objects in this restriction zone.
        /// </summary>
        public void SetMaterial(Material mat)
        {
            foreach(var c in SubElements)
            {
                var mR = c.GetComponent<MeshRenderer>();
                mR.material = mat;
            }
        }

        /// <summary>
        /// Updates game object with environment 
        /// </summary>
        public override void UpdateGameObject()
        {
            if (RestrictionZoneSpecs is RestrictionZoneRect)
            {
                if (SubElements.Count == 1)
                {
                    UpdateSingleRect(SubElements[0], RestrictionZoneSpecs as RestrictionZoneRect);
                }
                else
                {
                    Debug.LogError("Wrong list length for rectangular restriction zone game objects");
                }
            }
            else if (RestrictionZoneSpecs is RestrictionZoneCyl)
            {
                if (SubElements.Count == 1)
                {
                    UpdateSingleCyl(SubElements[0], RestrictionZoneSpecs as RestrictionZoneCyl);
                }
                else
                {
                    Debug.LogError("Wrong list length for cylindrical restriction zone game objects");
                }
            }
            else if (RestrictionZoneSpecs is RestrictionZoneCylStack)
            {
                var rZ = RestrictionZoneSpecs as RestrictionZoneCylStack;
                if (rZ == null || rZ.Elements.Length != SubElements.Count)
                {
                    Debug.LogError("List length mismatch in restriction zone");
                    return;
                }
                for (int i = 0; i < rZ.Elements.Length; i++)
                {
                    UpdateSingleCyl(SubElements[i], rZ.Elements[i]);
                }
            }
        }

        /// <summary>
        /// Instantiate rectangular component.
        /// </summary>
        private void InstantiateRect(RestrictionZoneRect rZ)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = transform;
            cube.transform.position = rZ.Position;
            cube.transform.localScale = rZ.Scale;
            SubElements.Add(cube);
        }

        /// <summary>
        /// Instantiates cylindrical component.
        /// </summary>
        private void InstantiateCyl(RestrictionZoneCyl rZ)
        {
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.transform.parent = transform;
            cyl.transform.position = rZ.Position;
            cyl.transform.localScale = rZ.Scale;
            SubElements.Add(cyl);
        }

        /// <summary>
        /// Updates a single game object of cyl type.
        /// </summary>
        private void UpdateSingleRect(GameObject gO, RestrictionZoneRect rZ)
        {
            gO.transform.localScale = new Vector3(rZ.Scale.x, rZ.Height, rZ.Scale.z);
            gO.transform.position = new Vector3(rZ.Position.x, rZ.Height / 2, rZ.Position.z);
            gO.transform.rotation = Quaternion.Euler(rZ.Rotation.x, rZ.Rotation.y, rZ.Rotation.z);
        }

        /// <summary>
        /// Updates a single game object of rect type.
        /// </summary>
        private void UpdateSingleCyl(GameObject gO, RestrictionZoneCyl rZ)
        {
            gO.transform.localScale = new Vector3(rZ.Scale.x, rZ.Height, rZ.Scale.z);
            gO.transform.position = new Vector3(rZ.Position.x, rZ.Height / 2, rZ.Position.z);
            gO.transform.rotation = Quaternion.Euler(rZ.Rotation.x, rZ.Rotation.y, rZ.Rotation.z);
        }
    }
}
