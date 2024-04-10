using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Shapes
{
    public interface IShape : ICloneable
    {
        void AddFirst(Point pt);
        void AddSecond(Point pt);

        UIElement Convert();
        public string Name { get; }
        public string ThumbnailPath {  get; }
        public Color StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public string StrokeBrush {  get; set; }
        public Color FillColor {  get; set; }
    }
}
