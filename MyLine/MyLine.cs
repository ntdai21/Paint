
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;
using System.Xml.Linq;

namespace MyLines
{
    public class MyLine : IShape
    {
        Point _startPoint;
        Point _endPoint;
        public void AddFirst(Point pt)
        {
            _startPoint = pt;
        }
        public string Name => "Line";
        public void AddSecond(Point pt)
        {
            _endPoint = pt;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            return new Line()
            {
                X1 = _startPoint.X,
                Y1 = _startPoint.Y,
                X2 = _endPoint.X,
                Y2 = _endPoint.Y,
                StrokeThickness = StrokeThickness,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeDashArray = StrokeDashArray
            };
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
