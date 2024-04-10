using Fluent;
using Paint.Converters;
using Shapes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Interaction logic for PaintWindow.xaml
    /// </summary>
    public partial class PaintWindow : RibbonWindow
    {
        bool _isDrawing = false;
        Point _start;
        Point _end;

        List<UIElement> _list = new List<UIElement>();
        List<IShape> _shapes = new List<IShape>();
        UIElement _lastElement = null;

        List<IShape> _painters = new List<IShape>();
        IShape _painter = null;

        List<string> icons = new List<string>();

        ColorToBrushConverter _colorToBrush = new ColorToBrushConverter();

        public PaintWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var fis = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach (var fi in fis)
            {
                var assembly = Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass && (typeof(IShape).IsAssignableFrom(type)))
                    {
                        _shapes.Add((IShape)Activator.CreateInstance(type));
                    }
                }
            }
            
            shapeGallery.ItemsSource = _shapes;
            strokeColorGallery.SelectedColor = Colors.Black;
            fillColorGallery.SelectedColor = Colors.Black;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_painter == null) return;
            _isDrawing = true;
            _start = e.GetPosition(canvas);
            _painter.StrokeColor = strokeColorGallery.SelectedColor ?? Colors.Transparent;
            _painter.FillColor = fillColorGallery.SelectedColor ?? Colors.Transparent;
            _painter.StrokeThickness = outlineThicknessSlider.Value;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(canvas);

                canvas.Children.Clear();

                foreach (IShape i in _painters)
                {
                    canvas.Children.Add(i.Convert());
                }

                _painter.AddFirst(_start);
                _painter.AddSecond(_end);
                canvas.Children.Add(_painter.Convert());
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing) return;
            _isDrawing = false;
            _painters.Add((IShape)_painter.Clone());
        }

        private void shapeGallery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IShape item = (IShape)shapeGallery.SelectedItem;
            _painter = item;
        }

        private void ItemGallery_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked item
            var clickedItem = (e.OriginalSource as FrameworkElement)?.DataContext;

            // Select the clicked item
            shapeGallery.SelectedItem = clickedItem;

            // Close the dropdown
            shapeGallery.IsDropDownOpen = false;

            // Handle the click event
            e.Handled = true;
        }
    }
}
