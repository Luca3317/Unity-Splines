using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    [System.Serializable]
    public class SplinePoint : SplinePointBase
    {
        public SplinePoint(Vector3 position) : base(position) { }
    }
}