using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.Environment
{
    public abstract class SceneElementBase : MonoBehaviour
    {
        public abstract string Guid { get; protected set; }
        public abstract void UpdateGameObject();
    }
}
