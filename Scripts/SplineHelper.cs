using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SplineHelper
{
    /// <summary>
    /// Converts a point index to the corresponding segment indeces.
    /// </summary>
    /// <param name="pointIndex">The point index that will be converted.</param>
    /// <returns>The indeces of all segments that contain the point at i.</returns>
    public static IEnumerable<int> PointToSegmentIndeces(int pointIndex, int segmentSize, int slideSize)
    {
        List<int> indeces = new List<int>();

        // Calculate index of first segment containing this point.
        int index = pointIndex < segmentSize ? 0 : (pointIndex - segmentSize) / slideSize + 1;

        while (pointIndex - index >= 0)
        {
            indeces.Add(index);
            index += slideSize;
        }
        return indeces;
    }
    /// <summary>
    /// Converts a segment index to the corresponding point index.
    /// </summary>
    /// <param name="segmentIndex">The segment index to convert.</param>
    /// <returns>The index of the first point contained in this segment.</returns>
    public static int SegmentToPointIndex(int segmentIndex, int segmentSize, int slideSize) => slideSize * segmentIndex;

    public static (int, float) PercentageToSegmentPercentage(float t)
    {
        int segmentIndex = (int)t;
        if (t % 1 == 0 && t > 0)
        {
            segmentIndex--;
            t = 1f;
        }
        else t %= 1;

        return (segmentIndex, t);
    }

    public static Matrix4x4 CreateTMatrix(float t, int order)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Matrix4x4 tMatrix = new Matrix4x4();
        switch (order)
        {
            case 0:
                tMatrix[0, 0] = 1;
                tMatrix[0, 1] = t;
                tMatrix[0, 2] = t2;
                tMatrix[0, 3] = t3;
                break;

            case 1:
                tMatrix[0, 0] = 0;
                tMatrix[0, 1] = 1;
                tMatrix[0, 2] = 2 * t;
                tMatrix[0, 3] = 3 * t2;
                break;

            case 2:
                tMatrix[0, 0] = 0;
                tMatrix[0, 1] = 0;
                tMatrix[0, 2] = 2;
                tMatrix[0, 3] = 6 * t;
                break;

            case 3:
                tMatrix[0, 0] = 0;
                tMatrix[0, 1] = 0;
                tMatrix[0, 2] = 0;
                tMatrix[0, 3] = 6;
                break;

            default: throw new System.NotImplementedException("");
        }

        return tMatrix;
    }

    public static Matrix4x4 CreatePointMatrix(params Vector3[] points) => CreatePointMatrix((IList<Vector3>)points);
    public static Matrix4x4 CreatePointMatrix(IList<Vector3> points)
    {
        if (points.Count > 4) throw new System.ArgumentException("");

        Matrix4x4 pMatrix = new Matrix4x4();
        for (int i = 0; i < points.Count; i++)
            pMatrix.SetRow(i, points[i]);

        return pMatrix;
    }

    public static Vector3 Evaluate(float t, int order, Matrix4x4 characteristicMatrix, Matrix4x4 pointMatrix) => (CreateTMatrix(t, order) * characteristicMatrix * pointMatrix).GetRow(0);
    public static Vector3 Evaluate(float t, int order, Matrix4x4 characteristicMatrix, params Vector3[] points) => (CreateTMatrix(t, order) * characteristicMatrix * CreatePointMatrix(points)).GetRow(0);
    public static Vector3 Evaluate(float t, int order, Matrix4x4 characteristicMatrix, IList<Vector3> points) => (CreateTMatrix(t, order) * characteristicMatrix * CreatePointMatrix(points)).GetRow(0);
}
