using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Contract
{
    public class BorderShape:ICloneable
    {
        public Point LeftTop { get; set; } = new();
        public Point RightBottom { get; set; }= new();
        public double RotateAngle { get; set; } = 0;

        virtual public Point GetCenterPoint()
        {
            Point centerPoint = new()
            {
                X = ((LeftTop.X + RightBottom.X) / 2),
                Y = ((LeftTop.Y + RightBottom.Y) / 2)
            };

            return centerPoint;
        }

        virtual public bool IsHovering(double x, double y)
        {
            return Util.Instance.IsBetween(x, RightBottom.X, LeftTop.X)
                && Util.Instance.IsBetween(y, RightBottom.Y, LeftTop.Y);
        }

        virtual public List<ControlPoint> GetControlPointList()
        {
            List<ControlPoint> list = new List<ControlPoint>();

            ControlPoint leftTop = new DiagPoint(LeftTop, GetCenterPoint(), new DiagControlPointStrategy("topleft"));
            ControlPoint left=new OneSidePoint(new Point(LeftTop.X,(RightBottom.Y + LeftTop.Y)/2),GetCenterPoint(), new DiagControlPointStrategy("left"));
            ControlPoint leftBottom=new DiagPoint(new Point(LeftTop.X, RightBottom.Y),GetCenterPoint(), new DiagControlPointStrategy("bottomleft"));
            ControlPoint bottom =  new OneSidePoint(new Point((LeftTop.X + RightBottom.X) / 2, RightBottom.Y), GetCenterPoint(), new DiagControlPointStrategy("bottom"));
            ControlPoint bottomRight=new DiagPoint(RightBottom,GetCenterPoint(), new DiagControlPointStrategy("bottomright"));
            ControlPoint right = new OneSidePoint(new Point(RightBottom.X, (RightBottom.Y + LeftTop.Y) / 2), GetCenterPoint(), new DiagControlPointStrategy("right"));
            ControlPoint topRight = new DiagPoint(new Point(RightBottom.X,LeftTop.Y), GetCenterPoint(), new DiagControlPointStrategy("topright"));
            ControlPoint rotatePoint = new RotatePoint(new Point((RightBottom.X + LeftTop.X) / 2, Math.Min(RightBottom.Y, LeftTop.Y) - 50), GetCenterPoint(), new RotateStrategy());
            ControlPoint top = new OneSidePoint(new Point((LeftTop.X + RightBottom.X) / 2, LeftTop.Y), GetCenterPoint(),new  DiagControlPointStrategy("top"));


            list.Add(leftTop);
            list.Add(left);
            list.Add(bottom);
            list.Add(bottomRight);
            list.Add(right);
            list.Add(leftBottom);
            list.Add(topRight);
            list.Add(rotatePoint);
            list.Add(top);

            return list;
        }

        virtual public UIElement RenderBorder()
        {
            var left = Math.Min(RightBottom.X, LeftTop.X);
            var top = Math.Min(RightBottom.Y, LeftTop.Y);

            var right = Math.Max(RightBottom.X, LeftTop.X);
            var bottom = Math.Max(RightBottom.Y, LeftTop.Y);

            var width = right - left;
            var height = bottom - top;

            var rect = new System.Windows.Shapes.Rectangle()
            {
                Width = width,
                Height = height,
                StrokeThickness = 2,
                Stroke = Brushes.Black,
                StrokeDashArray = { 4, 2, 4 }
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            RotateTransform transform = new RotateTransform(RotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;

            rect.RenderTransform = transform;

            return rect;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
