using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class TestingPerformance : MonoBehaviour
{
    Matrix4x4 pmat;
    Vector4 vec;

    System.TimeSpan time1 = new System.TimeSpan(0);
    System.TimeSpan time2 = new System.TimeSpan(0);
    System.TimeSpan time3 = new System.TimeSpan(0);
    System.TimeSpan time32 = new System.TimeSpan(0);

    public void Start()
    {
        pmat = new Matrix4x4();
        vec = new Vector4();

    }

    int count = 0;
    public void Update()
    {
        if (count > 1000)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    pmat[i, j] = Random.Range(0, 100);
                }
            }

            float t = (float)Random.Range(1, 100) / 100;
            vec = new Vector4(1, t, t * t, t * t * t);
          //  UnityEngine.Debug.Log("t =  " + t);
           // UnityEngine.Debug.Log(string.Join(",", vec));
           // UnityEngine.Debug.Log("Result 1: " + Test1(t, pmat.GetRow(0), pmat.GetRow(1), pmat.GetRow(2), pmat.GetRow(3)));
           // UnityEngine.Debug.Log("Result 3: " + Test3(t, pmat.GetRow(0), pmat.GetRow(1), pmat.GetRow(2), pmat.GetRow(3)));
           // UnityEngine.Debug.Log("Result 32: " + Test3(t, pmat.GetRow(0), pmat.GetRow(1), pmat.GetRow(2), pmat.GetRow(3)));

            count++;
        }
        if (count < 100)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    pmat[i, j] = Random.Range(0, 100);
                }
            }

            float t = (float)Random.Range(1, 100) / 100;
            vec = new Vector4(1, t, t * t, t * t * t);
            //UnityEngine.Debug.Log("t =  " + t);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Test1(t, pmat.GetRow(0), pmat.GetRow(1), pmat.GetRow(2), pmat.GetRow(3));
            sw.Stop();
            UnityEngine.Debug.Log("Elapsed 1 " + sw.Elapsed);
            time1 += sw.Elapsed;

            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            Test3(t, pmat.GetRow(0), pmat.GetRow(1), pmat.GetRow(2), pmat.GetRow(3));
            sw2.Stop();
            UnityEngine.Debug.Log("Elapsed 3 " + sw2.Elapsed);
            time3 += sw2.Elapsed;

            Stopwatch sw22 = new Stopwatch();
            sw22.Start();
            Test32(t, pmat.GetRow(0), pmat.GetRow(1), pmat.GetRow(2), pmat.GetRow(3));
            sw22.Stop();
            UnityEngine.Debug.Log("Elapsed 32 " + sw22.Elapsed);
            time32 += sw22.Elapsed;

            if (count < 10)
            {
                time1 = new System.TimeSpan(0);
                time2 = new System.TimeSpan(0);
                time3 = new System.TimeSpan(0);
                time32 = new System.TimeSpan(0);
            }

            if (sw22.Elapsed < sw.Elapsed) UnityEngine.Debug.Log("sw22 faster");
            else UnityEngine.Debug.Log("sw faster"); 
            count++;
        }
        else if (count == 100)
        {
            UnityEngine.Debug.Log("1: " + time1);
            UnityEngine.Debug.Log("3: " + time3);
            UnityEngine.Debug.Log("32: " + time32);
            count++;
        }
    }

    public Vector3 Test1(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t * t2;
        float mt = 1 - t;
        float mt2 = mt * mt;
        float mt3 = mt * mt2;

        // Bernstein Polynomial
        // ValueAt_Cubic(t) =
        // startPoint *      ( -t^3 + 3t^2 - 3t + 1 ) +
        // controlPoint[0] * ( t3t^3 - 6t^2 + 3t ) +
        // controlPoint[1] * ( -3t^3 + 3t^2 ) +
        // endPoint *        ( t^3 )
        var ret = p0 * mt3 +
            p1 * 3 * mt2 * t +
            p2 * 3 * mt * t2 +
            p3 * t3;

        return ret;
    }

    public Vector3 Test2(Vector4 tMatrix, Matrix4x4 characteristicMatrix, Matrix4x4 points)
    {
        var res = characteristicMatrix * points;
        for (int i = 0; i < 4; i++)
        {
            var col = res.GetColumn(0);
            res.SetColumn(0, Vector4.Scale(tMatrix, col));
        }


        var ret = res.GetColumn(0);
        return ret;
    }

    public Vector3 Test3(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Matrix4x4 tmat = new Matrix4x4();
        tmat.SetRow(0, new Vector4(1, t, t2, t3));

        Matrix4x4 pmat = new Matrix4x4();
        pmat.SetRow(0, p0);
        pmat.SetRow(1, p1);
        pmat.SetRow(2, p2);
        pmat.SetRow(3, p3);

        Vector3 ret = (tmat * cmat * pmat).GetRow(0);
        return ret;
    }

    public Vector3 Test32(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Matrix4x4 tmat = new Matrix4x4();
        tmat.SetRow(0, new Vector4(1, t, t2, t3));

        Matrix4x4 pmat = new Matrix4x4(p0, p1, p2, p3).transpose;

        Vector3 ret = (tmat * cmat * pmat).GetRow(0);
        return ret;
    }

    private static Matrix4x4 cmat = new Matrix4x4(new Vector4(1, -3, 3, -1), new Vector4(0, 3, -6, 3), new Vector4(0, 0, 3, -3), new Vector4(0, 0, 0, 1));
}
