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

        public bool IsInside(Vector2[] points) // czy punkty sa w srodku kwadratu
        {
            foreach (Vector2 point in points)
            {
                if (Helper.IsInside(point, this))
                    return true;
            }

            return false;
        }

        public bool IsInside(MyRectangle otherRectangle)
        {
            Vector2[] points = new Vector2[] { otherRectangle.point1, otherRectangle.point2, otherRectangle.point3, otherRectangle.point4 };
            
            return IsInside(points);
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
        public static Vector2 screenSize { get; private set; } // wielkosc ekranu
        public static Vector2 screenOrigin { get; private set; } // srodek ekranu

        public static Vector2 workAreaSize { get; private set; } // wielkosc robocza (wielkosc ekranu * skalowanie)
        public static Vector2 workAreaOrigin { get; private set; } // srodek roboczego ekranu
        private static float scale; // skalowanie

        private readonly static float maxScale = 1.5f; // maksymalna skala mapy
        private readonly static float minScale = 0.5f; // minimalna skala mapy
        public static Vector2 maxWorkAreaSize { get; private set; } // maksymalny rozmiar ekranu roboczego
        public static Vector2 minWorkAreaSize { get; private set; } // minimalny rozmiar ekranu roboczego

        public static Vector2 mapPos; // pozycja mapy

        // wywolac ZAWSZE po zmianie screenSize lub scale
        private static void CalculateWorkArea()
        {
            workAreaSize = screenSize * scale;
            workAreaOrigin = workAreaSize / 2;
        }

        public static void SetScale(float f_scale)
        {
            if (f_scale > minScale && f_scale < maxScale)
            {
                scale = f_scale;

                CalculateWorkArea();
            }
        }

        public static float GetScale()
        {
            return scale;
        }

        public static void SetScreenSize(float width, float height)
        {
            screenSize = new Vector2(width, height);
            maxWorkAreaSize = screenSize * maxScale;
            minWorkAreaSize = screenSize * minScale;
            screenOrigin = screenSize / 2;

            CalculateWorkArea();
        }

        public static Vector2 MapPosToScreenPos(Vector2 pos)
        {
            return pos - (mapPos - screenSize / 2);
        }

        /// <summary>
        /// Oblicza przeskalowany prostokąt służący do wyświetlania
        /// </summary>
        /// <param name="pos">Oryginalna pozycja w jednostkach ekranowych</param>
        /// <param name="size">Oryginalny rozmiar</param>
        /// <returns></returns>
        public static Rectangle CalculateScaleRectangle(Vector2 pos, Vector2 size)
        {
            Rectangle rect = new Rectangle();

            Vector2 v_mapPos = MapPosToScreenPos(mapPos);

            int x = (int)((pos.X - v_mapPos.X) / scale);
            rect.X = (int)(v_mapPos.X + x);
            int y = (int)((pos.Y - v_mapPos.Y) / scale);
            rect.Y = (int)(v_mapPos.Y + y);

            rect.Width = (int)(size.X / scale);
            rect.Height = (int)(size.Y / scale);

            return rect;
        }

        /// <summary>
        /// Oblicza przeskalowany prostokąt służący do wyświetlania
        /// </summary>
        /// <param name="o">obiekt, który chcemy przeskalować</param>
        /// <returns></returns>
        public static Rectangle CalculateScaleRectangle(Object o)
        {
            return CalculateScaleRectangle(o.pos, o.size);
        }

        /// <summary>
        /// Oblicza pozycję punktu po przeskalowaniu
        /// </summary>
        /// <param name="position">Oryginalna pozycja w jednostkach ekranowych</param>
        /// <returns></returns>
        public static Vector2 CalculateScalePoint(Vector2 position)
        {
            Vector2 newPos = new Vector2();

            Vector2 v_mapPos = MapPosToScreenPos(mapPos);

            newPos = v_mapPos + ((position - v_mapPos) / scale);

            return newPos;
        }

        /// <summary>
        /// Check if the point is inside the rectangle. This function is not working properly.
        /// </summary>
        /// <param name="point">Point.</param>
        /// <param name="givenRrectangle">Rectangle. Points should be given clockwise.</param>
        public static bool IsInside(Vector2 point, MyRectangle givenRrectangle)
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

        public static float ComputeRotationX(Vector2 point, Vector2 center, float angel)
        {
            return (float)(((point.X - center.X) * Math.Cos(MathHelper.ToRadians(angel))) - ((point.Y - center.Y) * Math.Sin(MathHelper.ToRadians(angel))) + center.X);
        }

        public static float ComputeRotationY(Vector2 point, Vector2 center, float angel)
        {
            return (float)(((point.X - center.X) * Math.Sin(MathHelper.ToRadians(angel))) + ((point.Y - center.Y) * Math.Cos(MathHelper.ToRadians(angel))) + center.Y);
        }

        public static Vector2 ComputeRotation(Vector2 point, Vector2 center, float angel)
        {
            return new Vector2(ComputeRotationX(point, center, angel), ComputeRotationY(point, center, angel));
        }

        // prostokat przed rotacja, srodek bezwzgledny (wzgledem lewego gornego rogu mapy), kat obrotu
        public static MyRectangle ComputeRectangleOnRotation(Rectangle myRectangle, Vector2 origin, float angel)
        {
            MyRectangle rotateRectangle = new MyRectangle();

            rotateRectangle.point1 = ComputeRotation(new Vector2(myRectangle.X, myRectangle.Y), origin, angel);
            rotateRectangle.point2 = ComputeRotation(new Vector2(myRectangle.X + myRectangle.Width, myRectangle.Y), origin, angel);
            rotateRectangle.point3 = ComputeRotation(new Vector2(myRectangle.X + myRectangle.Width, myRectangle.Y + myRectangle.Height), origin, angel);
            rotateRectangle.point4 = ComputeRotation(new Vector2(myRectangle.X, myRectangle.Y + myRectangle.Height), origin, angel);

            return rotateRectangle;
        }

        /// <summary>
        /// Calculate the distance beetween two points.
        /// </summary>
        public static float CalculateDistance(Vector2 point1, Vector2 point2)
        {
            return (float)Math.Round(Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2)), 2);
        }

        /// <summary>
        /// Calculate direction beetween two points.
        /// </summary>
        public static float CalculateDirection(Vector2 start, Vector2 end)
        {
            float direction = MathHelper.ToDegrees((float)Math.Atan2(end.X - start.X, start.Y - end.Y));
            if (direction < 0)
                direction += 360;

            return direction;
        }
    }
}