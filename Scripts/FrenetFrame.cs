using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FrenetFrame
{
    public Vector3 origin;
    public Vector3 tangent;
    public Vector3 rotationalAxis;
    public Vector3 normal;

    public override bool Equals(object obj)
    {
        return obj is FrenetFrame && Equals((FrenetFrame)obj);
    }

    public bool Equals(FrenetFrame frame)
    {
        return this.origin == frame.origin && this.tangent == frame.tangent && this.rotationalAxis == frame.rotationalAxis && this.normal == frame.normal;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(FrenetFrame lhs, FrenetFrame rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(FrenetFrame lhs, FrenetFrame rhs)
    {
        return !lhs.Equals(rhs);
    }
}