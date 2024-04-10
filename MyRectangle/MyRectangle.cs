
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace MyRectangle
{
    class MyRectangle : IShape
    {
        Point _start;
        Point _end;
        public void AddFirst(Point pt)
        {
            _start = pt;
        }

        public void AddSecond(Point pt)
        {
            _end = pt;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        public string Name => "Rectangle";

        public string ThumbnailPath
        {
            get
            {
                return "pack://application:,,,/MyRectangle;component/rectangle.png";
            }
        }

        public UIElement Convert()
        {
            UIElement rectangle = new Rectangle()
            {
                Width = Math.Abs(_end.X - _start.X),
                Height = Math.Abs(_end.Y - _start.Y),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = new SolidColorBrush(FillColor),
                StrokeDashArray = StrokeDashArray
            };
            Canvas.SetLeft(rectangle, Math.Min(_end.X, _start.X));
            Canvas.SetTop(rectangle, Math.Min(_end.Y, _start.Y));
            return rectangle;
        }

        public Color StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public string StrokeBrush { get; set; }
        public Color FillColor { get; set; }
        public DoubleCollection StrokeDashArray { get; set; }
    }


}
