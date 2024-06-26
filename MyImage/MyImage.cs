
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using Contract;
using System.Text.Json.Serialization;
using System.IO;

namespace MyImages
{
    public class MyImage : BorderShape, IShape
    {
        BitmapSource _imgSrc = null;

        public MyImage(BitmapSource imgSrc)
        {
            _imgSrc = imgSrc;
            LeftTop = new Point(0, 0);
            RightBottom = _imgSrc != null ? new Point(_imgSrc.Width, _imgSrc.Height) : new Point(0, 0);
        }
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
        public string Name => "Image";

        public Color StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public string StrokeBrush { get; set; }
        public Color FillColor { get; set; }
        public DoubleCollection StrokeDashArray { get; set; }

        public string ThumbnailPath
        {
            get
            {
                return "pack://application:,,,/MyRectangle;component/rectangle.png";
            }
        }

        public UIElement Convert()
        {
            Point _start = LeftTop;
            Point _end = RightBottom;
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = _imgSrc;
            //paintCanvas.Background = brush;

            UIElement rectangle = new Rectangle()
            {
                Width = Math.Abs(_end.X - _start.X),
                Height = Math.Abs(_end.Y - _start.Y),
                Fill = brush
            };


            RotateTransform transform = new RotateTransform(RotateAngle);
            transform.CenterX = Math.Abs(_end.X - _start.X) * 1.0 / 2;
            transform.CenterY = Math.Abs(_end.Y - _start.Y) * 1.0 / 2;
            Canvas.SetLeft(rectangle, Math.Min(_end.X, _start.X));
            Canvas.SetTop(rectangle, Math.Min(_end.Y, _start.Y));
            rectangle.RenderTransform = transform;
            return rectangle;
        }
        [JsonIgnore]
        public BitmapSource ImageSource
        {
            get { return _imgSrc; }
            set { _imgSrc = value; }
        }

        public string ImageBase64
        {
            get { return ImageToBase64(_imgSrc); }
            set { _imgSrc = Base64ToImage(value); }
        }
        public static string ImageToBase64(BitmapSource image)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            var bytes = ms.ToArray();
            return System.Convert.ToBase64String(bytes);
        }

        public static BitmapSource Base64ToImage(string base64)
        {
            var bytes = System.Convert.FromBase64String(base64);
            using var ms = new MemoryStream(bytes);
            var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            return decoder.Frames[0];
        }
    }

}

