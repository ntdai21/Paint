
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

using System.Xml.Linq;
namespace Contract;
public class ControlPoint
{
    protected const int SIZE = 8;
    public Point Position { get; set; }=new Point();
    public Point CenterPoint { get; set; }=new Point();
    virtual public string Type { get; set; } = "rotate";
    public ControlPoint() { }
    public IControlPointStrategy controlPointStrategy { get; set; }
    public ControlPoint(Point position, Point center, IControlPointStrategy strategy) 
    {
        this.Position = position;
        this.CenterPoint = center;
        controlPointStrategy = strategy;
    }

    virtual public UIElement Render(double angle, Point center)
    {
        UIElement point = new Ellipse()
        {
            Width = SIZE,
            Height = SIZE,
            Fill = Brushes.White,
            Stroke = Brushes.Black,
            StrokeThickness = 3,
        };

        Point noRotatePos = new Point(Position.X, Position.Y);
        Point centerPos = new Point (CenterPoint.X, CenterPoint.Y);

        Point transform=Helper.Instance.Rotate(noRotatePos,angle,centerPos);

        Canvas.SetLeft(point, transform.X-SIZE/2);
        Canvas.SetTop(point, transform.Y - SIZE / 2);

        return point;
    }

    virtual public bool IsHovering(double angle, double x, double y)
    {
        Point noRotatePos = new Point(Position.X, Position.Y);
        Point centerPos = new Point(CenterPoint.X, CenterPoint.Y);

        Point realPos = Helper.Instance.Rotate(noRotatePos, angle, centerPos);

        return Util.Instance.IsBetween(x, realPos.X + 15, realPos.X - 15)
            && Util.Instance.IsBetween(y, realPos.Y + 15, realPos.Y - 15);
    }

    virtual public string getEdge(double angle)
    {
        string[] edge = { "topleft", "topright", "bottomright", "bottomleft" };
        int index;
        if (Position.X > CenterPoint.X)
            if (Position.Y > CenterPoint.Y)
                index = 2;
            else
                index = 1;
        else
            if (Position.Y > CenterPoint.Y)
            index = 3;
        else
            index = 0;

        return edge[index];
    }

    virtual public bool IsBeingChosen(string currentChosenType, string currentChosenEdge, double rotateAngle)
    {
        if (currentChosenType == "rotate" || currentChosenType == "move")
            return currentChosenType == Type;

        return currentChosenType == Type && currentChosenEdge == getEdge(rotateAngle);
    }
}
