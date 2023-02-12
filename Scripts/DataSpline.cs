using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Wrapper class that maps generic data to a given spline's points.
 */

namespace UnitySplines
{

    [System.Serializable]
    public abstract class DataSplineMapperBase<T1, T2> where T1 : SplineBase where T2 : new()
    {
        public T1 Spline => _spline;

        public DataSplineMapperBase(T1 spline)
        {
            _spline = spline;
            _dataPointMapper = new DataSplineDictionary();
        }
        public DataSplineMapperBase(T1 spline, IEnumerable<T2> data)
        {
            _spline = spline;
            _dataPointMapper = new DataSplineDictionary();
        }

        public T2 GetData(int i)
        {
            if (i >= _spline.PointCount) throw new System.ArgumentOutOfRangeException();
            if (_dataPointMapper.ContainsKey(i))
                return _dataPointMapper[i];

            return default;
        }

        // TODO does this serialize?
        [System.Serializable]
        protected class DataSplineDictionary : Dictionary<int, T2>
        { }

        [SerializeField] protected T1 _spline;
        [SerializeField] protected DataSplineDictionary _dataPointMapper;
    }

    [System.Serializable]
    public class ReadOnlyDataSplineMapper<T1, T2> : DataSplineMapperBase<T1, T2> where T1 : SplineBase where T2 : new()
    {
        public ReadOnlyDataSplineMapper(T1 spline) : base(spline)
        { }
        public ReadOnlyDataSplineMapper(T1 spline, IEnumerable<T2> data) : base(spline, data)
        { }
    }

    [System.Serializable]
    public class ReadOnlyDataSplineMapper<T> : ReadOnlyDataSplineMapper<ReadOnlySpline, T> where T : new()
    {
        public ReadOnlyDataSplineMapper(ReadOnlySpline spline) : base(spline)
        { }
        public ReadOnlyDataSplineMapper(ReadOnlySpline spline, IEnumerable<T> data) : base(spline, data)
        { }
    }

    [System.Serializable]
    public class DataSplineMapper<T1, T2> : DataSplineMapperBase<T1, T2> where T1 : SplineBase where T2 : new()
    {
        public DataSplineMapper(T1 spline) : base(spline)
        { }
        public DataSplineMapper(T1 spline, IEnumerable<T2> data) : base(spline, data)
        { }

        public void SetData(int i, T2 data)
        {
            if (i >= _spline.PointCount) throw new System.ArgumentOutOfRangeException();
            _dataPointMapper.Add(i, data);
        }
    }

    [System.Serializable]
    public class DataSplineMapper<T> : DataSplineMapper<Spline, T> where T : new()
    {
        public DataSplineMapper(Spline spline) : base(spline)
        { }
        public DataSplineMapper(Spline spline, IEnumerable<T> data) : base(spline, data)
        { }
    }
}