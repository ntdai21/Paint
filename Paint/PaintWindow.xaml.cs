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

        List<IShape> _shapes = new List<IShape>();

        List<IShape> _painters = new List<IShape>();
        IShape _painter = null;

        List<string> _brushes = new();
        DoubleCollection _brush;

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
            fillColorGallery.SelectedColor = Colors.Transparent;

            //Add stroke brushes
            _brushes.Add("");
            _brushes.Add("1");
            _brushes.Add("1 6");
            _brushes.Add("6 1");
            _brushes.Add("0.25 1");
            _brushes.Add("4 1 1 1 1 1");
            _brushes.Add("5, 5, 1, 5");
            _brushes.Add("1 2 4");

            brushGallery.ItemsSource = _brushes;
            brushGallery.SelectedIndex = 0;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_painter == null) return;
            _isDrawing = true;
            _start = e.GetPosition(canvas);

            _painter.StrokeColor = strokeColorGallery.SelectedColor ?? Colors.Transparent;
            _painter.FillColor = fillColorGallery.SelectedColor ?? Colors.Transparent;
            _painter.StrokeThickness = strokeThicknessSlider.Value;
            _painter.StrokeDashArray = _brush;
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

        private void brushGallery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var converter = new DoubleCollectionConverter();
            string item = (string)brushGallery.SelectedItem;

            _brush = (DoubleCollection)converter.ConvertFromString(item);
        }

        private void brushGallery_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked item
            var clickedItem = (e.OriginalSource as FrameworkElement)?.DataContext;

            // Select the clicked item
            brushGallery.SelectedItem = clickedItem;

            // Close the dropdown
            brushGallery.IsDropDownOpen = false;

            // Handle the click event
            e.Handled = true;
        }
    }
}
