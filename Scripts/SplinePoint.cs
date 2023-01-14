using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System;

namespace UnitySplines
{
    [System.Serializable]
    public struct SplinePoint
    {
        public Vector3 Position => _position;
        public float NormalAngle => _normalAngle;

        public SplinePoint(Vector3 position) : this(position, 0f)
        { }
        public SplinePoint(float normalAngle) : this(Vector3.zero, normalAngle)
        { }
        public SplinePoint(Vector3 position, float normalAngle)
        {
            _position = position;
            _normalAngle = normalAngle;
        }

        [SerializeField] Vector3 _position;
        [SerializeField] float _normalAngle;
    }
}