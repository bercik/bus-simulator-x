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
            public bool EndReached(Vector2 position, float VehicleSizeY)
            {
                float aditionalSpace = VehicleSizeY * 1.5f;

                if (lane.direction == 0) // Droga w górę.
                {
                    if (position.Y - aditionalSpace <= end.Y)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 180) // Droga w dół.
                {
                    if (position.Y + aditionalSpace >= end.Y)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 90) // Droga w prawo.
                {
                    if (position.X + aditionalSpace >= end.X)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 270) // Droga w lewo.
                {
                    if (position.X - aditionalSpace <= end.X)
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

            public Vector2 moveSize;   // Pozwala przesuwać sprite.
            public Vector2 sizeOffset; // Pozwala modyfikować rozmiar, sprite'a.

            public Vector2 tailLightsOffset;  // Pozwala modyfikować przesunięcie świateł.
            public Vector2 headLightsOffset;
            public Vector2 exhaustPipeOffset; // Pozwala modyfikować przesunięcie rury wydechowej.

            public VehicleType(Vector2 size, int skin, int likelihoodOfApperance, Vector2 moveSize, Vector2 sizeOffset, Vector2 headLightsOffset, Vector2 tailLightsOffset, Vector2 exhaustPipeOffset) // Constructor.
            {
                this.size = size;
                this.skin = skin;
                this.likelihoodOfApperance = likelihoodOfApperance;
                this.moveSize = moveSize;
                this.sizeOffset = sizeOffset;
                this.tailLightsOffset = tailLightsOffset;
                this.headLightsOffset = headLightsOffset;
                this.exhaustPipeOffset = exhaustPipeOffset;
            }

            public VehicleType()
            {
            }
        }

        public class Vehicle // Klasa pojazdu.
        {
            public Road road; // Droga którą jedzie pojazd.
            public Road lastRoad; // Droga którą jechał pojazd.
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

            public Vector2 tailLightsOffset;  // Pozwala modyfikować przesunięcie świateł.
            public Vector2 headLightsOffset;
            public Vector2 exhaustPipeOffset; // Pozwala modyfikować przesunięcie rury wydechowej.

            private float detectionPointsDistance = 60; // Odległość detection points od przodu samochodu.
            private float forwardDetectionPointsDistance = 100; // Odległość forward detection points od samochodu.

            // Used for moving bus back in case of collision.
            private Vector2 oldPosition;
            private float oldDirection;

            public bool accident = false;      // Czy miał wypadek?
            public float indicatorCounter = 0; // Używane do generowania stałych przerw pomiezy włączeniem/wyłączeniem migaczy.
            public bool indicatorBlink = true; // Światła awaryjne aktywne.

            public Vector2 lastEnd = new Vector2(0, 0); // Koniec ostatniej drogi podawane do ChangeTrack podczas prośby o podanie nowej drogi.

            private float normalSpeed = 25;    // Prędkość standardowa przyjmowana podczas normalnego poruszania się.
            private float fastSpeed = 35;      // Prędkość na długich, prostych odcinkach.
            private float maxSpeed = 0;        // Aktualna maksymalna prędkość ustalana na podstawie drogi, pojazdów przed samochodem itp.
            private float minDistanceToFastSpeed = 150; // Odlagłość od końców drogi wymagana dla osiągnięcia tej prędkości.
            private float acceleration = 70;   // Standardowe przyspieszenie.
            private float lightAcceleration = 20;   // Małe przyspieszenie.
            private float speedMultiplier = 4;

            private bool breaking = false;      // Czy zwalnia? Używane do rysowania świateł tylnych.

            private float stopCounter = 0; // Licznik odpowiedzialny za długość postoju w razie zatrzymania się w celu uniknięcia kolizji.
            private float startAfter = 1;  // Po ilu sekundach od zatrzymania ma wystartować.

            public Vehicle(Vector2 start, Vector2 destination, Vector2 size, int skin, Vector2 moveSize, Vector2 sizeOffset, Vector2 junctionCenter, Vector2 additionalOutpoint, Vector2 tailLightsOffset, Vector2 headLightsOffset, Vector2 exhaustPipeOffset) // Constructor.
            {
                this.road = new Road(start, destination, junctionCenter);
                this.position = road.lane.start;
                this.direction = road.lane.direction;
                this.size = size;
                this.skin = skin;
                this.moveSize = moveSize;
                this.sizeOffset = sizeOffset;
                this.lastEnd = additionalOutpoint;
                this.tailLightsOffset = tailLightsOffset;
                this.headLightsOffset = headLightsOffset;
                this.exhaustPipeOffset = exhaustPipeOffset;
            }

            /// <summary>
            /// Returns 4 collision points.
            /// </summary>
            public Vector2[] GetCollisionPoints()
            {
                return GetCollisionPoints(new Vector2(0, 0));
            }

            public Vector2[] GetCollisionPoints(Vector2 offset)
            {
                Vector2 p1, p2, p3, p4; // Create 4 points.

                // Calculate their positions.
                p3.X = position.X + (((size.X + offset.X) * (float)Math.Cos(MathHelper.ToRadians(direction))) / 2);
                p3.Y = position.Y + (((size.X + offset.X) * (float)Math.Sin(MathHelper.ToRadians(direction))) / 2);

                p4.X = position.X - ((size.X + offset.X) * (float)Math.Cos(MathHelper.ToRadians(direction)) / 2);
                p4.Y = position.Y - ((size.X + offset.X) * (float)Math.Sin(MathHelper.ToRadians(direction)) / 2);

                p1.X = p4.X + ((size.Y + offset.Y) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p1.Y = p4.Y - ((size.Y + offset.Y) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p2.X = p3.X + ((size.Y + offset.Y) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p2.Y = p3.Y - ((size.Y + offset.Y) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                // Create list and add points.
                Vector2[] pointsArray = new Vector2[4] { p1, p2, p3, p4 };

                return pointsArray;
            }

            /// <summary>
            /// This version of the function takes no arguments and sets the default distance.
            /// </summary>
            /// <returns></returns>
            public Vector2[] GetDetectionPoints()
            {
                return GetDetectionPoints(detectionPointsDistance);
            }

            public Vector2[] GetForwardDetectionPoints()
            {
                return GetDetectionPoints(forwardDetectionPointsDistance);
            }

            /// <summary>
            /// Points used for detecting if the car should stop.
            /// </summary>
            /// <returns></returns>
            private Vector2[] GetDetectionPoints(float distanceFromFront)
            {
                float widthOffset = 25; // O ile na boki zwiększyć rozmiar dla obliczeń punktów.

                Vector2 frontPosition; // Pozycja środka przodu samochodu.

                frontPosition.X = position.X + (size.Y * (float)Math.Sin(MathHelper.ToRadians(direction)));
                frontPosition.Y = position.Y - (size.Y * (float)Math.Cos(MathHelper.ToRadians(direction)));

                float detectionDirection; // Kierunek detekcji.
                float detectionAngle; // Kąt detekcji.
                if (IsRedirecting() && !roadsSwitching.IsStraight())
                {
                    Vector2 forwardPoint = roadsSwitching.GetForwardPoint(distanceFromFront, frontPosition); //Oblicz kierunek z kierunku ruchu po skrzyżowaniu.
                    detectionDirection = Helper.CalculateDirection(frontPosition, forwardPoint);
                    detectionAngle = Helper.CalculateDirection(position, forwardPoint);
                }
                else
                {
                    detectionDirection = direction; // Przypisz kierunek ruchu samochodu.
                    detectionAngle = detectionDirection;
                }

                Vector2 p1, p2, p3;

                // Oblicz punkt p2.
                p2 = frontPosition; // Już obliczyliśmy tą wartość.

                p2.X += (distanceFromFront * (float)Math.Sin(MathHelper.ToRadians(detectionDirection))); // Przesuń punkt p2 w kierunku detekcji.
                p2.Y -= (distanceFromFront * (float)Math.Cos(MathHelper.ToRadians(detectionDirection)));

                // Oblicz punkt p1.
                p1.X = p2.X - (((size.X + widthOffset) * (float)Math.Cos(MathHelper.ToRadians(detectionAngle))) / 2);
                p1.Y = p2.Y - (((size.X + widthOffset) * (float)Math.Sin(MathHelper.ToRadians(detectionAngle))) / 2);


                // Oblicz punkt p3.
                p3.X = p2.X + (((size.X + widthOffset) * (float)Math.Cos(MathHelper.ToRadians(detectionAngle))) / 2);
                p3.Y = p2.Y + (((size.X + widthOffset) * (float)Math.Sin(MathHelper.ToRadians(detectionAngle))) / 2);

                Vector2[] array = new Vector2[] { p1, p2, p3 };
                return array;
            }

            /// <summary>
            /// Get the center of the vehicle.
            /// </summary>
            public Vector2 GetVehiclePosition()
            {
                return CalculateCenter(position, direction);
            }

            /// <summary>
            /// Get the position directly from the variable.
            /// </summary>
            public Vector2 GetRealVehiclePosition()
            {
                return position;
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
            /// Get the speed of the vehicle.
            /// </summary>
            public float GetVehicleSpeed()
            {
                return speed;
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
            /// Checks if vehicle is driving.
            /// </summary>
            public bool IsDriving()
            {
                return driving;
            }

            /// <summary>
            /// Checks if vehicle is breaking.
            /// </summary>
            public bool IsBreaking()
            {
                return breaking;
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
            private Vector2 CalculateNewPosition(float vehicleSpeed, float vehicleDirection)
            {
                Vector2 newPosition;
                newPosition.X = position.X + (speedMultiplier * Helper.timeCoherenceMultiplier * vehicleSpeed * (float)Math.Sin(MathHelper.ToRadians(vehicleDirection)));
                newPosition.Y = position.Y - (speedMultiplier * Helper.timeCoherenceMultiplier * vehicleSpeed * (float)Math.Cos(MathHelper.ToRadians(vehicleDirection)));
                return newPosition;
            }

            /// <summary>
            /// Start driving.
            /// </summary>
            public void Start()
            {
                stopCounter += Helper.timeCoherenceMultiplier;
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
            /// Used for accelerating.
            /// </summary>
            private void Accelerate(float acceleration)
            {
                speed += acceleration * Helper.timeCoherenceMultiplier; // Przyspieszaj.
                breaking = false;
            }

            /// <summary>
            /// Used for breaking.
            /// </summary>
            private void Break(float acceleration)
            {
                speed -= acceleration * Helper.timeCoherenceMultiplier; // Zwalniaj.
                breaking = true;
            }

            /// <summary>
            /// Simulate the collsion.
            /// </summary>
            public void Collision()
            {
                accident = true;
                driving = true;
                breaking = false;
                RewindPositionAndDirection();
            }

            /// <summary>
            /// Set MaxSpeed od the vehicle.
            /// </summary>
            public void SetMaxSpeed(float speed)
            {
                maxSpeed = speed;
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
                public float bezierT = 0; // Dynamiczny parametr dla funkcji generującej krzywe Beziera.
                private float bezierTInc = (float)0.01; // Co ile zmienić parametr z każdym wywołaniem?
                public Vector2 start; // Początek (koniec Lane'a poprzedniej drogi).
                public Vector2 end;   // Koniec (początek Lane'a nowej drogi).
                private Vector2 controlPoint; // Punkt dodatkowy dla krzywych Beziera.
                public Vector2 target; // Dokąd aktualnie jedzie pojazd?
                public Vector2 center; // Środek skrzyżowania.

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

                // Constructor.
                public RoadsSwitching(Vector2 start, Vector2 end, Vector2 center)
                {
                    this.start = start;
                    this.end = end;
                    this.controlPoint = CalculateControlPoint(start, end, center);
                    this.target = GetNewPoint();
                    this.center = center;
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
                /// Function generating new point from Bezier curve.
                /// </summary>
                public Vector2 GetNewPoint()
                {
                    Vector2 point;
                    bezierT += bezierTInc;

                    if (bezierT > 1)
                        return end;

                    point = GenerateBezierCurve(bezierT);

                    return point;
                }

                /// <summary>
                /// Get one of the next points from the Bezier curve without increasing the bezierT parameter.
                /// </summary>
                /// <param name="distance">The required distance from the given point.</param>
                /// <param name="from">Given point.</param>
                public Vector2 GetForwardPoint(float distance, Vector2 from)
                {
                    float bezierParameterT = bezierT;
                    Vector2 point;
                    float lastDistance;
                    bool increasing = false;

                    point = GenerateBezierCurve(bezierParameterT);
                    lastDistance = Helper.CalculateDistance(from, point);

                    while (true)
                    {
                        bezierParameterT += bezierTInc;
                        point = GenerateBezierCurve(bezierParameterT);

                        float currentDistance = Helper.CalculateDistance(from, point);

                        if (currentDistance > lastDistance)
                            increasing = true;

                        if (increasing && currentDistance >= distance)
                            return point;

                        lastDistance = currentDistance;
                    }
                }

                /// <summary>
                /// Private function generating Bezier curve.
                /// </summary>
                private Vector2 GenerateBezierCurve(float bezierParameterT)
                {
                    Vector2 point;
                    bezierParameterT = (float)Math.Round(bezierParameterT, 5);

                    point.X = (float)((1 - bezierParameterT) * (1 - bezierParameterT) * start.X + 2 * (1 - bezierParameterT) * bezierParameterT * controlPoint.X + bezierParameterT * bezierParameterT * end.X);
                    point.Y = (float)((1 - bezierParameterT) * (1 - bezierParameterT) * start.Y + 2 * (1 - bezierParameterT) * bezierParameterT * controlPoint.Y + bezierParameterT * bezierParameterT * end.Y);

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

                    if (start.X == end.X && start.Y < end.Y) // Down.
                    {
                        if (position.Y > point.Y)
                            return true;
                        else
                            return false;
                    }

                    if (start.X == end.X && start.Y > end.Y) // Up.
                    {
                        if (position.Y < point.Y)
                            return true;
                        else
                            return false;
                    }

                    if (start.Y == end.Y && start.X < end.X) // Right.
                    {
                        if (position.X > point.X)
                            return true;
                        else
                            return false;
                    }

                    if (start.Y == end.Y && start.X > end.X) // Left.
                    {
                        if (position.X < point.X)
                            return true;
                        else
                            return false;
                    }




                    return false;
                }
            }

            public void Update(DrawMap drawMap)
            {
                // Jeśli maxSpeed nie został nadpisany przez funkcję wykrywającą czy należy zwolnić z powodu innego pojazdu.
                if (maxSpeed == 0)
                {
                    // Dostosuj prędkość na podstawie odległości do końca drogi.
                    if (Helper.CalculateDistance(GetVehiclePosition(), road.lane.end) > minDistanceToFastSpeed && !IsRedirecting())
                    {
                        // Daleko, szybka jazda.
                        maxSpeed = fastSpeed;
                    }
                    else
                    {
                        // Blisko, wolna jazda.
                        maxSpeed = normalSpeed;
                    }
                }

                // Zaokrąglij maxSpeed (mogła zostać nadpisana i zawierać dziwny ułamek).
                maxSpeed = (float)Math.Round(maxSpeed, 0);

                // Accelerate or decelerate.
                if (driving)
                {
                    // Light speed changes from normal to fast speed etc.
                    if (speed <= maxSpeed)
                    {
                        Accelerate(lightAcceleration);
                        if (speed > maxSpeed)
                            speed = maxSpeed;
                    }
                    else
                    {
                        Break(lightAcceleration);
                        if (speed < 0)
                            speed = 0;
                    }
                }
                else
                {
                    // Hard breaking.
                    Break(acceleration);
                    if (speed < 0)
                        speed = 0;
                }

                // Remember old position and direction for collisions.
                oldDirection = direction;
                oldPosition = position;

                if (!redirecting)
                {
                    // Jeśli dojechał do końca drogi.
                    if (road.EndReached(position, size.Y))
                    {
                        Vector2 junctionCenter;
                        Connection getNewRoad;

                        // Ask for a new road.
                        drawMap.ChangeTrack(road.end, lastEnd, out getNewRoad, out junctionCenter);

                        // Koniec poprzedniej drogi to teraz koniec drogi aktualnej.
                        lastEnd = road.end;
                        lastRoad = road;

                        // Generujemy nową drogę w oparciu o punkty podane przez funkcje ChangeTrack.
                        Road newRoad = new Road(getNewRoad.point1, getNewRoad.point2, junctionCenter);

                        // Generujemy klasę przekierowującą.
                        roadsSwitching = new RoadsSwitching(road.lane.end, newRoad.lane.start, junctionCenter); 

                        redirecting = true;

                        // Auto otrzymuje nową drogę.
                        road = newRoad;
                    }
                    else
                    {
                        // Nie ma żanych przekierowywań, nie dojechał do końca etc.
                        position = CalculateNewPosition(speed, direction);
                    }
                }
                else
                {
                    if (roadsSwitching.Reached(position, road.lane.start))
                    {
                        // Jeśli już dojechaliśmy do nowej drogi ustawmy odpowiednio auto i dajmy mu normalnie jechać.
                        redirecting = false;
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

                        direction = Helper.CalculateDirection(position, roadsSwitching.target);
                        position = CalculateNewPosition(speed, direction);
                    }
                }

                maxSpeed = 0; // Zresetuj maxSpeed - potem będzie można wykryć czy został nadpisany.
            }
        }

        private float minVehicleSpawnDistance = 100; // Minimalna odleglość od innego samochodu aby zespanowac.
        private Vector2 indicatorTextureSize = new Vector2(50, 50); // Rozmiar tekstury migacza.
        private Vector2 stopLightTextureSize = new Vector2(80, 80); // Rozmiar tekstury tylnych świateł stopu.
        private Vector2 tailLightTextureSize = new Vector2(50, 50); // Rozmiar tekstury tylnych świateł stopu.
        private Vector2 headLightTextureSize = new Vector2(100, 250); // Rozmiar tekstury tylnych świateł stopu.
        float indicatorBlinkInterval = 1; // Jak czesto mają migać migacze.

        public List<Vehicle> vehicles = new List<Vehicle>();
        public VehicleType[] vehicleTypes;
        private int maxVehicles = 10;    // Maksymalna liczba pojazdów
        private float spawnInterval = 5; // Co ile spawnować nowe pojazdy [s]
        private int maxRandom = 0;
        private float lastSpawn = 0;     // Ostatni spawn [s]

        // Constructor. Tutaj zdefiniuj typy pojazdów.
        public TrafficLogic()
        {
            VehicleType vehicleType1 = new VehicleType(new Vector2(50, 100), 0, 3, new Vector2(0, 0), new Vector2(20, 0), new Vector2(-25, -15), new Vector2(0, 0), new Vector2(-10, 0));     // Czerwony.
            VehicleType vehicleType2 = new VehicleType(new Vector2(40, 100), 1, 3, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(0, 0), new Vector2(-10, 0));       // Niebieski.
            VehicleType vehicleType3 = new VehicleType(new Vector2(40, 100), 2, 3, new Vector2(0, 0), new Vector2(10, 5), new Vector2(-15, 0), new Vector2(-10, 0), new Vector2(-10, 0));     // Pickup.
            VehicleType vehicleType4 = new VehicleType(new Vector2(50, 100), 3, 1, new Vector2(0, 0), new Vector2(5, 10), new Vector2(-20, -10), new Vector2(-10, -10), new Vector2(-10, 0)); // Srebrny.
            VehicleType vehicleType5 = new VehicleType(new Vector2(40, 100), 4, 2, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, 0), new Vector2(-10, -10), new Vector2(-10, 0));    // Taxi.
            VehicleType vehicleType6 = new VehicleType(new Vector2(45, 100), 5, 3, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0));   // Metalicznyniebieski.
            VehicleType vehicleType7 = new VehicleType(new Vector2(45, 100), 6, 2, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0));   // Srebrnopomarańczowy.
            VehicleType vehicleType8 = new VehicleType(new Vector2(45, 100), 7, 1, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0));   // Złoty ze spoilerem.
            VehicleType vehicleType9 = new VehicleType(new Vector2(45, 100), 8, 1, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, 0), new Vector2(-10, -10), new Vector2(-10, 0));    // Ferrari.
            VehicleType vehicleType10 = new VehicleType(new Vector2(45, 100), 9, 1, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, 0), new Vector2(-10, -10), new Vector2(-10, 0));   // Ferrari z pokrywą silnika.
            VehicleType vehicleType11 = new VehicleType(new Vector2(45, 100), 10, 1, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0)); // Mustang.
            VehicleType vehicleType12 = new VehicleType(new Vector2(45, 100), 11, 1, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0)); // Mustang + stripes.
            VehicleType vehicleType13 = new VehicleType(new Vector2(35, 65), 12, 3, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0));  // Mini.
            VehicleType vehicleType14 = new VehicleType(new Vector2(45, 100), 13, 3, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0)); // Fioletowy.
            VehicleType vehicleType15 = new VehicleType(new Vector2(45, 100), 14, 3, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0)); // Red + stripes.
            VehicleType vehicleType16 = new VehicleType(new Vector2(45, 100), 15, 3, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0)); // Light blue.
            VehicleType vehicleType17 = new VehicleType(new Vector2(45, 90), 16, 2, new Vector2(0, 0), new Vector2(0, 0), new Vector2(-10, -5), new Vector2(-10, -10), new Vector2(-10, 0)); // Delorean.


            vehicleTypes = new VehicleType[17] { vehicleType1, vehicleType2, vehicleType3, vehicleType4, vehicleType5, vehicleType6, vehicleType7, vehicleType8, vehicleType9, vehicleType10, vehicleType11, vehicleType12, vehicleType13, vehicleType14, vehicleType15, vehicleType16, vehicleType17 };

            foreach (VehicleType vehicleType in vehicleTypes)
                maxRandom += vehicleType.likelihoodOfApperance;
        }

        /// <summary>
        /// Get the size of the indicator texture.
        /// </summary>
        public Vector2 GetIndicatorTextureSize()
        {
            return indicatorTextureSize;
        }

        /// <summary>
        /// Get the size of te taillight texture.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetStopLightTextureSize()
        {
            return stopLightTextureSize;
        }

        /// <summary>
        /// Get the array filled with sizes of the vehicle types.
        /// </summary>
        public Vector2[] GetVehicleTypesSizes()
        {
            Vector2[] vehicleTypesSizes = new Vector2[GameParams.numberOfCars];

            for (int i = 0; i < vehicleTypes.Length; i++)
            {
                vehicleTypesSizes[i] = vehicleTypes[i].size;
            }

            return vehicleTypesSizes;
        }

        /// <summary>
        /// Set low traffic density.
        /// </summary>
        public void SetTrafficDensityLow()
        {
            spawnInterval = 10;
            maxVehicles = 5;
        }

        /// <summary>
        /// Set medium traffic density.
        /// </summary>
        public void SetTrafficDensityMedium()
        {
            spawnInterval = 5;
            maxVehicles = 10;
        }

        /// <summary>
        /// Set high traffic density.
        /// </summary>
        public void SetTrafficDensityHigh()
        {
            spawnInterval = 0;
            maxVehicles = 20;
        }

        /// <summary>
        /// Check if the junction is blocked.
        /// </summary>
        /// <returns></returns>
        private bool IsJunctionBlocked(Vehicle vehicle)
        {
            // Jeśli pojazd jest przekierowywany i nie jest przekierowywany prosto (ten zapis chyba i tak nie ma sensu ale lepiej zostawić).
            if (vehicle.IsRedirecting() && !vehicle.roadsSwitching.IsStraight())
            {
                foreach (Vehicle checkedVehicle in vehicles)
                {
                    // Jeśli to auto jest inne, przekierowuje się i jedzie.
                    if (checkedVehicle != vehicle && checkedVehicle.IsRedirecting() && checkedVehicle.roadsSwitching.bezierT > vehicle.roadsSwitching.bezierT && checkedVehicle.roadsSwitching.bezierT < 0.8f)
                    {
                        // Jeśli są na tym samym skrzyżowaniu.
                        if (checkedVehicle.roadsSwitching.center == vehicle.roadsSwitching.center)
                        {
                            // Czy te pojazdy jadą do dróg które są naprzeciwko (lub są te same)?
                            if (checkedVehicle.road.start.Y == vehicle.road.start.Y || checkedVehicle.road.start.X == vehicle.road.start.X)
                            {
                                // Czy wyjechały z różnych dróg?
                                if (checkedVehicle.lastRoad.end != vehicle.lastRoad.end)
                                {
                                    // Zablokowane!
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the road in front of the car is clear.
        /// </summary>
        private bool IsRoadClear(Vehicle vehicle, BusLogic busLogic, DrawMap drawMap, List<MyRectangle> trafficLightsForCars)
        {
            Vector2[] points = vehicle.GetDetectionPoints();

            foreach (Vector2 point in points)
            {
                Vector2[] collisionPoints;
                MyRectangle rectangle;

                // Sprawdź czy należy zatrzymać się by uniknąć kolizji na skrzyżowaniu, kiedy samochód z przeciwka skręca w przeciwną stronę.
                if (IsJunctionBlocked(vehicle))
                    return false;

                // Sprawdź czy należy zatrzymać się na światłach.
                foreach (MyRectangle trafficLightsRectangle in trafficLightsForCars)
                {
                    if (Helper.IsInside(point, trafficLightsRectangle))
                        return false;
                }

                // Sprawdź czy należy zatrzymać się z powodu autobusu blokującego drogę.
                if (Helper.CalculateDistance(busLogic.GetBusPosition(), point) < 200) // Jeśli autobus jest blisko, sprawdź go.
                {
                    collisionPoints = busLogic.GetCollisionPoints();
                    rectangle = new MyRectangle(collisionPoints[3], collisionPoints[2], collisionPoints[1], collisionPoints[0]);
                    if (Helper.IsInside(point, rectangle))
                        return false;
                }

                // Sprawdź czy należy się zatrzymać z powodu któregoś z pojazdów.
                foreach (Vehicle checkedVehicle in vehicles)
                {
                    if (Helper.CalculateDistance(checkedVehicle.GetVehiclePosition(), point) < 200 && vehicle != checkedVehicle) // Jeśli dany pojazd jest dostatecznie blisko, sprawdż go.
                    {
                        collisionPoints = checkedVehicle.GetCollisionPoints();
                        rectangle = new MyRectangle(collisionPoints[0], collisionPoints[1], collisionPoints[2], collisionPoints[3]);
                        if (Helper.IsInside(point, rectangle))
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the car should match the speed of any other vehicle.
        /// </summary>
        public void MatchSpeeds(Vehicle vehicle, BusLogic busLogic)
        {
            Vector2[] points = vehicle.GetForwardDetectionPoints();

            foreach (Vector2 point in points)
            {
                Vector2[] collisionPoints;
                MyRectangle rectangle;

                if (Helper.CalculateDistance(busLogic.GetBusPosition(), point) < 200) // Jeśli autobus jest blisko, sprawdź go.
                {
                    collisionPoints = busLogic.GetCollisionPoints();
                    rectangle = new MyRectangle(collisionPoints[3], collisionPoints[2], collisionPoints[1], collisionPoints[0]);
                    if (Helper.IsInside(point, rectangle))
                    {
                        float direction = busLogic.GetCurrentDirection() - vehicle.GetVehicleDirection();
                        float speed = busLogic.GetCurrentSpeed() * (float)Math.Cos(MathHelper.ToRadians(Math.Abs(direction)));
                        vehicle.SetMaxSpeed(speed);
                        return;
                    }
                }

                foreach (Vehicle checkedVehicle in vehicles)
                {
                    // Jeśli dany pojazd jest dostatecznie blisko, sprawdż go.
                    if (Helper.CalculateDistance(checkedVehicle.GetVehiclePosition(), point) < 200 && vehicle != checkedVehicle)
                    {
                        collisionPoints = checkedVehicle.GetCollisionPoints();
                        rectangle = new MyRectangle(collisionPoints[0], collisionPoints[1], collisionPoints[2], collisionPoints[3]);
                        if (Helper.IsInside(point, rectangle))
                        {
                            float direction = checkedVehicle.GetVehicleDirection() - vehicle.GetVehicleDirection();
                            float speed = checkedVehicle.GetVehicleSpeed() * (float)Math.Cos(MathHelper.ToRadians(Math.Abs(direction)));
                            vehicle.SetMaxSpeed(speed);
                            return;
                        }
                    }
                }
            }
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

                pointsArray = vehicle.GetForwardDetectionPoints();

                foreach (Vector2 point in pointsArray)
                    list.Add(Helper.MapPosToScreenPos(point));


                if (vehicle.IsRedirecting())
                {
                    Vector2 frontPosition; // Pozycja środka przodu samochodu.

                    frontPosition.X = vehicle.GetRealVehiclePosition().X + (vehicle.GetVehicleSize().Y * (float)Math.Sin(MathHelper.ToRadians(vehicle.GetVehicleDirection())));
                    frontPosition.Y = vehicle.GetRealVehiclePosition().Y - (vehicle.GetVehicleSize().Y * (float)Math.Cos(MathHelper.ToRadians(vehicle.GetVehicleDirection())));

                    list.Add(Helper.MapPosToScreenPos(vehicle.roadsSwitching.GetForwardPoint(50, frontPosition)));
                    list.Add(Helper.MapPosToScreenPos(vehicle.roadsSwitching.GetForwardPoint(100, frontPosition)));
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Return the points where the exhaust fumes should appear.
        /// </summary>
        /// <returns>X,Y,direction,speed</returns>
        public List<Vector4> GetExhaustPipePositions()
        {
            List<Vector4> list = new List<Vector4>();
            Vector2[] pointsArray;

            foreach (Vehicle vehicle in vehicles)
            {
                pointsArray = vehicle.GetCollisionPoints(vehicle.exhaustPipeOffset);
                list.Add(new Vector4(pointsArray[2], vehicle.GetVehicleDirection(), vehicle.GetVehicleSpeed()));
            }
            return list;
        }

        /// <summary>
        /// Get the positions of all active indicators.
        /// </summary>
        public List<LightObject> GetIndicatorPoints()
        {
            List<LightObject> list = new List<LightObject>();
            Vector2[] pointsArray;

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.accident && vehicle.indicatorBlink)
                {
                    pointsArray = vehicle.GetCollisionPoints(vehicle.tailLightsOffset);
                    foreach (Vector2 point in pointsArray)
                        list.Add(new LightObject("light", Helper.MapPosToScreenPos(point), indicatorTextureSize, 0, Color.Yellow));
                }
            }

            return list;
        }

        /// <summary>
        /// Get the positions of all active stop lights.
        /// </summary>
        public List<LightObject> GetStopLightsPoints()
        {
            List<LightObject> list = new List<LightObject>();
            Vector2[] pointsArray;

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.IsBreaking())
                {
                    pointsArray = vehicle.GetCollisionPoints(vehicle.tailLightsOffset);

                    list.Add(new LightObject("light", Helper.MapPosToScreenPos(pointsArray[2]), stopLightTextureSize, 0, Color.Red));
                    list.Add(new LightObject("light", Helper.MapPosToScreenPos(pointsArray[3]), stopLightTextureSize, 0, Color.Red));
                }
            }

            return list;
        }

        /// <summary>
        /// Get the positions of all active tail lights.
        /// </summary>
        public List<LightObject> GetTailLightsPoints(EnvironmentSimulation environmentSimulation)
        {
            List<LightObject> list = new List<LightObject>();

            if (environmentSimulation.GetGlobalLightColor().Y < GameParams.trafficGlobalLightToTurnOnTheLights)
            {
                Vector2[] pointsArray;

                foreach (Vehicle vehicle in vehicles)
                {
                    pointsArray = vehicle.GetCollisionPoints(vehicle.tailLightsOffset);

                    list.Add(new LightObject("light", Helper.MapPosToScreenPos(pointsArray[2]), tailLightTextureSize, 0, Color.Red));
                    list.Add(new LightObject("light", Helper.MapPosToScreenPos(pointsArray[3]), tailLightTextureSize, 0, Color.Red));
                }
            }

            return list;
        }

        /// <summary>
        /// Get the positions of all active head lights.
        /// </summary>
        public List<LightObject> GetHeadLightsPoints(EnvironmentSimulation environmentSimulation)
        {
            List<LightObject> list = new List<LightObject>();

            if (environmentSimulation.GetGlobalLightColor().Y < GameParams.trafficGlobalLightToTurnOnTheLights)
            {
                Vector2[] pointsArray;

                foreach (Vehicle vehicle in vehicles)
                {
                    pointsArray = vehicle.GetCollisionPoints(vehicle.headLightsOffset);

                    list.Add(new LightObject("spotlight", Helper.MapPosToScreenPos(pointsArray[0]), headLightTextureSize, vehicle.GetVehicleDirection(), Color.White));
                    list.Add(new LightObject("spotlight", Helper.MapPosToScreenPos(pointsArray[1]), headLightTextureSize, vehicle.GetVehicleDirection(), Color.White));
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
                int randomNumber = Helper.random.Next(1, maxRandom + 1);
                int current = 0;

                VehicleType type = new VehicleType();

                foreach (VehicleType vehicleType in vehicleTypes)
                {
                    if (randomNumber > current && randomNumber <= current + vehicleType.likelihoodOfApperance)
                    {
                        type = vehicleType;
                        break;
                    }

                    current += vehicleType.likelihoodOfApperance;
                }

                Vector2 junctionCenter, additionalOutpoint;
                Connection getNewRoad;

                drawMap.CreateTrack(GameParams.trafficSpawnDistance, type.size.Y, out getNewRoad, out junctionCenter, out additionalOutpoint);
                
                /*
                if (!(getNewRoad.point1.Y == 750))
                  return;
                 */

                if (!getNewRoad.IsEmpty() && junctionCenter.X != 0 && junctionCenter.Y != 0)
                {
                    Vehicle vehicle = new Vehicle(getNewRoad.point1, getNewRoad.point2, type.size, type.skin, type.moveSize, type.sizeOffset, junctionCenter, additionalOutpoint, type.tailLightsOffset, type.headLightsOffset, type.exhaustPipeOffset);

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
        public void Update(DrawMap drawMap, BusLogic busLogic)
        {
            lastSpawn += Helper.timeCoherenceMultiplier; // Zwiększmy czas od ostatniego spawnu.
            CreateNewVehicles(drawMap); // Stwórzmy nowe pojazdy.

            vehicles.RemoveAll(delegate(Vehicle vehicle) // Usuńmy pojazdy będące za daleko.
            {
                float distance = Helper.CalculateDistance(busLogic.GetBusPosition(), vehicle.GetVehiclePosition());
                if (distance > GameParams.trafficDistanceToDelete || (distance > GameParams.trafficDistanceToDeleteWhenAccident && vehicle.accident))
                    return true;
                else
                    return false;
            });

            // Te zmienne przechowują dane żeby ciągle ich nie pobierać.
            List<MyRectangle> trafficLightsForCars;
            List<TrafficLightRectangle> trafficLightsForBus;

            // Pobierz dane jednorazowo.
            drawMap.GetRedLightRectangles(out trafficLightsForCars, out trafficLightsForBus);

            foreach (Vehicle vehicle in vehicles) // Dla każdego pojazdu...
            {
                if (!vehicle.accident)
                {
                    MatchSpeeds(vehicle, busLogic); // ...dopasuj prędkość do pojazdów z przodu

                    if (IsRoadClear(vehicle, busLogic, drawMap, trafficLightsForCars)) // ...sprawdź czy ma się zatrzymać
                        vehicle.Start();
                    else
                        vehicle.Stop();

                    vehicle.Update(drawMap); // ...zaktualizuj jego pozycję
                }
                else
                {
                    vehicle.indicatorCounter += Helper.timeCoherenceMultiplier; // ...zajmij się migaczami.
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