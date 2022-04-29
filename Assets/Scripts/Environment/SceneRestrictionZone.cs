using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.UI.Tags;
using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.UI.Tools;

namespace Assets.Scripts.Environment
{
    public class SceneRestrictionZone : SceneElementBase
    {
        public override string Guid { get; protected set; }
        public override GameObject Sprite2d { get; protected set; } = null;
        public override Canvas SceneCanvas { get; protected set; } = null;
        public RestrictionZoneBase RestrictionZoneSpecs { get; private set; }
        public List<GameObject> SubElements { get; private set; } = new List<GameObject>();

        public void Initialize(string guid, RestrictionZoneBase rZ, Canvas canvas, bool enableIcon)
        {
            Guid = guid;
            SceneCanvas = canvas;
            if (enableIcon)
            {
                Sprite2d = Instantiate(EnvironManager.Instance.RestrictionSpritePrefab, SceneCanvas.transform);
                var b = Sprite2d.GetComponentInChildren<Button>();
                if (b == null)
                {
                    Debug.LogError("Button not found on restriction zone sprite prefab");
                }
                b.onClick.AddListener(SpriteClicked);
                Sprite2d.transform.SetAsFirstSibling();
            }
            RestrictionZoneSpecs = rZ;
            _defaultMaterial = Instantiate(EnvironManager.Instance.RestrictionZoneMaterial);
            string type = "";
            if (rZ is RestrictionZoneRect)
            {
                type = "Rect";
                var rRect = rZ as RestrictionZoneRect;
                transform.position = new Vector3(rRect.Position.x, 0, rRect.Position.z);
                InstantiateRect(rRect);

            }
            else if (rZ is RestrictionZoneCyl)
            {
                type = "Cyl";
                var rCyl = rZ as RestrictionZoneCyl;
                transform.position = new Vector3(rCyl.Position.x, 0, rCyl.Position.z);
                InstantiateCyl(rCyl);
            }
            else if (rZ is RestrictionZoneCylStack)
            {
                type = "CylStacked";
                var tmp = rZ as RestrictionZoneCylStack;
                transform.position = new Vector3(tmp.Position.x, 0, tmp.Position.z);
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

            foreach (var c in SubElements)
            {
                c.AddComponent<SelectableGameObject>();
            }
            MakeSelectable();

            if (enableIcon)
            {
                var tag = Sprite2d.GetComponent<UIWorldTag>();
                tag?.SetWorldPos(RestrictionZoneSpecs.Position);
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
            var tag = Sprite2d.GetComponent<UIWorldTag>();
            tag?.SetWorldPos(RestrictionZoneSpecs.Position);

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
            var col = cube.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
            else
            {
                Debug.LogError("Collider not attached to restriction zone object");
            }
            UpdateSingleRect(cube, rZ);
            SubElements.Add(cube);
        }

        /// <summary>
        /// Instantiates cylindrical component.
        /// </summary>
        private void InstantiateCyl(RestrictionZoneCyl rZ)
        {
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.transform.parent = transform;
            var col = cyl.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
            else
            {
                Debug.LogError("Collider not attached to restriction zone object");
            }
            UpdateSingleCyl(cyl, rZ);
            SubElements.Add(cyl);
        }

        /// <summary>
        /// Updates a single game object of cyl type.
        /// </summary>
        private void UpdateSingleRect(GameObject gO, RestrictionZoneRect rZ)
        {
            gO.transform.localScale = new Vector3(rZ.Scale.x, rZ.Scale.y, rZ.Scale.z);
            gO.transform.localPosition = new Vector3(0, rZ.Position.y, 0);
            gO.transform.localRotation = Quaternion.Euler(rZ.Rotation.x, rZ.Rotation.y, rZ.Rotation.z);
        }

        /// <summary>
        /// Updates a single game object of rect type.
        /// </summary>
        private void UpdateSingleCyl(GameObject gO, RestrictionZoneCyl rZ)
        {
            gO.transform.localScale = new Vector3(rZ.Scale.x, rZ.Scale.y, rZ.Scale.z);
            gO.transform.localPosition = new Vector3(0, rZ.Position.y, 0);
            gO.transform.localRotation = Quaternion.Euler(rZ.Rotation.x, rZ.Rotation.y, rZ.Rotation.z);
        }
    }
}
