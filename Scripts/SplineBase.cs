using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    [System.Serializable]
    public abstract class SplineBase
    {
        public ISplineGenerator Generator => _generator;
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

        public SplineBase(ISplineGenerator generator, bool cache, params Vector3[] points) => InitSpline(generator, cache, SplineUtility.VectorsToSplinePoints(points));
        public SplineBase(ISplineGenerator generator, bool cache, IEnumerable<Vector3> points) => InitSpline(generator, cache, SplineUtility.VectorsToSplinePoints(points));
        public SplineBase(ISplineGenerator generator, bool cache, SegmentedCollection<Vector3> points) => InitSpline(generator, cache, SplineUtility.VectorsToSplinePoints(points.Items));
        public SplineBase(ISplineGenerator generator, bool cache, params SplinePoint[] points) => InitSpline(generator, cache, points);
        public SplineBase(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points) => InitSpline(generator, cache, points);
        public SplineBase(ISplineGenerator generator, bool cache, SegmentedCollection<SplinePoint> points) => InitSpline(generator, cache, points.Items);

        public Vector3 ValueAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineUtility.PercentageToSegmentPercentage(t);

            ListSegment<Vector3> segment = _pointPositions.Segment(segmentIndex);
            return _generator.Evaluate(segmentT, segment);
        }

        public Vector3 TangentAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineUtility.PercentageToSegmentPercentage(t);

            ListSegment<Vector3> segment = _pointPositions.Segment(segmentIndex);
            return _generator.EvaluateDerivative(segmentT, 1, segment);
        }

        public Vector3 NormalAt(float t, bool alignNormalsToCurveOrientation = false, FrenetFrame? initialOrientation = null)
                  => NormalAt(t, _accuracy, alignNormalsToCurveOrientation, initialOrientation);
        public Vector3 NormalAt(float t, int accuracy, bool alignNormalsToCurveOrientation = false, FrenetFrame? initialOrientation = null)
        {
            Vector3 tangent = TangentAt(t);
            (int segmentIndex, float segmentT) = SplineUtility.PercentageToSegmentPercentage(t);
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
            IReadOnlyList<Vector3> roFlattened = SplineUtility.GetFlattened(accuracy, _generator, segment);
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
        public FrenetFrame GetFrenetFrameAt(float t, int accuracy, FrenetFrame? initialOrientation = null)
        {
            GenerateFrenetFrames(accuracy, initialOrientation);

            (int segmentIndex, float segmentT) = SplineUtility.PercentageToSegmentPercentage(t);
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
            if (distance <= 0) return 0; // TODO probably throw error instead
            float length = GetLength(accuracy);
            if (distance >= length) return SegmentCount; // TODO probably throw error instead

            List<float> distances = GenerateDistanceLUT(accuracy);
            int index = distances.BinarySearch(distance);
            // If no exact match is found (the expected case)
            if (index < 0)
            {
                // Invert bits of index
                // See return value of binary search https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.list-1.binarysearch?view=net-7.0
                index = ~index;
            }
            // If exact match is found
            else
            { 
                return (float)(index) / (distances.Count - 1) * SegmentCount;
            }

            // TODO: Simply multiplying here with segmentcount* might*be sloppy
            float t0 = (float)(index - 1) / (distances.Count - 1) * SegmentCount;
            float t1 = (float)(index) / (distances.Count - 1) * SegmentCount;
            // Linearly interpolate between the two distances
            return t0 + (distance - distances[index - 1]) * ((t1 - t0) / (distances[index] - distances[index - 1]));
        }

        public float GetCurvatureAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineUtility.PercentageToSegmentPercentage(t);
            Vector3 derivative = _generator.EvaluateDerivative(segmentT, 1, _pointPositions.Segment(segmentIndex));
            Vector3 secondDerivative = _generator.EvaluateDerivative(segmentT, 2, _pointPositions.Segment(segmentIndex));
            float num = derivative.x * secondDerivative.y - derivative.y * secondDerivative.x;
            float qdsum = derivative.x * derivative.x + derivative.y * derivative.y;
            float dnm = Mathf.Pow(qdsum, (float)3 / 2);

            if (num == 0 || dnm == 0) return float.NaN;
            return num / dnm;
        }

        #region Intersection
        public bool Intersects(SplineBase spline, int accuracy)
            => SplineUtility.SplineSplineIntersect(this, spline, accuracy);
        public bool SegmentIntersects(int segmentIndex, SplineBase spline, int accuracy)
            => SplineUtility.CurveSplineIntersect(_generator, SegmentPositions(segmentIndex), spline, accuracy);

        public bool Intersects(ISplineGenerator generator, IList<Vector3> points, int accuracy)
            => SplineUtility.CurveSplineIntersect(generator, points, this, accuracy);
        public bool SegmentIntersects(int segmentIndex, ISplineGenerator generator, IList<Vector3> points, int accuracy)
            => SplineUtility.CurveSplineIntersect(_generator, SegmentPositions(segmentIndex), new Spline(generator, false, points), accuracy);

        public bool Intersects(Vector3 start, Vector3 end, int accuracy)
            => SplineUtility.SplineLineIntersect(this, start, end, accuracy);
        public bool SegmentIntersects(int segmentIndex, Vector3 start, Vector3 end, int accuracy, float epsilon)
            => SplineUtility.CurveLineIntersect(_generator, SegmentPositions(segmentIndex), start, end, accuracy, _space, epsilon);

        public IList<Vector3> IntersectionPoints(SplineBase spline, int accuracy)
            => SplineUtility.SplineSplineIntersectionPoints(this, spline, accuracy);
        public IList<Vector3> SegmentIntersectionPoints(int segmentIndex, SplineBase spline, int accuracy)
            => SplineUtility.CurveSplineIntersectionPoints(_generator, SegmentPositions(segmentIndex), spline, accuracy);

        public IList<Vector3> IntersectionPoints(ISplineGenerator generator, IList<Vector3> points, int accuracy)
            => SplineUtility.CurveSplineIntersectionPoints(generator, points, this, accuracy);
        public IList<Vector3> SegmentIntersectionPoints(int segmentIndex, ISplineGenerator generator, IList<Vector3> points, int accuracy)
            => SplineUtility.CurveSplineIntersectionPoints(_generator, SegmentPositions(segmentIndex), new Spline(generator, false, points), accuracy);

        public IList<Vector3> IntersectionPoints(Vector3 start, Vector3 end, int accuracy)
            => SplineUtility.SplineLineIntersectionPoints(this, start, end, accuracy);
        public IList<Vector3> SegmentIntersectionPoints(int segmentIndex, Vector3 start, Vector3 end, int accuracy)
            => SplineUtility.CurveLineIntersectionPoints(_generator, SegmentPositions(segmentIndex), start, end, accuracy);
        #endregion

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

        protected int NeededAccuracy(int accuracy) => accuracy + (SegmentCount - 1) * (accuracy - 1);

        // TODO:
        // Maybe move distances from curvecacher to splinecacher
        // If not; implement this one like the other cachable methods, ie getflattened
        private List<float> GenerateDistanceLUT(int accuracy = -1)
        {
            if (_cacher != null && _cacher.Distances.Count == NeededAccuracy(accuracy)) return _cacher.Distances;

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

            if (_cacher != null) _cacher.Distances = distances;
            return distances;
        }

        private List<FrenetFrame> GenerateFrenetFrames() => GenerateFrenetFrames(_accuracy);
        private List<FrenetFrame> GenerateFrenetFrames(int accuracy, FrenetFrame? initialOrientation = null)
        {
            if (_cacher != null && _cacher.Frames.Count == NeededAccuracy(accuracy)) return _cacher.Frames;

            List<FrenetFrame> frames = new List<FrenetFrame>();
            for (int i = 0; i < SegmentCount - 1; i++)
            {
                frames.AddRange(GenerateSegmentFrenetFrames(i, accuracy));
                frames.RemoveAt(frames.Count - 1);
            }
            frames.AddRange(GenerateSegmentFrenetFrames(SegmentCount - 1, accuracy));

            if (_cacher != null && accuracy == _accuracy) _cacher.Frames = frames;
            return frames;
        }

        private List<FrenetFrame> GenerateSegmentFrenetFrames(int segmentIndex) => GenerateSegmentFrenetFrames(segmentIndex, _accuracy);
        private List<FrenetFrame> GenerateSegmentFrenetFrames(int segmentIndex, int accuracy, FrenetFrame? initialOrientation = null)
        {
            if (_cacher != null && _cacher[segmentIndex].Frames.Count == accuracy) return _cacher[segmentIndex].Frames;

            ListSegment<Vector3> segment = _pointPositions[segmentIndex];
            List<FrenetFrame> frames = SplineUtility.GenerateFrenetFrames(accuracy, _generator, segment);
            if (_cacher != null && accuracy == _accuracy) _cacher[segmentIndex].Frames = frames;
            return frames;
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