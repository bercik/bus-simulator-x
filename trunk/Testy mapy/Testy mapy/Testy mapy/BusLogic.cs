using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class BusLogic
    {
        private Vector2 position;
        private float direction;
        private float speed;
        private float sideAcc;
        private float width;
        private float height;

        private float brakeAcc = 2;
        private float turnAcc = 2;
        private float sideAccLoss = (float)0.5;
        private float speedLoss = (float)0.5;


        private GearBox gearBox = new GearBox();     

        class GearBox //gearbox is responsible for calculating accelerations of the bus...
        {
            class Gear //...it is full of the gears...
            {
                class AccelerationCurve //...and gears are basically curves
                {
                    public float addToSpeed;
                    public float logBase;
                    public float addToAll;
                    
                    public AccelerationCurve(float addToSpeed, float logBase, float addToAll) //constructor
                    {
                        this.addToSpeed = addToSpeed;
                        this.logBase = logBase;
                        this.addToAll = addToAll;
                    }
                }

                float optimalSpeed;
                float accelerationMultiplier;
                AccelerationCurve accCurve;

                public Gear(float optimalSpeed, float accelerationMultiplier, float addToSpeed, float logBase, float addToAll) //constructor
                {
                    this.accCurve = new AccelerationCurve(addToSpeed, logBase, addToAll);
                    this.optimalSpeed = optimalSpeed;
                    this.accelerationMultiplier = accelerationMultiplier;
                }

                public float GetAcceleration(float busSpeed) //each gear can calculate its own acceleration for given speed
                {
                    if (busSpeed < optimalSpeed) //if below opimal speed recalculate it to get the right curve
                        busSpeed = optimalSpeed + (optimalSpeed - busSpeed);

                    float acceleration = accelerationMultiplier * ((float)Math.Log(busSpeed + accCurve.addToSpeed, accCurve.logBase) + accCurve.addToAll);
                    return acceleration;
                }
            }

            int currentGear = 1;
            int minGear = 0;
            int maxGear = 5;
            Gear[] gears = new Gear[6];

            public GearBox() //constructor
            {
                gears[1] = new Gear(0, 1, 10, (float)0.55, 5);
                gears[2] = new Gear(10, 1, 8, (float)0.5, 5);
                gears[3] = new Gear(25, 1, 0, (float)0.5, (float)5.3);
                gears[4] = new Gear(40, 1, 0, (float)0.4, (float)4.5);
                gears[5] = new Gear(60, 1, 0, (float)0.35, (float)4.2);
            }

            public float GetAcceleration(float busSpeed)
            {
                return gears[currentGear].GetAcceleration(busSpeed);
            }

            public void GearUp()
            {
                if (currentGear < maxGear)
                    currentGear++;
            }

            public void GearDown()
            {
                if (currentGear > minGear)
                    currentGear--;
            }
        }

        public BusLogic(float x, float y, float direction, float speed, float width, float height) //constructor
        {
            this.position.X = x;
            this.position.Y = y;
            this.direction = direction;
            this.speed = speed;
            this.width = width;
            this.height = height;
        }

        public Vector2 GetBusPosition() //Get the center of the bus to draw the center of the map
        {
            Vector2 busPosition;

            busPosition.X = position.X + (height * (float)Math.Sin(direction) / 2);
            busPosition.Y = position.Y - (height * (float)Math.Cos(direction) / 2);

            return busPosition;
        }
        
        private List<Vector3> GetCollisionPoints(Vector2 busPosition, float busDirection) //returns 4 collision points - they are
        {                                                                                 //used in IsPositionAvailable function
            Vector3 p1, p2, p3, p4; //create 4 points

            p3.X = busPosition.X + (width * (float)Math.Sin(45 - busDirection) / 2); //calculate their positions
            p3.Y = busPosition.Y + (width * (float)Math.Cos(45 - busDirection) / 2);
            p3.Z = 1;

            p4.X = busPosition.X - (width * (float)Math.Sin(45 - busDirection) / 2);
            p4.Y = busPosition.Y - (width * (float)Math.Cos(45 - busDirection) / 2);
            p4.Z = 0;

            p1.X = p4.X + (height * (float)Math.Sin(busDirection));
            p1.Y = p4.Y - (height * (float)Math.Cos(busDirection));
            p1.Z = 0;

            p2.X = p3.X + (height * (float)Math.Sin(busDirection));
            p2.Y = p3.Y - (height * (float)Math.Cos(busDirection));
            p2.Z = 0;

            var pointsList = new List<Vector3> {p1, p2, p3 , p4 }; //create list and add points
            return pointsList;
        }

        private bool IsPositionAvailable(Vector2 busPosition, float busDirection) //check if new position is available
        {            
           // List<Vector2> pointsList = GetCollisionPoints(busPosition, busDirection); //create list of the points

           // foreach(Vector2 point in pointsList)
            //{
                //[!marker] check if there is no collision on the map
                /*something like
                 if (isCollision(point))
                   return false;
                 */
           // }

            return true;
        }

        private Vector2 CalculateNewPosition(float busSpeed, float busDirection)
        {
            Vector2 newPosition;
            newPosition.X = position.X + (busSpeed * (float)Math.Sin(busDirection));
            newPosition.Y = position.Y - (busSpeed * (float)Math.Cos(busDirection));
            return newPosition;
        }

        private void Collision()
        {
            speed = 0;
        }

        public void Update(bool accelerate, bool brake, bool left, bool right, TimeSpan framesInterval) //main update function adjsuting speed, direction and so on
        {
            if (accelerate)
                speed += gearBox.GetAcceleration(speed);

            if (brake)
                speed -= brakeAcc;

            if (left)
                sideAcc -= turnAcc;

            if (right)
                sideAcc += turnAcc;

            if (!left && !right && sideAcc != 0)
                if (sideAcc > 0)
                {
                    sideAcc -= sideAccLoss;
                    if (sideAcc < 0)
                        sideAcc = 0;
                }
                else
                {
                    sideAcc += sideAccLoss;
                    if (sideAcc > 0)
                        sideAcc = 0;
                }

            float newDirection = direction + sideAcc;                        //calculate new postion and direction, they cant be
            Vector2 newPosition = CalculateNewPosition(speed, newDirection); //changed without collisions check

            if (IsPositionAvailable(newPosition, newDirection))
            {
                position = newPosition;
                direction = newDirection;
            }
            else
                Collision();

        }

        public List<Vector3> GetPointsToDraw() //temp
        {
            List<Vector3> points = GetCollisionPoints(position, direction);
            Vector3 point = new Vector3();
            point.X = position.X;
            point.Y = position.Y;
            point.Z = 1;

            points.Add(point);
            return points;
        }
    }
}
