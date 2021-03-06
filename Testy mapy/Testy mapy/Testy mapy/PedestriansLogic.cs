﻿using System;
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
        static readonly float rotateEscapeSpeed = 0.25f; // predkosc rotacji przy ucieczce
        static readonly float rotateNormalSpeed = 0.08f; // normalna predkosc rotacji
        static readonly float escapeSpeed = 0.08f; // predkosc ucieczki pieszego

        float rotateSpeed = rotateNormalSpeed; // predkosc rotacji
        bool invertRotate; // czy odwrocic rotacje
        float speed; // predkosc poruszania sie pieszych
        bool invertSpeed; // czy odwrocic predkosc

        bool collision; // czy wystapila kolizja
        bool makeTurn; // czy wykonac kolejny obrot
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
        public Vector2 size { get; private set; } // rozmiar

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

        public Pedestrian(Vector2 pos, Vector2 size, int id, Location location, float min, float max, float startRotation)
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

            rotate = startRotation;
            RandomDestination();
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
                        if (!makeTurn)
                        {
                            pos_y += (float)(((invertSpeed) ? -speed : speed) * framesInterval.TotalMilliseconds);

                            if (CheckIsComeToDestination(framesInterval))
                            {
                                makeTurn = true;

                                RandomDestinationRotate();
                            }
                            else
                            {
                                // sprawdzamy czy pieszy nie wychodzi poza chodnik
                                CheckIsOutsideOfSidewalk();
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

        private void CheckIsOutsideOfSidewalk()
        {
            if (pos_y > max)
            {
                direction = (location == Location.horizontal) ? Direction.Left : Direction.Up; // zmiana kierunku
                invertSpeed = true; // odwracamy predkosc poruszania sie
            }
            else if (pos_y < min)
            {
                direction = (location == Location.horizontal) ? Direction.Right : Direction.Down; // zmiana kierunku
                invertSpeed = false; // nie odwracamy predkosc poruszania sie
            }
        }

        public bool CheckCollision(Vector2[] busCollisionPoints)
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

                if (busCollisionRectangle.IsInside(pedestrianCollisionRectangle) || pedestrianCollisionRectangle.IsInside(busCollisionRectangle)) // true = kolizja
                {
                    name = "died_pedestrian" + Helper.random.Next(GameParams.numberOfDiedPedestriansTextures).ToString();
                    size = GameParams.diedPedestrianSize;
                    collision = true;
                    Score.AddAction("killed pedestrian", 1.0f);
                    return true;
                }
                else // jezeli nie wykryto bezposredniej kolizji sprawdzamy czy autobus nie znajduje sie niedaleko pieszego
                {
                    pedestrianCollisionRectangle.point1 += new Vector2(-size.X, -size.Y); // 1
                    pedestrianCollisionRectangle.point2 += new Vector2(size.X, -size.Y); // 2
                    pedestrianCollisionRectangle.point3 += new Vector2(size.X, size.Y); // 3
                    pedestrianCollisionRectangle.point4 += new Vector2(-size.X, size.Y); // 4

                    if (busCollisionRectangle.IsInside(pedestrianCollisionRectangle) || pedestrianCollisionRectangle.IsInside(busCollisionRectangle)) // true = kolizja
                    {
                        UniqueCollision(busCollisionPoints);
                        return false;
                    }
                    else
                    {
                        rotateSpeed = rotateNormalSpeed;
                        RandomSpeed();
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

        private bool IsGoInDirectionOfBus(Vector2[] busCollisionPoints)
        {
            float coordinate = 0.0f;

            for (int i = 0; i < 4; ++i)
            {
                switch (direction)
                {
                    case Direction.Up:
                        if (i == 0)
                            coordinate = busCollisionPoints[i].Y;
                        else if (busCollisionPoints[i].Y < coordinate)
                            coordinate = busCollisionPoints[i].Y;

                        if (i == 3)
                        {
                            if (coordinate < pos_y)
                                return true;
                            else
                                return false;
                        }
                        break;
                    case Direction.Down:
                        if (i == 0)
                            coordinate = busCollisionPoints[i].Y;
                        else if (busCollisionPoints[i].Y > coordinate)
                            coordinate = busCollisionPoints[i].Y;

                        if (i == 3)
                        {
                            if (coordinate > pos_y)
                                return true;
                            else
                                return false;
                        }
                        break;
                    case Direction.Right:
                        if (i == 0)
                            coordinate = busCollisionPoints[i].X;
                        else if (busCollisionPoints[i].X > coordinate)
                            coordinate = busCollisionPoints[i].X;

                        if (i == 3)
                        {
                            if (coordinate > pos_y)
                                return true;
                            else
                                return false;
                        }
                        break;
                    case Direction.Left:
                        if (i == 0)
                            coordinate = busCollisionPoints[i].X;
                        else if (busCollisionPoints[i].X < coordinate)
                            coordinate = busCollisionPoints[i].X;

                        if (i == 3)
                        {
                            if (coordinate < pos_y)
                                return true;
                            else
                                return false;
                        }
                        break;
                }
            }

            return false;
        }

        // uciekamy pieszym od autobusu (który jest prowadzony przez Borka i chce nas zabić!)
        private void Escape()
        {
            direction = (Direction)((int)(direction + 2) % 4); // zmiana kierunku na przeciwny
            speed = escapeSpeed; // losujemy nowa predkosc pieszego
            rotateSpeed = rotateEscapeSpeed; // zmieniamy predkosc rotacji na ta dla ucieczki
            makeTurn = false; // nie ma wykonywac obrotow w miejscu
            time = waitingTime; // pieszy od razu sie poruszy
            invertSpeed = !invertSpeed; // odwracamy predkosc poruszania sie

            // losujemy nowa pozycje w przeciwnym kierunku do aktualnego
            if (direction == Direction.Up || direction == Direction.Left)
            {
                if (pos_y > min)
                    destinationPos = rand.Next((int)min, (int)pos_y);
            }
            else if (direction == Direction.Down || direction == Direction.Right)
            {
                if (pos_y < max)
                    destinationPos = rand.Next((int)pos_y, (int)max);
            }
        }

        // losuje docelowe miejsce ruchu pieszego, w przeciwnym kierunku niz obecny
        private void UniqueCollision(Vector2[] busCollisionPoints)
        {
            // sprawdzamy czy pieszy idzie w kierunku autobusu
            if (IsGoInDirectionOfBus(busCollisionPoints))
            {
                Escape();
            }
        }

        private void RandomSpeed()
        {
            speed = (float)rand.Next(6, 12) / 400.0f;
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
    }

    class PedestriansLogic
    {
        List<Sidewalk> sidewalks; // lista chodnikow

        Vector2 pedestrianOrigin; // srodek tekstur pieszych

        Vector2 originalPedestrianSize; // oryginalny rozmiar teksturki pieszych
        Vector2 originalDiedPedestrianSize; // oryginalny rozmiar teksturki rozjechanych pieszych

        List<SidewalkPedestrian> spawnSidewalks; // list chodnikow z utworzonymi pieszymi

        float oneSidewalkHeight; // wysokosc jednego chodnika
        float distanceToDelete = 400; //chodniki wraz z pieszymi bedace dalej niz podany dystans od krawedzi mapy zostana usuniete
        int[] frequences; // tablica z czestoscia wystepowania pieszych na danym (id) chodniku

        float lastUpdateTime = 0.0f; // ostatni czas update
        int updateTime = 100; // co ile nalezy wykonac update (w milisekundach)

        Random rand; // klasa losujaca

        public PedestriansLogic()
        {
            rand = new Random();
            spawnSidewalks = new List<SidewalkPedestrian>();

            pedestrianOrigin = GameParams.pedestrianSize / 2;
        }

        public void SetSidewalks(List<Sidewalk> sidewalks)
        {
            this.sidewalks = sidewalks;
        }

        public void SetProperties(Vector2 originalPedestrianSize, Vector2 originalDiedPedestrianSize, float oneSidewalkHeight)
        {
            this.originalPedestrianSize = originalPedestrianSize;
            this.originalDiedPedestrianSize = originalDiedPedestrianSize;
            this.oneSidewalkHeight = oneSidewalkHeight;
        }

        // ustawia jak duzo ludzi ma sie pojawiac na danym typie (id) chodnika
        public void SetFrequencyOfOccurrencePedestrians(int[] frequences)
        {
            this.frequences = frequences;
        }

        public void Update(TimeSpan framesInterval, Vector2[] busCollisionPoints, float globalLightValue)
        {
            if (lastUpdateTime > updateTime)
            {
                SpawnPedestrian(globalLightValue);
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
                    if (alivePedestrians[i].CheckCollision(busCollisionPoints))
                    {
                        sidewalkPedestrian.DiePedestrian(alivePedestrians[i]);
                        --i;
                    }
                }
            }
        }

        public bool AddPedestrian(int id, Vector2 position, float rotation)
        {
            foreach (SidewalkPedestrian sidewalk in spawnSidewalks)
            {
                if (IsPedestrianOnSidewalk(position, sidewalk.sidewalk))
                {
                    Pedestrian pedestrian = new Pedestrian(position, GameParams.pedestrianSize, id, sidewalk.sidewalk.location,
                            sidewalk.sidewalk.min, sidewalk.sidewalk.max, rotation);

                    sidewalk.AddPedestrian(pedestrian);

                    return true;
                }
            }

            return false;
        }

        public bool IsPedestrianOnSidewalk(Vector2 pos, Sidewalk sidewalk)
        {
            Vector2 sidewalkOrigin = (sidewalk.location == Location.horizontal) ? // srodek chodnika wg. konwencjii
                    new Vector2(sidewalk.origin.Y, sidewalk.origin.X) : sidewalk.origin;

            // wyliczamy skrajne punkty chodnika:
            float sx1 = sidewalk.pos.X - sidewalkOrigin.X;
            float sx2 = sidewalk.pos.X + sidewalkOrigin.X;
            float sy1 = sidewalk.pos.Y - sidewalkOrigin.Y;
            float sy2 = sidewalk.pos.Y + sidewalkOrigin.Y;

            // wyliczamy skrajne punkty pieszego:
            float px1 = pos.X - pedestrianOrigin.X;
            float px2 = pos.X + pedestrianOrigin.X;
            float py1 = pos.Y - pedestrianOrigin.Y;
            float py2 = pos.Y + pedestrianOrigin.Y;

            // porównujemy skrajne punkty w celu sprawdzenia czy pieszy znajduje się na chodniku:
            if (px1 > sx1 && px2 < sx2 && py1 > sy1 && py2 < sy2)
                return true;
            else
                return false;
        }

        // pobiera liste pieszych do wyswietlenia
        public List<Object> GetPedestriansToShow()
        {
            List<Object> pedestriansToShow = new List<Object>();

            foreach (SidewalkPedestrian sp in spawnSidewalks)
            {
                foreach (Pedestrian p in sp.GetAllPedestrians())
                {
                    Vector2 originalSize;

                    if (p.name.StartsWith("died_pedestrian"))
                        originalSize = originalDiedPedestrianSize;
                    else
                        originalSize = originalPedestrianSize;

                    pedestriansToShow.Add(new Object(p.name, Helper.MapPosToScreenPos(p.pos), p.size, p.rotate, originalSize));
                }
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
        private bool IsPointInWorkArea(Vector2 point, Vector2 leftUp, Vector2 rightDown)
        {
            return (point.X < rightDown.X && point.X > leftUp.X && point.Y < rightDown.Y && point.Y > leftUp.Y);
        }

        private bool IsLineBeetweenWorkArea(Line line, Vector2 leftUp, Vector2 rightDown)
        {
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

            Vector2 leftUp, rightDown;
            GetExtremePoints(out leftUp, out rightDown);
            // zwiekszamy troche obszar (0 10%) w ktorym sprawdzane sa chodniki:
            leftUp -= leftUp * 0.1f;
            rightDown += rightDown * 0.1f;

            Vector2[] points = GetSidewalkPoints(sidewalk);

            for (int i = 0; i < 4; ++i)
            {
                if (IsPointInWorkArea(points[i], leftUp, rightDown))
                    ++numberOfPointsInWorkArea;
            }

            if (numberOfPointsInWorkArea >= 1)
            {
                return true;
            }
            else
            {
                // sprawdzamy czy chodnik nie znajduje sie pomiedzy obszarem roboczym
                if (sidewalk.location == Location.horizontal)
                {
                    if (IsLineBeetweenWorkArea(new Line(points[0], points[1]), leftUp, rightDown)
                            || IsLineBeetweenWorkArea(new Line(points[3], points[2]), leftUp, rightDown))
                        return true;
                }
                else
                {
                    if (IsLineBeetweenWorkArea(new Line(points[0], points[3]), leftUp, rightDown)
                            || IsLineBeetweenWorkArea(new Line(points[1], points[2]), leftUp, rightDown))
                        return true;
                }
            }

            return false;
        }

        private void GeneratePedestrian(ref SidewalkPedestrian sidewalkPedestrian, float globalLightValue)
        {
            Vector2 pos = new Vector2();

            Sidewalk sidewalk = sidewalkPedestrian.sidewalk;

            int numberOfSidewalks = (int)(sidewalk.size.Y / oneSidewalkHeight); // ilosc "kawalkow" chodnika
            globalLightValue += ((1 - globalLightValue) / 4); // zmieniamy zakres wartość z 0.2 - 1 na 0.4 - 1
            int numberOfDraws = (int)Math.Ceiling(numberOfSidewalks * frequences[sidewalk.id] * globalLightValue); // liczba prób utworzenia pieszego

            for (int i = 0; i < numberOfDraws; ++i)
            {
                if (rand.Next(4) == 1) // 25 % szans na utworzenie pieszego
                {
                    int id = rand.Next(0, GameParams.numberOfPedestriansTextures); // losowy typ pieszego

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

                    sidewalkPedestrian.AddPedestrian(new Pedestrian(pos, GameParams.pedestrianSize, id, sidewalk.location, sidewalk.min, sidewalk.max));
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

        private void SpawnPedestrian(float globalLightValue)
        {
            foreach (Sidewalk sidewalk in sidewalks)
            {
                if (!ContainSidewalk(sidewalk))
                {
                    if (IsSidewalkInWorkArea(sidewalk))
                    {
                        SidewalkPedestrian sidewalkPedestrian = new SidewalkPedestrian(sidewalk);
                        spawnSidewalks.Add(sidewalkPedestrian);
                        GeneratePedestrian(ref sidewalkPedestrian, globalLightValue);
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
