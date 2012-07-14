using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class BusLogic
    {
        public Vector2 position;
        private float direction;
        private float speed;
        private float sideAcc;
        private Vector2 size;

        private float speedMultiplier = 4;
        private float brakeAcc = 10;
        private float turnAcc = (float)0.005;
        private float sideAccLoss = (float)0.01;
        private float speedDecay = (float)0.5;

        private GearBox gearBox = new GearBox();
        private Wheel wheel = new Wheel();     

        public int GetCurrentGear()
        {
            return gearBox.currentGear;
        }

        public float GetCurrentSpeed()
        {
            return speed;
        }

        public float GetCurrentDirection()
        {
            return direction;
        }
        
        public float GetCurrentAcceleration()
        {
            return gearBox.GetAcceleration(speed);
        }
        
        public Vector2 GetBusPosition() //Get the center of the bus to draw the center of the map
        {
            Vector2 busPosition;

            busPosition.X = position.X + (size.Y * (float)Math.Sin(DegToRad(direction)) / 2);
            busPosition.Y = position.Y - (size.Y * (float)Math.Cos(DegToRad(direction)) / 2);

            return busPosition;
        }

        public Vector2 GetOrigin()
        {
            return size / 2;
        }

        public Vector2 GetSize()
        {
            return size;
        }

        public float GetDirection()
        {
            return direction;
        }

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

            public int currentGear = 1;
            int minGear = 1;
            int maxGear = 5;
            Gear[] gears = new Gear[6];

            public GearBox() //constructor
            {
                gears[0] = new Gear(0, 1, 1, 1, 1);
                gears[1] = new Gear(0, 10, 10, (float)0.55, 5);
                gears[2] = new Gear(10, 10, 8, (float)0.5, 5);
                gears[3] = new Gear(25, 10, 0, (float)0.5, (float)5.3);
                gears[4] = new Gear(40, 10, 0, (float)0.4, (float)4.5);
                gears[5] = new Gear(60, 10, 0, (float)0.35, (float)4.2);
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

        class Wheel //wheel is responsible for calculating changes in direction...
        {
            class AccelerationCurve //...and it is basically a curve
            {
                public float addToSpeed;
                public bool numberIsNegative;
                public float logBase;
                public float addToAll;
                public float ratio;

                public AccelerationCurve(float addToSpeed, bool numberIsNegative, float logBase, float addToAll, float ratio) //constructor
                {
                    this.addToSpeed = addToSpeed;
                    this.numberIsNegative = numberIsNegative;
                    this.logBase = logBase;
                    this.addToAll = addToAll;
                    this.ratio = ratio;
                }
            }

            private float optimalSpeed;
            AccelerationCurve[] curves = new AccelerationCurve[2];

            public Wheel() //constructor
            {
                curves[0] = new AccelerationCurve(20, true, (float)0.5, 5, (float)0.5);
                curves[1] = new AccelerationCurve(20, false, (float)0.35, (float)5.3, (float)0.5);
                optimalSpeed = 12;
            }

          /*  public float GetAcceleration(float busSpeed)
            {
                float acceleration;
                AccelerationCurve curve;

               // if (busSpeed >= )

               // acceleration = Math.Log();
                //return;
            }*/
        }

        public BusLogic(float x, float y, float direction, float speed, Vector2 size) //constructor
        {
            this.position.X = x;
            this.position.Y = y;
            this.direction = direction;
            this.speed = speed;
            this.size = size;
        }

        private float DegToRad(float degrees) //convert degrees to radians
        {
            return MathHelper.ToRadians(degrees);
        }
        
        private List<Vector2> GetCollisionPoints(Vector2 busPosition, float busDirection) //returns 4 collision points - they are
        {                                                                                 //used in IsPositionAvailable function
            Vector2 p1, p2, p3, p4; //create 4 points

            p3.X = busPosition.X + ((size.X * (float)Math.Cos(DegToRad(busDirection))) / 2); //calculate their positions
            p3.Y = busPosition.Y + ((size.X * (float)Math.Sin(DegToRad(busDirection))) / 2);

            p4.X = busPosition.X - (size.X * (float)Math.Cos(DegToRad(busDirection)) / 2);
            p4.Y = busPosition.Y - (size.X * (float)Math.Sin(DegToRad(busDirection)) / 2);

            p1.X = p4.X + (size.Y * (float)Math.Sin(DegToRad(busDirection)));
            p1.Y = p4.Y - (size.Y * (float)Math.Cos(DegToRad(busDirection)));

            p2.X = p3.X + (size.Y * (float)Math.Sin(DegToRad(busDirection)));
            p2.Y = p3.Y - (size.Y * (float)Math.Cos(DegToRad(busDirection)));

            var pointsList = new List<Vector2> {p1, p2, p3 , p4 }; //create list and add points
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
            newPosition.X = position.X + (speedMultiplier * busSpeed * (float)Math.Sin(DegToRad(busDirection)));
            newPosition.Y = position.Y - (speedMultiplier * busSpeed * (float)Math.Cos(DegToRad(busDirection)));
            return newPosition;
        }

        private void Collision()
        {
            speed = 0;
        }

        public void Update(bool accelerate, bool brake, bool left, bool right, bool gearUp, bool gearDown, TimeSpan framesInterval) //main update function adjsuting speed, direction and so on
        {
            float timeCoherenceMultiplier = (float)framesInterval.Milliseconds / 1000;

            if (accelerate)
                speed += gearBox.GetAcceleration(speed) * timeCoherenceMultiplier;

            if (brake)
                speed -= brakeAcc * timeCoherenceMultiplier;

            if (speed < 0)
                speed = 0;

            if (left)
                sideAcc -= turnAcc;

            if (right)
                sideAcc += turnAcc;

            if (gearUp)
                gearBox.GearUp();

            if (gearDown)
                gearBox.GearDown();

            if (speed > 0)
                speed = speed - speedDecay * timeCoherenceMultiplier;

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
            Vector2 newPosition = CalculateNewPosition(speed * timeCoherenceMultiplier, newDirection); //changed without collisions check

            if (newDirection > 360)
                newDirection -= 360;

            if (newDirection < 0)
                newDirection += 360;

            if (IsPositionAvailable(newPosition, newDirection))
            {
                position = newPosition;
                direction = newDirection;
            }
            else
                Collision();

        }

        public List<Vector2> GetPointsToDraw() //temp
        {
            List<Vector2> points = GetCollisionPoints(position, direction);
            Vector2 point = new Vector2();
            point.X = position.X;
            point.Y = position.Y;

            points.Add(point);

            point.X = GetBusPosition().X;
            point.Y = GetBusPosition().Y;

            points.Add(point);

            return points;
        }
    }
}
