using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UnityEngine;

namespace UnitySplines
{
    /*
     * Representation of a full spline. Primarily consists of a segmented point-collection (SplinePoints) and a SplineGenerator.
     * 
     * Maybe inherit from SplinePoints instead of wrapper methods?
     */
    [System.Serializable]
    public class Spline<T> where T : SplinePoint
    {
        public int Accuracy => _accuracy;

        #region SplinePoints Property Wrappers
        public int SegmentSize => _generator.SegmentSize;
        public int SlideSize => _generator.SlideSize;

        /// <summary>
        /// Returns the amount of points in the collection.
        /// </summary>
        public int PointCount => _points.ItemCount;
        /// <summary>
        /// Returns the point at index i of the collection.
        /// </summary>
        /// <param name="i">The index of the point to return.</param>
        /// <returns></returns>
        public T Point(int i) => _points.Item(i);
        /// <summary>
        /// Returns all points in the collection as an IEnumerable.
        /// </summary>
        public IEnumerable<T> Points => _points.Items;

        /// <summary>
        /// Return the amount of segments in the collection.
        /// </summary>
        public int SegmentCount => _points.SegmentCount;
        /// <summary>
        /// Returns the i-th segment in the collection.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IList<T> Segment(int i) => _points.Segment(i); // TODO test this
        /// <summary>
        /// Returns all segments in the collection as an IEnumerable.
        /// </summary>
        public IEnumerable<IList<T>> Segments => _points.Segments;
        #endregion

        public Spline(ISplineGenerator generator, bool cache, params T[] points) => Init(generator, cache, points);
        public Spline(ISplineGenerator generator, bool cache, IEnumerable<T> points) => Init(generator, cache, points);
        public Spline(ISplineGenerator generator, bool cache, SegmentedCollection<T> points) => Init(generator, cache, points.Items);

        public Vector3 ValueAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineHelper.PercentageToSegmentPercentage(t);
            return _generator.Evaluate(segmentT, _points.Segment(segmentIndex));
        }

        public Vector3 TangentAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineHelper.PercentageToSegmentPercentage(t);
            return _generator.EvaluateDerivative(segmentT, 1, _points.Segment(segmentIndex));
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

            foreach (var extremaT in _generator.GetExtremaTs(_points.Segment(segmentIndex)))
            {
                extrema.InsertValueT(segmentIndex + extremaT, this);
            }
            extrema.InsertValueT(segmentIndex, this);
            extrema.InsertValueT(segmentIndex + 1, this);

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

