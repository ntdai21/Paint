
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;
using System.Xml.Linq;
using Contract;
using System.Drawing;

namespace MyEllipse
{
    class MyEllipse : BorderShape, IShape
    {

        public void AddFirst(System.Windows.Point pt)
        {
            LeftTop = pt;
        }

        public void AddSecond(System.Windows.Point pt)
        {
            RightBottom = pt;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public string Name => "Ellipse";

        public UIElement Convert()
        {
            System.Windows.Point _start = LeftTop;
            System.Windows.Point _end = RightBottom;
            UIElement ellipse = new Ellipse()
            {
                Width = Math.Abs(_end.X - _start.X),
                Height = Math.Abs(_end.Y - _start.Y),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = new SolidColorBrush(FillColor),
                StrokeDashArray = StrokeDashArray
            };
            RotateTransform transform = new RotateTransform(RotateAngle);
            transform.CenterX = Math.Abs(_end.X - _start.X) * 1.0 / 2;
            transform.CenterY = Math.Abs(_end.Y - _start.Y) * 1.0 / 2;

            Canvas.SetLeft(ellipse, Math.Min(_end.X, _start.X));
            Canvas.SetTop(ellipse, Math.Min(_end.Y, _start.Y));

            ellipse.RenderTransform = transform;

            return ellipse;
        }
        public string ThumbnailPath
        {
            get
            {
                return "pack://application:,,,/MyEllipse;component/ellipse.png";
            }
        }

        public System.Windows.Media.Color StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public string StrokeBrush { get; set; }
        public System.Windows.Media.Color FillColor { get; set; }
        public DoubleCollection StrokeDashArray { get; set; }
    }
}
