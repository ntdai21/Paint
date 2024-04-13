using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Contract
{
    public  class Util
    {
        private static  Util instance;
        public static Util Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Util();
                }

                return instance;
            }
        }

        public bool IsBetween(double cur, double bound1, double bound2)
        {
            if (bound1 > bound2)
                return cur < bound1 && cur > bound2;
            else
                return cur < bound2 && cur > bound1;
        }

        public float GetAlphaAngleRadian(double RotateAngle)
        {
            float a;
            if (RotateAngle >= 0 && RotateAngle < 180) { a = (float)(RotateAngle / (180 / Math.PI)); }
            else
            {
                a = (float)((RotateAngle - 360) / (180 / Math.PI));
            }

            a %= (float)(2 * Math.PI);

            return a;
        }

        public  Tuple<double, double> GetCenterPointTranslation(Point oldCenterPoint, Point newCenterPoint, BorderShape shape)
        {
            var ir = new RotateTransform(shape.RotateAngle, oldCenterPoint.X, oldCenterPoint.Y);
            var fr = new RotateTransform(shape.RotateAngle, newCenterPoint.X, newCenterPoint.Y);
            var ip = ir.Transform(new Point(shape.LeftTop.X, shape.LeftTop.Y));
            var fp = fr.Transform(new Point(shape.LeftTop.X, shape.LeftTop.Y));

            var txx = ip.X - fp.X;
            var tyy = ip.Y - fp.Y;

            return new Tuple<double, double>(txx, tyy);
        }
    }
}
