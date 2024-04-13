
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using Contract;

namespace MyRectangle
{
    class MyRectangle : BorderShape, IShape
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
            _start=LeftTop; _end=RightBottom;
            UIElement rectangle = new Rectangle()
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
            Canvas.SetLeft(rectangle, Math.Min(_end.X, _start.X));
            Canvas.SetTop(rectangle, Math.Min(_end.Y, _start.Y));
            rectangle.RenderTransform=transform;
            return rectangle;
        }

        public Color StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public string StrokeBrush { get; set; }
        public Color FillColor { get; set; }
        public DoubleCollection StrokeDashArray { get; set; }
    }


}
