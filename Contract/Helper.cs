using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Contract
{
    internal class Helper
    {
        private static Helper instance;
        public static Helper Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new Helper();
                }

                return instance;
            }
        }

        public Point Rotate(Point pt, double angle, Point center)
        {
            Vector v = new Vector(pt.X - center.X, pt.Y - center.Y);
            Vector rotatedVector=Rotate(v, angle);
            return new Point(rotatedVector.X + center.X, rotatedVector.Y + center.Y);
        }

        public Vector Rotate( Vector v, double degrees)
        {
            return RotateRadians(v,degrees * Math.PI / 180);
        }

        public Vector RotateRadians(Vector v, double radians)
        {
            double ca = Math.Cos(radians);
            double sa = Math.Sin(radians);
            return new Vector(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }


    }
}
