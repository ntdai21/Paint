using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Controls;

namespace Contract
{
    public interface IControlPointStrategy
    {
        void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas);
    }

    public class RotateStrategy : IControlPointStrategy
    {
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas) 
        { 
            Point centerPoint=shape.GetCenterPoint();
            Point cursor = e.GetPosition(canvas);
            Vector2 vector=new Vector2((float)(cursor.X-centerPoint.X),(float)(cursor.Y-centerPoint.Y));
            double angle = (MathF.Atan2(vector.Y, vector.X) * (180f / Math.PI) + 450f) % 360f;
            shape.RotateAngle=angle;
        }

    }

    public class MoveStrategy : IControlPointStrategy
    {
        double _dx = 0, _dy = 0;
        public MoveStrategy(double dx, double dy)
        {
            _dx = dx;
            _dy = dy;
        }
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            Point leftTop = shape.LeftTop;
            Point rightBottom=shape.RightBottom;
            leftTop.X= leftTop.X + _dx;
            leftTop.Y = leftTop.Y + _dy;
            rightBottom.X = rightBottom.X + _dx;
            rightBottom.Y = rightBottom.Y + _dy;
            shape.LeftTop= leftTop;
            shape.RightBottom= rightBottom;
        }
    }

    public class DiagControlPointStrategy : IControlPointStrategy
    {
        private string _type;
        public DiagControlPointStrategy(string type)
        {
            _type = type;
        }

        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            IControlPointStrategy handler = DiagControlPointStrategyFactory.Instance.CreateStrategy(_type);
            handler.HandleMouseMove(ref shape,e,canvas);
        }
    }


    public class DiagControlPointStrategyFactory
    {
        private static DiagControlPointStrategyFactory instance;
        public static DiagControlPointStrategyFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DiagControlPointStrategyFactory();
                }

                return instance;
            }
        }

        public IControlPointStrategy CreateStrategy(string type)
        {
            switch (type)
            {
                case "topleft":
                    return new TopLeftDiagControlPointStrategy();
                case "topright":
                    return new TopRightDiagControlPointStrategy();
                case "bottomright":
                    return new BottomRightDiagControlPointStrategy();
                case "bottomleft":
                    return new BottomLeftDiagControlPointStrategy();
                case "right":
                    return new RightDiagControlPointStrategy();
                case "left":
                    return new LeftDiagControlPointStrategy();
                case "top":
                    return new TopDiagControlPointStrategy();
                case "bottom":
                    return new BottomDiagControlPointStrategy();
            }

            return null;
        }
    }


    public class TopLeftDiagControlPointStrategy : IControlPointStrategy
    {
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            Point cursor=e.GetPosition(canvas); 
            Point centerPoint=shape.GetCenterPoint();
            var vector = new Vector2((float)(cursor.X - centerPoint.X), (float)(cursor.Y - centerPoint.Y));
            float a = Util.Instance.GetAlphaAngleRadian(shape.RotateAngle);
            Point newPoint = new Point((vector.X * (float)Math.Cos(a) + vector.Y * (float)Math.Sin(a)) + centerPoint.X, (-vector.X * Math.Sin(a) + vector.Y * Math.Cos(a) + centerPoint.Y));

            shape.LeftTop = newPoint;
            Point newCenterPoint=shape.GetCenterPoint();
            (double txx, double tyy) = Util.Instance.GetCenterPointTranslation(centerPoint, newCenterPoint, shape);

            Point leftTop = shape.LeftTop;
            Point rightBottom=shape.RightBottom;
            leftTop.X += txx;
            leftTop.Y += tyy;
            rightBottom.X += txx;
            rightBottom.Y += tyy;

            shape.LeftTop=leftTop;
            shape.RightBottom=rightBottom;
        }
    }

    public class TopRightDiagControlPointStrategy : IControlPointStrategy
    {
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            Point cursor = e.GetPosition(canvas);
            Point centerPoint = shape.GetCenterPoint();
            var vector = new Vector2((float)(cursor.X - centerPoint.X), (float)(cursor.Y - centerPoint.Y));
            float a = Util.Instance.GetAlphaAngleRadian(shape.RotateAngle);
            Point newPoint = new Point((vector.X * (float)Math.Cos(a) + vector.Y * (float)Math.Sin(a)) + centerPoint.X, (-vector.X * Math.Sin(a) + vector.Y * Math.Cos(a) + centerPoint.Y));

            Point rightBottom= shape.RightBottom;
            Point leftTop = shape.LeftTop;
            rightBottom.X = newPoint.X;
            leftTop.Y=newPoint.Y;
            shape.LeftTop=leftTop;
            shape.RightBottom=rightBottom;


            Point newCenterPoint = shape.GetCenterPoint();
            (double txx, double tyy) = Util.Instance.GetCenterPointTranslation(centerPoint, newCenterPoint, shape);

            leftTop = shape.LeftTop;
            rightBottom = shape.RightBottom;
            leftTop.X += txx;
            leftTop.Y += tyy;
            rightBottom.X += txx;
            rightBottom.Y += tyy;

            shape.LeftTop = leftTop;
            shape.RightBottom = rightBottom;
        }
    }

    public class BottomRightDiagControlPointStrategy : IControlPointStrategy
    {
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            Point cursor = e.GetPosition(canvas);
            Point centerPoint = shape.GetCenterPoint();
            var vector = new Vector2((float)(cursor.X - centerPoint.X), (float)(cursor.Y - centerPoint.Y));
            float a = Util.Instance.GetAlphaAngleRadian(shape.RotateAngle);
            Point newPoint = new Point((vector.X * (float)Math.Cos(a) + vector.Y * (float)Math.Sin(a)) + centerPoint.X, (-vector.X * Math.Sin(a) + vector.Y * Math.Cos(a) + centerPoint.Y));

            shape.RightBottom = newPoint;

            Point newCenterPoint = shape.GetCenterPoint();
            (double txx, double tyy) = Util.Instance.GetCenterPointTranslation(centerPoint, newCenterPoint, shape);

            Point leftTop = shape.LeftTop;
            Point rightBottom = shape.RightBottom;
            leftTop.X += txx;
            leftTop.Y += tyy;
            rightBottom.X += txx;
            rightBottom.Y += tyy;

            shape.LeftTop = leftTop;
            shape.RightBottom = rightBottom;
        }
    }

    public class BottomLeftDiagControlPointStrategy : IControlPointStrategy
    {
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            Point cursor = e.GetPosition(canvas);
            Point centerPoint = shape.GetCenterPoint();
            var vector = new Vector2((float)(cursor.X - centerPoint.X), (float)(cursor.Y - centerPoint.Y));
            float a = Util.Instance.GetAlphaAngleRadian(shape.RotateAngle);
            Point newPoint = new Point((vector.X * (float)Math.Cos(a) + vector.Y * (float)Math.Sin(a)) + centerPoint.X, (-vector.X * Math.Sin(a) + vector.Y * Math.Cos(a) + centerPoint.Y));

            Point leftTop=shape.LeftTop;
            Point rightBottom = shape.RightBottom;

            leftTop.X = newPoint.X;
            rightBottom.Y=newPoint.Y;
            shape.LeftTop = leftTop;
            shape.RightBottom=rightBottom;


            Point newCenterPoint = shape.GetCenterPoint();
            (double txx, double tyy) = Util.Instance.GetCenterPointTranslation(centerPoint, newCenterPoint, shape);

            leftTop = shape.LeftTop;
            rightBottom = shape.RightBottom;
            leftTop.X += txx;
            leftTop.Y += tyy;
            rightBottom.X += txx;
            rightBottom.Y += tyy;

            shape.LeftTop = leftTop;
            shape.RightBottom = rightBottom;
        }
    }

    public class RightDiagControlPointStrategy : IControlPointStrategy
    {
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas  )
        {
            Point cursor = e.GetPosition(canvas);
            Point centerPoint = shape.GetCenterPoint();
            var vector = new Vector2((float)(cursor.X - centerPoint.X), (float)(cursor.Y - centerPoint.Y));
            float a = Util.Instance.GetAlphaAngleRadian(shape.RotateAngle);
            Point newPoint = new Point((vector.X * (float)Math.Cos(a) + vector.Y * (float)Math.Sin(a)) + centerPoint.X, (-vector.X * Math.Sin(a) + vector.Y * Math.Cos(a) + centerPoint.Y));

            Point rightBottom = shape.RightBottom;
            rightBottom.X = newPoint.X;
            shape.RightBottom= rightBottom;

            Point newCenterPoint = shape.GetCenterPoint();
            (double txx, double tyy) = Util.Instance.GetCenterPointTranslation(centerPoint, newCenterPoint, shape);

            Point leftTop = shape.LeftTop;
            rightBottom = shape.RightBottom;
            leftTop.X += txx;
            leftTop.Y += tyy;
            rightBottom.X += txx;
            rightBottom.Y += tyy;

            shape.LeftTop = leftTop;
            shape.RightBottom = rightBottom;
        }
    }

    public class LeftDiagControlPointStrategy : IControlPointStrategy
    {
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            Point cursor = e.GetPosition(canvas);
            Point centerPoint = shape.GetCenterPoint();
            var vector = new Vector2((float)(cursor.X - centerPoint.X), (float)(cursor.Y - centerPoint.Y));
            float a = Util.Instance.GetAlphaAngleRadian(shape.RotateAngle);
            Point newPoint = new Point((vector.X * (float)Math.Cos(a) + vector.Y * (float)Math.Sin(a)) + centerPoint.X, (-vector.X * Math.Sin(a) + vector.Y * Math.Cos(a) + centerPoint.Y));

            Point leftTop = shape.LeftTop;
            leftTop.X = newPoint.X;
            shape.LeftTop = leftTop;

            Point newCenterPoint = shape.GetCenterPoint();
            (double txx, double tyy) = Util.Instance.GetCenterPointTranslation(centerPoint, newCenterPoint, shape);

            leftTop = shape.LeftTop;
            Point rightBottom = shape.RightBottom;
            leftTop.X += txx;
            leftTop.Y += tyy;
            rightBottom.X += txx;
            rightBottom.Y += tyy;

            shape.LeftTop = leftTop;
            shape.RightBottom = rightBottom;
        }
    }

    public class TopDiagControlPointStrategy : IControlPointStrategy
    {
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            Point cursor = e.GetPosition(canvas);
            Point centerPoint = shape.GetCenterPoint();
            var vector = new Vector2((float)(cursor.X - centerPoint.X), (float)(cursor.Y - centerPoint.Y));
            float a = Util.Instance.GetAlphaAngleRadian(shape.RotateAngle);
            Point newPoint = new Point((vector.X * (float)Math.Cos(a) + vector.Y * (float)Math.Sin(a)) + centerPoint.X, (-vector.X * Math.Sin(a) + vector.Y * Math.Cos(a) + centerPoint.Y));

            Point leftTop = shape.LeftTop;
            leftTop.Y = newPoint.Y;
            shape.LeftTop = leftTop;

            Point newCenterPoint = shape.GetCenterPoint();
            (double txx, double tyy) = Util.Instance.GetCenterPointTranslation(centerPoint, newCenterPoint, shape);

            leftTop = shape.LeftTop;
            Point rightBottom = shape.RightBottom;
            leftTop.X += txx;
            leftTop.Y += tyy;
            rightBottom.X += txx;
            rightBottom.Y += tyy;

            shape.LeftTop = leftTop;
            shape.RightBottom = rightBottom;
        }
    }

    public class BottomDiagControlPointStrategy : IControlPointStrategy
    {
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            Point cursor = e.GetPosition(canvas);
            Point centerPoint = shape.GetCenterPoint();
            var vector = new Vector2((float)(cursor.X - centerPoint.X), (float)(cursor.Y - centerPoint.Y));
            float a = Util.Instance.GetAlphaAngleRadian(shape.RotateAngle);
            Point newPoint = new Point((vector.X * (float)Math.Cos(a) + vector.Y * (float)Math.Sin(a)) + centerPoint.X, (-vector.X * Math.Sin(a) + vector.Y * Math.Cos(a) + centerPoint.Y));

            Point rightBottom = shape.RightBottom;
            rightBottom.Y = newPoint.Y;
            shape.RightBottom = rightBottom;

            Point newCenterPoint = shape.GetCenterPoint();
            (double txx, double tyy) = Util.Instance.GetCenterPointTranslation(centerPoint, newCenterPoint, shape);

            Point leftTop = shape.LeftTop;
            rightBottom = shape.RightBottom;
            leftTop.X += txx;
            leftTop.Y += tyy;
            rightBottom.X += txx;
            rightBottom.Y += tyy;

            shape.LeftTop = leftTop;
            shape.RightBottom = rightBottom;
        }
    }

    public class LineControlPointStrategy : IControlPointStrategy
    {
        Point _currentPosition;
        public LineControlPointStrategy(Point currentPosition) 
        {
            _currentPosition = currentPosition;
        }
        public void HandleMouseMove(ref BorderShape shape, MouseEventArgs e, Canvas canvas)
        {
            if(_currentPosition==shape.LeftTop)
            {
                shape.LeftTop = Mouse.GetPosition(canvas);
            }
            else if (_currentPosition==shape.RightBottom)
            {
                shape.RightBottom= Mouse.GetPosition(canvas);
            }
        }
    }
}
