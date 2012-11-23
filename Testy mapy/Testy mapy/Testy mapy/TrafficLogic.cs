using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class TrafficLogic
    {
        public class Road // Droga, otrzymuje ją każdy pojazd.
        {
            public class Lane // Pas ruchu, przechowuje dane pozwalając uniknąć powtarzalnych obliczeń.
            {
                public Vector2 start;
                public Vector2 end;
                public float direction;
            }

            public Vector2 start;          // Punkt początkowy drogi.
            public Vector2 end;            // Punkt końcowy drogi.
            public Lane lane = new Lane(); // Pas ruchu.
            private float width = 50;      // Określa o ile zmieniana jest współrzędna podczas obliczania przesunięcia pasa ruchu.

            /// <summary>
            /// Checks if end of the road has been reached.
            /// </summary>
            /// <param name="position">Current position</param>
            public bool EndReached(Vector2 position)
            {
                if (lane.direction == 0) // Droga w górę.
                {
                    if (position.Y <= end.Y)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 180) // Droga w dół.
                {
                    if (position.Y >= end.Y)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 90) // Droga w prawo.
                {
                    if (position.X >= end.X)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 270) // Droga w lewo.
                {
                    if (position.X <= end.X)
                        return true;
                    else
                        return false;
                }

                return false;
            }

            public Road(Vector2 start, Vector2 end, Vector2 center) // Constructor.
            {
                this.start = start;    // Przypisz początek.
                this.end = end;        // Przypisz koniec.
                CalculateLane(center); // Przelicz pas ruchu dla tej drogi (dane klasy Lane).
            }

            /// <summary>
            /// Calculate the direction beetween two points.
            /// </summary>
            /// <param name="start">Point 1.</param>
            /// <param name="end">Point 2.</param>
            /// <returns>Degrees.</returns>
            private float CalculateDirection(Vector2 start, Vector2 end)
            {
                float direction = MathHelper.ToDegrees((float)Math.Atan2(end.X - start.X, start.Y - end.Y)); //oblicz kierunek

                if (direction < 0) //jesli wyjdzie ujemny skoryguj to
                    direction += 360;

                return direction;
            }

            /// <summary>
            /// Calculate lane offset for the road. This function is used by the constructor.
            /// </summary>
            /// <param name="center">Road's junction center.</param>
            private void CalculateLane(Vector2 center)
            {
                if (start.X == end.X && start.Y == end.Y) // Jeśli droga jest jednym punktem, oblicz jej kierunek w oparciu o centrum skrzyżowania, z którego wychodzi.
                {
                    float preDirection = CalculateDirection(center, start);

                    if (preDirection > 315 || preDirection < 45)
                        this.lane.direction = 0;

                    if (preDirection > 45 && preDirection < 135)
                        this.lane.direction = 90;

                    if (preDirection > 135 && preDirection < 225)
                        this.lane.direction = 180;

                    if (preDirection > 225 && preDirection < 315)
                        this.lane.direction = 270;
                }
                else // Jeśli nie jest jednym punktem, licz w oparciu o początek i koniec.
                {
                    if (end.X > start.X)
                        this.lane.direction = 90;

                    if (end.X < start.X)
                        this.lane.direction = 270;

                    if (end.Y > start.Y)
                        this.lane.direction = 180;

                    if (end.Y < start.Y)
                        this.lane.direction = 0;
                }


                // Przelicz przesunięcie pasa ruchu w zależności od kierunku drogi.
                if (lane.direction == 0)
                {
                    lane.start = new Vector2(start.X + width, start.Y);
                    lane.end = new Vector2(end.X + width, end.Y);
                }

                if (lane.direction == 180)
                {
                    lane.start = new Vector2(start.X - width, start.Y);
                    lane.end = new Vector2(end.X - width, end.Y);
                }

                if (lane.direction == 90)
                {
                    lane.start = new Vector2(start.X, start.Y + width);
                    lane.end = new Vector2(end.X, end.Y + width);
                }

                if (lane.direction == 270)
                {
                    lane.start = new Vector2(start.X, start.Y - width);
                    lane.end = new Vector2(end.X, end.Y - width);
                }
            }
        }

        public class VehicleType // Klasa przechowujaca typy pojazdów (jak wyglada, rozmiar itd. - wykorzystywane przy tworzeniu pojazdu).
        {
            public Vector2 size; // Rozmiar pojazdu.
            public int skin;     // Numer sprite'a.
            public int likelihoodOfApperance; // Prawdopodobieństwo pojawienia się - liczba całkowita. Przykład: jeśli wynosi 1 a dla pozostałych dwóch samochodów po 2, to: 1/(2 + 2 + 1) = 0.2

            public Vector2 moveSize;   // Pozwala przesuwać collision points.
            public Vector2 sizeOffset; // Pozwala modyfikować wielkość pojazdu, na podstawie której są obliczne collision points.

            public VehicleType(Vector2 size, int skin, int likelihoodOfApperance, Vector2 moveSize, Vector2 sizeOffset) // Constructor.
            {
                this.size = size;
                this.skin = skin;
                this.likelihoodOfApperance = likelihoodOfApperance;
                this.moveSize = moveSize;
                this.sizeOffset = sizeOffset;
            }

            public VehicleType()
            {
            }
        }

        public class Vehicle // Klasa pojazdu.
        {
            public Road road; // Droga którą jedzie pojazd.
            public RoadsSwitching roadsSwitching; //klasa odpowiadajaca za przekierowania od drogi do drogi (zakrety - czyli także zakręty).

            private bool driving = true;      // Jedzie czy został zmuszony do zatrzymania się?
            private bool redirecting = false; // Czy kieruje się do nowej drogi?
            private float speed;              // Aktualna prędkość.
            private Vector2 position;         // Aktualna pozycja.
            private Vector2 size = new Vector2(50, 100); // Rozmiar.
            private float direction;          // Kierunek ruchu.
            public int skin;                  // Numer sprite'a.
            public Vector2 moveSize;          // Pozwala przesuwać collision points.
            public Vector2 sizeOffset;        // Pozwala modyfikować wielkość pojazdu, na podstawie której są obliczne collision points.

            // Used for moving bus back in case of collision.
            private Vector2 oldPosition;
            private float oldDirection;

            public bool accident = false;      // Czy miał wypadek?
            public float indicatorCounter = 0; // Używane do generowania stałych przerw pomiezy włączeniem/wyłączeniem migaczy.
            public bool indicatorBlink = true; // Światła awaryjne aktywne.

            public Vector2 lastEnd = new Vector2(0, 0); // Koniec ostatniej drogi podawane do ChangeTrack podczas prośby o podanie nowej drogi.

            private float normalSpeed = 20;    // Prędkość standardowa przyjmowana podczas normalnego poruszania się.
            private float acceleration = 70;   // Standardowe przyspieszenie.
            private float speedMultiplier = 4;

            private float stopCounter = 0; // Licznik odpowiedzialny za długość postoju w razie zatrzymania się w celu uniknięcia kolizji.
            private float startAfter = 3;  // Po ilu sekundach od zatrzymania ma wystartować.

            public Vehicle(Vector2 start, Vector2 destination, Vector2 size, int skin, Vector2 moveSize, Vector2 sizeOffset, Vector2 junctionCenter, Vector2 additionalOutpoint) // Constructor.
            {
                this.road = new Road(start, destination, junctionCenter);
                this.position = road.lane.start;
                this.direction = road.lane.direction;
                this.size = size;
                this.skin = skin;
                this.moveSize = moveSize;
                this.sizeOffset = sizeOffset;
                this.lastEnd = additionalOutpoint;
            }

            /// <summary>
            /// Returns 4 collision points.
            /// </summary>
            public Vector2[] GetCollisionPoints()
            {
                Vector2 p1, p2, p3, p4; // Create 4 points.

                // Calculate their positions.
                p3.X = position.X + ((size.X * (float)Math.Cos(MathHelper.ToRadians(direction))) / 2);
                p3.Y = position.Y + ((size.X * (float)Math.Sin(MathHelper.ToRadians(direction))) / 2);

                p4.X = position.X - (size.X * (float)Math.Cos(MathHelper.ToRadians(direction)) / 2);
                p4.Y = position.Y - (size.X * (float)Math.Sin(MathHelper.ToRadians(direction)) / 2);

                p1.X = p4.X + (size.Y * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p1.Y = p4.Y - (size.Y * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p2.X = p3.X + (size.Y * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p2.Y = p3.Y - (size.Y * (float)Math.Cos(MathHelper.ToRadians(direction)));

                // Create list and add points.
                Vector2[] pointsArray = new Vector2[4] { p1, p2, p3, p4 };

                return pointsArray;
            }

            /// <summary>
            /// Points used for detecting if the car should stop.
            /// </summary>
            /// <returns></returns>
            public Vector2[] GetDetectionPoints()
            {
                Vector2 p2, p3;

                p2.X = position.X + ((size.Y + 30) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p2.Y = position.Y - ((size.Y + 30) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p3.X = position.X + (((size.X + 5) * (float)Math.Cos(MathHelper.ToRadians(direction))) / 2);
                p3.Y = position.Y + (((size.X + 5) * (float)Math.Sin(MathHelper.ToRadians(direction))) / 2);

                p3.X = p3.X + ((size.Y + 30) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p3.Y = p3.Y - ((size.Y + 30) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                Vector2[] array = new Vector2[] { p2, p3 };
                return array;
            }

            /// <summary>
            /// Get the center of the vehicle.
            /// </summary>
            /// <returns></returns>
            public Vector2 GetVehiclePosition()
            {
                return CalculateCenter(position, direction);
            }

            /// <summary>
            /// Get vehicle size.
            /// </summary>
            /// <returns></returns>
            public Vector2 GetVehicleSize()
            {
                return size;
            }

            /// <summary>
            /// Get Vehicle direction.
            /// </summary>
            public float GetVehicleDirection()
            {
                return direction;
            }

            /// <summary>
            /// Check if the vehicle is being redirected.
            /// </summary>
            public bool IsRedirecting()
            {
                return redirecting;
            }

            /// <summary>
            /// Calculate center of the vehicle.
            /// </summary>
            private Vector2 CalculateCenter(Vector2 vehiclePosition, float vehicleDirection)
            {
                Vector2 center;

                center.X = vehiclePosition.X + (size.Y * (float)Math.Sin(MathHelper.ToRadians(vehicleDirection)) / 2);
                center.Y = vehiclePosition.Y - (size.Y * (float)Math.Cos(MathHelper.ToRadians(vehicleDirection)) / 2);

                return center;
            }

            /// <summary>
            /// Calculate the new position based on speed and direction.
            /// </summary>
            private Vector2 CalculateNewPosition(float vehicleSpeed, float vehicleDirection, float timeCoherenceMultiplier)
            {
                Vector2 newPosition;
                newPosition.X = position.X + (speedMultiplier * timeCoherenceMultiplier * vehicleSpeed * (float)Math.Sin(MathHelper.ToRadians(vehicleDirection)));
                newPosition.Y = position.Y - (speedMultiplier * timeCoherenceMultiplier * vehicleSpeed * (float)Math.Cos(MathHelper.ToRadians(vehicleDirection)));
                return newPosition;
            } 

            /// <summary>
            /// Start driving.
            /// </summary>
            public void Start(float timeCoherenceMultiplier)
            {
                stopCounter += timeCoherenceMultiplier;
                if (stopCounter > startAfter)
                    driving = true;
            } 

            /// <summary>
            /// Stop driving.
            /// </summary>
            public void Stop()
            {
                driving = false;
                stopCounter = 0;
            } 

            /// <summary>
            /// Simulate the collsion.
            /// </summary>
            public void Collision()
            {
                accident = true;
                RewindPositionAndDirection();
            }

            /// <summary>
            /// Rewind position and direction in case of collision (avoid being stuck inside someting).
            /// </summary>
            public void RewindPositionAndDirection()
            {
                direction = oldDirection;
                position = oldPosition;
            }

            public class RoadsSwitching // Odpowiada za zmianę drogi.
            {
                private float bezierT = 0; // Dynamiczny parametr dla funkcji generującej krzywe Beziera.
                private float bezierTInc = (float)0.01; // Co ile zmienić parametr z każdym wywołaniem?
                private Vector2 start; // Początek (koniec Lane'a poprzedniej drogi).
                private Vector2 end;   // Koniec (początek Lane'a nowej drogi).
                private Vector2 controlPoint; // Punkt dodatkowy dla krzywych Beziera.
                public Vector2 target; // Dokąd aktualnie jedzie pojazd?

                /// <summary>
                /// Calculate control point for Bezier curve.
                /// </summary>
                /// <param name="start">Previous road ending.</param>
                /// <param name="end">New road beginning.</param>
                /// <param name="center">Junction center.</param>
                private Vector2 CalculateControlPoint(Vector2 start, Vector2 end, Vector2 center)
                {
                    Vector2 controlPoint = new Vector2(0, 0);

                    if (start.X == end.X || start.Y == end.Y)
                    {
                        controlPoint.X = start.X - (start.X - end.X);
                        controlPoint.Y = start.Y - (start.Y - end.Y);
                    }
                    else
                    {
                        if (((end.X < start.X && end.Y < start.Y) || (end.X < start.X && end.Y > start.Y)) && (center.X < start.X && center.Y >= start.Y)) // Z prawego.
                        {
                            controlPoint.X = start.X - (start.X - end.X);
                            controlPoint.Y = start.Y;
                        }

                        if (((end.X < start.X && end.Y < start.Y) || (end.X > start.X && end.Y < start.Y)) && (center.X < start.X && center.Y < start.Y)) // Z dolnego.
                        {
                            controlPoint.X = start.X;
                            controlPoint.Y = start.Y - (start.Y - end.Y);
                        }

                        if (((end.Y < start.Y && end.X > start.X) || (end.Y > start.Y && end.X > start.X)) && (center.X > start.X && center.Y <= start.Y)) // Z lewego.
                        {
                            controlPoint.X = start.X + (end.X - start.X);
                            controlPoint.Y = start.Y;
                        }

                        if (((end.X < start.X && end.Y > start.Y) || (end.X > start.X && end.Y > start.Y)) && (center.X > start.X && center.Y > start.Y)) // Z górnego.
                        {
                            controlPoint.X = start.X;
                            controlPoint.Y = start.Y + (end.Y - start.Y);
                        }
                    }



                    return controlPoint;
                }

                public RoadsSwitching(Vector2 start, Vector2 end, Vector2 center) // Constructor.
                {
                    this.start = start;
                    this.end = end;
                    this.controlPoint = CalculateControlPoint(start, end, center);
                    this.target = GetNewPoint();
                }

                /// <summary>
                /// Is the new road simply the continuation of the previous one?
                /// </summary>
                public bool IsStraight()
                {
                    bool straight = true;
                    if (start.X == end.X || start.Y == end.Y)
                        straight = true;
                    else
                        straight = false;
                    return straight;
                }

                /// <summary>
                /// Calculate direction beetween two points.
                /// </summary>
                /// <param name="start">Point 1.</param>
                /// <param name="end">Point 2.</param>
                public float CalculateDirection(Vector2 start, Vector2 end)
                {
                    float direction = MathHelper.ToDegrees((float)Math.Atan2(end.X - start.X, start.Y - end.Y));
                    if (direction < 0)
                        direction += 360;

                    return direction;
                }

                /// <summary>
                /// Dunction generating Bezier curve.
                /// </summary>
                /// <returns></returns>
                public Vector2 GetNewPoint()
                {
                    Vector2 point;
                    bezierT += bezierTInc;

                    if (bezierT > 1)
                        return end;

                    bezierT = (float)Math.Round(bezierT, 5);

                    point.X = (float)((1 - bezierT) * (1 - bezierT) * start.X + 2 * (1 - bezierT) * bezierT * controlPoint.X + bezierT * bezierT * end.X);
                    point.Y = (float)((1 - bezierT) * (1 - bezierT) * start.Y + 2 * (1 - bezierT) * bezierT * controlPoint.Y + bezierT * bezierT * end.Y);

                    return point;
                }

                /// <summary>
                /// Hes the given point been reached?
                /// </summary>
                public bool Reached(Vector2 position, Vector2 point)
                {
                    if (start.X < end.X && start.Y > end.Y)
                    {
                        if (position.X + 5 > point.X && position.Y - 5 < point.Y)
                            return true;
                        else
                            return false;
                    }

                    if (start.X < end.X && start.Y < end.Y)
                    {
                        if (position.X + 5 > point.X && position.Y + 5 > point.Y)
                            return true;
                        else
                            return false;
                    }

                    if (start.X > end.X && start.Y < end.Y)
                    {
                        if (position.X - 5 < point.X && position.Y + 5 > point.Y)
                            return true;
                        else
                            return false;
                    }

                    if (start.X > end.X && start.Y > end.Y)
                    {
                        if (position.X - 5 < point.X && position.Y - 5 < point.Y)
                            return true;
                        else
                            return false;
                    }

                    return false;
                }
            }

            public void Update(DrawMap drawMap, float timeCoherenceMultiplier)
            {
                if (driving)
                {
                    speed += acceleration * timeCoherenceMultiplier; // Przyspieszaj.
                    if (speed > normalSpeed)
                        speed = normalSpeed;
                }
                else
                {
                    speed -= acceleration * timeCoherenceMultiplier; // Zwalniaj.
                    if (speed < 0)
                        speed = 0;
                }

                oldDirection = direction;
                oldPosition = position;

                if (!redirecting)
                {
                    if (road.EndReached(position))
                    {
                        Vector2 junctionCenter;
                        Connection getNewRoad;

                        drawMap.ChangeTrack(road.end, lastEnd, out getNewRoad, out junctionCenter); // Zapytaj o nową drogę.

                        lastEnd = road.end; // Koniec poprzedniej drogi to teraz koniec drogi aktualnej.

                        Road newRoad = new Road(getNewRoad.point1, getNewRoad.point2, junctionCenter); // Generujemy nową drogę w oparciu o punkty podane przez funkcje ChangeTrack.

                        roadsSwitching = new RoadsSwitching(road.lane.end, newRoad.lane.start, junctionCenter); // Generujemy klasę przekierowującą.

                        if (!roadsSwitching.IsStraight()) // Jeśli droga nie jest naprzeciwko rozpoczynamy przekierowanie.
                            redirecting = true;

                        road = newRoad; // Auto otrzymuje nową drogę.
                    }
                    else
                    {
                        position = CalculateNewPosition(speed, direction, timeCoherenceMultiplier);
                    }
                }
                else
                {
                    if (roadsSwitching.Reached(position, road.lane.start))
                    {
                        redirecting = false; // Jeśli już dojechaliśmy do nowej drogi ustawmy odpowiednio auto i dajmy mu normalnie jechać.
                        direction = road.lane.direction;
                        position = road.lane.start;
                    }
                    else
                    {
                        Vector2 newPoint = new Vector2(0, 0);

                        while (roadsSwitching.Reached(position, roadsSwitching.target)) 
                        {                                              // Jeśli dojechaliśmy do punktu wygenerowanego poprzednio
                            newPoint = roadsSwitching.GetNewPoint();   // szukaj takiego nowego do którego jeszcze nie dojechalismy.
                            roadsSwitching.target = newPoint;
                        }

                        direction = roadsSwitching.CalculateDirection(position, roadsSwitching.target);
                        position = CalculateNewPosition(speed, direction, timeCoherenceMultiplier);
                    }
                }
            }
        }

        public List<Vehicle> vehicles = new List<Vehicle>();
        private int maxVehicles = 10;    // Maksymalna liczba pojazdów
        private float spawnInterval = 5; // Co ile spawnować nowe pojazdy [s]
        private float lastSpawn = 0;     // Ostatni spawn [s]
        private Vector2 spawnDistance = new Vector2(2000, 2000); // Maksymalna odleglość spawowania od autobusu.
        private float minVehicleSpawnDistance = 100; // Minimalna odleglość od innego samochodu aby zespanowac.
        private float distanceToDelete = 2000; // Samochody będace dalej niż podany dystans zostaną usunięte.

        Vector2 indicatorTextureSize = new Vector2(50, 50); // Rozmiar tekstury migacza.
        float indicatorBlinkInterval = 1; // Jak czesto mają migać migacze.

        private int maxRandom = 0;

        private VehicleType[] vehiclesTypes;

        public TrafficLogic() // Constructor. Tutaj zdefiniuj typy pojazdów.
        {
            VehicleType vehicleType1 = new VehicleType(new Vector2(40, 100), 0, 1, new Vector2(5, -18), new Vector2(14, 10));
            VehicleType vehicleType2 = new VehicleType(new Vector2(40, 100), 1, 1, new Vector2(-1, -7), new Vector2(5, 5));
            VehicleType vehicleType3 = new VehicleType(new Vector2(40, 100), 2, 1, new Vector2(0, -15), new Vector2(10, 5));

            vehiclesTypes = new VehicleType[3] { vehicleType1, vehicleType2, vehicleType3 };

            foreach (VehicleType vehicleType in vehiclesTypes)
            {
                maxRandom += vehicleType.likelihoodOfApperance;
            }
        }

        /// <summary>
        /// Check if the road in front of the car is clear.
        /// </summary>
        public bool IsRoadClear(Vector2[] points, BusLogic busLogic)
        {
            foreach (Vector2 point in points)
            {
                Vector2[] collisionPoints;
                MyRectangle rectangle;

                if (Helper.CalculateDistance(busLogic.GetBusPosition(), point) < 200) // Jeśli autobus jest blisko, sprawdź go.
                {
                    collisionPoints = busLogic.GetCollisionPoints(busLogic.GetRealPosition(), busLogic.GetDirection());
                    rectangle = new MyRectangle(collisionPoints[3], collisionPoints[2], collisionPoints[1], collisionPoints[0]);
                    if (Helper.IsInside(point, rectangle))
                        return false;
                }

                foreach (Vehicle vehicle in vehicles)
                {
                    if (Helper.CalculateDistance(vehicle.GetVehiclePosition(), point) < 200) // Jeśli dany pojazd jest dostatecznie blisko, sprawdż go.
                    {
                        collisionPoints = vehicle.GetCollisionPoints();
                        rectangle = new MyRectangle(collisionPoints[0], collisionPoints[1], collisionPoints[2], collisionPoints[3]);
                        if (Helper.IsInside(point, rectangle))
                            return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Get the list of all vehicles as Objects.
        /// </summary>
        public List<Object> GetAllVehicles()
        {
            List<Object> list = new List<Object>();

            foreach (Vehicle vehicle in vehicles)
            {
                Vector2 size = vehicle.GetVehicleSize();
                size.X += vehicle.sizeOffset.X;
                size.Y += vehicle.sizeOffset.Y;

                Vector2 position = vehicle.GetVehiclePosition();
                position.X = position.X + (vehicle.moveSize.Y * (float)Math.Sin(MathHelper.ToRadians(vehicle.GetVehicleDirection()))); // Przesuwamy do góry.
                position.Y = position.Y - (vehicle.moveSize.Y * (float)Math.Cos(MathHelper.ToRadians(vehicle.GetVehicleDirection())));

                position.X = position.X + (vehicle.moveSize.X * (float)Math.Sin(MathHelper.ToRadians(vehicle.GetVehicleDirection() + 90))); // Przesuwamy w prawo.
                position.Y = position.Y - (vehicle.moveSize.X * (float)Math.Cos(MathHelper.ToRadians(vehicle.GetVehicleDirection() + 90)));

                list.Add(new Object(vehicle.skin.ToString(), position, size, vehicle.GetVehicleDirection()));
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

            foreach (Vehicle vehicle in vehicles)
            {
                pointsArray = vehicle.GetCollisionPoints();
                foreach (Vector2 point in pointsArray)
                    list.Add(Helper.MapPosToScreenPos(point));

                pointsArray = vehicle.GetDetectionPoints();

                foreach (Vector2 point in pointsArray)
                    list.Add(Helper.MapPosToScreenPos(point));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Get the positions of all active indicators.
        /// </summary>
        public List<Object> GetIndicatorPoints()
        {
            List<Object> list = new List<Object>();
            Vector2[] pointsArray;

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.accident && vehicle.indicatorBlink)
                {
                    pointsArray = vehicle.GetCollisionPoints();
                    foreach (Vector2 point in pointsArray)
                        list.Add(new Object("", point, indicatorTextureSize, 0));
                }
            }

            return list;
        }

        /// <summary>
        /// Checks if there are no cars too close to the spawned car.
        /// </summary>
        private bool NoVehicleNearby(Vector2 center, Vector2 size)
        {
            foreach (Vehicle vehicle in vehicles)
            {
                float minDistance = (float)Math.Sqrt(Math.Pow(size.X / 2, 2) + Math.Pow(size.Y / 2, 2)) + (float)Math.Sqrt(Math.Pow(vehicle.GetVehicleSize().X / 2, 2) + Math.Pow(vehicle.GetVehicleSize().Y / 2, 2));
                if (Helper.CalculateDistance(center, vehicle.GetVehiclePosition()) < minDistance + minVehicleSpawnDistance)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Function creating new vehicles.
        /// </summary>
        /// <param name="drawMap"></param>
        private void CreateNewVehicles(DrawMap drawMap)
        {
            if (vehicles.Count() < maxVehicles && lastSpawn > spawnInterval)
            {
                Vector2 junctionCenter, additionalOutpoint;
                Connection getNewRoad;

                drawMap.CreateTrack(spawnDistance, out getNewRoad, out junctionCenter, out additionalOutpoint);

                //if (!(getNewRoad.point1.X == 600 && getNewRoad.point1.Y == 450))
                  //  return;

                if (!getNewRoad.IsEmpty() && junctionCenter.X != 0 && junctionCenter.Y != 0)
                {
                    Random random = new Random();
                    int randomNumber = random.Next(1, maxRandom + 1);
                    int current = 0;

                    VehicleType type = new VehicleType();

                    foreach (VehicleType vehicleType in vehiclesTypes)
                    {
                        if (randomNumber > current && randomNumber <= current + vehicleType.likelihoodOfApperance)
                        {
                            type = vehicleType;
                            break;
                        }

                        current += vehicleType.likelihoodOfApperance;
                    }

                    Vehicle vehicle = new Vehicle(getNewRoad.point1, getNewRoad.point2, type.size, type.skin, type.moveSize, type.sizeOffset, junctionCenter, additionalOutpoint);

                    if (NoVehicleNearby(vehicle.GetVehiclePosition(), vehicle.GetVehicleSize()))
                    {
                        vehicles.Add(vehicle);
                        lastSpawn = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Main function.
        /// </summary>
        public void Update(DrawMap drawMap, BusLogic busLogic, TimeSpan framesInterval)
        {
            float timeCoherenceMultiplier = (float)framesInterval.Milliseconds / 1000; // Czas pomiędzy dwoma tickami służący do utrzymania spójności obliczeń. [s]

            lastSpawn += timeCoherenceMultiplier; // Zwiększmy czas od ostatniego spawnu.
            CreateNewVehicles(drawMap); // Stwórzmy nowe pojazdy.

            vehicles.RemoveAll(delegate(Vehicle vehicle) // Usuńmy pojazdy będące za daleko.
            {
                return Helper.CalculateDistance(busLogic.GetBusPosition(), vehicle.GetVehiclePosition()) > distanceToDelete;
            });


            foreach (Vehicle vehicle in vehicles) // Dla każdego pojazdu...
            {
                if (!vehicle.accident)
                {
                    if (IsRoadClear(vehicle.GetDetectionPoints(), busLogic)) // ...sprawdź czy ma się zatrzymać
                        vehicle.Start(timeCoherenceMultiplier);
                    else
                        vehicle.Stop();

                    if (!vehicle.accident) //jesli nie mial wypadku
                        vehicle.Update(drawMap, timeCoherenceMultiplier); // ...zaktualizuj jego pozycję
                }
                else
                {
                    vehicle.indicatorCounter += timeCoherenceMultiplier; // ...zajmij się migaczami.
                    if (vehicle.indicatorCounter > indicatorBlinkInterval)
                    {
                        vehicle.indicatorBlink = !vehicle.indicatorBlink;
                        vehicle.indicatorCounter = 0;
                    }
                }
            }
        }
    }
}