using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Contract;

public class RotatePoint : ControlPoint
{
    public RotatePoint(Point position, Point center, IControlPointStrategy strategy) : base(position, center,strategy) 
    {
    }
    public override string Type => "rotate";
    
}

public class DiagPoint : ControlPoint
{
    public DiagPoint(Point position, Point center,IControlPointStrategy strategy) : base(position, center, strategy)
    {

    }
    public override string Type => "diag";
}

public class OneSidePoint : ControlPoint
{
    public OneSidePoint(Point position, Point center, IControlPointStrategy strategy) : base(position, center, strategy) { }
    public override string Type => "diag";

    public override string getEdge(double angle)
    {
        string[] edge = ["top", "right", "bottom", "left"];
        int index = 0;
        if (CenterPoint.X == Position.X)
            if (CenterPoint.Y > Position.Y)
                index = 0;
            else
                index = 2;
        else
            if (CenterPoint.Y == Position.Y)
            if (CenterPoint.X > Position.X)
                index = 3;
            else
                index = 1;

        return edge[index];
    }

}

public class LinePoint : ControlPoint
{
    public LinePoint() {}

    public LinePoint(Point position, Point center, IControlPointStrategy strategy) : base(position, center, strategy) { }
    public override string Type => "linePoint";
    public string Edge { get; set; }
    public override string getEdge(double angle)
    {
        return Edge;
    }
}
