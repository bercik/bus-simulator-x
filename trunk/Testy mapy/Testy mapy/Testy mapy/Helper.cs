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
    }
}