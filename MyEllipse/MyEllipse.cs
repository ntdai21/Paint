
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;
using System.Xml.Linq;
using Contract;

namespace MyEllipse
{
    class MyEllipse : BorderShape, IShape
    {
        Point _start;
        Point _end;
        public void AddFirst(Point pt)
        {
            LeftTop = pt;
        }

        public void AddSecond(Point pt)
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
            _start = LeftTop; _end=RightBottom;
            UIElement ellipse = new Ellipse()
            {
                Width = Math.Abs(_end.X - _start.X),
                Height = Math.Abs(_end.Y - _start.Y),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = new SolidColorBrush(FillColor),
                StrokeDashArray = StrokeDashArray
            };
            Canvas.SetLeft(ellipse, Math.Min(_end.X, _start.X));
            Canvas.SetTop(ellipse, Math.Min(_end.Y, _start.Y));
            return ellipse;
        }
        public string ThumbnailPath
        {
            get
            {
                return "pack://application:,,,/MyEllipse;component/ellipse.png";
            }
        }

        public Color StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public string StrokeBrush { get; set; }
        public Color FillColor { get; set; }
        public DoubleCollection StrokeDashArray { get; set; }
    }
}
