using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Curve : MonoBehaviour
{
    public List<CurvePoint> points = new List<CurvePoint>()
    {
        new CurvePoint(){position = new Vector3(-1f, 0, -1f), tangentIn = new Vector3(-1f, 0, -1f), tangentOut = new Vector3(1f, 0, 1f)},
        new CurvePoint(){position = new Vector3( 1f, 0,  1f), tangentIn = new Vector3(-1f, 0, -1f), tangentOut = new Vector3(1f, 0, 1f)}
    };

    public bool useLocalSpace = true;
    public bool loop = false;

    public Vector3 InterpolatePoints ( CurvePoint cp0, CurvePoint cp1, float t )
    {
        var p0 = cp0.position;
        var p1 = cp0.position + cp0.tangentOut;
        var p2 = cp1.position + cp1.tangentIn;
        var p3 = cp1.position;

        var t2 = t * t;
        var t3 = t2 * t;
        
        var omt = 1f - t;
        var omt2 = omt * omt;
        var omt3 = omt * omt2;

        return omt3 * p0 + 3f * t * omt2 * p1 + 3f * t2 * omt * p2 + t3 * p3;
    }

    public Vector3 InterpolatePoints(int index0, int index1, float t)
    {
        if (index0 < 0 || index0 > points.Count || index1 < 0 || index1 > points.Count ) return Vector3.zero;

        return InterpolatePoints(points[index0], points[index1], t);
    }
    
    [System.Serializable]
    public class CurvePoint
    {
        public Vector3 position;
        public Vector3 tangentIn;
        public Vector3 tangentOut;
    }
}
