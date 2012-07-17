using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class TrafficLogic
    {
        class Road //po prostu droga, ma ja kazdy pojazd
        {
            public class Lane //pas ruchu aby nie liczyc wszystkiego za kazdym razem
            {
                public bool isVertical;
                public float coordinate;
                public float direction;
            }

            public bool EndReached(Vector2 position)
            {
                if (lane.isVertical)
                {
                    if (lane.direction == 0)
                    {
                        if (position.Y < destination.Y)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        if (position.Y > destination.Y)
                            return true;
                        else
                            return false;
                    }
                }
                else
                {
                    if (lane.direction == 90)
                    {
                        if (position.X > destination.X)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        if (position.X < destination.X)
                            return true;
                        else
                            return false;
                    }
                }
            }

            public Vector2 start; //punkt poczatkowy
            public Vector2 destination; //punkt końcowy (cel)
            public Lane lane; //pas ruchu
            private float width = 10; //o ile zmenic wspolzedna

            public Road(Vector2 start, Vector2 destination) //constructor
            {
                this.start = start;
                this.destination = destination;
                CalculateLane();
            }

            private float CalculateDirection(Vector2 start, Vector2 destination) //oblicz kierunek miedzy dwoma punktami 
            {
                return MathHelper.ToDegrees((float)Math.Atan(destination.X - start.X / start.Y - destination.Y));
            }

            private void CalculateLane()
            {
                this.lane.direction = CalculateDirection(start, destination);
                if (lane.direction == 90 || lane.direction == 270)
                    lane.isVertical = false;
                else
                    lane.isVertical = true;

                if (lane.isVertical)
                {
                    if (lane.direction == 0)
                        lane.coordinate = start.X + width;
                    else
                        lane.coordinate = start.X - width;
                }
                else
                {
                    if (lane.direction == 90)
                        lane.coordinate = start.Y + width;
                    else
                        lane.coordinate = start.Y - width;
                }
            }
        }

        class Vehicle //klasa pojazd
        {
            public Road road;
            public RoadsSwitching roadsSwitching;
            public RoadsFollowing roadsFollowing;

            private bool driving = true; //jedzie czy został zmuszony do zatrzymania się?
            public bool redirecting = false; //true kieruje sie do nowej drogi
            private float speed; //aktualna predkosc
            private Vector2 position; //aktualna pozycja
            private Vector2 size = new Vector2(50, 100);
            private float direction;

            private float normalSpeed = 30; //predkosc standardowa przyjmowana podczas normalnego poruszania sie
            private float acceleration = 20; //standardowe przyspieszenie
            private float sideAcceleration = 2; //standardowy skręt

            private float stopCounter; //licznik odpowiedzialny za dlugosc postoju w razie zatrzymania się

            public Vehicle(Vector2 start, Vector2 destination) //constructor
            {
                this.road = new Road(start, destination);
                this.position = start;
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

                Vector2[] pointsArray = new Vector2[4] {p4, p3, p2, p1}; //create list and add points

                return pointsArray;
            }

            public Vector2[] GetDetectionPoints()
            {
                Vector2 p1, p2; //create 2 points

                p1.X = position.X + ((size.Y + 120) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p1.Y = position.Y - ((size.Y + 120) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p2.X = position.X + ((size.Y + 60) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p2.Y = position.Y - ((size.Y + 60) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                Vector2[] array = new Vector2[2] {p1, p2};
                return array;
            }

            public class RoadsSwitching
            {
                private float bezierT = 0;
                private Vector2 start;
                private Vector2 destination;

                public RoadsSwitching(Vector2 start, Vector2 destination)
                {
                    this.start = start;
                    this.destination = destination;
                }


                private Vector2 GetNewPoint(Vector2 start, Vector2 destination, Vector2 controlPoint, float t)
                {  
                    //List<Vector2> pointsList = new List<Vector2>();
                    Vector2 point;

                    //for(double t = 0.0; t <= 1;t += 0.01)  
                    //{  
                        point.X = (int) ( (1-t) * (1-t) * start.X + 2 * (1-t) * t * controlPoint.X + t * t * destination.X);  
                        point.Y = (int) ( (1-t) * (1-t) * start.Y + 2 * (1-t) * t * controlPoint.Y + t * t * destination.Y);  

                      //  pointsList.Add(point);
                    //} 

                        return point;
                }

                public void Update()
                {

                }
            }

            public class RoadsFollowing
            {
                public void Update()
                {

                }

            }

            public void Update(BusLogic busLogic)
            {
               // if (!IsRoadClear())

                if (!redirecting)
                {
                    if (road.EndReached(position))
                    {

                    }
                    else
                    {
                        roadsFollowing.Update();
                    }
                }
            }
        }
        
        private List<Vehicle> vehicles;
        private int maxVehicles = 10; //maksymalna liczba pojazdow
        private float spawnInterval = 1; //co ile spawnowac nowe pojazdy [s]
        private float lastSpawn = 0; //ostatni spawn [s]
        private Vector2 spawnDistance = new Vector2(500, 500);

        private bool IsRoadClear(Vector2[] points, BusLogic busLogic) //sprawdzanie czy mozna jechac czy trzeba hamowac
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
        
        private void CreateNewVehicles(TrackLogic trackLogic) //tworzenie nowych pojazdów
        {
            if (vehicles.Count() < maxVehicles && lastSpawn > spawnInterval)
            {
                Connection road = trackLogic.CreateTrack(spawnDistance);
                Vehicle vehicle = new Vehicle(road.point1, road.point2);
                vehicles.Add(vehicle);    
            }
        }

        public void Update(TrackLogic trackLogic, BusLogic busLogic, TimeSpan framesInterval)
        {
            float timeCoherenceMultiplier = (float)framesInterval.Milliseconds / 1000; //czas pomiedzy dwoma tickami sluzacy do utrzymania spojnosci obliczen [s]

            lastSpawn += timeCoherenceMultiplier; //zwiekszmy czas od ostatniego spawnu
            CreateNewVehicles(trackLogic); //stworzmy nowe pojazdy

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.redirecting)
                    vehicle.roadsSwitching.Update();
                else
                    vehicle.roadsFollowing.Update();
            }
        }
    }
}