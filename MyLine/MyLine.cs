﻿
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;
using System.Xml.Linq;
using Contract;

namespace MyLines
{
    public class MyLine : BorderShape, IShape
    {

        public void AddFirst(Point pt)
        {
            LeftTop = pt;
        }
        public string Name => "Line";
        public void AddSecond(Point pt)
        {
            RightBottom = pt;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            Point _startPoint = LeftTop;
            Point _endPoint = RightBottom;
            Line line= new Line()
            {
                X1 = _startPoint.X,
                Y1 = _startPoint.Y,
                X2 = _endPoint.X,
                Y2 = _endPoint.Y,
                StrokeThickness = StrokeThickness,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeDashArray = StrokeDashArray
            };

            RotateTransform transform = new(RotateAngle);
            line.RenderTransform=transform;
            return line;
        }

        override public List<ControlPoint> GetControlPointList()
        {
            Point _startPoint = LeftTop;
            Point _endPoint = RightBottom;
            List<ControlPoint> controlPoints = new List<ControlPoint>();

            ControlPoint start=new LinePoint() {Position=_startPoint,CenterPoint=GetCenterPoint(),Edge="leftTop", controlPointStrategy = new LineControlPointStrategy(_startPoint) };

            ControlPoint end = new LinePoint() { Position = _endPoint, CenterPoint = GetCenterPoint(), Edge = "rightBottom", controlPointStrategy = new LineControlPointStrategy(_endPoint) };
            controlPoints.Add(start);
            controlPoints.Add(end);
            return controlPoints;
        }

        public override UIElement RenderBorder()
        {
            Point _startPoint = LeftTop;
            Point _endPoint = RightBottom;
            var line = new Line()
            {
                X1 = _startPoint.X,
                Y1 = _startPoint.Y,
                X2 = _endPoint.X,
                Y2 = _endPoint.Y,
                StrokeThickness = StrokeThickness,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeDashArray = StrokeDashArray
            };

            RotateTransform rotateTransform = new(RotateAngle);
            rotateTransform.CenterX = Math.Abs(_startPoint.X - _endPoint.X);
            rotateTransform.CenterY=Math.Abs(_startPoint.Y - _endPoint.Y);
            line.RenderTransform=rotateTransform;

            return line;
        }

        public string ThumbnailPath
        {
            get
            {
                return "pack://application:,,,/MyLine;component/line.png";
            }
        }

        public Color StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public string StrokeBrush { get; set; }
        public Color FillColor { get; set; }
        public DoubleCollection StrokeDashArray { get; set; }
    }

}
