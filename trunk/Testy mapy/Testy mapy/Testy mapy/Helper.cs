using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    enum Location { vertical, horizontal } // polozenie (pionowe lub poziome)

    enum Position { up, downRight, right, upRight, down, downLeft, left, upLeft } // nie zmieniac kolejnosci

    struct MyRectangle
    {
        public Vector2 point1;
        public Vector2 point2;
        public Vector2 point3;
        public Vector2 point4;

        public MyRectangle(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
        {
            this.point1 = point1;
            this.point2 = point2;
            this.point3 = point3;
            this.point4 = point4;
        }

        public bool IsInside(Vector2 point) // czy punkt jest w środku kwadratu
        {
            return Helper.IsInside(point, this);
        }
    }

    struct Size
    {
        public int Width;
        public int Height;

        public Size(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
        }
    }

    struct Line
    {
        public Vector2 start; //punkt poczatkowy
        public Vector2 end; //punkt koncowy

        public Line(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }
    }

    static class Helper
    {
        public static Vector2 screenSize;
        public static Vector2 mapPos;

        public static void SetScreenSize(float width, float height)
        {
            screenSize = new Vector2(width, height);
        }

        public static Vector2 MapPosToScreenPos(Vector2 pos)
        {
            return pos - (mapPos - screenSize / 2);
        }

        public static bool IsInside(Vector2 point, MyRectangle givenRrectangle) //podaj punkt i kwadrat 
        {
            /*Line line1 = new Line(rectangle.point1, rectangle.point2);
            Line line2 = new Line(rectangle.point2, rectangle.point3);
            Line line3 = new Line(rectangle.point3, rectangle.point4);
            Line line4 = new Line(rectangle.point4, rectangle.point1);

            if (CheckOneLine(point, line1) && CheckOneLine(point, line2) && CheckOneLine(point, line3) && CheckOneLine(point, line4))
                return true;
            else
                return false;*/

            Vector2 p1, p2;

            Vector2[] rectangle = new Vector2[4] { givenRrectangle.point1, givenRrectangle.point2, givenRrectangle.point3, givenRrectangle.point4 };

            bool inside = false;

            if (rectangle.Length < 3)
                return inside;

            Vector2 oldPoint = new Vector2(rectangle[rectangle.Length - 1].X, rectangle[rectangle.Length - 1].Y);

            for (int i = 0; i < rectangle.Length; i++)
            {
                Vector2 newPoint = new Vector2(rectangle[i].X, rectangle[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < point.X) == (point.X <= oldPoint.X) && ((long)point.Y - (long)p1.Y) * (long)(p2.X - p1.X) < ((long)p2.Y - (long)p1.Y) * (long)(point.X - p1.X))
                    inside = !inside;

                oldPoint = newPoint;
            }
            return inside;
        }

        private static bool CheckOneLine(Vector2 point, Line line) //sprawdza jeden bok prostokata dla IsInside
        {
            //A * x + B * y + C = 0
            float A = -(line.end.Y - line.start.Y);
            float B = (line.end.X - line.start.X);
            float C = -(A * line.start.X + B * line.start.Y);

            float D = A * point.X + B * point.Y + C;

            if (D > 0)
                return true;
            else
                return false;
        }
    }
}