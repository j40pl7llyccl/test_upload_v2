using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.LibBase.Utility
{
    public class Point2dDf
    {
        double _X = 0.0;
        double _Y = 0.0;

        public double X { get { return _X; } set { _X = value; } }
        public double Y { get { return _Y; } set { _Y = value; } }

        public Point2dDf() { }
        public Point2dDf(double x, double y)
        {
            _X = x;
            _Y = y;
        }
    }
    public class VectorDf
    {
        public double _X;
        public double _Y;
        public VectorDf() { }
        public VectorDf(double x, double y)
        {
            _X = x;
            _Y = y;
        }

        public static VectorDf ToVector(int x1, int y1, int x2, int y2)
        {
            return new VectorDf(x2 - x1, y2 - y1);
        }
        public static VectorDf ToVector(double x1, double y1, double x2, double y2)
        {
            return new VectorDf(x2 - x1, y2 - y1);
        }
        public static VectorDf ToVector(Point2dDf p1, Point2dDf p2)
        {
            return ToVector(p1.X, p1.Y, p2.X, p2.Y);
        }
        public VectorDf Scale(double s)
        {
            return new VectorDf(_X * s, _Y * s);
        }
        public double Square()
        {
            return _X * _X + _Y * _Y;
        }
        public bool MagnitudeRoot(out double d)
        {
            d = Square();
            if (d < 0.0)
            {
                d = 0.0;
                return false;
            }

            d = Math.Sqrt(d);
            return true;
        }
        public VectorDf Normalize()
        {
            double d = 0.0;
            if (!MagnitudeRoot(out d))
                return null;

            if (d == 0.0)
                return null;

            return new VectorDf(_X / d, _Y / d);
        }
        public VectorDf NormalVector()
        {
            return new VectorDf(-_X, _Y);
        }
    }

    public class ProjectionResult
    {
        public VectorDf _vProjection;
        public VectorDf _vOrthogonal;
        public double _dfLenProjection;
        public double _dfLenOrthogonal;
        public ProjectionResult() { }
    }

    public static class UVector2d
    {
        /// <summary>
        /// v1 - v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VectorDf Substract(VectorDf v1, VectorDf v2)
        {
            if (v1 == null || v2 == null) return null;

            return new VectorDf(v1._X - v2._X, v1._Y - v2._Y);
        }
        /// <summary>
        /// V1 + v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VectorDf Add(VectorDf v1, VectorDf v2)
        {
            if (v1 == null || v2 == null)
                return null;
            return new VectorDf(v1._X + v2._X, v1._Y + v2._Y);
        }
        /// <summary>
        /// r.x = scale.x * v1.x + scale.y * v2.x;
        /// r.y = scale.x * v1.y + scale.y * v2.y;
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool LinearCombination(VectorDf scale, VectorDf v1, VectorDf v2, out VectorDf r)
        {
            r = null;
            if (scale == null || v1 == null || v2 == null)
                return false;

            r = new VectorDf();
            r._X = scale._X * v1._X + scale._Y * v2._X;
            r._Y = scale._X * v1._Y + scale._Y * v2._Y;

            return true;
        }
        public static bool Product(VectorDf v1, VectorDf v2, out double val)
        {
            val = 0.0;
            if (v1 == null || v2 == null)
                return false;
            val = v1._X * v2._X + v1._Y * v2._Y;
            return true;
        }
        public static bool VectorAngleRadian(VectorDf v1, VectorDf v2, out double radian)
        {
            radian = 0.0;
            if (v1 == null || v2 == null)
                return false;

            VectorDf n1 = v1.Normalize();
            VectorDf n2 = v2.Normalize();
            if (n1 == null || n2 == null)
                return false;

            double cosVal;
            bool status = Product(n1, n2, out cosVal);
            if (!status)
                return false;

            if (cosVal > 1.0 || cosVal < -1.0)
                return false;

            radian = Math.Acos(cosVal);
            return true;
        }
        static double RadianToDeg(double r)
        {
            return 180.0 * r / Math.PI;
        }
        public static bool VectorAngleDegree(VectorDf v1, VectorDf v2, out double deg)
        {
            if (!VectorAngleRadian(v1, v2, out deg))
                return false;
            deg = RadianToDeg(deg);
            return true;
        }


        /// <summary>
        /// xv = (1, 0), sign -1 should add in y < 0
        /// </summary>
        /// <param name="v">vector to be eval</param>
        /// <param name="radian">return radian of angle</param>
        /// <returns>true:ok, otherwise NG</returns>
        public static bool VectorAngleRadianFromXAxis(VectorDf v, out double radian)
        {
            radian = 0.0;
            if (v == null)
                return false;

            VectorDf xv = VectorDf.ToVector(0, 0, 1, 0);
            return VectorAngleRadian(v, xv, out radian);
        }
        /// <summary>
        /// xv = (1, 0), sign -1 should add in y < 0
        /// </summary>
        /// <param name="v">vector to be eval</param>
        /// <param name="deg">return degree of angle</param>
        /// <returns>true:ok, otherwise NG</returns>
        public static bool VectorAngleDegreeFromXAxis(VectorDf v, out double deg)
        {
            if (!VectorAngleRadianFromXAxis(v, out deg))
                return false;
            deg = RadianToDeg(deg);
            return true;
        }

        public static bool IsPerpendicular(VectorDf v1, VectorDf v2, out bool bPerpendicular)
        {
            bPerpendicular = false;
            if (v1 == null || v2 == null) return false;
            double val;
            if (!Product(v1, v2, out val))
                return false;
            bPerpendicular = val == 0.0;
            return true;
        }

        public static bool ProjectAndResolve(VectorDf v0, VectorDf v1, out ProjectionResult r)
        {
            r = null;
            if (v0 == null || v1 == null)
                return false;

            VectorDf vProj = new VectorDf();
            VectorDf vOrth = null;
            double proj1 = 0.0;
            double prodV0V1 = 0.0, prodV1V1 = 0.0;

            if (!Product(v0, v1, out prodV0V1)) return false;
            if (!Product(v1, v1, out prodV1V1)) return false;
            if (prodV1V1 == 0.0)
                return false;

            ProjectionResult tmp = new ProjectionResult();

            proj1 = prodV0V1 / prodV1V1;
            vProj._X = v1._X * proj1;
            vProj._Y = v1._Y * proj1;

            vOrth = Substract(v0, vProj);

            if (!vProj.MagnitudeRoot(out tmp._dfLenProjection))
                return false;
            if (!vOrth.MagnitudeRoot(out tmp._dfLenOrthogonal))
                return false;

            tmp._vProjection = vProj.Normalize();
            tmp._vOrthogonal = vOrth._X == 0.0 && vOrth._Y == 0.0 ? vOrth : vOrth.Normalize();
            r = tmp;
            return true;
        }
        /// <summary>
        /// begin (lineX0, lineY0), vector line from 0 -> 1, vector test from 0 -> test 
        /// </summary>
        /// <param name="lineX0"></param>
        /// <param name="lineY0"></param>
        /// <param name="lineX1"></param>
        /// <param name="lineY1"></param>
        /// <param name="ptXToTest"></param>
        /// <param name="ptYToTest"></param>
        /// <returns></returns>
        public static ProjectionResult DistanceFromPointToLine(double lineX0, double lineY0, double lineX1, double lineY1, double ptXToTest, double ptYToTest, out double dist)
        {
            dist = 0.0;
            VectorDf vLine = VectorDf.ToVector(lineX0, lineY0, lineX1, lineY1);
            VectorDf vTest = VectorDf.ToVector(lineX0, lineY0, ptXToTest, ptYToTest);
            ProjectionResult r = null;
            if (!ProjectAndResolve(vTest, vLine, out r))
                return null;

            dist = r._dfLenOrthogonal;
            return r;
        }
    }
}
