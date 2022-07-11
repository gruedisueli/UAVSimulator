using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.Environment;

namespace Assets.Scripts.UI.Tools
{
    /// <summary>
    /// A panel of add buttons.
    /// </summary>
    public class AddButtonPanel : ButtonPanel
    {
        public ElementFamily _family;

        protected override void Awake()
        {
            base.Awake();

            switch (_family)
            {
                case ElementFamily.DronePort:
                    {
                        var assets = EnvironManager.Instance.DronePortAssets;
                        foreach (var kvp in assets)
                        {
                            var a = kvp.Value;
                            InstantiateButton(_family, kvp.Key, a, "Landing Pad");
                        }
                        break;
                    }
                case ElementFamily.ParkingStruct:
                    {
                        var assets = EnvironManager.Instance.ParkingStructAssets;
                        foreach (var kvp in assets)
                        {
                            var a = kvp.Value;
                            string n = "";
                            if (a.Category == ParkingStructCategory.Rect)
                            {
                                n = "Rect Parking";
                            }
                            else if (a.Specs != null)
                            {
                                n = a.Specs.Type.Contains("LowAltitude") ? "Low Altitude Parking" : "Corridor Parking";
                            }
                            InstantiateButton(_family, kvp.Key, a, n);
                        }
                        break;
                    }
                case ElementFamily.RestrictionZone:
                    {
                        var assets = EnvironManager.Instance.RestrictionZoneAssets;
                        foreach (var kvp in assets)
                        {
                            var a = kvp.Value;
                            string n = "";
                            if (a.Category == RestrictionZoneCategory.Rect)
                            {
                                n = "Rect Restriction";
                            }
                            else if (a.Category == RestrictionZoneCategory.Cylindrical)
                            {
                                n = "Cylindrical Restriction";
                            }
                            InstantiateButton(_family, kvp.Key, a, n);
                        }
                        break;
                    }
            }

            gameObject.SetActive(false);//default of panel should be "off"
        }

        /// <summary>
        /// Creates basic configuration of button instance.
        /// </summary>
        private void InstantiateButton(ElementFamily family, string type, AssetPackBase asset, string nameOverride = "")
        {
            var sprite = asset.PreviewImage;
            var prefab = EnvironManager.Instance.AddButtonPrefab;
            if (prefab == null)
            {
                return;
            }

            var clone = Instantiate(prefab);
            Button b = clone.GetComponentInChildren<Button>(true);
            if (b == null)
            {
                Debug.LogError("Button not found in prefab of add tool");
                return;
            }
            switch (family)
            {
                case ElementFamily.DronePort:
                    {
                        var aT = clone.AddComponent<AddDronePortTool>();
                        aT._type = type;
                        aT._category = (asset as DronePortAssetPack).Category;
                        b.onClick.AddListener(aT.Add);
                        break;
                    }
                case ElementFamily.ParkingStruct:
                    {
                        var aT = clone.AddComponent<AddParkingStructTool>();
                        aT._type = type;
                        aT._category = (asset as ParkingStructureAssetPack).Category;
                        b.onClick.AddListener(aT.Add);
                        break;
                    }
                case ElementFamily.RestrictionZone:
                    {
                        var aT = clone.AddComponent<AddRestrictZoneTool>();
                        aT._type = type;
                        aT._category = (asset as RestrictionZoneAssetPack).Category;
                        b.onClick.AddListener(aT.Add);
                        break;
                    }
            }

            var image = b.GetComponent<Image>();
            if (image == null)
            {
                Debug.LogError("Could not find image component in children of add button");
            }
            else
            {
                image.sprite = Instantiate(sprite);
            }

            var text = clone.GetComponentInChildren<Text>(true);
            if (text == null)
            {
                Debug.LogError("Could not find text component in children of add button");
            }
            else
            {
                text.text = nameOverride == "" ? type : nameOverride;
            }

            clone.transform.SetParent(transform);

            _buttons.Add(clone);
        }
    }
}
