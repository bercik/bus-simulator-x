using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class Pedestrian
    {
        static readonly float speed = 1.0f; // predkosc poruszania sie pieszych
        static Random rand = new Random(); // klasa losujaca

        public string name; // nazwa
        public Vector2 pos; // pozycja

        Direction d_direction;
        Direction direction // kierunek
        {
            set
            {
                d_direction = value;
                rotate = (int)d_direction * 90.0f;
            }
            get
            {
                return d_direction;
            }
        }
        public float rotate; // rotacja

        Location location; // polozenie (poziome lub pionowe)
        float min, max; // min i maks wspolrzedna (X lub Y zalezy od polozenia)

        public Pedestrian(Vector2 pos, int id, Location location, float min, float max)
        {
            this.pos = pos;
            this.name = "pedestrian" + id.ToString();

            this.location = location;
            this.min = min;
            this.max = max;

            RandomDirection();
        }

        private void RandomDirection()
        {
            if (rand.Next(1) == 0) // 0 - gora lub lewo, 1 - dol lub prawo
            {
                direction = (location == Location.horizontal) ? Direction.Left : Direction.Up;
            }
            else
            {
                direction = (location == Location.horizontal) ? Direction.Right : Direction.Down;
            }
        }
    }

    class PedestriansLogic
    {
        List<Sidewalk> sidewalks; // lista chodnikow

        Vector2 v_pedestrianSize;
        Vector2 pedestrianSize // rozmiar pieszego
        {
            set
            {
                v_pedestrianSize = value;
                pedestrianOrigin = v_pedestrianSize / 2;
            }
            get
            {
                return v_pedestrianSize;
            }
        }
        Vector2 pedestrianOrigin; // srodek pieszego

        int amountOfPedestrians; // ilosc pieszych

        List<Pedestrian> pedestrians; // lista pieszych

        private int maxPedestrians = 2; //maksymalna liczba pieszych na jednym kawalku chodnika
        private float spawnDistance = 1000; //maksymalna odleglosc spawnowania
        private float distanceToDelete = 1000; //piesi bedacy dalej niz podany dystans zostana usunieci

        Random rand; // klasa losujaca

        public PedestriansLogic()
        {
            rand = new Random();
            pedestrians = new List<Pedestrian>();
        }

        public void SetSidewalks(List<Sidewalk> sidewalks)
        {
            this.sidewalks = sidewalks;
        }

        public void SetProperties(Vector2 pedestrianSize, int amountOfPedestrians)
        {
            this.pedestrianSize = pedestrianSize;
            this.amountOfPedestrians = amountOfPedestrians;
        }

        public void Update(TimeSpan framesInterval)
        {
            SpawnPedestrian();
        }

        // pobiera liste pieszych do wyswietlenia
        public List<Object> GetPedestriansToShow()
        {
            List<Object> pedestriansToShow = new List<Object>();

            foreach (Pedestrian pedestrian in pedestrians)
            {
                pedestriansToShow.Add(new Object(pedestrian.name, Helper.MapPosToScreenPos(pedestrian.pos), pedestrianSize, pedestrian.rotate));
            }

            return pedestriansToShow;
        }

        // czy dany punkt znajduje sie w obszarze roboczym gry
        private bool IsPointInWorkArea(Vector2 point)
        {
            Vector2 leftUp = Helper.mapPos - Helper.workAreaOrigin;
            Vector2 rightDown = Helper.mapPos + Helper.workAreaOrigin;

            return (point.X < rightDown.X && point.X > leftUp.X && point.Y < rightDown.Y && point.Y > leftUp.Y);
        }

        private bool IsLineBeetweenWorkArea(Line line)
        {
            Vector2 leftUp = Helper.mapPos - Helper.workAreaOrigin;
            Vector2 rightDown = Helper.mapPos + Helper.workAreaOrigin;

            if (line.start.X < leftUp.X && line.end.X > rightDown.X) // linia pozioma
            {
                if (line.start.Y > leftUp.Y && line.start.Y < rightDown.Y)
                    return true;
            }
            else if (line.start.Y < leftUp.Y && line.end.Y > rightDown.Y) // linia pionowa
            {
                if (line.start.X > leftUp.X && line.start.X < rightDown.X)
                    return true;
            }

            return false;
        }

        // true: 1 lub 2 punkty znajduja sie w obszarze; false: 0, 3 lub 4 punkty znajduja sie w obszarze
        private bool IsSidewalkInWorkArea(Sidewalk sidewalk)
        {
            int numberOfPointsInWorkArea = 0; // liczba punktow w obszarze

            Vector2[] points = new Vector2[4]; // punkty prostokata chodnika
            Vector2 sidewalkOrigin = (sidewalk.location == Location.horizontal) ? // srodek chodnika wg. konwencjii
                    new Vector2(sidewalk.origin.Y, sidewalk.origin.X) : sidewalk.origin;

            points[0] = new Vector2(sidewalk.pos.X - sidewalkOrigin.X, sidewalk.pos.Y - sidewalkOrigin.Y);
            points[1] = new Vector2(sidewalk.pos.X + sidewalkOrigin.X, sidewalk.pos.Y - sidewalkOrigin.Y);
            points[2] = new Vector2(sidewalk.pos.X + sidewalkOrigin.X, sidewalk.pos.Y + sidewalkOrigin.Y);
            points[3] = new Vector2(sidewalk.pos.X - sidewalkOrigin.X, sidewalk.pos.Y + sidewalkOrigin.Y);

            for (int i = 0; i < 4; ++i)
            {
                if (IsPointInWorkArea(points[i]))
                    ++numberOfPointsInWorkArea;
            }

            if (numberOfPointsInWorkArea == 1 || numberOfPointsInWorkArea == 2)
            {
                return true;
            }
            else
            {
                // sprawdzamy czy chodnik nie znajduje sie pomiedzy obszarem roboczym
                if (sidewalk.location == Location.horizontal)
                {
                    if (IsLineBeetweenWorkArea(new Line(points[0], points[1]))
                            || IsLineBeetweenWorkArea(new Line(points[3], points[2])))
                        return true;
                }
                else
                {
                    if (IsLineBeetweenWorkArea(new Line(points[0], points[3]))
                            || IsLineBeetweenWorkArea(new Line(points[1], points[2])))
                        return true;
                }
            }

            return false;
        }

        private void GeneratePedestrian(Sidewalk sidewalk)
        {
            Vector2 pos = new Vector2();

            if (sidewalk.location == Location.horizontal)
            {
                pos.Y = rand.Next((int)pedestrianOrigin.X, (int)(sidewalk.size.X - pedestrianOrigin.X))
                        + sidewalk.pos.Y - sidewalk.origin.X;
                pos.X = rand.Next((int)pedestrianOrigin.Y, (int)(sidewalk.size.Y - pedestrianOrigin.Y)) 
                        + sidewalk.pos.X - sidewalk.origin.Y;
            }
            else
            {
                pos.X = rand.Next((int)pedestrianOrigin.X, (int)(sidewalk.size.X - pedestrianOrigin.X)) 
                        + sidewalk.pos.X - sidewalk.origin.X;
                pos.Y = rand.Next((int)pedestrianOrigin.Y, (int)(sidewalk.size.Y - pedestrianOrigin.Y))
                        + sidewalk.pos.Y - sidewalk.origin.Y;
            }

            int id = rand.Next(0, amountOfPedestrians);

            float min = (sidewalk.location == Location.horizontal) ? sidewalk.pos.X - sidewalk.origin.Y
                    : sidewalk.pos.Y - sidewalk.origin.Y;
            float max = (sidewalk.location == Location.horizontal) ? sidewalk.pos.X + sidewalk.origin.Y
                    : sidewalk.pos.Y + sidewalk.origin.Y;

            pedestrians.Add(new Pedestrian(pos, id, sidewalk.location, min, max));
        }

        private void SpawnPedestrian()
        {
            List<Sidewalk> spawnSidewalks = new List<Sidewalk>();

            foreach (Sidewalk sidewalk in sidewalks)
            {
                if (IsSidewalkInWorkArea(sidewalk))
                {
                    GeneratePedestrian(sidewalk);
                }
            }
        }
    }
}
