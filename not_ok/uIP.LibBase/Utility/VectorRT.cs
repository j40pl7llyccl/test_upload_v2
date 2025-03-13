using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.LibBase.Utility
{
    // vector Radius, theta from cx, cy
    public class UVectorRT
    {
        public static bool Transfer(double cx, double cy, double p1X, double p1Y, double p2X, double p2Y,
            out double r, out double radian)
        {
            r = radian = 0.0;
            ProjectionResult prj = UVector2d.DistanceFromPointToLine(p1X, p1Y, p2X, p2Y, cx, cy, out r);
            if (prj == null)
                return false;

            double intersectX = prj._vProjection._X * prj._dfLenProjection;
            double intersectY = prj._vProjection._Y * prj._dfLenProjection;

            VectorDf v = VectorDf.ToVector(cx, cy, intersectX, intersectY);
            return UVector2d.VectorAngleRadianFromXAxis(v, out radian);
        }
        public static double RadianToDeg(double r)
        {
            return 180.0 * r / Math.PI;
        }
        public static bool RadianToDeg(double r, out double deg)
        {
            deg = 180.0 * r / Math.PI;
            return true;
        }
    }
}
