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
    [JsonObject(MemberSerialization.OptIn)]
    public class RestrictionZoneCylStack : RestrictionZoneBase
    {
        [JsonProperty]
        private string _type = "Cyl_Stacked";
        public override string Type
        {
            get
            {
                return _type;
            }
        }


        [JsonProperty]
        private string _description = "A restriction zone consisting of stacked cylindrical sub-zones";
        public override string Description
        {
            get
            {
                return _description;
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
        private RestrictionZoneCyl[] _elements;
        public RestrictionZoneCyl[] Elements
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

        /// <summary>
        /// Empty constructor for json deserialization
        /// </summary>
        public RestrictionZoneCylStack()
        {
            ConfigureDefaultStack();
        }

        public RestrictionZoneCylStack(Vector3 pos)
        {
            Position = pos;
            ConfigureDefaultStack();
        }

        public RestrictionZoneCylStack(RestrictionZoneCylStack rZ)
        {
            _type = rZ.Type;
            _description = rZ.Description;
            _elements = new RestrictionZoneCyl[rZ.Elements.Length];
            for (int i = 0; i < rZ.Elements.Length; i++)
            {
                _elements[i] = new RestrictionZoneCyl(rZ.Elements[i]);
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

        /// <summary>
        /// Creates default stack of cylinders
        /// </summary>
        private void ConfigureDefaultStack()
        {
            var cyl0 = new RestrictionZoneCyl(Position, 0, 579.12f, 2500);
            var cyl1 = new RestrictionZoneCyl(Position, 579.12f, 1097.28f, 4000);
            var cyl2 = new RestrictionZoneCyl(Position, 1097.28f, 3048, 7200);
            _elements = new RestrictionZoneCyl[] { cyl0, cyl1, cyl2 };
        }
    }
}
