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

            public bool EndReached(Vector2 position) //funkcja sprawdza czy bedac na podanej pozycji osiagnieto koniec drogi
            {

                if (lane.direction == 0) //droga w gore
                {
                    if (position.Y <= end.Y)

                        return true;
                    else
                        return false;
                }


                if (lane.direction == 180) //droga w dol
                {
                    if (position.Y >= end.Y)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 90) //droga w prawo
                {
                    if (position.X >= end.X)
                        return true;
                    else
                        return false;
                }

                if (lane.direction == 270) //droga w lewo
                {
                    if (position.X <= end.X)
                        return true;
                    else
                        return false;
                }

                return false;
            }

            public Road(Vector2 start, Vector2 end, Vector2 center) //constructor
            {
                this.start = start;
                this.end = end;
                CalculateLane(center); //oblicz lane dla tej drogi
            }

            private float CalculateDirection(Vector2 start, Vector2 end) //oblicz kierunek miedzy dwoma punktami 
            {
                float direction = MathHelper.ToDegrees((float)Math.Atan2(end.X - start.X, start.Y - end.Y)); //oblicz kierunek

                if (direction < 0) //jesli wyjdzie ujemny skoryguj to
                    direction += 360;

                return direction;
            }

            private void CalculateLane(Vector2 center) //oblicza lane dla drogi, wywolywana przez konstruktor
            {
                //this.lane.direction = (float)Math.Round(CalculateDirection(start, end));
                //if (this.lane.direction < 0)
                //this.lane.direction += 360;

                if (start.X == end.X && start.Y == end.Y) //jesli droga jest jednym punktem oblicz jej kierunek w oparciu o centrum skrzyzowania z ktorego wychodzi
                {
                    float preDirection = CalculateDirection(center, start);
                    //float preDirection = CalculateDirection(start, center);

                    if (preDirection > 315 || preDirection < 45)
                        this.lane.direction = 0;

                    if (preDirection > 45 && preDirection < 135)
                        this.lane.direction = 90;

                    if (preDirection > 135 && preDirection < 225)
                        this.lane.direction = 180;

                    if (preDirection > 225 && preDirection < 315)
                        this.lane.direction = 270;
                }
                else //jesli nie jest jednym punktem licz w oparciu o jej punkty
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


                //ponizej nastepuje przeliczanie przesuniec lane w zaleznosci od kata drogi
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

        public class VehicleType //klasa przechowujaca typy pojazdow (jak wyglada, rozmiar itp. - wykorzystywane przy tworzeniu)
        {
            public Vector2 size;
            public int skin;
            public int likelihoodOfApperance;

            public Vector2 moveSize; //pozwala przesuwać collision points
            public Vector2 sizeOffset; //pozwala modyfikowac wielkosc pojazdu z ktorej sa brane collision points

            public VehicleType(Vector2 size, int skin, int likelihoodOfApperance, Vector2 moveSize, Vector2 sizeOffset) //constructor
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

        public class Vehicle //klasa pojazd
        {
            public Road road; //droga ktora jedzie
            public RoadsSwitching roadsSwitching; //klasa odpowiadajaca za przekierowania od drogi do drogi, zakrety

            private bool driving = true; //jedzie czy został zmuszony do zatrzymania się?
            public bool redirecting = false; //true kieruje sie do nowej drogi
            private float speed; //aktualna predkosc
            private Vector2 position; //aktualna pozycja
            public Vector2 size = new Vector2(50, 100); //rozmiar
            public float direction; //kierunek
            public int skin; //jaki wyglad, skin
            public Vector2 moveSize;
            public Vector2 sizeOffset;

            public bool accident = false;
            public float indicatorCounter = 0;
            public bool indicatorBlink = true;

            public Vector2 lastEnd = new Vector2(0, 0); //koniec ostatniej drogi podawane do ChangeTrack

            private float normalSpeed = 20; //predkosc standardowa przyjmowana podczas normalnego poruszania sie
            private float acceleration = 70; //standardowe przyspieszenie
            //private float sideAcceleration = 2; //standardowy skręt
            private float speedMultiplier = 4;

            private float stopCounter = 0; //licznik odpowiedzialny za dlugosc postoju w razie zatrzymania się
            private float startAfter = 3; //po ilu sekundach ma wystartować

            public Vehicle(Vector2 start, Vector2 destination, Vector2 size, int skin, Vector2 moveSize, Vector2 sizeOffset, Vector2 junctionCenter, Vector2 additionalOutpoint) //constructor
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

            public Vector2[] GetDetectionPoints() //punkty uzywane do wykrywania czy nalezy zatrzymac samochod
            {
                Vector2 p2, p3; //create 2 points

                // p1.X = position.X + ((size.Y + 50) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                // p1.Y = position.Y - ((size.Y + 50) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p2.X = position.X + ((size.Y + 30) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p2.Y = position.Y - ((size.Y + 30) * (float)Math.Cos(MathHelper.ToRadians(direction)));


                p3.X = position.X + (((size.X + 5) * (float)Math.Cos(MathHelper.ToRadians(direction))) / 2);
                p3.Y = position.Y + (((size.X + 5) * (float)Math.Sin(MathHelper.ToRadians(direction))) / 2);

                // p4.X = position.X - ((size.X + 5) * (float)Math.Cos(MathHelper.ToRadians(direction)) / 2);
                // p4.Y = position.Y - ((size.X + 5) * (float)Math.Sin(MathHelper.ToRadians(direction)) / 2);

                // p4.X = p4.X + ((size.Y + 30) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                // p4.Y = p4.Y - ((size.Y + 30) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                p3.X = p3.X + ((size.Y + 30) * (float)Math.Sin(MathHelper.ToRadians(direction)));
                p3.Y = p3.Y - ((size.Y + 30) * (float)Math.Cos(MathHelper.ToRadians(direction)));

                Vector2[] array = new Vector2[] { p2, p3 };
                return array;
            }

            public Vector2 GetVehiclePosition() //Get the center of the vehicle
            {
                return CalculateCenter(position, direction);
            }

            private Vector2 CalculateCenter(Vector2 vehiclePosition, float vehicleDirection) //oblicza srodek pojazdu
            {
                Vector2 center;

                center.X = vehiclePosition.X + (size.Y * (float)Math.Sin(MathHelper.ToRadians(vehicleDirection)) / 2);
                center.Y = vehiclePosition.Y - (size.Y * (float)Math.Cos(MathHelper.ToRadians(vehicleDirection)) / 2);

                return center;
            }

            private Vector2 CalculateNewPosition(float vehicleSpeed, float vehicleDirection, float timeCoherenceMultiplier)//oblicza nowa pozycje biorac pod uwage predkosc i kierunek
            {
                Vector2 newPosition;
                newPosition.X = position.X + (speedMultiplier * timeCoherenceMultiplier * vehicleSpeed * (float)Math.Sin(MathHelper.ToRadians(vehicleDirection)));
                newPosition.Y = position.Y - (speedMultiplier * timeCoherenceMultiplier * vehicleSpeed * (float)Math.Cos(MathHelper.ToRadians(vehicleDirection)));
                return newPosition;
            } 

            public void Start(float timeCoherenceMultiplier)//wznawia jazde
            {
                stopCounter += timeCoherenceMultiplier;
                if (stopCounter > startAfter)
                    driving = true;
            } 

            public void Stop()//zatrzymuje pojazd
            {
                driving = false;
                stopCounter = 0;
            } 

            public void Collision() //wywolywana w razie kolizji
            {
                accident = true;
            }

            public class RoadsSwitching //odpowiada za zmiane drog
            {
                private float bezierT = 0; //podstawiane do funckji generujacej krzywe beziera
                private float bezierTInc = (float)0.01; //co ile ma sie zmieniac za kazdym wywolaniem
                private Vector2 start; //poczatek (koniec lana poprzedniej drogi)
                private Vector2 end; //koniec (poczatek lane nowej drogi)
                private Vector2 controlPoint; //punkt dodatkowy dla krzywych beziera
                public Vector2 target; //dokad aktualnie jedzie pojazd

                private Vector2 CalculateControlPoint(Vector2 start, Vector2 end, Vector2 center)//oblicz punkt kontrolny dla krzywej beziera
                {
                    Vector2 controlPoint = new Vector2(0, 0);

                    if (start.X == end.X || start.Y == end.Y)
                    {
                        controlPoint.X = start.X - (start.X - end.X);
                        controlPoint.Y = start.Y - (start.Y - end.Y);
                    }
                    else
                    {
                        if (((end.X < start.X && end.Y < start.Y) || (end.X < start.X && end.Y > start.Y)) && (center.X < start.X && center.Y >= start.Y)) //z prawego
                        {
                            controlPoint.X = start.X - (start.X - end.X);
                            controlPoint.Y = start.Y;
                        }

                        if (((end.X < start.X && end.Y < start.Y) || (end.X > start.X && end.Y < start.Y)) && (center.X < start.X && center.Y < start.Y)) //z dolnego
                        {
                            controlPoint.X = start.X;
                            controlPoint.Y = start.Y - (start.Y - end.Y);
                        }

                        if (((end.Y < start.Y && end.X > start.X) || (end.Y > start.Y && end.X > start.X)) && (center.X > start.X && center.Y <= start.Y)) //z lewego
                        {
                            controlPoint.X = start.X + (end.X - start.X);
                            controlPoint.Y = start.Y;
                        }

                        if (((end.X < start.X && end.Y > start.Y) || (end.X > start.X && end.Y > start.Y)) && (center.X > start.X && center.Y > start.Y)) //z gornego
                        {
                            controlPoint.X = start.X;
                            controlPoint.Y = start.Y + (end.Y - start.Y);
                        }
                    }



                    return controlPoint;
                }

                public RoadsSwitching(Vector2 start, Vector2 end, Vector2 center) //constructor
                {
                    this.start = start;
                    this.end = end;
                    this.controlPoint = CalculateControlPoint(start, end, center);
                    this.target = GetNewPoint();
                }

                public bool IsStraight() //czy droga przekierowania jest na przeciwko (nie ma zakrętu)
                {
                    bool straight = true;
                    if (start.X == end.X || start.Y == end.Y)
                        straight = true;
                    else
                        straight = false;
                    return straight;
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

                public Vector2 GetNewPoint()//funkcja generujaca punkty na podstawie krzywej beziera
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

                public bool Reached(Vector2 position, Vector2 point)//czy osiagnieto dany punkt bedac w punkcie position
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
                    speed += acceleration * timeCoherenceMultiplier; //przyspieszamy
                    if (speed > normalSpeed)
                        speed = normalSpeed;
                }
                else
                {
                    speed -= acceleration * timeCoherenceMultiplier; //zwalniamy
                    if (speed < 0)
                        speed = 0;
                }

                if (!redirecting)
                {
                    if (road.EndReached(position))
                    {
                        Vector2 junctionCenter;
                        Connection getNewRoad;

                        drawMap.ChangeTrack(road.end, lastEnd, out getNewRoad, out junctionCenter); //bierzemy nową drogę

                        lastEnd = road.end; //koniec poprzedniej drogi to teraz koniec drogi aktualnej

                        Road newRoad = new Road(getNewRoad.point1, getNewRoad.point2, junctionCenter); //generujemy nową drogę w oparciu o punkty z changetrack

                        roadsSwitching = new RoadsSwitching(road.lane.end, newRoad.lane.start, junctionCenter); //generujemy klasę przekierowującą

                        if (!roadsSwitching.IsStraight()) //jesli droga nie jest na przeciwko rozpoczynamy przekierowanie
                            redirecting = true;

                        road = newRoad; //droga to nowa droga
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
                        redirecting = false; //jesli juz dojechalismy do nowej drogi ustawmy odpowiednio auto i dajmy mu normalnie jechac
                        direction = road.lane.direction;
                        position = road.lane.start;
                    }
                    else
                    {
                        Vector2 newPoint = new Vector2(0, 0);

                        while (roadsSwitching.Reached(position, roadsSwitching.target)) 
                        {                                              //jesli dojechalismy do punktu wygenerowaneg poprzednio
                            newPoint = roadsSwitching.GetNewPoint();   //szukaj takiego nowego do ktorego jeszcze nie dojechalismy
                            roadsSwitching.target = newPoint;
                        }

                        direction = roadsSwitching.CalculateDirection(position, roadsSwitching.target);
                        position = CalculateNewPosition(speed, direction, timeCoherenceMultiplier);
                    }
                }
            }
        }

        private List<Vehicle> vehicles = new List<Vehicle>();
        private int maxVehicles = 10; //maksymalna liczba pojazdow
        private float spawnInterval = 5; //co ile spawnowac nowe pojazdy [s]
        private float lastSpawn = 0; //ostatni spawn [s]
        private Vector2 spawnDistance = new Vector2(1000, 1000); //maksymalna odleglosc spawnowania
        private float distanceToDelete = 1000; //samochody bedace dalej niz podany dystans zostana usuniete

        float indicatorBlinkInterval = 1; //jak czesto maja migac migacze

        private int maxRandom = 0;

        private VehicleType[] vehiclesTypes;

        public TrafficLogic()//constructor
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

        public bool IsRoadClear(Vector2[] points, BusLogic busLogic) //sprawdzanie czy mozna jechac czy trzeba hamowac
        {
            foreach (Vector2 point in points)
            {
                Vector2[] collisionPoints;
                MyRectangle rectangle;

                if (Helper.CalculateDistance(busLogic.GetBusPosition(), point) < 200) //jesli autobus jest blisko sprawdz go
                {
                    collisionPoints = busLogic.GetCollisionPoints(busLogic.position, busLogic.GetDirection());
                    rectangle = new MyRectangle(collisionPoints[3], collisionPoints[2], collisionPoints[1], collisionPoints[0]);
                    if (Helper.IsInside(point, rectangle))
                        return false;
                }

                foreach (Vehicle vehicle in vehicles)
                {
                    if (Helper.CalculateDistance(vehicle.GetVehiclePosition(), point) < 200) //jesli dany autobus jest dostatecznie blisko sprawdz
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

        public bool IsCollision(Vector2[] points, BusLogic busLogic) //sprawdzanie czy wystepuje kolizja
        {
            foreach (Vector2 point in points)
            {
                Vector2[] collisionPoints;
                MyRectangle rectangle;

                if (Helper.CalculateDistance(busLogic.GetBusPosition(), point) < 200) //jesli autobus jest blisko sprawdz go
                {
                    collisionPoints = busLogic.GetCollisionPoints(busLogic.position, busLogic.GetDirection());
                    rectangle = new MyRectangle(collisionPoints[3], collisionPoints[2], collisionPoints[1], collisionPoints[0]);
                    if (Helper.IsInside(point, rectangle))
                    {
                        busLogic.Collision();
                        return true;
                    }
                }

                foreach (Vehicle vehicle in vehicles)
                {
                    if (Helper.CalculateDistance(vehicle.GetVehiclePosition(), point) < 200) //jesli dany pojazd jest dostatecznie blisko sprawdz
                    {
                        collisionPoints = vehicle.GetCollisionPoints();
                        if (collisionPoints[0] != points[0] && collisionPoints[1] != points[1] && collisionPoints[2] != points[2] && collisionPoints[3] != points[3])
                        {
                            rectangle = new MyRectangle(collisionPoints[0], collisionPoints[1], collisionPoints[2], collisionPoints[3]);
                            if (Helper.IsInside(point, rectangle))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public List<Object> GetAllVehicles() //uzywane do rysowania
        {
            List<Object> list = new List<Object>();

            foreach (Vehicle vehicle in vehicles)
            {
                Vector2 size = vehicle.size;
                size.X += vehicle.sizeOffset.X;
                size.Y += vehicle.sizeOffset.Y;

                Vector2 position = vehicle.GetVehiclePosition();
                position.X = position.X + (vehicle.moveSize.Y * (float)Math.Sin(MathHelper.ToRadians(vehicle.direction))); //przesuwamy do gory
                position.Y = position.Y - (vehicle.moveSize.Y * (float)Math.Cos(MathHelper.ToRadians(vehicle.direction)));

                position.X = position.X + (vehicle.moveSize.X * (float)Math.Sin(MathHelper.ToRadians(vehicle.direction + 90))); //przesuwamy w prawo
                position.Y = position.Y - (vehicle.moveSize.X * (float)Math.Cos(MathHelper.ToRadians(vehicle.direction + 90)));

                list.Add(new Object(vehicle.skin.ToString(), position, size, vehicle.direction));
            }

            return list;
        }

        private List<MyRectangle> GetCollisionRectangles(Vector2 busPosition) //kolizja autobusu
        {
            List<MyRectangle> list = new List<MyRectangle>();
            Vector2[] pointsArray;

            foreach (Vehicle vehicle in vehicles)
            {
                if (Helper.CalculateDistance(busPosition, vehicle.GetVehiclePosition()) < 200)
                {
                    pointsArray = vehicle.GetCollisionPoints();
                    list.Add(new MyRectangle(pointsArray[0], pointsArray[1], pointsArray[2], pointsArray[3]));
                }
            }

            return list;
        }

        public bool IsCollision(Vector2[] collisionPoints, Vector2 busPosition)
        {
            List<MyRectangle> boxesList = GetCollisionRectangles(busPosition);

            foreach (MyRectangle box in boxesList)
            {
                foreach (Vector2 point in collisionPoints)
                {
                    if (Helper.IsInside(point, box))
                        return true;
                }
            }
            return false;
        }

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
                        list.Add(new Object("", point, new Vector2(25, 25), 0));
                }
            }

            return list;
        }

        private void CreateNewVehicles(DrawMap drawMap) //tworzenie nowych pojazdów
        {
            if (vehicles.Count() < maxVehicles && lastSpawn > spawnInterval)
            {
                Vector2 junctionCenter, additionalOutpoint;
                Connection getNewRoad;

                drawMap.CreateTrack(spawnDistance, out getNewRoad, out junctionCenter, out additionalOutpoint);

                if (!(getNewRoad.point1.X == 300 && getNewRoad.point1.Y == 150))
                    return;
                
                //Road newRoad = new Road(getNewRoad.point1, getNewRoad.point2, junctionCenter);

                if (!getNewRoad.IsEmpty() && junctionCenter.X != 0 && junctionCenter.Y != 0)
                {
                    Random random = new Random();
                    int randomNumber = random.Next(1, maxRandom);
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
                    vehicles.Add(vehicle);
                    lastSpawn = 0;
                }
            }
        }

        public void Update(DrawMap drawMap, BusLogic busLogic, TimeSpan framesInterval) //glowna funkcja aktualizujaca
        {
            float timeCoherenceMultiplier = (float)framesInterval.Milliseconds / 1000; //czas pomiedzy dwoma tickami sluzacy do utrzymania spojnosci obliczen [s]

            lastSpawn += timeCoherenceMultiplier; //zwiekszmy czas od ostatniego spawnu
            CreateNewVehicles(drawMap); //stworzmy nowe pojazdy

            vehicles.RemoveAll(delegate(Vehicle vehicle)
            {
                return Helper.CalculateDistance(busLogic.GetBusPosition(), vehicle.GetVehiclePosition()) > distanceToDelete;
            });


            foreach (Vehicle vehicle in vehicles) //dla kazdego pojazdu...
            {
                if (!vehicle.accident)
                {
                    if (IsRoadClear(vehicle.GetDetectionPoints(), busLogic)) //...sprawdz czy ma sie zatrzymac
                        vehicle.Start(timeCoherenceMultiplier);
                    else
                        vehicle.Stop();

                    if (IsCollision(vehicle.GetCollisionPoints(), busLogic)) //...czy pojawia sie kolizja
                        vehicle.Collision();

                    if (!vehicle.accident) //jesli nie mial wypadku
                        vehicle.Update(drawMap, timeCoherenceMultiplier); //...zaktualizuj jego pozycje
                }
                else
                {
                    vehicle.indicatorCounter += timeCoherenceMultiplier;
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