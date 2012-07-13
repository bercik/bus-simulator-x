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
        private GearBox gearBox;     

        class GearBox //gearbox is responsible for calculating accelerations of the bus...
        {
            class Gear //...it is full of the gears...
            {
                public Gear(float optimalSpeed, float accelerationMultiplier, float addToSpeed, float logBase, float addToAll)
                {
                    this.accCurve = new AccelerationCurve(addToSpeed, logBase, addToAll);
                    this.optimalSpeed = optimalSpeed;
                    this.accelerationMultiplier = accelerationMultiplier;
                }

                class AccelerationCurve //...and gears are basically curves
                {
                    float addToSpeed;
                    float logBase;
                    float addToAll;
                    
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
            }

            int currentGear = 1;
            Gear[] gears = new Gear[5];

          /*  public float GetAcceleration()
            {
                float acceleration;

                acceleration = gears[currentGear];

                return acceleration;
            }*/

            public GearBox()
            {
                gears[1] = new Gear(0, 1, 10, (float)0.55, 5);
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
        
        private List<Vector2> GetCollisionPoints(Vector2 busPosition, float busDirection) //returns 4 collision points - they are
        {                                                                                 //used in IsPositionAvailable function
            Vector2 p1, p2, p3, p4; //create 4 points

            p3.X = busPosition.X + (width * (float)Math.Sin(45 - busDirection) / 2); //calculate their positions
            p3.Y = busPosition.Y + (width * (float)Math.Cos(45 - busDirection) / 2);

            p4.X = busPosition.X - (width * (float)Math.Sin(45 - busDirection) / 2);
            p4.Y = busPosition.Y - (width * (float)Math.Cos(45 - busDirection) / 2);

            p1.X = p4.X + (height * (float)Math.Sin(busDirection));
            p1.Y = p4.Y - (height * (float)Math.Cos(busDirection));

            p2.X = p3.X + (height * (float)Math.Sin(busDirection));
            p2.Y = p3.Y - (height * (float)Math.Cos(busDirection));

            var pointsList = new List<Vector2> {p1, p2, p3 , p4 }; //create list and add points
            return pointsList;
        }

        private bool IsPositionAvailable(Vector2 busPosition, float busDirection) //check if new position is available
        {            
            List<Vector2> pointsList = GetCollisionPoints(busPosition, busDirection); //create list of the points

            foreach(Vector2 point in pointsList)
            {
                //[!marker] check if there is no collision on the map
                /*something like
                 if (isCollision(point))
                   return false;
                 */
            }

            return true;
        }

        public void Update(bool accelerate, bool brake, bool left, bool right, TimeSpan framesInterval) //main update function adjsuting speed, direction and so on
        {
            


        }

        public List<Vector2> GetPointsToDraw() //temp
        {
            List<Vector2> points = GetCollisionPoints(position, direction);
            return points;
        }
    }
}
