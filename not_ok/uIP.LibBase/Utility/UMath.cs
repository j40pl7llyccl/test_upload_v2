using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.LibBase.Utility
{
    public static class UMath
    {
        public static double Deg2Rad(double deg)
        {
            return deg * Math.PI / 180.0;
        }
        public static double Rad2Deg(double rad)
        {
            return rad * 180.0 / Math.PI;
        }
        public static void RotateDeg(double deg, double r, out double x, out double y)
        {
            x = r * Math.Cos(Deg2Rad(deg));
            y = r * Math.Sin(Deg2Rad(deg));
        }
        public static bool RotateRad(double rad, double r, out double x, out double y)
        {
            x = y = 0.0;
            if (rad < -Math.PI || rad > Math.PI)
                return false;

            x = r * Math.Cos(rad);
            y = r * Math.Sin(rad);
            return true;
        }
        public static void RotateDeg(double deg, double oriX, double oriY, double epX, double epY, out double finX, out double finY, bool isCCW = true)
        {
            deg = isCCW ? deg : -deg;

            double dx = epX - oriX;
            double dy = epY - oriY;

            double ang = deg < 0.0 ? -deg : deg;
            double radian = Deg2Rad(ang);

            if (!isCCW && deg > 0.0 || isCCW && deg < 0.0)
            {
                finX = (int)((double)dx * Math.Cos(radian) - (double)dy * Math.Sin(radian));
                finY = (int)((double)dy * Math.Cos(radian) + (double)dx * Math.Sin(radian));
            }
            else
            {
                finX = (int)((double)dx * Math.Cos(radian) + (double)dy * Math.Sin(radian));
                finY = (int)((double)dy * Math.Cos(radian) - (double)dx * Math.Sin(radian));
            }
        }
        // coor: image, pt sequence: RT, RB, LB, LT
        public static void RotateDeg(double deg, double cx, double cy, double w, double h, out double[] ptXs, out double[] ptYs, bool isCCW = true)
        {
            List<double> xx = new List<double>();
            List<double> yy = new List<double>();
            double epx, epy, finx, finy;

            // RT
            epx = cx + w / 2.0;
            epy = cy - h / 2.0;
            RotateDeg(deg, cx, cy, epx, epy, out finx, out finy, isCCW);
            xx.Add(finx);
            yy.Add(finy);

            // RB
            epx = cx + w / 2.0;
            epy = cy + h / 2.0;
            RotateDeg(deg, cx, cy, epx, epy, out finx, out finy, isCCW);
            xx.Add(finx);
            yy.Add(finy);

            // LB
            epx = cx - w / 2.0;
            epy = cy + h / 2.0;
            RotateDeg(deg, cx, cy, epx, epy, out finx, out finy, isCCW);
            xx.Add(finx);
            yy.Add(finy);

            // LT
            epx = cx - w / 2.0;
            epy = cy - h / 2.0;
            RotateDeg(deg, cx, cy, epx, epy, out finx, out finy, isCCW);
            xx.Add(finx);
            yy.Add(finy);

            ptXs = xx.ToArray();
            ptYs = yy.ToArray();
        }
        public static void RotateDegRect(double deg, double l, double t, double r, double b, out double[] ptXs, out double[] ptYs, bool isCCW = true)
        {
            double cx = (l + r) / 2.0;
            double cy = (t + b) / 2.0;

            RotateDeg(deg, cx, cy, r - l, b - t, out ptXs, out ptYs, isCCW);
        }
    }
}
