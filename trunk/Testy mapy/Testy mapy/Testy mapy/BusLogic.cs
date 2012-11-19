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
        private Vector2 size;

        private Vector2 newPosition;
        private float newDirection;

        private float speedMultiplier = 4;
        private float brakeAcc = 20;
        private float speedDecay = (float)0.5;

        private GearBox gearBox = new GearBox();
        private Wheel wheel = new Wheel();

        /// <summary>
        /// Set bus direction.
        /// </summary>
        public void SetDirection(float direction)
        {
            this.direction = direction;
        }

        /// <summary>
        /// Set bus position.
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            this.position = position;
        }

        /// <summary>
        /// Get current gear.
        /// </summary>
        public int GetCurrentGear()
        {
            return gearBox.currentGear;
        }

        /// <summary>
        /// Get current speed.
        /// </summary>
        public float GetCurrentSpeed()
        {
            return speed;
        }

        /// <summary>
        /// Get current direction.
        /// </summary>
        public float GetCurrentDirection()
        {
            return direction;
        }

        /// <summary>
        /// Get current acceleration.
        /// </summary>
        public float GetCurrentAcceleration()
        {
            return gearBox.GetAcceleration(speed);
        }

        /// <summary>
        /// Get the center of the bus eg. for drawing.
        /// </summary>
        public Vector2 GetBusPosition() //Get the center of the bus to draw the center of the map
        {
            return CalculateCenter(position, direction);
        }

        /// <summary>
        /// Get real bus position - directly from the variable.
        /// </summary>
        public Vector2 GetRealPosition() //Get the real position
        {
            return position;
        }

        public Vector2 CalculateCenter(Vector2 busPosition, float busDirection)
        {
            Vector2 center;

            center.X = busPosition.X + (size.Y * (float)Math.Sin(DegToRad(busDirection)) / 2);
            center.Y = busPosition.Y - (size.Y * (float)Math.Cos(DegToRad(busDirection)) / 2);

            return center;
        }

        public Vector2 GetOrigin()
        {
            return size / 2;
        }

        public float GetSideAcceleration()
        {
            return wheel.GetSideAcceleration();
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
                bool reversedGear;

                public Gear(bool reversedGear, float optimalSpeed, float accelerationMultiplier, float addToSpeed, float logBase, float addToAll) //constructor
                {
                    this.reversedGear = reversedGear;
                    this.accCurve = new AccelerationCurve(addToSpeed, logBase, addToAll);
                    this.optimalSpeed = optimalSpeed;
                    this.accelerationMultiplier = accelerationMultiplier;
                }

                public float GetAcceleration(float busSpeed) //each gear can calculate its own acceleration for given speed
                {
                    float calculationsSpeed = busSpeed;

                    if (!reversedGear)
                    {
                        if (busSpeed < optimalSpeed) //if below opimal speed recalculate it to get the right curve
                            calculationsSpeed = optimalSpeed + (optimalSpeed - busSpeed);
                    }
                    else
                    {
                        if (calculationsSpeed < 0)
                            calculationsSpeed = -calculationsSpeed;

                        if (busSpeed > 0)
                            calculationsSpeed = 0;
                    }

                    float acceleration = accelerationMultiplier * ((float)Math.Log(calculationsSpeed + accCurve.addToSpeed, accCurve.logBase) + accCurve.addToAll);

                    if (!reversedGear && busSpeed < 0 && acceleration < 0)
                        acceleration = -acceleration;

                    if (reversedGear && acceleration > 0)
                        acceleration = -acceleration;


                    return acceleration;
                }
            }

            public int currentGear = 1;
            int minGear = 0;
            int maxGear = 5;
            Gear[] gears = new Gear[6];

            public GearBox() //constructor
            {
                gears[0] = new Gear(true, 0, 10, 10, (float)0.55, 5);
                gears[1] = new Gear(false, 0, 10, 10, (float)0.55, 5);
                gears[2] = new Gear(false, 10, 10, 8, (float)0.5, 5);
                gears[3] = new Gear(false, 25, 10, 0, (float)0.5, (float)5.3);
                gears[4] = new Gear(false, 40, 10, 0, (float)0.4, (float)4.5);
                gears[5] = new Gear(false, 60, 10, 0, (float)0.35, (float)4.2);
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
            AccelerationCurve[] maxCurves = new AccelerationCurve[2];
            private float sideAcc;
            private float sideAccLoss = (float)1;

            public Wheel() //constructor
            {
                curves[0] = new AccelerationCurve(20, true, (float)0.6, 6, (float)1);
                curves[1] = new AccelerationCurve(20, false, (float)0.35, (float)5.3, (float)1);
                maxCurves[0] = new AccelerationCurve(20, true, (float)0.6, 6, (float)0.5);
                maxCurves[1] = new AccelerationCurve(20, false, (float)0.35, (float)5.3, (float)0.5);
                optimalSpeed = 12;
            }

            public float GetSideAcceleration()
            {
                return sideAcc;
            }

            private float GetAcceleration(float busSpeed)
            {
                if (busSpeed == 0)
                    return 0;

                AccelerationCurve curve;

                if (busSpeed >= optimalSpeed)
                    curve = curves[1];
                else
                    curve = curves[0];

                float speed = busSpeed;

                if (busSpeed < 0)
                    busSpeed = -busSpeed;

                if (curve.numberIsNegative)
                    speed = -speed;

                float acceleration = ((float)Math.Log(speed + curve.addToSpeed, curve.logBase) + curve.addToAll) * curve.ratio;
                return acceleration;
            }

            private float GetMaxSideAcceleration(float busSpeed)
            {
                if (busSpeed == 0)
                    return 0;

                AccelerationCurve curve;

                if (busSpeed >= optimalSpeed)
                    curve = maxCurves[1];
                else
                    curve = maxCurves[0];

                float speed = busSpeed;

                if (busSpeed < 0)
                    speed = -busSpeed;

                if (curve.numberIsNegative)
                    speed = -speed;

                float acceleration = ((float)Math.Log(speed + curve.addToSpeed, curve.logBase) + curve.addToAll) * curve.ratio;
                return acceleration;
            }

            public float GetDirectionChange(float busSpeed, bool right, bool left, float timeCoherenceMultiplier)
            {
                if (right)
                    sideAcc = sideAcc + GetAcceleration(busSpeed) * timeCoherenceMultiplier;

                if (left)
                    sideAcc = sideAcc - GetAcceleration(busSpeed) * timeCoherenceMultiplier;

                float maximalSideAcc;

                /* if (busSpeed < 10)
                     maximalSideAcc = (float)maxSideAcc / 2;
                 else
                     maximalSideAcc = maxSideAcc;*/

                maximalSideAcc = GetMaxSideAcceleration(busSpeed);

                if (sideAcc > maximalSideAcc)
                    sideAcc = maximalSideAcc;

                if (sideAcc < -maximalSideAcc)
                    sideAcc = -maximalSideAcc;

                if (sideAcc != 0 && !right && !left)
                {
                    if (sideAcc > 0)
                    {
                        sideAcc = sideAcc - sideAccLoss * timeCoherenceMultiplier;
                        if (sideAcc < 0)
                            sideAcc = 0;
                    }
                    else
                    {
                        sideAcc = sideAcc + sideAccLoss * timeCoherenceMultiplier;
                        if (sideAcc > 0)
                            sideAcc = 0;
                    }
                }

                return sideAcc;
            }
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

        public Vector2[] GetCollisionPoints(Vector2 busPosition, float busDirection) //returns 4 collision points
        {
            Vector2 p1, p2, p3, p4; //create 4 points

            p3.X = busPosition.X + ((size.X * (float)Math.Cos(DegToRad(busDirection))) / 2); //calculate their positions
            p3.Y = busPosition.Y + ((size.X * (float)Math.Sin(DegToRad(busDirection))) / 2);

            p4.X = busPosition.X - (size.X * (float)Math.Cos(DegToRad(busDirection)) / 2);
            p4.Y = busPosition.Y - (size.X * (float)Math.Sin(DegToRad(busDirection)) / 2);

            p1.X = p4.X + (size.Y * (float)Math.Sin(DegToRad(busDirection)));
            p1.Y = p4.Y - (size.Y * (float)Math.Cos(DegToRad(busDirection)));

            p2.X = p3.X + (size.Y * (float)Math.Sin(DegToRad(busDirection)));
            p2.Y = p3.Y - (size.Y * (float)Math.Cos(DegToRad(busDirection)));

            Vector2[] pointsArray = new Vector2[4] { p1, p2, p3, p4 }; //create list and add points

            return pointsArray;
        }

        public Vector2[] GetCollisionPoints() //returns 4 collision points without parameters
        {
            return GetCollisionPoints(position, direction);
        }

        public Vector2 GetDesiredPosition()
        {
            return newPosition;
        }

        public float GetDesiredDirection()
        {
            return newDirection;
        }

        public void AcceptNewPositionAndDirection()
        {
            direction = newDirection;
            position = newPosition;
        }

        private Vector2 CalculateNewPosition(float busSpeed, float busDirection)
        {
            Vector2 newPosition;
            newPosition.X = position.X + (speedMultiplier * busSpeed * (float)Math.Sin(DegToRad(busDirection)));
            newPosition.Y = position.Y - (speedMultiplier * busSpeed * (float)Math.Cos(DegToRad(busDirection)));
            return newPosition;
        }

        public void Collision()
        {
            speed = 0;
        }

        public void Update(bool accelerate, bool brake, bool left, bool right, bool gearUp, bool gearDown, TimeSpan framesInterval) //main update function adjsuting speed, direction and so on
        {
            float timeCoherenceMultiplier = (float)framesInterval.Milliseconds / 1000;

            if (accelerate)
                speed += gearBox.GetAcceleration(speed) * timeCoherenceMultiplier;

            if (brake)
            {
                if (speed > 0)
                {
                    speed -= brakeAcc * timeCoherenceMultiplier;
                    if (speed < 0)
                        speed = 0;
                }

                if (speed < 0)
                {
                    speed += brakeAcc * timeCoherenceMultiplier;
                    if (speed > 0)
                        speed = 0;
                }
            }

            if (gearUp)
                gearBox.GearUp();

            if (gearDown)
                gearBox.GearDown();

            if (speed > 0)
            {
                speed = speed - speedDecay * timeCoherenceMultiplier;
                if (speed < 0)
                    speed = 0;
            }

            if (speed < 0)
            {
                speed = speed + speedDecay * timeCoherenceMultiplier;
                if (speed > 0)
                    speed = 0;
            }

            newDirection = direction + wheel.GetDirectionChange(speed, right, left, timeCoherenceMultiplier);
            newPosition = CalculateNewPosition(speed * timeCoherenceMultiplier, newDirection);
        }

        public Vector2[] GetPointsToDraw()
        {
            List<Vector2> list = new List<Vector2>();
            Vector2[] pointsArray;

            pointsArray = GetCollisionPoints(position, direction);
            foreach (Vector2 point in pointsArray)
                list.Add(Helper.MapPosToScreenPos(point));

            return list.ToArray();
        }
    }
}