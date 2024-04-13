using Contract;
using Fluent;
using Paint.Converters;
using Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
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
    public partial class PaintWindow : RibbonWindow, INotifyPropertyChanged
    {
        bool _isDrawing = false;
        Point _start;
        Point _end;

        List<IShape> _selectedShapes = new List<IShape>();
        bool _isEdit = false;
        List<ControlPoint> _currentControlPointList = new List<ControlPoint>();
        int _selectedControlPointIndex = -1;
        Point _currentCursor = new Point(-1, -1);
        List<IShape> _copyBuffer = new List<IShape>();
        Stack<List<IShape>> _undoStack = new Stack<List<IShape>>();
        Stack<List<IShape>> _redoStack = new Stack<List<IShape>>();
        bool isChange = false;

        List<IShape> _shapes = new List<IShape>();

        List<IShape> _painters = new List<IShape>();
        IShape _painter = null;

        List<string> _brushes = new();
        DoubleCollection _brush;

        public int CanvasWidth { get; set; } = 2000;
        public int CanvasHeight { get; set; } = 2000;

        public PaintWindow()
        {
            InitializeComponent();
            KeyDown += Canvas_PreviewKeyDown;
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


            //Init screen
            AddToUndo(_painters);

            brushGallery.SelectedIndex = 0;

            scrollViewer.PreviewMouseRightButtonUp += ScrollViwer_OnMouseRightButtonUp;
            scrollViewer.MouseRightButtonUp += ScrollViwer_OnMouseRightButtonUp;
            scrollViewer.PreviewMouseRightButtonDown += ScrollViewer_OnMouseRightButtonDown;
            scrollViewer.MouseMove += ScrollViewer_OnMouseMove;

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
                _painter.StrokeThickness = strokeThicknessSlider.Value;
                _painter.StrokeDashArray = _brush;
            }


            if (_isEdit)
            {
                //When edit mode
                Point cursor = e.GetPosition(canvas);
                _currentCursor = cursor;

                bool isAction = false;
                //Logic for choose a shape
                for (int i = 0; i < _painters.Count; i++)
                {
                    BorderShape current = (BorderShape)_painters[i];
                    if (current.IsHovering(cursor.X, cursor.Y))
                    {
                        isAction = true;
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
                            if (_selectedShapes.Count < 2)
                            {

                                //no hold ctrl to drop all and select one
                                _selectedShapes.Clear();
                                _selectedShapes.Add(_painters[i]);
                                _currentControlPointList = current.GetControlPointList();
                            }
                        }


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
                                isAction = true;
                                _selectedControlPointIndex = i;
                                break;
                            }
                        }
                    }

                }

                //Click outside all not doing anything -> remove all selected shape
                if (isAction == false)
                {
                    _selectedShapes.Clear();
                }
                RenderCanvas();


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
            else if (cursorToggle.IsChecked == true)
            {

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
                    isChange = true;

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
                    isChange = true;

                    BorderShape shape = (BorderShape)_selectedShapes[0];

                    if (_selectedControlPointIndex != -1)
                    {
                        _currentControlPointList[_selectedControlPointIndex].controlPointStrategy.HandleMouseMove(ref shape, e, canvas);

                        RenderCanvas();
                    }
                    else
                    {
                        //Handle move single shape
                        isChange = true;

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

                if (isChange == true)
                {
                    AddToUndo(_painters);
                    isChange = false;
                }

                _selectedControlPointIndex = -1;
            }
            if (_isDrawing)
            {
                _isDrawing = false;
                _painters.Add((IShape)_painter.Clone());
                AddToUndo(_painters);
                _selectedControlPointIndex = -1;

            }


        }

        private void shapeGallery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IShape item = (IShape)shapeGallery.SelectedItem;
            _painter = item;
            cursorToggle.IsChecked = false;
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
            /*
            if (_isEdit == false)
            {
                AddToUndo(_painters);

            }
            */
            _isEdit = !_isEdit;
        }

        private void HandleCopyEvent()
        {
            if (_isEdit)
            {
                _copyBuffer.Clear();
                foreach (var shape in _selectedShapes)
                {
                    _copyBuffer.Add((IShape)shape.Clone());
                }
            }
        }

        private void HandlePasteEvent(Point cursor)
        {
            if (_isEdit && _copyBuffer.Count > 0)
            {
                AddToUndo(_painters);

                List<IShape> temporary = new List<IShape>();
                Point newCursor = Mouse.GetPosition(canvas);
                Point minTopLeft;
                double mintLength = double.MaxValue;

                //Calculate min length from top left canvas
                foreach (BorderShape shape in _copyBuffer)
                {
                    double dX = shape.LeftTop.X;
                    double dY = shape.LeftTop.Y;
                    double length = Math.Sqrt(dX * dX + dY * dY);

                    if (length < mintLength)
                    {
                        mintLength = length;
                        minTopLeft = shape.LeftTop;
                    }
                }

                double deltaX = newCursor.X - minTopLeft.X;
                double deltaY = newCursor.Y - minTopLeft.Y;

                foreach (BorderShape shape in _copyBuffer)
                {

                    Point leftTop = shape.LeftTop;
                    Point rightBottom = shape.RightBottom;

                    leftTop.X += deltaX;
                    leftTop.Y += deltaY;
                    rightBottom.X += deltaX;
                    rightBottom.Y += deltaY;

                    shape.LeftTop = leftTop;
                    shape.RightBottom = rightBottom;

                    temporary.Add((IShape)shape.Clone());

                }

                foreach (IShape shape in temporary)
                {
                    _painters.Add(shape);
                }

                RenderCanvas();

                _currentCursor = newCursor;
            }
        }

        private void HandleCutEvent()
        {
            if (_isEdit)
            {
                _copyBuffer.Clear();
                foreach (var shape in _selectedShapes)
                {
                    _copyBuffer.Add((IShape)shape.Clone());
                    _painters.Remove(shape);
                }

                _selectedShapes.Clear();

                RenderCanvas();
            }
        }

        private void HandleUndoEvent()
        {
            if (true)
            {
                if (_undoStack.Count > 1)
                {
                    List<IShape> current = _undoStack.Pop(); // Remove current screen
                    AddToRedo(current);
                    List<IShape> old = _undoStack.Peek();
                    _painters = old;
                    _selectedShapes.Clear();
                    RenderCanvas();
                }

            }
        }

        private void HandleRedoEvent()
        {
            if (true)
            {
                if (_redoStack.Count > 0)
                {
                    List<IShape> last = _redoStack.Pop();
                    AddToUndo(last);
                    _painters = last;
                    _selectedShapes.Clear();
                    RenderCanvas();

                }
            }
        }

        private void Canvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                HandleCopyEvent();
                e.Handled = true;

            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                Point mouseCursor = Mouse.GetPosition(canvas);
                if (mouseCursor.X >= 0 && mouseCursor.Y >= 0)
                {
                    HandlePasteEvent(mouseCursor);

                }
                e.Handled = true;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
            {
                HandleCutEvent();
                e.Handled = true;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                HandleUndoEvent();
                e.Handled = true;
            }
            if ((Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y))
            {
                HandleRedoEvent();
                e.Handled = true;
            }
        }

        private void AddToUndo(List<IShape> shapes)
        {
            List<IShape> clone = new List<IShape>();

            foreach (var shape in shapes)
            {

                clone.Add((IShape)shape.Clone());

            }

            _undoStack.Push(clone);
        }

        private void AddToRedo(List<IShape> shapes)
        {
            List<IShape> clone = new List<IShape>();

            foreach (var shape in shapes)
            {
                clone.Add((IShape)shape.Clone());
            }

            _redoStack.Push(clone);
        }


        Point? lastDragPoint;

        void ScrollViewer_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }
        void ScrollViewer_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y <
                scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }

        void ScrollViwer_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        private void cursorToggle_Click(object sender, RoutedEventArgs e)
        {
            shapeGallery.SelectedItem = null;
            _painter = null;
            cursorToggle.IsChecked = true;
        }

    }

}
