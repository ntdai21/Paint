using Contract;
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

        List<IShape> _selectedShapes = new List<IShape>();
        bool _isEdit = false;
        List<ControlPoint> _currentControlPointList = new List<ControlPoint>();
        int _selectedControlPointIndex = -1;
        Point _currentCursor = new Point(-1, -1);

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
        }

        private void RenderCanvas()
        {
            canvas.Children.Clear();

            foreach (IShape i in _painters)
            {
                canvas.Children.Add(i.Convert());
            }

            if (_isEdit && _selectedShapes.Count > 0)
            {
                foreach (IShape shape in _selectedShapes)
                {
                    BorderShape cur = (BorderShape)shape;
                    canvas.Children.Add(cur.RenderBorder());

                    if (_selectedShapes.Count == 1)
                    {
                        List<ControlPoint> controlPoints = cur.GetControlPointList();
                        _currentControlPointList = controlPoints;

                        foreach (ControlPoint controlPoint in controlPoints)
                        {
                            canvas.Children.Add(controlPoint.Render(cur.RotateAngle, cur.GetCenterPoint()));
                        }

                    }
                }
            }

        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_painter == null) return;

            if (!_isEdit)
            {
                //Draw
                _isDrawing = true;
                _start = e.GetPosition(canvas);

                _painter.StrokeColor = strokeColorGallery.SelectedColor ?? Colors.Transparent;
                _painter.FillColor = fillColorGallery.SelectedColor ?? Colors.Transparent;
                _painter.StrokeThickness = outlineThicknessSlider.Value;
                _painter.StrokeDashArray = _brush;
            }


            if (_isEdit)
            {
                //When edit mode
                Point cursor = e.GetPosition(canvas);
                _currentCursor = cursor;

                //Logic for choose a shape
                for (int i = 0; i < _painters.Count; i++)
                {
                    BorderShape current = (BorderShape)_painters[i];
                    if (current.IsHovering(cursor.X, cursor.Y))
                    {
                        // hold ctrl to select multiple object
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            if (!_selectedShapes.Contains(_painters[i]))
                            {
                                _selectedShapes.Add(_painters[i]);
                            }
                            else
                            {
                                _selectedShapes.Remove(_painters[i]);
                            }

                        }
                        else
                        {
                            //no hold ctrl to drop all and select one
                            _selectedShapes.Clear();
                            _selectedShapes.Add(_painters[i]);
                            _currentControlPointList = current.GetControlPointList();
                        }
                        RenderCanvas();
                    }

                }

                //Logic for choosing control point of single shape
                if (_selectedShapes.Count == 1)
                {
                    BorderShape selected = (BorderShape)_selectedShapes[0];
                    if (_currentControlPointList.Count > 0)
                    {
                        for (int i = 0; i < _currentControlPointList.Count; i++)
                        {
                            if (_currentControlPointList[i].IsHovering(selected.RotateAngle, cursor.X, cursor.Y))
                            {
                                _selectedControlPointIndex = i;
                                break;
                            }
                        }
                    }

                }


            }
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

            if (_isEdit)
            {
                if (_selectedShapes.Count < 1 || (Mouse.LeftButton != MouseButtonState.Pressed))
                {
                    return;
                }

                if (_selectedShapes.Count > 1)
                {
                    //Handle multiple move
                    double deltaX, deltaY;
                    Point newCursor = e.GetPosition(canvas);

                    deltaX = newCursor.X - _currentCursor.X;
                    deltaY = newCursor.Y - _currentCursor.Y;


                    foreach (BorderShape s in _selectedShapes)
                    {
                        if (true)
                        {
                            Point leftTop = s.LeftTop;
                            Point rightBottom = s.RightBottom;

                            leftTop.X += deltaX;
                            leftTop.Y += deltaY;
                            rightBottom.X += deltaX;
                            rightBottom.Y += deltaY;

                            s.LeftTop = leftTop;
                            s.RightBottom = rightBottom;
                        }


                    }

                    RenderCanvas();


                    _currentCursor = newCursor;
                }
                else
                {
                    //Handle edit single shape
                    BorderShape shape = (BorderShape)_selectedShapes[0];

                    if (_selectedControlPointIndex != -1)
                    {
                        _currentControlPointList[_selectedControlPointIndex].controlPointStrategy.HandleMouseMove(ref shape, e, canvas);

                        RenderCanvas();
                    }
                    else
                    {
                        //Handle move single shape
                        double deltaX, deltaY;
                        Point newCursor = e.GetPosition(canvas);

                        deltaX = newCursor.X - _currentCursor.X;
                        deltaY = newCursor.Y - _currentCursor.Y;

                        if (_selectedShapes.Count > 0)
                        {
                            foreach (BorderShape s in _selectedShapes)
                            {
                                if (s.IsHovering(newCursor.X, newCursor.Y))
                                {
                                    Point leftTop = s.LeftTop;
                                    Point rightBottom = s.RightBottom;

                                    leftTop.X += deltaX;
                                    leftTop.Y += deltaY;
                                    rightBottom.X += deltaX;
                                    rightBottom.Y += deltaY;

                                    s.LeftTop = leftTop;
                                    s.RightBottom = rightBottom;
                                }
                            }

                            RenderCanvas();
                        }

                        _currentCursor = newCursor;
                    }
                }
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isEdit)
            {
                _selectedControlPointIndex = -1;
            }
            if (_isDrawing)
            {
                _isDrawing = false;
                _painters.Add((IShape)_painter.Clone());
                _selectedControlPointIndex = -1;
            }


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

        private void ChangeMode_Click(object sender, RoutedEventArgs e)
        {
            _isEdit = !_isEdit;
        }
    }
}
