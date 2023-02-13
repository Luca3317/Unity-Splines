using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    [System.Serializable]
    public struct PosRotScale
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}