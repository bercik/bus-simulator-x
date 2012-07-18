using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class TrafficLogic
    {
        public class Road //po prostu droga, ma ja kazdy pojazd
        {
            public class Lane //pas ruchu aby nie liczyc wszystkiego za kazdym razem
            {
                public Vector2 start;
                public Vector2 end;
                public float direction;
            }

            public Vector2 start; //punkt poczatkowy
            public Vector2 end; //punkt końcowy (cel)
            public Lane lane = new Lane(); //pas ruchu
            private float width = 50; //o ile zmenic wspolzedna

            public bool EndReached(Vector2 position)
            {

                if (lane.direction == 0)
                {
                    if (position.Y < end.Y)

                        return true;
                    else
                        return false;
                }


                if (lane.direction == 180)
                {
                    if (position.Y > end.Y)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 90)
                {
                    if (position.X > end.X)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 270)
                {
                    if (position.X < end.X)
                        return true;
                    else
                        return false;
                }

                return false;
            }

            public Road(Vector2 start, Vector2 end) //constructor
            {
                this.start = start;
                this.end = end;
                CalculateLane();
            }

            private float CalculateDirection(Vector2 start, Vector2 destination) //oblicz kierunek miedzy dwoma punktami 
            {
                return MathHelper.ToDegrees((float)Math.Atan(destination.X - start.X / start.Y - destination.Y));
            }

            private void CalculateLane()
            {
                //this.lane.direction = (float)Math.Round(CalculateDirection(start, end));
                //if (this.lane.direction < 0)
                    //this.lane.direction += 360;

                if (end.X > start.X)
                    this.lane.direction = 90;

                if (end.X < start.X)
                    this.lane.direction = 270;

                if (end.Y > start.Y)
                    this.lane.direction = 180;

                if (end.Y < start.Y)
                    this.lane.direction = 0;

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

        public class Vehicle //klasa pojazd
        {
            public Road road;
            public RoadsSwitching roadsSwitching;

            private bool driving = true; //jedzie czy został zmuszony do zatrzymania się?
            public bool redirecting = false; //true kieruje sie do nowej drogi
            private float speed; //aktualna predkosc
            private Vector2 position; //aktualna pozycja
            public Vector2 size = new Vector2(50, 100);
            public float direction;

            public Vector2 lastEnd = new Vector2(0, 0);

            private float normalSpeed = 20; //predkosc standardowa przyjmowana podczas normalnego poruszania sie
            private float acceleration = 20; //standardowe przyspieszenie
            private float sideAcceleration = 2; //standardowy skręt
            private float speedMultiplier = 4;

            private float stopCounter; //licznik odpowiedzialny za dlugosc postoju w razie zatrzymania się

            public Vehicle(Vector2 start, Vector2 destination) //constructor
            {
                this.road = new Road(start, destination);
                this.position = road.lane.start;
                this.direction = road.lane.direction;
            }

            public Vector2[] GetCollisionPoints() //returns 4 collision points
            {
                Vector2 p1, p2, p3, p4; //create 4 points

                p3.X = position.X + ((size.X * (float)Math.Cos(MathHelper.ToRadians(direction))) / 2); //calculate their positions
                p3.Y = position.Y + ((size.X * (float)Math.Sin(MathHelper.ToRadians(direction))) / 2);

                p4.X = position.X - (size.X * (float)Math.Cos(MathHelper.ToRadians(direction)) / 2);
                p4.Y = position.Y - (size.X * (float)Math.Sin(MathHelper.ToRadians(direction)) / 2);

                p1.X = p4.X + (size.Y * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p1.Y = p4.Y - (size.Y * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p2.X = p3.X + (size.Y * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p2.Y = p3.Y - (size.Y * (float)Math.Cos(MathHelper.ToRadians(direction)));

                Vector2[] pointsArray = new Vector2[4] { p4, p3, p2, p1 }; //create list and add points

                return pointsArray;
            }

            public Vector2[] GetDetectionPoints()
            {
                Vector2 p1, p2; //create 2 points

                p1.X = position.X + ((size.Y + 120) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p1.Y = position.Y - ((size.Y + 120) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p2.X = position.X + ((size.Y + 60) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p2.Y = position.Y - ((size.Y + 60) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                Vector2[] array = new Vector2[2] { p1, p2 };
                return array;
            }

            public Vector2 GetVehiclePosition() //Get the center of the vehicle
            {
                return CalculateCenter(position, direction);
            }

            private Vector2 CalculateCenter(Vector2 vehiclePosition, float vehicleDirection)
            {
                Vector2 center;

                center.X = vehiclePosition.X + (size.Y * (float)Math.Sin(MathHelper.ToRadians(vehicleDirection)) / 2);
                center.Y = vehiclePosition.Y - (size.Y * (float)Math.Cos(MathHelper.ToRadians(vehicleDirection)) / 2);

                return center;
            }

            private Vector2 CalculateNewPosition(float vehicleSpeed, float vehicleDirection, float timeCoherenceMultiplier)
            {
                Vector2 newPosition;
                newPosition.X = position.X + (speedMultiplier * timeCoherenceMultiplier * vehicleSpeed * (float)Math.Sin(MathHelper.ToRadians(vehicleDirection)));
                newPosition.Y = position.Y - (speedMultiplier * timeCoherenceMultiplier * vehicleSpeed * (float)Math.Cos(MathHelper.ToRadians(vehicleDirection)));
                return newPosition;
            }

            public void Start()
            {
                driving = true;
            }

            public void Stop()
            {
                driving = false;
                stopCounter = 0;
            }

            public class RoadsSwitching
            {
                private float bezierT = 0;
                private float bezierTInc = (float)0.005;
                private Vector2 start;
                private Vector2 end;
                private Vector2 controlPoint;

                private Vector2 CalculateControlPoint(Vector2 start, Vector2 end)
                {
                    Vector2 controlPoint;

                    if (start.X == end.X || start.Y == end.Y)
                    {
                        controlPoint.X = start.X - (start.X - end.X);
                        controlPoint.Y = start.Y - (start.Y - end.Y); ;
                    }
                    else
                    {
controlPoint.X = start.X - Math.Abs(start.X - end.X);
                    controlPoint.Y = start.Y;
                    }

                    

                    return controlPoint;
                }

                public RoadsSwitching(Vector2 start, Vector2 end, Vector2 controlPoint)
                {
                    this.start = start;
                    this.end = end;
                    this.controlPoint = CalculateControlPoint(start, end);
                }

                public float CalculateDirection(Vector2 start, Vector2 end) //oblicz kierunek miedzy dwoma punktami 
                {
                    //float direction =  MathHelper.ToDegrees((float)Math.Atan(destination.X - start.X / start.Y - destination.Y));
                    //float direction = MathHelper.ToDegrees((float)Math.Atan2(start.Y - destination.Y, destination.X - start.X));
                    float direction = MathHelper.ToDegrees((float)Math.Atan2(end.X - start.X, start.Y - end.Y));
                    if (direction < 0)
                        direction += 360;

                    return direction;
                }

                public Vector2 GetNewPoint()
                {
                    Vector2 point;
                    bezierT += bezierTInc;
                    bezierT = (float)Math.Round(bezierT, 5);

                    point.X = (float)((1 - bezierT) * (1 - bezierT) * start.X + 2 * (1 - bezierT) * bezierT * controlPoint.X + bezierT * bezierT * end.X);
                    point.Y = (float)((1 - bezierT) * (1 - bezierT) * start.Y + 2 * (1 - bezierT) * bezierT * controlPoint.Y + bezierT * bezierT * end.Y);

                    return point;
                }
            }

            public void Update(BusLogic busLogic, DrawMap drawMap, float timeCoherenceMultiplier)
            {
                if (driving)
                {
                    speed += acceleration * timeCoherenceMultiplier;
                    if (speed > normalSpeed)
                        speed = normalSpeed;
                }
                else
                {
                    speed -= acceleration * timeCoherenceMultiplier;
                    if (speed < 0)
                        speed = 0;
                }

                if (!redirecting)
                {
                    if (road.EndReached(position))
                    {
                        redirecting = true;
                        Vector2 junctionCenter;
                        Connection getNewRoad;

                        drawMap.ChangeTrack(road.end, lastEnd, out getNewRoad, out junctionCenter);

                        lastEnd = road.end;

                        Road newRoad = new Road(getNewRoad.point1, getNewRoad.point2);

                        roadsSwitching = new RoadsSwitching(road.lane.end, newRoad.lane.start, junctionCenter);

                        road = newRoad;
                    }
                    else
                    {
                        position = CalculateNewPosition(speed, direction, timeCoherenceMultiplier);
                    }
                }
                else
                {
                    Vector2 newPoint = roadsSwitching.GetNewPoint();
                    float newDirection = roadsSwitching.CalculateDirection(position, newPoint);

                    direction = newDirection;
                    
                    position = CalculateNewPosition(speed, direction, timeCoherenceMultiplier);
                    
                    //position = newPoint;

                    if (newPoint == road.lane.start)
                    {
                        redirecting = false;
                        direction = road.lane.direction;
                        position = road.lane.start;
                    }
                }
            }
        }

        private List<Vehicle> vehicles = new List<Vehicle>();
        private int maxVehicles = 1; //maksymalna liczba pojazdow
        private float spawnInterval = 5; //co ile spawnowac nowe pojazdy [s]
        private float lastSpawn = 0; //ostatni spawn [s]
        private Vector2 spawnDistance = new Vector2(500, 500);

        public bool IsRoadClear(Vector2[] points, BusLogic busLogic) //sprawdzanie czy mozna jechac czy trzeba hamowac
        {
            foreach (Vector2 point in points)
            {
                Vector2[] collisionPoints = busLogic.GetCollisionPoints(busLogic.position, busLogic.GetDirection());
                MyRectangle rectangle = new MyRectangle(collisionPoints[3], collisionPoints[2], collisionPoints[1], collisionPoints[0]);
                if (Helper.IsInside(point, rectangle))
                    return false;

                foreach (Vehicle vehicle in vehicles)
                {
                    collisionPoints = vehicle.GetCollisionPoints();
                    rectangle = new MyRectangle(collisionPoints[0], collisionPoints[1], collisionPoints[2], collisionPoints[3]);
                    if (Helper.IsInside(point, rectangle))
                        return false;
                }
            }

            return true;
        }

        public List<Object> GetAllVehicles()
        {
            List<Object> list = new List<Object>();

            foreach (Vehicle vehicle in vehicles)
            {
                list.Add(new Object("vehicle", vehicle.GetVehiclePosition(), vehicle.size, vehicle.direction, false));
            }

            return list;
        }

        private void CreateNewVehicles(DrawMap drawMap) //tworzenie nowych pojazdów
        {
            if (vehicles.Count() < maxVehicles && lastSpawn > spawnInterval)
            {
                Connection road = drawMap.CreateTrack(spawnDistance);

                if (!(road.point1.X == 900 && road.point1.Y == 1050))
                    return;

                if (!road.IsEmpty())
                {
                    Vehicle vehicle = new Vehicle(road.point1, road.point2);
                    vehicles.Add(vehicle);
                    lastSpawn = 0;
                }
            }
        }

        public void Update(DrawMap drawMap, BusLogic busLogic, TimeSpan framesInterval)
        {
            float timeCoherenceMultiplier = (float)framesInterval.Milliseconds / 1000; //czas pomiedzy dwoma tickami sluzacy do utrzymania spojnosci obliczen [s]

            lastSpawn += timeCoherenceMultiplier; //zwiekszmy czas od ostatniego spawnu
            CreateNewVehicles(drawMap); //stworzmy nowe pojazdy

            foreach (Vehicle vehicle in vehicles)
            {
                /*if (IsRoadClear(vehicle.GetDetectionPoints(), busLogic))
                    vehicle.Stop();
                else
                    vehicle.Start();*/
                vehicle.Update(busLogic, drawMap, timeCoherenceMultiplier);
            }
        }
    }
}