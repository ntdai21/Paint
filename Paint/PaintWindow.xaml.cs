using Contract;
using Fluent;
using Newtonsoft.Json;
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
using Path = System.IO.Path;
using MyImages;

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
        bool _isEdit { get => cursorToggle.IsChecked == true; }
        List<ControlPoint> _currentControlPointList = new List<ControlPoint>();
        int _selectedControlPointIndex = -1;
        Point _currentCursor = new Point(-1, -1);
        List<IShape> _copyBuffer = new List<IShape>();
        Stack<List<IShape>> _undoStack = new Stack<List<IShape>>();
        Stack<List<IShape>> _redoStack = new Stack<List<IShape>>();
        bool isChange = false; //expand, rotate, move,...
        bool _isChangeProperty = false; //color,thickness,...

        List<IShape> _shapes = new List<IShape>();

        List<IShape> _painters = new List<IShape>();
        IShape? _painter = null;

        List<string> _brushes = new();
        DoubleCollection _brush;

        public int CanvasWidth { get; set; } = 2000;
        public int CanvasHeight { get; set; } = 2000;
        public string BackgroundImagePath = string.Empty;
        public bool IsDrawing { get; set; } = false;
        public bool IsSaved { get; set; } = false;
        private string savedFilePath = string.Empty;

        private WriteableBitmap buffer;
        private Image image;

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
                    if (type.IsClass && (!typeof(MyImage).IsAssignableFrom(type)) && (typeof(IShape).IsAssignableFrom(type)))
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
            LoadShapesFromXml("data.xml");
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

        private void AddPreviewToCanvas()
        {
            previewCanvas.Children.Clear();
            previewCanvas.Children.Add(_painter.Convert());
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isEdit)
            {
                if (_painter == null) return;
                //Draw
                _isDrawing = true;
                _start = e.GetPosition(canvas);

                _painter.StrokeColor = strokeColorGallery.SelectedColor ?? Colors.Transparent;
                _painter.FillColor = fillColorGallery.SelectedColor ?? Colors.Transparent;
                _painter.StrokeThickness = strokeThicknessSlider.Value;
                _painter.StrokeDashArray = _brush;

                previewCanvas.CaptureMouse();
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

                //canvas.Children.Clear();
                //foreach (IShape i in _painters)
                //{
                    //canvas.Children.Add(i.Convert());
                //}


                _painter.AddFirst(_start);
                _painter.AddSecond(_end);
                //canvas.Children.Add(_painter.Convert());
                previewCanvas.Children.Clear();
                previewCanvas.Children.Add(_painter.Convert());
            }

            if (_isEdit)
            {
                if (_selectedShapes.Count < 1 || (Mouse.LeftButton != MouseButtonState.Pressed) || _isChangeProperty == true)
                {
                    _isChangeProperty = false;
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
                    //isChange = true;

                    BorderShape shape = (BorderShape)_selectedShapes[0];

                    if (_selectedControlPointIndex != -1)
                    {
                        isChange = true;
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

                previewCanvas.Children.Clear();
                canvas.Children.Add(_painter.Convert());

                previewCanvas.ReleaseMouseCapture();
            }
        }
        
        private void shapeGallery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IShape item = (IShape)shapeGallery.SelectedItem;
            _painter = item;
            cursorToggle.IsChecked = false;
            _selectedControlPointIndex = -1;
            _selectedShapes.Clear();
            RenderCanvas();
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

            if (_isEdit)
            {
                _isChangeProperty = true;

                foreach (var shape in _selectedShapes)
                {
                    shape.StrokeDashArray = (DoubleCollection)converter.ConvertFromString(item);
                }

                AddToUndo(_painters);
                RenderCanvas();
            }
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
            shapeGallery.SelectedItem = null;
            _painter = null;
            cursorToggle.IsChecked = true;
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

            if (Clipboard.GetImage() != null)
            {
                _painters.Add((IShape)new MyImage(Clipboard.GetImage()));
                Clipboard.Clear();
                AddToUndo(_painters);
                _selectedControlPointIndex = -1;
                RenderCanvas();
            }
            else if (_isEdit && _copyBuffer.Count > 0)
            {

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
                AddToUndo(_painters);
                RenderCanvas();

                _currentCursor = newCursor;
            }
            
        }

        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp | TIFF (*.tiff)|*.tiff"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;
                _painters.Add((IShape)new MyImage(new BitmapImage(new Uri(path, UriKind.Absolute))));
                AddToUndo(_painters);
                _selectedControlPointIndex = -1;
                RenderCanvas();
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
                AddToUndo(_painters);

                RenderCanvas();
            }
        }

        private void HandleUndoEvent()
        {

            if (_undoStack.Count > 1)
            {
                List<IShape> current = _undoStack.Pop(); // Remove current screen
                AddToRedo(current);
                List<IShape> old = _undoStack.Peek();
                _painters.Clear();
                _painters = old;
                _selectedShapes.Clear();
                RenderCanvas();
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
            if ((Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S))
            {
                Save_Click(sender, e);
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

        private void strokeThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isEdit && _selectedShapes.Count > 0)
            {
                _isChangeProperty = true;


                foreach (var shape in _selectedShapes)
                {
                    shape.StrokeThickness = strokeThicknessSlider.Value;
                }

                AddToUndo(_painters);

                RenderCanvas();
            }
        }

        private void strokeColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = strokeColorGallery.SelectedColor;

            if (_isEdit && selectedColor.HasValue && _selectedShapes.Count > 0)
            {
                _isChangeProperty = true;


                foreach (var shape in _selectedShapes)
                {
                    shape.StrokeColor = (Color)selectedColor;
                }

                AddToUndo(_painters);

                RenderCanvas();
            }
        }

        private void fillColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = fillColorGallery.SelectedColor;

            if (_isEdit && selectedColor.HasValue && _selectedShapes.Count > 0)
            {
                _isChangeProperty = true;


                foreach (var shape in _selectedShapes)
                {
                    shape.FillColor = (Color)selectedColor;
                }

                AddToUndo(_painters);

                RenderCanvas();
            }
        }
        private void AddBackground(string path)
        {
            BackgroundImagePath = path;

            ImageBrush brush = new()
            {
                ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute)),
                Stretch = Stretch.UniformToFill,
            };

            canvas.Background = brush;
        }
        private bool SaveImage(string filename, BitmapEncoder encoder, RenderTargetBitmap renderBitmap, string json)
        {
            try
            {
                BitmapMetadata metadata = new BitmapMetadata("png");
                metadata.SetQuery("/Text/JSON", json);

                encoder.Frames.Add(BitmapFrame.Create(renderBitmap, null, metadata, null));

                using (FileStream file = File.Create(filename))
                {
                    encoder.Save(file);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private BitmapEncoder CreateEncoder(string extension)
        {
            switch (extension)
            {
                case ".png":
                    return new PngBitmapEncoder();
                case ".jpeg":
                    return new JpegBitmapEncoder();
                case ".tiff":
                    return new TiffBitmapEncoder();
                case ".bmp":
                    return new BmpBitmapEncoder();
                default:
                    return null;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (savedFilePath == string.Empty || Path.GetExtension(savedFilePath) != ".png")
            {
                var dialog = new System.Windows.Forms.SaveFileDialog
                {
                    Filter = "PNG (*.png)|*.png"
                };
                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    savedFilePath = dialog.FileName;
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    MessageBox.Show("Failed to save image");
                    return;
                }
            }


            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
            };
            StringBuilder builder = new();

            var serializedShapeList = JsonConvert.SerializeObject(_painters, settings);
            builder.Append(serializedShapeList).Append('\n').Append($"{BackgroundImagePath}").Append('\n').Append($"{backgroundColor.SelectedColor.ToString()}");
            string content = builder.ToString();

            if (!string.IsNullOrEmpty(savedFilePath))
            {
                string extension = Path.GetExtension(savedFilePath).ToLower();

                RenderTargetBitmap renderBitmap = new(
                    (int)canvas.ActualWidth, (int)canvas.ActualHeight,
                    96d, 96d, PixelFormats.Pbgra32);

                canvas.Measure(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight));
                canvas.Arrange(new Rect(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight)));
                renderBitmap.Render(canvas);

                BitmapEncoder encoder = CreateEncoder(extension);

                if (encoder != null)
                {
                    bool isSaved = SaveImage(savedFilePath, encoder, renderBitmap, content);
                    IsSaved = isSaved;

                    if (isSaved)
                    {
                        MessageBox.Show("Saved Successfully");
                    }
                    else
                    {
                        MessageBox.Show("Failed to save image");
                    }
                }
            }
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (canvas.Children.Count != 0 && IsSaved == false)
            {
                var result = MessageBox.Show("Your current session will be lost?", "Do you want to save this working session?", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    SaveCurrentSession();
                }
                _painters.Clear();
                RenderCanvas();
            }
            _painters.Clear();
            RenderCanvas();
            OpenImageFile();
        }
        private void OpenImageFile()
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Image Files|*.png";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute));
                IsSaved = true;
                savedFilePath = path;

                // Read JSON from metadata
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    BitmapFrame frame = decoder.Frames[0];
                    BitmapMetadata metadata = (BitmapMetadata)frame.Metadata;

                    if (metadata != null)
                    {
                        string content = metadata.GetQuery("/Text/JSON") as string;

                        if (!string.IsNullOrEmpty(content))
                        {
                            string[] lines = content.Split('\n');
                            string json = lines[0];
                            string backgroundColorString = lines.Length > 2 ? lines[2] : "";

                            var settings = new JsonSerializerSettings()
                            {
                                TypeNameHandling = TypeNameHandling.Objects,
                            };

                            _painters = JsonConvert.DeserializeObject<List<IShape>>(json, settings);

                            if (!string.IsNullOrEmpty(backgroundColorString))
                            {
                                Color backgroundColorFile = (Color)ColorConverter.ConvertFromString(backgroundColorString);
                                backgroundColor.SelectedColor = backgroundColorFile;
                            }

                            RenderCanvas();
                        }
                    }
                }
            }
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
            };

            var serializedShapeList = JsonConvert.SerializeObject(_painters, settings);

            StringBuilder builder = new();
            builder.Append(serializedShapeList)
            .Append('\n')
            .Append($"{BackgroundImagePath}")
            .Append('\n')
            .Append($"{backgroundColor.SelectedColor.ToString()}");
            string content = builder.ToString();

            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json"
            };

            if (savedFilePath != string.Empty && Path.GetExtension(savedFilePath) == ".json")
            {
                File.WriteAllText(savedFilePath, content);
                MessageBox.Show("Saved Successfully");
            }
            else
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = dialog.FileName;
                    savedFilePath = path;
                    File.WriteAllText(path, content);
                    MessageBox.Show("Saved Successfully");
                    IsSaved = true;
                }
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "JSON (*.json)|*.json"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;
                savedFilePath = path;

                string content = File.ReadAllText(path);

                if (!string.IsNullOrEmpty(content))
                {
                    string[] lines = content.Split('\n');
                    string json = lines[0];
                    string backgroundColorString = lines.Length > 2 ? lines[2] : "";

                    var settings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                    };

                    _painters = JsonConvert.DeserializeObject<List<IShape>>(json, settings);

                    if (!string.IsNullOrEmpty(backgroundColorString))
                    {
                        Color backgroundColorFile = (Color)ColorConverter.ConvertFromString(backgroundColorString);
                        backgroundColor.SelectedColor = backgroundColorFile;
                    }

                    RenderCanvas();
                }
            }
        }

        private void SaveCurrentSession()
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            var serializedShapeList = JsonConvert.SerializeObject(_painters, settings);

            StringBuilder builder = new();
            builder.Append(serializedShapeList).Append('\n').Append($"{BackgroundImagePath}").Append('\n').Append($"{backgroundColor.SelectedColor.ToString()}");
            string content = builder.ToString();

            var savedialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json"
            };

            if (savedialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = savedialog.FileName;
                File.WriteAllText(path, content);
            }
        }
        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (canvas.Children.Count != 0 && IsSaved == false)
            {
                var result = MessageBox.Show("Your current session will be lost?", "Do you want to save this working session?", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    SaveCurrentSession();
                }
            }
            _painters.Clear();
            savedFilePath = string.Empty;
            RenderCanvas();
        }
        private void Closing_Click(object sender, CancelEventArgs e)
        {
            if (_painters.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save your work for the next session?", "Save work", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Color backgroundColor = ((SolidColorBrush)canvas.Background).Color;
                    SaveShapesToXml("data.xml", backgroundColor);
                }
                else
                {
                    File.WriteAllText("data.xml", "No");
                }
            }
        }
        private void SaveShapesToXml(string filename, Color backgroundColor)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects // Add this line
            };

            var serializedShapeList = JsonConvert.SerializeObject(_painters, settings);

            StringBuilder builder = new StringBuilder();
            builder.Append(serializedShapeList)
                   .Append('\n')
                   .Append($"{backgroundColor.ToString()}");

            File.WriteAllText(filename, builder.ToString());
        }
        private void LoadShapesFromXml(string filename)
        {
            if (File.Exists(filename))
            {
                string[] content = File.ReadAllLines(filename);
                if (content[0] != "No")
                {
                    var settings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects // Add this line
                    };

                    _painters = JsonConvert.DeserializeObject<List<IShape>>(content[0], settings);

                    if (content.Length > 1)
                    {
                        Color backgroundColorFile = (Color)ColorConverter.ConvertFromString(content[1]);
                        backgroundColor.SelectedColor = backgroundColorFile;
                    }

                    RenderCanvas();
                }
            }
        }
        private void cutButton_Click(object sender, RoutedEventArgs e)
        {
            HandleCutEvent();
        }
        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            HandleCopyEvent();
        }
        private void redoButton_Click(object sender, RoutedEventArgs e)
        {
            HandleRedoEvent();
        }
        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            HandleUndoEvent();
        }
    }
}
