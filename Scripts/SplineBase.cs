using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    [System.Serializable]
    public abstract class SplineBase
    {
        public string Generator => _generator.GeneratorType;
        public int Accuracy => _accuracy;
        public SplineSpace Space => _space;
        public float NormalAngleOffset => _normalAngleOffset;

        #region SplinePoints Property Wrappers
        public int SegmentSize => _generator.SegmentSize;
        public int SlideSize => _generator.SlideSize;

        /// <summary>
        /// Returns the amount of points in the collection.
        /// </summary>
        public int PointCount => _pointPositions.ItemCount;
        /// <summary>
        /// Return the amount of segments in the collection.
        /// </summary>
        public int SegmentCount => _pointPositions.SegmentCount;

        public Vector3 PointPosition(int pointIndex) => _pointPositions.Item(pointIndex);
        public float PointNormal(int pointIndex) => _pointNormals.Item(pointIndex);

        public ListSegment<Vector3> SegmentPositions(int segmentIndex) => _pointPositions.Segment(segmentIndex);
        public ListSegment<float> SegmentNormals(int segmentIndex) => _pointNormals.Segment(segmentIndex);
        #endregion

        public SplineBase(ISplineGenerator generator, bool cache, params SplinePoint[] points) => InitSpline(generator, cache, points);
        public SplineBase(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points) => InitSpline(generator, cache, points);
        public SplineBase(ISplineGenerator generator, bool cache, SegmentedCollection<SplinePoint> points) => InitSpline(generator, cache, points.Items);

        public Vector3 ValueAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineHelper.PercentageToSegmentPercentage(t);

            ListSegment<Vector3> segment = _pointPositions.Segment(segmentIndex);
            return _generator.Evaluate(segmentT, segment);
        }

        public Vector3 TangentAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineHelper.PercentageToSegmentPercentage(t);

            ListSegment<Vector3> segment = _pointPositions.Segment(segmentIndex);
            return _generator.EvaluateDerivative(segmentT, 1, segment);
        }

        public Vector3 NormalAt(float t, bool alignNormalsToCurveOrientation = false, FrenetFrame? initialOrientation = null)
            => NormalAt(t, _accuracy, alignNormalsToCurveOrientation, initialOrientation);
        public Vector3 NormalAt(float t, int accuracy, bool alignNormalsToCurveOrientation = false, FrenetFrame? initialOrientation = null)
        {
            Vector3 tangent = TangentAt(t);
            (int segmentIndex, float segmentT) = SplineHelper.PercentageToSegmentPercentage(t);
            float xt, zt, yt;

            Vector3 normal;
            switch (_space)
            {
                case SplineSpace.XY:
                    xt = tangent.x / tangent.magnitude;
                    yt = tangent.y / tangent.magnitude;
                    normal = new Vector3(yt, -xt, 0f);
                    float normalModifier = _generator.GetNormalsModifier(normal, t, _pointNormals.Segment(segmentIndex));
                    return Quaternion.AngleAxis((_normalAngleOffset + normalModifier) % 360, tangent) * normal;

                case SplineSpace.XZ:
                    xt = tangent.x / tangent.magnitude;
                    zt = tangent.z / tangent.magnitude;
                    normal = new Vector3(-zt, 0f, xt);
                    normalModifier = _generator.GetNormalsModifier(normal, t, _pointNormals.Segment(segmentIndex));
                    return Quaternion.AngleAxis((_normalAngleOffset + normalModifier) % 360, tangent) * normal;

                default:

                    if (alignNormalsToCurveOrientation)
                    {
                        normal = GetFrenetFrameAt(t, accuracy).normal; // t, accuracy, initialOrientation
                    }
                    else
                    {
                        normal = Vector3.Cross(Vector3.up, tangent);
                    }

                    normalModifier = _generator.GetNormalsModifier(normal, t, _pointNormals.Segment(segmentIndex));
                    return Quaternion.AngleAxis((_normalAngleOffset + normalModifier) % 360, tangent) * normal;
            }
        }

        public Bounds GetBounds()
        {
            SplineExtrema extrema = GetExtrema();
            return new Bounds((extrema.Maxima + extrema.Minima) / 2, extrema.Maxima - extrema.Minima);
        }

        public Bounds GetSegmentBounds(int segmentIndex)
        {
            SplineExtrema extrema = GetSegmentExtrema(segmentIndex);
            return new Bounds((extrema.Maxima + extrema.Minima) / 2, extrema.Maxima - extrema.Minima);
        }

        public SplineExtrema GetExtrema()
        {
            if (_cacher != null && _cacher.Extrema != null) return _cacher.Extrema.Value;

            SplineExtrema extrema = new SplineExtrema();

            for (int i = 0; i < SegmentCount; i++)
            {
                extrema.Combine(GetSegmentExtrema(i));
            }

            if (_cacher != null) _cacher.Extrema = extrema;
            return extrema;
        }

        public SplineExtrema GetSegmentExtrema(int segmentIndex)
        {
            if (_cacher != null && _cacher[segmentIndex].Extrema != null) return _cacher[segmentIndex].Extrema.Value;

            SplineExtrema extrema = new SplineExtrema();

            foreach (var extremaT in _generator.GetExtremaTs(_pointPositions.Segment(segmentIndex)))
            {
                extrema.InsertValueT(extremaT, segmentIndex, this);
            }
            extrema.InsertValueT(0, segmentIndex, this);
            extrema.InsertValueT(1, segmentIndex, this);

            if (_cacher != null) _cacher[segmentIndex].Extrema = extrema;
            return extrema;
        }

        // TODO: Issue:
        // GetFlattened uses points * segmentcount to check if the currently cached version is more accurate already
        // However; since GetFlattenedSegment is public, it is possible that one flattened segment contains way more points than others,
        // so GetFlattened would falsely indicate that accuracy is higher than points.

        // Potential fixes (spontaneously):
        //      Make GetFlattenedSegment private
        //      Make GetFlattenedSegment not cache
        //      More general change: Modify the entire accuracy concept for caching?


        public IReadOnlyList<Vector3> GetFlattened() => GetFlattened(_accuracy);
        public IReadOnlyList<Vector3> GetFlattened(int accuracy)
        {
            if (_cacher != null && _cacher.Flattened.Count == NeededAccuracy(accuracy)) return _cacher.Flattened;

            List<Vector3> flattened = new List<Vector3>();
            for (int i = 0; i < SegmentCount; i++)
            {
                flattened.AddRange(GetFlattenedSegment(i, accuracy));
                flattened.RemoveAt(flattened.Count - 1);
            }
            flattened.Add(ValueAt(SegmentCount));

            IReadOnlyList<Vector3> roFlattened = flattened.AsReadOnly();
            if (_cacher != null && accuracy == _accuracy) _cacher.Flattened = roFlattened;
            return roFlattened;
        }

        public IReadOnlyList<Vector3> GetFlattenedSegment(int segmentIndex) => GetFlattenedSegment(segmentIndex, _accuracy);
        public IReadOnlyList<Vector3> GetFlattenedSegment(int segmentIndex, int accuracy)
        {
            if (_cacher != null && _cacher[segmentIndex].Flattened.Count == _accuracy) return _cacher[segmentIndex].Flattened;

            ListSegment<Vector3> segment = _pointPositions.Segment(segmentIndex);
            IReadOnlyList<Vector3> roFlattened = SplineHelper.GetFlattened(accuracy, _generator, segment);
            if (_cacher != null && accuracy == _accuracy) _cacher[segmentIndex].Flattened = roFlattened;
            return roFlattened;
        }

        public float GetLength() => GetLength(_accuracy);
        public float GetLength(int accuracy)
        {
            // Todo i do need lengthaccuracy here; though here i could also say use anything higher than current accuracy, since it doesnt take more storage /iterations
            if (_cacher != null && _cacher.LengthAccuracy == NeededAccuracy(accuracy)) return _cacher.Length;

            float length = 0f;
            for (int i = 0; i < SegmentCount; i++)
            {
                length += GetSegmentLength(i, accuracy);
            }

            if (_cacher != null && accuracy == _accuracy)
            {
                _cacher.Length = length;
                _cacher.LengthAccuracy = NeededAccuracy(_accuracy);
            }
            return length;
        }

        public float GetSegmentLength(int segmentIndex) => GetSegmentLength(segmentIndex, _accuracy);
        public float GetSegmentLength(int segmentIndex, int accuracy)
        {
            if (_cacher != null && _cacher[segmentIndex].LengthAccuracy == accuracy) return _cacher[segmentIndex].Length;

            IReadOnlyList<Vector3> flattened = GetFlattenedSegment(segmentIndex, accuracy);
            float length = 0f;
            for (int i = 1; i < flattened.Count; i++)
            {
                length += (flattened[i - 1] - flattened[i]).magnitude;
            }

            if (_cacher != null && accuracy == _accuracy)
            {
                _cacher[segmentIndex].Length = length;
                _cacher[segmentIndex].LengthAccuracy = accuracy;
            }
            return length;
        }

        public FrenetFrame GetFrenetFrameAt(float t) => GetFrenetFrameAt(t, _accuracy);
        public FrenetFrame GetFrenetFrameAt(float t, int accuracy)
        {
            (int segmentIndex, float segmentT) = SplineHelper.PercentageToSegmentPercentage(t);
            IReadOnlyList<FrenetFrame> roFrames = GenerateSegmentFrenetFrames(segmentIndex, accuracy);
            t = segmentT;

            // TODO make better check for "t is exact index"
            if (Mathf.Abs((int)Mathf.Floor((roFrames.Count - 1) * t) - (int)Mathf.Ceil((roFrames.Count - 1) * t)) < 0.0001f)
                return roFrames[(int)Mathf.Floor((roFrames.Count - 1) * t)];

            FrenetFrame frame1 = roFrames[(int)Mathf.Floor((roFrames.Count - 1) * t)];
            FrenetFrame frame2 = roFrames[(int)Mathf.Ceil((roFrames.Count - 1) * t)];
            float weight2 = ((roFrames.Count - 1) * t) % 1;
            float weight1 = 1f - weight2;

            return new FrenetFrame()
            {
                origin = frame1.origin * weight1 + frame2.origin * weight2,
                tangent = frame1.tangent * weight1 + frame2.tangent * weight2,
                rotationalAxis = frame1.rotationalAxis * weight1 + frame2.rotationalAxis * weight2,
                normal = frame1.normal * weight1 + frame2.normal * weight2
            };
        }

        public float DistanceToT(float distance) => DistanceToT(distance, _accuracy);
        public float DistanceToT(float distance, int accuracy)
        {
            if (distance == 0) return 0;

            IReadOnlyList<Vector3> flattened = GetFlattened(accuracy);
            IReadOnlyList<float> distances = GenerateDistanceLUT(flattened.Count);
            float length = GetLength(accuracy);

            if (distance > 0 && distance < length)
            {
                for (int i = 0; i < distances.Count - 1; i++)
                {
                    if (distances[i] <= distance && distance <= distances[i + 1])
                    {
                        // TODO: Simply multiplying here with segmentcount *might* be sloppy
                        float t0 = (float)i / (distances.Count - 1) * SegmentCount;
                        float t1 = (float)(i + 1) / (distances.Count - 1) * SegmentCount;
                        // Linearly interpolate between the two distances
                        return t0 + (distance - distances[i]) * ((t1 - t0) / (distances[i + 1] - distances[i]));
                    }
                }
            }

            if (distance <= 0)
                return 0;
            else
                return SegmentCount;
        }

        public float GetCurvatureAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineHelper.PercentageToSegmentPercentage(t);
            Vector3 derivative = _generator.EvaluateDerivative(segmentT, 1, _pointPositions.Segment(segmentIndex));
            Vector3 secondDerivative = _generator.EvaluateDerivative(segmentT, 2, _pointPositions.Segment(segmentIndex));
            float num = derivative.x * secondDerivative.y - derivative.y * secondDerivative.x;
            float qdsum = derivative.x * derivative.x + derivative.y * derivative.y;
            float dnm = Mathf.Pow(qdsum, (float)3 / 2);

            if (num == 0 || dnm == 0) return float.NaN;
            return num / dnm;
        }

        public void SetCache(bool cache)
        {
            if (cache && _cacher == null)
            {
                _cacher = new SplineCacher();
            }
            else if (!cache && _cacher != null)
            {
                _cacher = null;
            }
        }

        public void SetAccuracy(int accuracy)
        {
            if (_accuracy == accuracy) return;
            if (accuracy < 1) throw new System.ArgumentOutOfRangeException();
            _accuracy = accuracy;
            ClearCache();
        }

        [SerializeField] protected SegmentedCollection<Vector3> _pointPositions;
        [SerializeField] protected SegmentedCollection<float> _pointNormals;

        [SerializeField] protected ISplineGenerator _generator;
        [SerializeField] protected SplineCacher _cacher;

        [SerializeField] protected SplineSpace _space;
        [SerializeField] protected int _accuracy = 20;
        [SerializeField] protected float _normalAngleOffset;

        protected virtual void InitSpline(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points)
        {
            List<Vector3> pointsPositions = new List<Vector3>();
            List<float> pointsNormalAngles = new List<float>();

            foreach (SplinePoint pointStruct in points)
            {
                pointsPositions.Add(pointStruct.Position);
                pointsNormalAngles.Add(pointStruct.NormalAngle);
            }

            _pointPositions = new SegmentedCollection<Vector3>(generator.SegmentSize, generator.SlideSize, pointsPositions);
            _pointNormals = new SegmentedCollection<float>(generator.SegmentSize, generator.SlideSize, pointsNormalAngles);
            _generator = generator;

            if (cache)
            {
                _cacher = new SplineCacher();
                for (int i = 0; i < SegmentCount; i++) _cacher.Add();
            }

            _space = SplineSpace.XYZ;
        }

        protected virtual void AddRange(ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
            }

            _pointPositions.AddRange(positions);
            _pointNormals.AddRange(normalAngles);

            // TODO this will throw an error if actually adding more than one segment
            if (_cacher != null)
            {
                _cacher.Add(SegmentCount - 1);
            }
        }

        protected virtual void Add(ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
            }

            _pointPositions.Add(positions);
            _pointNormals.Add(normalAngles);

            if (_cacher != null)
            {
                _cacher.Add(SegmentCount - 1);
            }
        }

        protected virtual void Insert(int i, ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
            }

            _pointPositions.InsertAtSegment(i, positions);
            _pointNormals.InsertAtSegment(i, normalAngles);

            if (_cacher != null)
            {
                _cacher.Add(i);
            }
        }

        protected virtual void InsertRange(int i, ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
            }

            _pointPositions.InsertRangeAtSegment(i, positions);
            _pointNormals.InsertRangeAtSegment(i, normalAngles);

            if (_cacher != null)
            {
                _cacher.Add(i);
            }
        }

        protected virtual void Remove(int i)
        {
            if (_cacher != null)
            {
                _cacher.Remove(i);
            }
            _pointPositions.RemoveAtSegment(i);
            _pointNormals.RemoveAtSegment(i);
        }

        protected int NeededAccuracy(int accuracy) => accuracy + (SegmentCount - 1) * (accuracy - 1);

        // TODO:
        // Maybe move distances from curvecacher to splinecacher
        // If not; implement this one like the other cachable methods, ie getflattened
        private IReadOnlyList<float> GenerateDistanceLUT(int accuracy = -1)
        {
            if (_cacher != null && _cacher.Distances.Count >= accuracy) return _cacher.Distances;

            Debug.Log("Calculating distance lut");

            IReadOnlyList<Vector3> flattened = GetFlattened(accuracy);
            List<float> distances = new List<float>();

            Vector3 prevPos = _pointPositions.Item(0);
            float cumulativeDistance = 0f;
            for (int i = 0; i < flattened.Count; i++)
            {
                cumulativeDistance += (flattened[i] - prevPos).magnitude;
                prevPos = flattened[i];
                distances.Add(cumulativeDistance);
            }

            ReadOnlyCollection<float> roDistances = distances.AsReadOnly();
            if (_cacher != null) _cacher.Distances = roDistances;
            return roDistances;
        }

        private IReadOnlyList<FrenetFrame> GenerateFrenetFrames() => GenerateFrenetFrames(_accuracy);
        private IReadOnlyList<FrenetFrame> GenerateFrenetFrames(int accuracy)
        {
            if (_cacher != null && _cacher.Frames.Count == NeededAccuracy(accuracy)) return _cacher.Frames;

            List<FrenetFrame> frames = new List<FrenetFrame>();
            for (int i = 0; i < SegmentCount - 1; i++)
            {
                frames.AddRange(GenerateSegmentFrenetFrames(i, accuracy));
                frames.RemoveAt(frames.Count - 1);
            }
            frames.AddRange(GenerateSegmentFrenetFrames(SegmentCount - 1, accuracy));

            IReadOnlyList<FrenetFrame> roFrames = frames.AsReadOnly();
            if (_cacher != null && accuracy == _accuracy) _cacher.Frames = roFrames;
            return roFrames;
        }

        private IReadOnlyList<FrenetFrame> GenerateSegmentFrenetFrames(int segmentIndex) => GenerateSegmentFrenetFrames(segmentIndex, _accuracy);
        private IReadOnlyList<FrenetFrame> GenerateSegmentFrenetFrames(int segmentIndex, int accuracy)
        {
            if (_cacher != null && _cacher[segmentIndex].Frames.Count == accuracy) return _cacher[segmentIndex].Frames;

            ListSegment<Vector3> segment = _pointPositions[segmentIndex];
            IReadOnlyList<FrenetFrame> roFrames = SplineHelper.GenerateFrenetFrames(accuracy, _generator, segment);
            if (_cacher != null && accuracy == _accuracy) _cacher[segmentIndex].Frames = roFrames;
            return roFrames;
        }

        // testing
        public void ClearCache()
        {
            if (_cacher == null) return;
            for (int i = 0; i < SegmentCount; i++)
            {
                if (_cacher[i] == null) Debug.LogError("Encountered empty cacher!");
                _cacher[i].Clear();
            }
            _cacher.Clear();
        }
        public SplineCacher Cacher => _cacher;
    }
}