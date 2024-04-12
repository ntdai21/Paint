using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shapes;


namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool _isDrawing = false;
        bool _isEdit = true;
        Point _start;
        Point _end;


        List<UIElement> _list = new List<UIElement>();
        List<IShape> shapes = new List<IShape>();
        UIElement _lastElement = null;

        List<IShape> _painters = new List<IShape>();
        IShape _painter = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory; 
            var fis = new DirectoryInfo(folder).GetFiles("*.dll");
            
            foreach(var fi in fis)
            {
                var assembly = Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass && (typeof(IShape).IsAssignableFrom(type))) {
                        shapes.Add((IShape) Activator.CreateInstance(type));
                    }
                }
            }

            foreach (IShape item in shapes)
            {
                var btn = new Button()
                {
                    Width = 100,
                    Height = 32,
                    Content = item.Name,
                    Tag = item
                };
                btn.Click += ControllButton_Click;
                ActionButtons.Children.Add(btn);
            }

        }

        private void ControllButton_Click(object sender, RoutedEventArgs e)
        {
            IShape item = (IShape)(sender as Button).Tag;
            _painter = item;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _start = e.GetPosition(myCanvas);

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(myCanvas);

                myCanvas.Children.Clear();

                foreach(IShape i in _painters)
                {
                    myCanvas.Children.Add(i.Convert());
                }

                _painter.AddFirst(_start);
                _painter.AddSecond(_end);
                myCanvas.Children.Add(_painter.Convert());
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            _painters.Add((IShape)_painter.Clone());

            if(_isEdit)
            {
                Point current=e.GetPosition(myCanvas);


            }
        }
        
    }
}