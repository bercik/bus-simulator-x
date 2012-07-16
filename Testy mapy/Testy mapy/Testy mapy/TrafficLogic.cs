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

            private bool driving; //jedzie czy został zmuszony do zatrzymania się?
            public bool redirecting; //true kieruje sie do nowej drogi
            private float speed; //aktualna predkosc
            private Vector2 position;

            private float normalSpeed = 30; //predkosc standardowa przyjmowana podczas normalnego poruszania sie
            private float acceleration = 20; //standardowe przyspieszenie
            private float sideAcceleration = 2; //standardowy skręt

            private float stopCounter; //licznik odpowiedzialny za dlugosc postoju w razie zatrzymania się

            public Vehicle(Vector2 start, Vector2 destination) //constructor
            {
                this.road.start = start;
                this.road.destination = destination;
                this.position = start;
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
        }
        
        private List<Vehicle> vehicles;
        private int maxVehicles = 10;

        private void CreateNewVehicles() //tworzenie nowych pojazdów
        {
            int currentAmountOfVehicles = vehicles.Count();

            if (currentAmountOfVehicles < maxVehicles)
            { 
                int neededVehicles = maxVehicles - currentAmountOfVehicles;
                for (int i = 0; i < neededVehicles; i++)
                {
                    //tworzenie pojazdow podajac im poczatek i koniec z funkcji roberta
                }
            }
        }

        public void Update(TimeSpan framesInterval)
        {
            float timeCoherenceMultiplier = (float)framesInterval.Milliseconds / 1000;

            CreateNewVehicles();

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