using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System;

namespace UnitySplines
{
    [System.Serializable]
    public abstract class SplinePointBase : INotifyPropertyChanged
    {
        public Vector3 Position => _position;
        public float NormalAngle => _normalAngle;

        public float x => Position.x;
        public float y => Position.y;
        public float z => Position.z;

        public event PropertyChangedEventHandler PropertyChanged;

        public SplinePointBase(Vector3 position) : this(position, 0f) { }
        public SplinePointBase(Vector3 position, float normalAngle)
        {
            _position = position;
            _normalAngle = normalAngle;
        }

        public void SetPosition(Vector3 newPosition)
        {
            if (newPosition == _position) return;
            
            _position = newPosition;
            NotifyPropertyChanged("Position");
        }

        public void SetNormalAngle(float newAngle)
        {
            if (newAngle == _normalAngle) return;

            _normalAngle = newAngle;
            NotifyPropertyChanged("NormalAngle");
        }

        protected void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [SerializeField] private Vector3 _position;
        [SerializeField] private float _normalAngle;
    }
}