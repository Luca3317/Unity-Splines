using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace UnitySplines
{
    public class CurveCacher
    {
        public SplineExtrema? Extrema
        {
            get => _extrema;
            set => _extrema = value;
        }
        public IReadOnlyList<Vector3> Flattened
        {
            get => _flattened;
            set => _flattened = value;
        }
        public float Length
        {
            get => _length;
            set => _length = value;
        }
        public int LengthAccuracy
        {
            get => _lengthAccuracy;
            set => _lengthAccuracy = value;
        }
        public IReadOnlyList<float> Distances
        {
            get => _distances;
            set => _distances = value;
        }
        public IReadOnlyList<FrenetFrame> Frames
        {
            get => _frames;
            set => _frames = value;
        }

        public CurveCacher() => Clear();

        public void Clear()
        {
            _extrema = null;
            _flattened = new List<Vector3>();
            _length = -1;
            _lengthAccuracy = -1;
            _distances = new List<float>(); ;
            _frames = new List<FrenetFrame>(); ;
        }

        // TODO
        // Make the Ireadonlylists ienumerables?
        // SplineCacher could iterate over its curvecachers and yield them
        private SplineExtrema? _extrema;
        private IReadOnlyList<Vector3> _flattened;
        private float _length;
        private int _lengthAccuracy;
        private IReadOnlyList<float> _distances;
        private IReadOnlyList<FrenetFrame> _frames;
    }
}