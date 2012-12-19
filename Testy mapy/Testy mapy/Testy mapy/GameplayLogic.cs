using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace Testy_mapy
{
    class GameplayLogic
    {
        /// <summary>
        /// This is the bus stop.
        /// </summary>
        protected class BusStop
        {
            // Constructor.
            public BusStop(int id, Vector2 stopArea, float stopAreaDirection, Vector2 waitingArea, float waitingAreaDirection, Vector2 sign, float signDirection)
            {
                this.id = id;
                this.stopArea = stopArea;
                this.stopAreaDirection = stopAreaDirection;
                this.waitingArea = waitingArea;
                this.waitingAreaDirection = waitingAreaDirection;
                this.sign = sign;
                this.signDirection = signDirection;
                SpawnNewPedestrians();
            }

            public class Pedestrian
            {
                // Constructor.
                public Pedestrian(Vector2 waitingArea, Vector2 waitingAreaSize, float waitingAreaDirection)
                {
                    Vector2 position = new Vector2(Helper.random.Next(0, (int)waitingAreaSize.X + 1), Helper.random.Next(0, (int)waitingAreaSize.Y + 1));

                    switch ((int)waitingAreaDirection)
                    {
                        case 0:
                            this.waitingPosition.X = waitingArea.X - (waitingAreaSize.X / 2) + position.X;
                            this.waitingPosition.Y = waitingArea.Y - (waitingAreaSize.Y / 2) + position.Y;
                            break;
                        case 90:
                            this.waitingPosition.X = waitingArea.X - (waitingAreaSize.Y / 2) + position.Y;
                            this.waitingPosition.Y = waitingArea.Y - (waitingAreaSize.X / 2) + position.X;
                            break;
                        case 180:
                            this.waitingPosition.X = waitingArea.X - (waitingAreaSize.X / 2) + position.X;
                            this.waitingPosition.Y = waitingArea.Y - (waitingAreaSize.Y / 2) + position.Y;
                            break;
                        case 270:
                            this.waitingPosition.X = waitingArea.X - (waitingAreaSize.Y / 2) + position.Y;
                            this.waitingPosition.Y = waitingArea.Y - (waitingAreaSize.X / 2) + position.X;
                            break;
                    }

                    this.position = this.waitingPosition;
                    this.direction = GetRandomDirection();
                    this.skin = Helper.random.Next(0, 3);
                }

                /// <summary>
                /// This constructor is used for spawning pedestrians getting off the bus.
                /// </summary>
                public Pedestrian(Vector2 waitingArea, Vector2 waitingAreaSize, float waitingAreaDirection, Vector2 startPosition)
                {
                    Vector2 position = new Vector2(Helper.random.Next(0, (int)waitingAreaSize.X + 1), Helper.random.Next(0, (int)waitingAreaSize.Y + 1));

                    switch ((int)waitingAreaDirection)
                    {
                        case 0:
                            this.waitingPosition.X = waitingArea.X - (waitingAreaSize.X / 2) + position.X;
                            this.waitingPosition.Y = waitingArea.Y - (waitingAreaSize.Y / 2) + position.Y;
                            break;
                        case 90:
                            this.waitingPosition.X = waitingArea.X - (waitingAreaSize.Y / 2) + position.Y;
                            this.waitingPosition.Y = waitingArea.Y - (waitingAreaSize.X / 2) + position.X;
                            break;
                        case 180:
                            this.waitingPosition.X = waitingArea.X - (waitingAreaSize.X / 2) + position.X;
                            this.waitingPosition.Y = waitingArea.Y - (waitingAreaSize.Y / 2) + position.Y;
                            break;
                        case 270:
                            this.waitingPosition.X = waitingArea.X - (waitingAreaSize.Y / 2) + position.Y;
                            this.waitingPosition.Y = waitingArea.Y - (waitingAreaSize.X / 2) + position.X;
                            break;
                    }

                    this.position = startPosition;
                    this.direction = Helper.CalculateDirection(this.position, this.waitingPosition);
                    this.skin = Helper.random.Next(0, 3);
                }

                private int GetRandomDirection()
                {
                    return Helper.random.Next(0, 361);
                }

                public Vector2 GetPosition()
                {
                    return position;
                }

                public Vector2 GetSize()
                {
                    return size;
                }

                public float GetDirection()
                {
                    return direction;
                }

                public int GetSkin()
                {
                    return skin;
                }

                /// <summary>
                /// Get collision points.
                /// </summary>
                public Vector2[] GetCollisionPoints()
                {
                    Vector2 p1, p2, p3, p4; // Create 4 points.
                    /*
                     * |p1 p2|
                     * |p4 p3|
                     */

                    p3.X = position.X + ((size.X * (float)Math.Cos(MathHelper.ToRadians(direction))) / 2); // Calculate their positions.
                    p3.Y = position.Y + ((size.X * (float)Math.Sin(MathHelper.ToRadians(direction))) / 2);

                    p4.X = position.X - (size.X * (float)Math.Cos(MathHelper.ToRadians(direction)) / 2);
                    p4.Y = position.Y - (size.X * (float)Math.Sin(MathHelper.ToRadians(direction)) / 2);

                    p1.X = p4.X + (size.Y / 2 * (float)Math.Sin(MathHelper.ToRadians(direction)));
                    p1.Y = p4.Y - (size.Y / 2 * (float)Math.Cos(MathHelper.ToRadians(direction)));

                    p2.X = p3.X + (size.Y / 2 * (float)Math.Sin(MathHelper.ToRadians(direction)));
                    p2.Y = p3.Y - (size.Y / 2 * (float)Math.Cos(MathHelper.ToRadians(direction)));

                    p3.X = p3.X - (size.Y / 2 * (float)Math.Sin(MathHelper.ToRadians(direction)));
                    p3.Y = p3.Y + (size.Y / 2 * (float)Math.Cos(MathHelper.ToRadians(direction)));

                    p4.X = p4.X - (size.Y / 2 * (float)Math.Sin(MathHelper.ToRadians(direction)));
                    p4.Y = p4.Y + (size.Y / 2 * (float)Math.Cos(MathHelper.ToRadians(direction)));



                    Vector2[] pointsArray = new Vector2[4] { p1, p2, p3, p4 }; // Create list and add points.

                    return pointsArray;
                }

                /// <summary>
                /// Check if the pedestrian is idle.
                /// </summary>
                public bool Idle()
                {
                    return (waitingPosition == position);
                }

                /// <summary>
                /// Decide if the pedestrian should rotate left or right.
                /// </summary>
                /// <returns>TRUE for left and FALSE for right.</returns>
                private bool ShouldRotateLeft(float currentDirection, float desiredDirection)
                {
                    bool rotateLeft = false;
                    if (Math.Max(currentDirection, desiredDirection) - Math.Min(currentDirection, desiredDirection) < 180)
                    {
                        rotateLeft = true;
                    }

                    if (currentDirection < desiredDirection)
                    {
                        rotateLeft = !rotateLeft;
                    }

                    return rotateLeft;
                }

                private void RotateToDirection(float desiredDirection, float timeCoherenceMultiplier)
                {
                    if (ShouldRotateLeft(direction, desiredDirection))
                    {
                        direction -= directionChange * timeCoherenceMultiplier;

                        // Nie pozwól na ujemny direction.
                        if (direction < 0)
                        {
                            direction += 360;
                        }
                    }
                    else
                    {
                        direction += directionChange * timeCoherenceMultiplier;

                        // Nie pozwól na direction powyżej 360.
                        if (direction > 360)
                        {
                            direction -= 360;
                        }
                    }
                }

                public void HandleIdle(float waitingAreaDirection, float timeCoherenceMultiplier)
                {
                    // Kierunek w którym mają patrzec pieszy ma być prostopadły do kierunku podanego i połozony z lewej.
                    waitingAreaDirection -= 90;

                    // Nie pozwól na ujemny kierunek.
                    if (waitingAreaDirection < 0)
                    {
                        waitingAreaDirection += 360;
                    }

                    // Jeśli miną czas bezruchu obracaj się lub zmniejsz licznik w przeciwnym wypadku.
                    if (stayIdleFor < 0)
                    {
                        RotateToDirection(idleDirection, timeCoherenceMultiplier);
                    }
                    else
                    {
                        stayIdleFor -= timeCoherenceMultiplier;
                    }

                    // Jeśli osiągnięto odpowiedni kierunek wybierz nowy.
                    if (Math.Abs(idleDirection - direction) < 5)
                    {
                        idleDirection = Helper.random.Next((int)waitingAreaDirection - 70, (int)waitingAreaDirection + 70);
                        stayIdleFor = Helper.random.Next(idleMin * 10, idleMax * 10 + 1) / 10;
                    }
                }

                /// <summary>
                /// Handles the movement of the pedestrians who got off the bus.
                /// </summary>
                public void HandleGetOff(float timeCoherenceMultiplier)
                {
                    if (position != waitingPosition)
                    {
                        float idleDirection = Helper.CalculateDirection(position, waitingPosition);

                        // Obracaj się do odpowiedniego kierunku lub idź w kierunku przystanku.
                        if (Math.Abs(idleDirection - direction) > 5)
                        {
                            RotateToDirection(idleDirection, timeCoherenceMultiplier);
                        }
                        else
                        {
                            ChangePosition(speed, direction, timeCoherenceMultiplier);
                        }

                        if (Helper.CalculateDistance(position, waitingPosition) < 2)
                        {
                            position = waitingPosition;
                        }
                    }
                }

                private Vector2 CalculateNewPosition(float speed, float direction, float timeCoherenceMultiplier)
                {
                    Vector2 newPosition;
                    newPosition.X = position.X + (speedMultiplier * timeCoherenceMultiplier * speed * (float)Math.Sin(MathHelper.ToRadians(direction)));
                    newPosition.Y = position.Y - (speedMultiplier * timeCoherenceMultiplier * speed * (float)Math.Cos(MathHelper.ToRadians(direction)));
                    return newPosition;
                }

                private void ChangePosition(float speed, float direction, float timeCoherenceMultiplier)
                {
                    position = CalculateNewPosition(speed, direction, timeCoherenceMultiplier);
                }

                public void Delete()
                {
                    delete = true;
                }

                /// <summary>
                /// Handles going in the direction of the bus.
                /// </summary>
                public void GoToTheBus(BusLogic busLogic, float timeCoherenceMultiplier)
                {
                    float idleDirection = Helper.CalculateDirection(position, busLogic.GetBusPosition());

                    // Obracaj się do odpowiedniego kierunku lub idź w kierunku autobusu.
                    if (Math.Abs(idleDirection - direction) > 5)
                    {
                        RotateToDirection(idleDirection, timeCoherenceMultiplier);
                    }
                    else
                    {
                        ChangePosition(speed, direction, timeCoherenceMultiplier);
                    }
                }

                /// <summary>
                /// Handles going back to the bus stop.
                /// </summary>
                /// <param name="timeCoherenceMultiplier"></param>
                public void GoToTheWaitingArea(float timeCoherenceMultiplier)
                {
                    if (position != waitingPosition)
                    {
                        float idleDirection = Helper.CalculateDirection(position, waitingPosition);

                        // Obracaj się do odpowiedniego kierunku lub idź w kierunku autobusu.
                        if (Math.Abs(idleDirection - direction) > 5)
                        {
                            RotateToDirection(idleDirection, timeCoherenceMultiplier);
                        }
                        else
                        {
                            ChangePosition(speed, direction, timeCoherenceMultiplier);
                        }

                        if (Helper.CalculateDistance(position, waitingPosition) < 2)
                        {
                            position = waitingPosition;
                        }
                    }
                }

                public bool BusReached(BusLogic busLogic)
                {
                    if (Helper.CalculateDistance(position, busLogic.GetBusPosition()) < size.Y / 2)
                        return true;
                    else
                        return false;
                }

                public Vector2 waitingPosition;    // Pozycja oczekiwania na autobus na przystanku.
                public Vector2 position;           // Aktualna pozycja.
                public float direction;            // Aktualny kierunek.

                public bool collisionActive = true;

                protected float idleDirection;        // Kierunek do którego będzie dążył pieszy aby symulować losowe, anturalne ruchy rozglądania się.
                protected float directionChange = 50; // Szybkość obrotu.
                protected float stayIdleFor = 0;      // Jak długo nie ma wykonywać kolejnego obrotu.
                protected int idleMin = 2;            // Minimalny czas oczekiwania na następny obrót.
                protected int idleMax = 5;            // Maksymany czas oczekiwania na następny obrót.

                public bool delete = false;           // Should be deleted?

                protected int speed = 10;
                protected float speedMultiplier = 4;

                public int skin;                           // Wygląd.
                public Vector2 size = new Vector2(20, 20); // Rozmiar.
            }

            public int id;

            public Vector2 stopArea;
            public float stopAreaDirection;
            public Vector2 stopAreaSize = new Vector2(80, 200);

            public Vector2 waitingArea;
            public float waitingAreaDirection;
            public Vector2 waitingAreaSize = new Vector2(40, 100);

            public Vector2 sign;
            public float signDirection;
            public Vector2 signSize = new Vector2(20, 20);

            public List<Pedestrian> pedestrians = new List<Pedestrian>();           // Piesi którzy mają wsiąść do autobusu.
            public List<Pedestrian> pedestriansWhoGotOff = new List<Pedestrian>();  // Piesi którzy wysiedli na tym przystanku.

            protected int minPedestrians = 1;
            protected int maxPedestrians = 5;

            /// <summary>
            /// Get collision points.
            /// </summary>
            private Vector2[] GetCollisionPoints(Vector2 position, float direction, Vector2 size)
            {
                Vector2 p1, p2, p3, p4; // Create 4 points.
                /*
                 * |p1 p2|
                 * |p4 p3|
                 */

                p3.X = position.X + ((size.X * (float)Math.Cos(MathHelper.ToRadians(direction))) / 2); // Calculate their positions.
                p3.Y = position.Y + ((size.X * (float)Math.Sin(MathHelper.ToRadians(direction))) / 2);

                p4.X = position.X - (size.X * (float)Math.Cos(MathHelper.ToRadians(direction)) / 2);
                p4.Y = position.Y - (size.X * (float)Math.Sin(MathHelper.ToRadians(direction)) / 2);

                p1.X = p4.X + (size.Y / 2 * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p1.Y = p4.Y - (size.Y / 2 * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p2.X = p3.X + (size.Y / 2 * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p2.Y = p3.Y - (size.Y / 2 * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p3.X = p3.X - (size.Y / 2 * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p3.Y = p3.Y + (size.Y / 2 * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p4.X = p4.X - (size.Y / 2 * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p4.Y = p4.Y + (size.Y / 2 * (float)Math.Cos(MathHelper.ToRadians(direction)));



                Vector2[] pointsArray = new Vector2[4] { p1, p2, p3, p4 }; // Create list and add points.

                return pointsArray;
            }

            public Vector2[] GetStopAreaCollisionPoints()
            {
                return GetCollisionPoints(stopArea, stopAreaDirection, stopAreaSize);
            }

            public Vector2[] GetWaitingAreaCollisionPoints()
            {
                return GetCollisionPoints(waitingArea, waitingAreaDirection, waitingAreaSize);
            }

            public Vector2[] GetSignCollisionPoints()
            {
                return GetCollisionPoints(sign, signDirection, signSize);
            }

            public Vector2 GetStopAreaPosition()
            {
                return stopArea;
            }

            public Vector2 GetWaitingAreaPosition()
            {
                return waitingArea;
            }

            public Vector2 GetSignPosition()
            {
                return sign;
            }

            public void SpawnNewPedestrians()
            {
                //Random random = new Random();
                int randomNumber = Helper.random.Next(minPedestrians, maxPedestrians + 1);

                pedestrians.Clear();

                for (int i = 0; i < randomNumber; i++)
                {
                    Pedestrian pedestrian = new Pedestrian(waitingArea, waitingAreaSize, waitingAreaDirection);
                    pedestrians.Add(pedestrian);
                }
            }
            
            /// <summary>
            /// Get the amount of pedestrians waiting for the bus.
            /// </summary>
            public int NumberOfPedestriansWaiting()
            {
                return pedestrians.Count();
            }

            /// <summary>
            /// Get the amount of pedestrians who got off the bus.
            /// </summary>
            public int NumberOfPedestriansWhoGotOff()
            {
                return pedestriansWhoGotOff.Count();
            }

            public int Update(bool GoToTheBus, BusLogic busLogic, float timeCoherenceMultiplier)
            {
                int pedestriansWhoReachedTheBus = 0;

                foreach (Pedestrian pedestrian in pedestrians)
                {
                    // Powinniśmy iść do autobusu czy kierować się na miejsce wyjściowe?
                    if (GoToTheBus)
                    {
                        // Włącz kolizję.
                        pedestrian.collisionActive = false;

                        pedestrian.GoToTheBus(busLogic, timeCoherenceMultiplier);

                        if (pedestrian.BusReached(busLogic))
                        {
                            pedestriansWhoReachedTheBus++;
                            pedestrian.Delete();
                        }
                    }
                    else
                    {
                        if (!pedestrian.collisionActive)
                        {
                            // Sprawdź czy należy ponownie włączyć kolizje (aktualnie nie ma kolizji).
                            bool isCollision = false;

                            Vector2[] pedestrianCollisionPoints = pedestrian.GetCollisionPoints();
                            MyRectangle pedestrianRectangle = new MyRectangle(pedestrianCollisionPoints[3], pedestrianCollisionPoints[2], pedestrianCollisionPoints[1], pedestrianCollisionPoints[0]);

                            Vector2[] busCollisionPoints = busLogic.GetCollisionPoints();
                            MyRectangle busRectangle = new MyRectangle(busCollisionPoints[3], busCollisionPoints[2], busCollisionPoints[1], busCollisionPoints[0]);

                            foreach (Vector2 point in pedestrianCollisionPoints)
                            {
                                if (Helper.IsInside(point, busRectangle))
                                {
                                    isCollision = true;
                                    break;
                                }
                            }

                            foreach (Vector2 point in busCollisionPoints)
                            {
                                if (Helper.IsInside(point, pedestrianRectangle))
                                {
                                    isCollision = true;
                                    break;
                                }
                            }

                            if (!isCollision)
                            {
                                pedestrian.collisionActive = true;
                            }
                        }

                        pedestrian.GoToTheWaitingArea(timeCoherenceMultiplier);
                    }

                    // Jeśli pieszy stoi na pozycji wyjściowej symuluj losowe obroty.
                    if (pedestrian.Idle())
                    {
                        pedestrian.HandleIdle(waitingAreaDirection, timeCoherenceMultiplier);
                    }
                }

                foreach (Pedestrian pedestrian in pedestriansWhoGotOff)
                {
                    // Powinniśmy iść do autobusu czy kierować się na miejsce wyjściowe?
                    pedestrian.GoToTheWaitingArea(timeCoherenceMultiplier);

                    // Jeśli pieszy stoi na pozycji wyjściowej symuluj losowe obroty.
                    if (pedestrian.Idle())
                    {
                        pedestrian.HandleIdle(waitingAreaDirection, timeCoherenceMultiplier);
                    }
                }

                // Zwracamy ilość pieszych którzy zdołali dotrzeć do autobusu.
                return pedestriansWhoReachedTheBus;
            }
        }

        protected List<BusStop> busStops = new List<BusStop>();
        protected List<Int32> busStopsOrder;
        protected SettingsHandling settingsHandling = new SettingsHandling();

        protected int currentBusStop = 0;
        protected int peopleInTheBus = 0;

        // Heh. :) He he he he... He... Yeah.
        protected int peopleGettingOff = 0;
        protected float getOffCounter = 0;
        protected float getOffInterval = 2; // Seconds.

        protected int minGettingOff = 1;
        protected int maxGettingOff = 5;

        protected bool BusOnTheBusStop = false;

        private void NextBusStop()
        {
            if (currentBusStop + 1 >= busStopsOrder.Count)
                currentBusStop = 0;
            else
                currentBusStop += 1;

            if (busStops[busStopsOrder[currentBusStop]].pedestrians.Count == 0)
            {
                busStops[busStopsOrder[currentBusStop]].SpawnNewPedestrians();
            }

            peopleGettingOff = Helper.random.Next(minGettingOff, maxGettingOff + 1);
            if (peopleGettingOff > peopleInTheBus)
            {
                peopleGettingOff = peopleInTheBus;
            }
        }

        /// <summary>
        /// Load map file.
        /// </summary>
        public void LoadMapFile(string fileName)
        {
            busStops = settingsHandling.LoadMap(fileName);
        }

        /// <summary>
        /// Load bus stops order file.
        /// </summary>
        public void LoadOrderFile(string fileName)
        {
            busStopsOrder = settingsHandling.LoadOrder(fileName);
        }

        /// <summary>
        /// Get all stop areas for drawing.
        /// </summary>
        /// <returns></returns>
        public List<Object> GetAllStopAreas()
        {
            List<Object> list = new List<Object>();
            /*
            foreach (BusStop busStop in busStops)
            {
                list.Add(new Object("", busStop.GetStopAreaPosition(), busStop.stopAreaSize, busStop.stopAreaDirection));
            }
            */
            BusStop busStop = busStops[busStopsOrder[currentBusStop]];

            list.Add(new Object(BusOnTheBusStop.ToString(), busStop.GetStopAreaPosition(), busStop.stopAreaSize, busStop.stopAreaDirection));

            return list;
        }

        /// <summary>
        /// Get all stop areas for drawing.
        /// </summary>
        /// <returns></returns>
        public List<Object> GetAllPedestrians()
        {
            List<Object> list = new List<Object>();

            foreach (BusStop busStop in busStops)
            {
                foreach (BusStop.Pedestrian pedestrian in busStop.pedestrians)
                    list.Add(new Object(pedestrian.GetSkin().ToString(), pedestrian.GetPosition(), pedestrian.GetSize(), pedestrian.GetDirection()));

                foreach (BusStop.Pedestrian pedestrian in busStop.pedestriansWhoGotOff)
                    list.Add(new Object(pedestrian.GetSkin().ToString(), pedestrian.GetPosition(), pedestrian.GetSize(), pedestrian.GetDirection()));
            }

            return list;
        }

        /// <summary>
        /// Get points which should be displayed on the screen.
        /// </summary>
        public Vector2[] GetPointsToDraw()
        {
            List<Vector2> list = new List<Vector2>();
            Vector2[] pointsArray;

            foreach (BusStop busStop in busStops)
            {
                pointsArray = busStop.GetStopAreaCollisionPoints();
                foreach (Vector2 point in pointsArray)
                    list.Add(Helper.MapPosToScreenPos(point));

                list.Add(Helper.MapPosToScreenPos(busStop.GetStopAreaPosition()));

                pointsArray = busStop.GetWaitingAreaCollisionPoints();

                foreach (Vector2 point in pointsArray)
                    list.Add(Helper.MapPosToScreenPos(point));

                list.Add(Helper.MapPosToScreenPos(busStop.GetWaitingAreaPosition()));

                pointsArray = busStop.GetSignCollisionPoints();

                foreach (Vector2 point in pointsArray)
                    list.Add(Helper.MapPosToScreenPos(point));

                list.Add(Helper.MapPosToScreenPos(busStop.GetSignPosition()));

                foreach (BusStop.Pedestrian pedestrian in busStop.pedestrians)
                {
                    list.Add(Helper.MapPosToScreenPos(pedestrian.GetPosition()));

                    pointsArray = pedestrian.GetCollisionPoints();

                    foreach (Vector2 point in pointsArray)
                        list.Add(Helper.MapPosToScreenPos(point));
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Get the number of passangers.
        /// </summary>
        public int NumberOfPedestriansInTheBus()
        {
            return peopleInTheBus;
        }

        /// <summary>
        /// Pozycja przystanku do którego jedziemy.
        /// </summary>
        public Vector2 GetCurrentBusStopPosition()
        {
            return busStops[busStopsOrder[currentBusStop]].GetStopAreaPosition();
        }

        protected class SettingsHandling
        {
            /// <summary>
            /// Check if the text is a comment (begins with //).
            /// </summary>
            private bool TextIsComment(string text)
            {
                if (text[0] == '/' && text[1] == '/')
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// Load bus stops.
            /// </summary>
            public List<BusStop> LoadMap(string filePath)
            {
                filePath = "maps/" + filePath;
                List<BusStop> tempBusStops = new List<BusStop>();

                if (File.Exists(filePath))
                {
                    // Przygotuj StreamReader.
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    Encoding encoding = Encoding.GetEncoding("Windows-1250");
                    StreamReader streamReader = new StreamReader(fileStream, encoding);

                    // Zmienna pozwalająca na ustalenie czy czas rozpocząć odczytywanie.
                    bool beginningEncountered = false;

                    while (true)
                    {
                        // Odczytaj nową linię.
                        string line = streamReader.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        // Jeśli rozpoczęto odczyt.
                        if (beginningEncountered)
                        {
                            // Jeśli należy zakończyć odczyt, wyjdź z pętli (koniec tekstu v koniec sekcji bus_stops).
                            if (line[0] == '*')
                            {
                                break;
                            }

                            // Jeśli odczytano tekst niebędący komentarzem.
                            if (!TextIsComment(line))
                            {
                                string[] split = line.Split(new char[] { ';' });
                                BusStop busStop = new BusStop(int.Parse(split[0]), new Vector2(float.Parse(split[1]), float.Parse(split[2])), int.Parse(split[3]), new Vector2(float.Parse(split[4]), float.Parse(split[5])), int.Parse(split[6]), new Vector2(float.Parse(split[7]), float.Parse(split[8])), int.Parse(split[9]));

                                tempBusStops.Add(busStop);
                            }
                        }

                        if (line == "*busstops*")
                        {
                            beginningEncountered = true;
                        }
                    }
                }

                return tempBusStops;
            }

            /// <summary>
            /// Load the order of the bus stops.
            /// </summary>
            public List<Int32> LoadOrder(string filePath)
            {
                filePath = "maps/" + filePath;
                List<Int32> tempOrder = new List<Int32>();

                if (File.Exists(filePath))
                {
                    // Przygotuj StreamReader.
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    Encoding encoding = Encoding.GetEncoding("Windows-1250");
                    StreamReader streamReader = new StreamReader(fileStream, encoding);

                    string line;

                    // Przeliteruj przez wszystkie linie przypisując je do line.
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        // Jeśli odczytano tekst niebędący komentarzem.
                        if (!TextIsComment(line))
                        {
                            tempOrder.Add(Int32.Parse(line));
                        }
                    }
                }

                return tempOrder;
            }
        }

        public void Update(BusLogic busLogic, TimeSpan framesInterval)
        {
            float timeCoherenceMultiplier = (float)framesInterval.Milliseconds / 1000;

            for (int i = 0; i < busStops.Count; i++)
            {
                if (i == busStopsOrder[currentBusStop])
                {
                    // Sprawdź czy autobus stoi na przystanku i czy ma otwarte drzwi.
                    // 1. Przygotuj punkty kolizji itp.
                    Vector2[] busCollisionPoints = busLogic.GetCollisionPoints();
                    Vector2[] busStopCollisionPoints = busStops[i].GetStopAreaCollisionPoints();
                    MyRectangle busStopRectangle = new MyRectangle(busStopCollisionPoints[3], busStopCollisionPoints[2], busStopCollisionPoints[1], busStopCollisionPoints[0]);

                    // 2. Sprawdź czy autobus stoi na przystanku (przyjmij true i zmień na false jeśli któryś punkt nie znajduje się w prostokącie).
                    BusOnTheBusStop = true;
                    foreach (Vector2 point in busCollisionPoints)
                    {
                        if (!Helper.IsInside(point, busStopRectangle))
                            BusOnTheBusStop = false;
                    }

                    // Przyjmij, że pomimo dobrego ustawienia nie należy iść do autobusu.
                    bool goToTheBus = false;

                    // Jeśli autobus jest dobrze ustawiony i drzwi są otwarte.
                    if (BusOnTheBusStop && busLogic.DoorsAreOpen())
                    {
                        // Idź do autobusu.
                        goToTheBus = true;

                        // Jeśli są pasażerowie którzy mieliby potencjalnie wysiadać
                        if (peopleGettingOff > 0)
                        {
                            // Zmniejsz interwał od ostatniego wyjścia z autobusu.
                            getOffCounter -= timeCoherenceMultiplier;

                            // Jeśli interwał jest dostatecznie mały podejmij działania.
                            if (getOffCounter < 0)
                            {
                                // Wysadzamy ludzi.
                                busStops[i].pedestriansWhoGotOff.Add(new BusStop.Pedestrian(busStops[i].waitingArea, busStops[i].waitingAreaSize, busStops[i].waitingAreaDirection, busLogic.GetBusPosition()));
                                peopleGettingOff -= 1;
                                peopleInTheBus -= 1;

                                // Zresetuj licznik.
                                getOffCounter = getOffInterval;
                            }
                        }
                    }

                    // Funkcja aktualizująca zwraca ilość ludzi którzy zdołali dotrzeć do autubusu.
                    peopleInTheBus += busStops[i].Update(goToTheBus, busLogic, timeCoherenceMultiplier);

                    // Przełączenie do następnego przystanku.
                    if (busStops[i].pedestrians.Count == 0 && peopleGettingOff == 0)
                        NextBusStop();
                }
                else
                {
                    busStops[i].Update(false, busLogic, timeCoherenceMultiplier);
                }

                // Usuwamy wszystkich pieszych którzy wcześniej zostali przeznaczeni do skasowania ze względu na osiągnięcie autobusu.
                busStops[i].pedestrians.RemoveAll(delegate(BusStop.Pedestrian pedestrian)
                {
                    if (pedestrian.delete)
                        return true;
                    else
                        return false;
                });
            }
        }
    }
}