            IReadOnlyList<Vector3> roFlattened = SplineHelper.GetFlattened(accuracy, _generator, Segment(segmentIndex));
            if (_cacher != null && accuracy == _accuracy) _cacher[segmentIndex].Flattened = SplineHelper.GetFlattened(accuracy, _generator, Segment(segmentIndex));
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
            Vector3 derivative = _generator.EvaluateDerivative(segmentT, 1, _points.Segment(segmentIndex));
            Vector3 secondDerivative = _generator.EvaluateDerivative(segmentT, 2, _points.Segment(segmentIndex));
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
                foreach (T point in _points.Items) point.PropertyChanged += OnPointChanged;
                _cacher = new SplineCacher();
            }
            else if (!cache && _cacher != null)
            {
                foreach (T point in _points.Items) point.PropertyChanged -= OnPointChanged;
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

        public void SetGenerator(ISplineGenerator generator)
        {
            if (generator == _generator) return;
            _generator = generator;
            _points.SetSegmentSizes(_generator.SegmentSize, _generator.SlideSize);
        }

        #region SplinePoints Method Wrappers
        /// <summary>
        /// Appends a new segment to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void AddSegment(params T[] points) => Add(points);
        /// <summary>
        /// Appends a new segment to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void AddSegment(ICollection<T> points) => Add(points);
        /// <summary>
        /// Inserts a new segment into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the new segment, as segment index.</param>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegment(int segmentIndex, params T[] points) => Insert(segmentIndex, points);
        /// <summary>
        /// Inserts a new segment into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the new segment, as segment index.</param>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegment(int segmentIndex, ICollection<T> points) => Insert(segmentIndex, points);

        /// <summary>
        /// Appends new segments to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void AddSegments(params T[] points) => AddRange(points);
        /// Appends new segments to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void AddSegments(ICollection<T> points) => AddRange(points);
        /// <summary>
        /// Inserts new segments into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the first of the new segment, as segment index.</param>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void InsertSegments(int segmentIndex, params T[] points) => InsertRange(segmentIndex, points);
        /// <summary>
        /// Inserts new segments into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the first of the new segments, as segment index.</param>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void InsertSegments(int segmentIndex, ICollection<T> points) => InsertRange(segmentIndex, points);

        /// <summary>
        /// Deletes the last segment of the collection. Preserves all other current segments.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the collection does not contain at least two segments. It must always contain at least one full segment.</exception>
        public void DeleteLastSegment() => Remove(SegmentCount - 1);
        /// <summary>
        /// Deletes the si-th segment of the collection. Preserves all other current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the segment to remove, as segmentIndex.</param>
        /// <exception cref="InvalidOperationException">Thrown if the collection does not contain at least two segments. It must always contain at least one full segment.</exception>
        public void DeleteSegment(int segmentIndex) => Remove(segmentIndex);
        /// <summary>
        /// Interpretes the points pi to pi + slideSize as segment, and deletes it from the collection. 
        /// </summary>
        /// <param name="pi">The index of the segment to remove, as pointIndex.</param>
        /// <exception cref="InvalidOperationException">Thrown if the collection does not contain at least two segments. It must always contain at least one full segment.</exception>
        #endregion

        [SerializeField] private SegmentedCollection<T> _points;
        [SerializeField] private ISplineGenerator _generator;
        [SerializeField] private SplineCacher _cacher;
        [SerializeField] private int _accuracy = 20;

        protected void Init(ISplineGenerator generator, bool cache, IEnumerable<T> points)
        {
            _points = new SegmentedCollection<T>(generator.SegmentSize, generator.SlideSize, points);
            _generator = generator;
            if (cache)
            {
                _cacher = new SplineCacher();
                foreach (T point in points) point.PropertyChanged += OnPointChanged;
                for (int i = 0; i < SegmentCount; i++) _cacher.Add();
            }
        }

        // TODO:
        // Maybe move distances from curvecacher to splinecacher
        // If not; implement this one like the other cachable methods, ie getflattened
        private IReadOnlyList<float> GenerateDistanceLUT(int accuracy = -1)
        {
            if (_cacher != null && _cacher.Distances.Count >= accuracy) return _cacher.Distances;

            Debug.Log("Calculating distance lut");

            IReadOnlyList<Vector3> flattened = GetFlattened(accuracy);
            List<float> distances = new List<float>();

            Vector3 prevPos = _points.Item(0).Position;
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

            IReadOnlyList<FrenetFrame> roFrames = SplineHelper.GenerateFrenetFrames(accuracy, _generator, Segment(segmentIndex));
            if (_cacher != null && accuracy == _accuracy) _cacher[segmentIndex].Frames = roFrames;
            return roFrames;
        }

        private void AddRange(ICollection<T> points)
        {
            if (points.Contains(null)) throw new System.ArgumentNullException();
            _points.AddRange(points);
            if (_cacher != null)
            {
                foreach (T point in points) point.PropertyChanged += OnPointChanged;
                _cacher.Add(SegmentCount - 1);
            }
        }

        private void Add(ICollection<T> points)
        {
            if (points.Contains(null)) throw new System.ArgumentNullException();
            _points.Add(points);
            if (_cacher != null)
            {
                foreach (T point in points) point.PropertyChanged += OnPointChanged;
                _cacher.Add(SegmentCount - 1);
            }
        }

        private void Insert(int i, ICollection<T> points)
        {
            if (points.Contains(null)) throw new System.ArgumentNullException();
            _points.InsertAtSegment(i, points);
            if (_cacher != null)
            {
                foreach (T point in points) point.PropertyChanged += OnPointChanged;
                _cacher.Add(i);
            }
        }

        private void InsertRange(int i, ICollection<T> points)
        {
            if (points.Contains(null)) throw new System.ArgumentNullException();
            _points.InsertRangeAtSegment(i, points);
            if (_cacher != null)
            {
                foreach (T point in points) point.PropertyChanged += OnPointChanged;
                _cacher.Add(i, points.Count % SlideSize);
            }
        }

        private void Remove(int i)
        {
            if (_cacher != null)
            {
                var points = Segment(i);
                foreach (T point in points) point.PropertyChanged -= OnPointChanged;
                _cacher.Remove(i);
            }
            _points.RemoveAtSegment(i);
        }

        private void OnPointChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "Position") return;

            var indeces = _points.SegmentIndecesOf((T)sender);

            foreach (int index in indeces)
            {
                _cacher[index].Clear();
            }

            _cacher.Clear();
        }

        private int NeededAccuracy(int accuracy) => accuracy + (SegmentCount - 1) * (accuracy - 1);

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
        public void SetPoint(int i, Vector3 vec) => _points.Item(i).SetPosition(vec);
    }

    [System.Serializable]
    public class Spline : Spline<SplinePoint>
    {
        public Spline(ISplineGenerator generator, bool cache, params SplinePoint[] points) : base(generator, cache, points) { }
        public Spline(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points) : base(generator, cache, points) { }
        public Spline(ISplineGenerator generator, bool cache, SegmentedCollection<SplinePoint> points) : base(generator, cache, points.Items) { }
    }
}