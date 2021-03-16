using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Newtonsoft.Json;

using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.Serialization;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class RestrictionZoneCylStack : RestrictionZoneBase
    {
        [JsonProperty]
        private string _type = "Cyl_Stacked";
        public string Type
        {
            get
            {
                return _type;
            }
        }

        [JsonProperty]
        private SerVect3f _position = new SerVect3f();
        /// <summary>
        /// This is the position at the BOTTOM of the stack. Element positions are LOCAL positions relative to this parent.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _position.ToVector3();
            }
            set
            {
                _position = new SerVect3f(value);
            }
        }

        [JsonProperty]
        private List<RestrictionZoneCyl> _elements = new List<RestrictionZoneCyl>();
        public List<RestrictionZoneCyl> Elements
        {
            get
            {
                return _elements;
            }
            set
            {
                _elements = value;
            }
        }

        public RestrictionZoneCylStack(Vector3 pos)
        {
            Position = pos;
        }

        public RestrictionZoneCylStack(RestrictionZoneCylStack rZ)
        {
            _type = rZ.Type;
            foreach(var e in rZ.Elements)
            {
                _elements.Add(new RestrictionZoneCyl(e));
            }
        }

        public override RestrictionZoneBase GetCopy()
        {
            return new RestrictionZoneCylStack(this);
        }

        public override void UpdateParams(ModifyPropertyArgBase args)
        {
            try
            {
                switch (args.Type)
                {
                    case ElementPropertyType.Position:
                        {
                            Position = (args as ModifyVector3PropertyArg).Value;
                            break;
                        }
                }
            }
            catch
            {
                Debug.LogError("Casting error in restriction zone property update");
                return;
            }
        }
    }
}
