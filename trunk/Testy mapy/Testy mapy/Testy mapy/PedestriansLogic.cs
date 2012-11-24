using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class SidewalkPedestrian
    {
        public Sidewalk sidewalk { get; private set; } // chodnik
        List<Pedestrian> pedestrians; // piesi przypisani do chodnika
        List<Pedestrian> diedPedestrians; // zabici piesi

        public SidewalkPedestrian(Sidewalk sidewalk)
        {
            this.sidewalk = sidewalk;
            this.pedestrians = new List<Pedestrian>();
            this.diedPedestrians = new List<Pedestrian>();
        }

        public void AddPedestrian(Pedestrian pedestrian)
        {
            pedestrians.Add(pedestrian);
        }

        public List<Pedestrian> GetAlivePedestrians()
        {
            return pedestrians;
        }

        public List<Pedestrian> GetAllPedestrians()
        {
            List<Pedestrian> pedestriansToGive = new List<Pedestrian>();

            pedestriansToGive.AddRange(diedPedestrians);
            pedestriansToGive.AddRange(pedestrians);

            return pedestriansToGive;
        }

        public void DiePedestrian(Pedestrian pedestrian)
        {
            pedestrians.Remove(pedestrian);
            diedPedestrians.Add(pedestrian);
        }
    }

    class Pedestrian
    {
        bool invertRotate; // czy odwrocic rotacje
        static readonly float rotateSpeed = 0.05f; // predkosc rotacji
        float speed; // predkosc poruszania sie pieszych
        bool invertSpeed; // czy odwrocic predkosc

        bool collision; // czy wystapila kolizja
        bool makeTurn; // czy wykonac kolejny obrot
        bool uniqueCollisionWithBus = false; // czy wlasnie unikamy kolizji z autobusem i zmieniamy kierunek poruszania sie
        static readonly float diffrenceRotateOnTurn = 80.0f; // maksymalna roznica rotacji stopni przy obracaniu sie

        float time; // jaki czas odczekano
        static readonly float waitingTime = 2.6f; // jaki czas nalezy odczekac po ogladnieciu sie (w sekundach)

        static Random rand = new Random(); // klasa losujaca

        public string name; // nazwa

        float pos_x, pos_y; // pozycja_y: wzdluz chodnika, pozycja_x: wszerz chodnika
        public Vector2 pos // pozycja
        {
            get
            {
                if (location == Location.horizontal)
                    return new Vector2(pos_y, pos_x);
                else
                    return new Vector2(pos_x, pos_y);
            }
        }

        float destinationPos; // pozycja przeznaczenia
        float destinationRotate; // rotacja przeznaczenia

        Direction d_direction;
        Direction direction // kierunek
        {
            set
            {
                d_direction = value;
                destinationRotate = (int)d_direction * 90.0f;

                invertRotate = Convert.ToBoolean(rand.Next(2));
            }
            get
            {
                return d_direction;
            }
        }
        public float rotate { get; private set; } // rotacja
        Vector2 origin; // srodek (polowa rozmiaru)
        Vector2 size; // rozmiar

        Location location; // polozenie (poziome lub pionowe)
        float min, max; // min i maks wspolrzedna (X lub Y zalezy od polozenia)

        public Pedestrian(Vector2 pos, Vector2 size, int id, Location location, float min, float max)
        {
            this.collision = false; // na poczatek pieszy jest zywy :)
            this.time = waitingTime; // dzieki temu od razu po utworzeniu pieszy porusza sie

            this.name = "pedestrian" + id.ToString();
            this.size = size;
            this.origin = size / 2;

            this.location = location;

            pos_x = (location == Location.horizontal) ? pos.Y : pos.X;
            pos_y = (location == Location.horizontal) ? pos.X : pos.Y;

            this.min = min;
            this.max = max;

            RandomDestination();
            rotate = destinationRotate;
        }

        public void Update(TimeSpan framesInterval)
        {
            if (!collision)
            {
                if (time > waitingTime)
                {
                    if (!CheckIsRotate(framesInterval))
                    {
                        rotate += (float)(((invertRotate) ? -rotateSpeed : rotateSpeed) * framesInterval.TotalMilliseconds);

                        if (rotate > 360.0f)
                            rotate -= 360.0f;
                        else if (rotate < 0.0f)
                            rotate += 360.0f;
                    }
                    else
                    {
                        uniqueCollisionWithBus = false;

                        if (!makeTurn)
                        {
                            pos_y += (float)(((invertSpeed) ? -speed : speed) * framesInterval.TotalMilliseconds);

                            if (CheckIsComeToDestination(framesInterval))
                            {
                                makeTurn = true;

                                RandomDestinationRotate();
                            }
                        }
                        else
                        {
                            makeTurn = Convert.ToBoolean(rand.Next(2));

                            if (!makeTurn)
                                RandomDestination();
                            else
                                RandomDestinationRotate();

                            time = 0.0f;
                        }
                    }
                }
                else
                {
                    time += (float)framesInterval.TotalSeconds;
                }
            }
        }

        public bool CheckCollision(Vector2[] busCollisionPoints, float busSpeed)
        {
            if (!collision)
            {
                MyRectangle busCollisionRectangle = new MyRectangle(busCollisionPoints[0], busCollisionPoints[1], busCollisionPoints[2], busCollisionPoints[3]);
                MyRectangle pedestrianCollisionRectangle = new MyRectangle();

                Vector2 v_pos = pos;

                pedestrianCollisionRectangle.point1 = new Vector2(v_pos.X - origin.X, v_pos.Y - origin.Y); // 1
                pedestrianCollisionRectangle.point2 = new Vector2(v_pos.X + origin.X, v_pos.Y - origin.Y); // 2
                pedestrianCollisionRectangle.point3 = new Vector2(v_pos.X + origin.X, v_pos.Y + origin.Y); // 3
                pedestrianCollisionRectangle.point4 = new Vector2(v_pos.X - origin.X, v_pos.Y + origin.Y); // 4

                if (busSpeed != 0.0f) // jezeli autobus sie porusza
                {
                    if (busCollisionRectangle.IsInside(pedestrianCollisionRectangle) || pedestrianCollisionRectangle.IsInside(busCollisionRectangle)) // true = kolizja
                    {
                        name = "died_pedestrian";
                        collision = true;
                        return true;
                    }
                }
                else // jezeli autobus sie nie porusza (zwiekszamy obszar kolizji, zeby piesi zatrzymywali sie wczesniej przed autobusem
                {
                    pedestrianCollisionRectangle.point1 += new Vector2(-size.X, -size.Y); // 1
                    pedestrianCollisionRectangle.point2 += new Vector2(size.X, -size.Y); // 2
                    pedestrianCollisionRectangle.point3 += new Vector2(size.X, size.Y); // 3
                    pedestrianCollisionRectangle.point4 += new Vector2(-size.X, size.Y); // 4

                    if (busCollisionRectangle.IsInside(pedestrianCollisionRectangle) || pedestrianCollisionRectangle.IsInside(busCollisionRectangle)) // true = kolizja
                    {
                        if (!uniqueCollisionWithBus) // zatrzymaj sie, zmien kierunek chodu
                        {
                            RandomOpositeDestination();
                            uniqueCollisionWithBus = true;
                            return false;
                        }
                    }
                }

                return false;
            }

            return true;
        }

        private bool CheckIsRotate(TimeSpan framesInterval)
        {
            if (rotate < destinationRotate + rotateSpeed * framesInterval.TotalMilliseconds
                    && rotate > destinationRotate - rotateSpeed * framesInterval.TotalMilliseconds)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckIsComeToDestination(TimeSpan framesInterval)
        {
            if (pos_y < destinationPos + speed * framesInterval.TotalMilliseconds 
                    && pos_y > destinationPos - speed * framesInterval.TotalMilliseconds)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // losuje docelowe miejsce ruchu pieszego, w przeciwnym kierunku niz obecny
        private void RandomOpositeDestination()
        {
            direction = (Direction)((int)(direction + 2) % 4); // zmiana kierunku na przeciwny
            RandomSpeed(); // losujemy nowa predkos pieszego
            makeTurn = false; // nie ma wykonywac obrotow w miejscu
            time = waitingTime; // pieszy od razu sie poruszy
            invertSpeed = !invertSpeed; // odwracamy predkosc poruszania sie

            // losujemy nowa pozycje w przeciwnym kierunku do aktualnego
            if (direction == Direction.Up || direction == Direction.Left)
            {
                destinationPos = rand.Next((int)min, (int)pos_y);
            }
            else if (direction == Direction.Down || direction == Direction.Right)
            {
                destinationPos = rand.Next((int)pos_y, (int)max);
            }
        }

        private void RandomSpeed()
        {
            speed = (float)rand.Next(8, 14) / 400.0f;
        }

        private void RandomDestinationRotate()
        {
            destinationRotate = rand.Next((int)(rotate - diffrenceRotateOnTurn),(int)(rotate + diffrenceRotateOnTurn));

            if (destinationRotate < rotate)
                invertRotate = true;
            else
                invertRotate = false;

            if (destinationRotate > 360.0f)
                destinationRotate -= 360.0f;
            else if (destinationRotate < 0.0f)
                destinationRotate += 360.0f;
        }

        private void RandomDestination()
        {
            RandomSpeed();
            destinationPos = rand.Next((int)min, (int)max);

            if (destinationPos > pos_y)
            {
                invertSpeed = false;

                direction = (location == Location.horizontal) ? Direction.Right : Direction.Down;
            }
            else
            {
                invertSpeed = true;

                direction = (location == Location.horizontal) ? Direction.Left : Direction.Up;
            }
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

        List<SidewalkPedestrian> spawnSidewalks; // list chodnikow z utworzonymi pieszymi

        float oneSidewalkHeight; // wysokosc jednego chodnika
        float distanceToDelete = 400; //chodniki wraz z pieszymi bedace dalej niz podany dystans od krawedzi mapy zostana usuniete
        int[] frequences; // tablica z czestoscia wystepowania pieszych na danym (id) chodniku

        float lastUpdateTime = 0.0f; // ostatni czas update
        int updateTime = 200; // co ile nalezy wykonac update (w milisekundach)

        Random rand; // klasa losujaca

        public PedestriansLogic()
        {
            rand = new Random();
            spawnSidewalks = new List<SidewalkPedestrian>();
        }

        public void SetSidewalks(List<Sidewalk> sidewalks)
        {
            this.sidewalks = sidewalks;
        }

        public void SetProperties(Vector2 pedestrianSize, int amountOfPedestrians, float oneSidewalkHeight)
        {
            this.pedestrianSize = pedestrianSize;
            this.amountOfPedestrians = amountOfPedestrians;
            this.oneSidewalkHeight = oneSidewalkHeight;
        }

        // ustawia jak duzo ludzi ma sie pojawiac na danym typie (id) chodnika
        public void SetFrequencyOfOccurrencePedestrians(int[] frequences)
        {
            this.frequences = frequences;
        }

        public void Update(TimeSpan framesInterval, Vector2[] busCollisionPoints, float busSpeed)
        {
            if (lastUpdateTime > updateTime)
            {
                SpawnPedestrian();
                RemoveSpawnSidewalksOutOfArea();

                lastUpdateTime = 0.0f;
            }
            else
            {
                lastUpdateTime += (float)framesInterval.TotalMilliseconds;
            }

            foreach (SidewalkPedestrian sidewalkPedestrian in spawnSidewalks)
            {
                List<Pedestrian> alivePedestrians = sidewalkPedestrian.GetAlivePedestrians();

                for (int i = 0; i < alivePedestrians.Count; ++i)
                {
                    alivePedestrians[i].Update(framesInterval);
                    if (alivePedestrians[i].CheckCollision(busCollisionPoints, busSpeed))
                    {
                        sidewalkPedestrian.DiePedestrian(alivePedestrians[i]);
                        --i;
                    }
                }
            }
        }

        // pobiera liste pieszych do wyswietlenia
        public List<Object> GetPedestriansToShow()
        {
            List<Object> pedestriansToShow = new List<Object>();

            foreach (SidewalkPedestrian sp in spawnSidewalks)
            {
                foreach (Pedestrian p in sp.GetAllPedestrians())
                    pedestriansToShow.Add(new Object(p.name, Helper.MapPosToScreenPos(p.pos), pedestrianSize, p.rotate));
            }

            return pedestriansToShow;
        }

        // pobiera skrajne punkty
        private void GetExtremePoints(out Vector2 leftUp, out Vector2 rightDown)
        {
            leftUp = Helper.mapPos - Helper.workAreaOrigin - pedestrianOrigin;
            rightDown = Helper.mapPos + Helper.workAreaOrigin + pedestrianOrigin;
        }

        // czy dany punkt znajduje sie w obszarze roboczym gry
        private bool IsPointInWorkArea(Vector2 point)
        {
            Vector2 leftUp, rightDown;
            GetExtremePoints(out leftUp, out rightDown);

            return (point.X < rightDown.X && point.X > leftUp.X && point.Y < rightDown.Y && point.Y > leftUp.Y);
        }

        private bool IsLineBeetweenWorkArea(Line line)
        {
            Vector2 leftUp, rightDown;
            GetExtremePoints(out leftUp, out rightDown);

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

        private bool IsSidewalkOutOfSpawnArea(Sidewalk sidewalk)
        {
            Vector2[] points = GetSidewalkPoints(sidewalk);

            Vector2 leftUp = Helper.mapPos - (Helper.workAreaOrigin + new Vector2(distanceToDelete, distanceToDelete));
            Vector2 size = Helper.workAreaSize + new Vector2(distanceToDelete * 2, distanceToDelete * 2);

            Rectangle spawnArea = new Rectangle((int)leftUp.X, (int)leftUp.Y, (int)size.X, (int)size.Y);

            if (points[0].X > spawnArea.X + spawnArea.Width || points[1].X < spawnArea.X
                    || points[0].Y > spawnArea.Y + spawnArea.Height || points[3].Y < spawnArea.Y)
                return true;
            else
                return false;
        }

        // cztery skrajne punkty chodnika
        private Vector2[] GetSidewalkPoints(Sidewalk sidewalk)
        {
            Vector2[] points = new Vector2[4]; // punkty prostokata chodnika

            Vector2 sidewalkOrigin = (sidewalk.location == Location.horizontal) ? // srodek chodnika wg. konwencjii
                    new Vector2(sidewalk.origin.Y, sidewalk.origin.X) : sidewalk.origin;

            points[0] = new Vector2(sidewalk.pos.X - sidewalkOrigin.X, sidewalk.pos.Y - sidewalkOrigin.Y);
            points[1] = new Vector2(sidewalk.pos.X + sidewalkOrigin.X, sidewalk.pos.Y - sidewalkOrigin.Y);
            points[2] = new Vector2(sidewalk.pos.X + sidewalkOrigin.X, sidewalk.pos.Y + sidewalkOrigin.Y);
            points[3] = new Vector2(sidewalk.pos.X - sidewalkOrigin.X, sidewalk.pos.Y + sidewalkOrigin.Y);

            return points;
        }

        // true: 1 lub 2 punkty znajduja sie w obszarze; false: 0, 3 lub 4 punkty znajduja sie w obszarze
        private bool IsSidewalkInWorkArea(Sidewalk sidewalk)
        {
            int numberOfPointsInWorkArea = 0; // liczba punktow w obszarze

            Vector2[] points = GetSidewalkPoints(sidewalk);

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

        private void GeneratePedestrian(ref SidewalkPedestrian sidewalkPedestrian)
        {
            Vector2 pos = new Vector2();

            Sidewalk sidewalk = sidewalkPedestrian.sidewalk;

            int id = rand.Next(0, amountOfPedestrians);

            float min = (sidewalk.location == Location.horizontal) ? sidewalk.pos.X - sidewalk.origin.Y
                    : sidewalk.pos.Y - sidewalk.origin.Y;
            float max = (sidewalk.location == Location.horizontal) ? sidewalk.pos.X + sidewalk.origin.Y
                    : sidewalk.pos.Y + sidewalk.origin.Y;

            int numberOfSidewalks = (int)(sidewalk.size.Y / oneSidewalkHeight); // ilosc "kawalkow" chodnika
            int numberOfDraws = numberOfSidewalks * frequences[sidewalk.id];

            for (int i = 0; i < numberOfDraws; ++i)
            {
                if (rand.Next(4) == 1) // 25 % szans na utworzenie pieszego
                {
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

                    sidewalkPedestrian.AddPedestrian(new Pedestrian(pos, pedestrianSize, id, sidewalk.location, min, max));
                }
            }
        }

        // sprawdza czy spawnSidewalks zawiera juz taki chodnik
        private bool ContainSidewalk(Sidewalk sidewalk)
        {
            foreach (SidewalkPedestrian spawnSidewalk in spawnSidewalks)
            {
                if (sidewalk.pos == spawnSidewalk.sidewalk.pos)
                    return true;
            }

            return false;
        }

        private void SpawnPedestrian()
        {
            foreach (Sidewalk sidewalk in sidewalks)
            {
                if (!ContainSidewalk(sidewalk))
                {
                    if (IsSidewalkInWorkArea(sidewalk))
                    {
                        SidewalkPedestrian sidewalkPedestrian = new SidewalkPedestrian(sidewalk);
                        spawnSidewalks.Add(sidewalkPedestrian);
                        GeneratePedestrian(ref sidewalkPedestrian);
                    }
                }
            }
        }

        private void RemoveSpawnSidewalksOutOfArea()
        {
            for (int i = 0; i < spawnSidewalks.Count; ++i)
            {
                SidewalkPedestrian spawnPedestrian = spawnSidewalks[i];

                if (IsSidewalkOutOfSpawnArea(spawnPedestrian.sidewalk))
                {
                    spawnSidewalks.RemoveAt(i);
                    --i;
                }
            }
        }
    }
}
