using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class ParticlesLogic
    {
        class ParticlesEnvironment
        {
            public class Particle
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                public Particle(Vector2 position, float direction, float speed, Vector2 size, float alpha, int skin)
                {
                    this.position = position;
                    this.direction = direction;
                    this.speed = speed;
                    this.size = size;
                    this.skin = skin;
                    this.alpha = alpha;
                }

                public Vector2 position;
                public float direction;
                public float speed;
                public Vector2 size;                
                public float alpha;
                public int skin;

                private Vector2 CalculateNewPosition()
                {
                    Vector2 newPosition;
                    newPosition.X = position.X + (Helper.timeCoherenceMultiplier * speed * (float)Math.Sin(MathHelper.ToRadians(direction)));
                    newPosition.Y = position.Y - (Helper.timeCoherenceMultiplier * speed * (float)Math.Cos(MathHelper.ToRadians(direction)));
                    return newPosition;
                }

                public void Move()
                {
                    position = CalculateNewPosition();
                }
            }

            public class Wind
            {
                public Wind(float strength, float direction, Vector2 randDirection, float directionChange)
                {
                    this.strength = strength;
                    this.direction = direction;
                    this.randDirection = randDirection;
                    this.directionChange = directionChange;
                    this.newDirection = direction;
                    this.defaultDirection = direction;
                }

                public Wind()
                {
                    this.strength = 0;
                    this.direction = 0;
                    this.randDirection = new Vector2(0, 0);
                    this.directionChange = 0;
                    this.newDirection = 0;
                    this.defaultDirection = 0;
                }

                public float strength;
                public float direction;

                private Vector2 randDirection = new Vector2(0, 0);
                private float directionChange = (float)1;
                private float newDirection;
                private float defaultDirection;

                /// <summary>
                /// Decide if the wind should rotate left or right.
                /// </summary>
                /// <returns>TRUE for left and FALSE for right.</returns>
                private bool ShouldRotateLeft(float currentDirection, float desiredDirection)
                {
                    bool rotateLeft = false;

                    if (Math.Max(currentDirection, desiredDirection) - Math.Min(currentDirection, desiredDirection) < 180)
                    {
                        rotateLeft = true;
                    }

                    if (currentDirection < desiredDirection)
                    {
                        rotateLeft = !rotateLeft;
                    }

                    return rotateLeft;
                }

                /// <summary>
                /// Gradually rotates pedestrian to the given direction.
                /// </summary>
                private void RotateToDirection(float desiredDirection)
                {
                    if (ShouldRotateLeft(direction, desiredDirection))
                    {
                        direction -= directionChange * Helper.timeCoherenceMultiplier;

                        // Nie pozwól na ujemny direction.
                        if (direction < 0)
                        {
                            direction += 360;
                        }
                    }
                    else
                    {
                        direction += directionChange * Helper.timeCoherenceMultiplier;

                        // Nie pozwól na direction powyżej 360.
                        if (direction > 360)
                        {
                            direction -= 360;
                        }
                    }
                }

                private float GetRandomNumber(float minimum, float maximum)
                {
                    return (float)Helper.random.NextDouble() * (maximum - minimum) + minimum;
                }

                public void Update()
                {
                    RotateToDirection(newDirection);

                    // Jeśli osiągnięto odpowiedni kierunek wybierz nowy.
                    if (Math.Abs(newDirection - direction) < 5)
                    {
                        newDirection = GetRandomNumber(defaultDirection + randDirection.X, defaultDirection + randDirection.Y);
                    }
                }
            }

            public Wind wind;
            public float shrinkRate;
            public float alphaRate;
            public float constAcc;
            
            public List<Particle> particlesList = new List<Particle>();

            /// <summary>
            /// Constructor.
            /// </summary>
            public ParticlesEnvironment(float shrinkRate, float alphaRate, Wind wind, float constAcc)
            {
                this.shrinkRate = shrinkRate;
                this.alphaRate = alphaRate;
                this.wind = wind;
                this.constAcc = constAcc;
            }

            /// <summary>
            /// This functions spawns the particle subjected to the conditions of the environment.
            /// </summary>
            public void Spawn(Vector2 position, float direction, float speed, Vector2 size, float opacity, int skin)
            {
                Particle particle = new Particle(position, direction, speed, size, opacity, skin);
                particlesList.Add(particle);
            }

            /// <summary>
            /// Convert physical vector (value, direction) to the mathematical vector [x, y].
            /// </summary>
            private Vector2 ToMathVector(float value, float direction)
            {
                Vector2 vector;
                vector.X = (float)Math.Sin(MathHelper.ToRadians(direction)) * value;
                vector.Y = -(float)Math.Cos(MathHelper.ToRadians(direction)) * value;
                return vector;
            }

            /// <summary>
            /// Convert mathematical vector [x, y] to the physical vector (value, direction).
            /// </summary>
            /// <returns>Vector2(value, direction)</returns>
            private Vector2 ToPhysVector(Vector2 vector)
            {
                float direction = (float)Math.Atan2(vector.X, -vector.Y);
                float value = vector.X / (float)Math.Sin(direction);
                direction = MathHelper.ToDegrees(direction);
                
                if (direction < 0)
                    direction += 360;

                if (direction > 360)
                    direction -= 360;

                return new Vector2(value, direction);
            }

            /// <summary>
            /// Update this environment.
            /// </summary>
            public void Update()
            {
                //Vector2 vector = ToMathVector(10, 45);
                //vector = ToPhysVector(new Vector2(-7, -7));

                // Update the wind.
                wind.Update();

                foreach (Particle particle in particlesList)
                {
                    // Scale the particle.
                    particle.size *= shrinkRate;

                    // Change the alpha value.
                    particle.alpha *= alphaRate;

                    // Change the speed and direction (wind).
                    Vector2 particleVector = ToMathVector(particle.speed, particle.direction);
                    Vector2 windVector = ToMathVector(wind.strength, wind.direction);
                    particleVector += windVector;
                    
                    particleVector = ToPhysVector(particleVector);

                    particle.speed = particleVector.X;
                    particle.direction = particleVector.Y;

                    // Change the speed (constAcc).
                    particle.speed += constAcc;

                    // Move the particle.
                    particle.Move();
                }
            }
        }

        // Define environments here as normal variables.

        // Exhaust fumes.
        ParticlesEnvironment exhaustFumes = new ParticlesEnvironment((float)1.05, (float)0.95, new ParticlesEnvironment.Wind((float)2, 45, new Vector2(-30, 30), (float)10), 0);
        float lastFumesSpawn = 0;
        float spawnFumesEvery = (float)0.03;
        Vector2 fumeSize = new Vector2(10, 10);       // Starting size.
        float fumesSpeed = 15;                        // Starting speed of the exhaust fumes.
        Vector2 randDirection = new Vector2(-25, 25); // Random value from this interval is added to the direction. [X - min, Y - max]

        // Rain.
        ParticlesEnvironment rain = new ParticlesEnvironment(1, 1, new ParticlesEnvironment.Wind(), 0);
        float lastRainSpawn = 0;
        float currentSpawnRainEvery = 0;
        float spawnRainEvery = (float)0.02;
        Vector2 rainSize = new Vector2(5, 40);       // Starting size.
        float rainSpeed = 800;                       // Starting speed of the rain particle.
        float rainDirection = 170;

        bool rainEnabled = false; // Decyduje czy deszcz pada czy nie.

        /// <summary>
        /// Uruchom deszcz.
        /// </summary>
        public void EnableRain()
        {
            rainEnabled = true;
        }

        /// <summary>
        /// Wyłącz deszcz.
        /// </summary>
        public void DisableRain()
        {
            rainEnabled = false;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ParticlesLogic()
        {
            
        }

        public List<Object> GetParticlesToDraw()
        {
            List<Object> list = new List<Object>();
            
            foreach (ParticlesEnvironment.Particle particle in exhaustFumes.particlesList)
            {
                list.Add(new Object(particle.skin.ToString() + "$" + particle.alpha, particle.position, particle.size, particle.direction));
            }

            foreach (ParticlesEnvironment.Particle particle in rain.particlesList)
            {
                list.Add(new Object(particle.skin.ToString() + "$" + particle.alpha, particle.position, particle.size, particle.direction));
            }

            return list;
        }

        private float GetRandomNumber(float minimum, float maximum)
        {
            return (float)Helper.random.NextDouble() * (maximum - minimum) + minimum;
        }

        public int CountFumes()
        {
            return exhaustFumes.particlesList.Count;
        }

        public void Update(TrafficLogic trafficLogic, BusLogic busLogic)
        {
            // EXHAUST FUMES.
            // Increase last spawned counter.
            lastFumesSpawn += Helper.timeCoherenceMultiplier;

            // If it is time to spawn.
            if (lastFumesSpawn > spawnFumesEvery)
            {
                // Get the information about the exhaust pipes.
                List<Vector4> spawnPoints = trafficLogic.GetExhaustPipePositions();
                spawnPoints.Add(busLogic.GetExhaustPipePosition());

                // Spawn at each location.
                foreach (Vector4 spawnPoint in spawnPoints)
                {
                    // Calculate the direction (car direction - 180).
                    float direction = spawnPoint.Z - 180;

                    direction += GetRandomNumber(randDirection.X, randDirection.Y);

                    if (direction < 0)
                        direction += 360;

                    if (direction > 0)
                        direction -= 360;
                    
                    // Spawn.
                    exhaustFumes.Spawn(new Vector2(spawnPoint.X, spawnPoint.Y), direction, fumesSpeed - spawnPoint.W /* Speed must be relative to the car. */, fumeSize, (float)0.5, 0);  
                }

                lastFumesSpawn = 0;
            }

            exhaustFumes.Update();

            exhaustFumes.particlesList.RemoveAll(delegate(ParticlesEnvironment.Particle particle) // Usuńmy cząsteczki, które są za duże.
            {
                if (particle.size.X > 100)
                    return true;
                else
                    return false;
            });

            // RAIN.                   
            lastRainSpawn += Helper.timeCoherenceMultiplier;

            if (rainEnabled)
            {
                currentSpawnRainEvery -= 0.1f;
                if (currentSpawnRainEvery < spawnRainEvery)
                    currentSpawnRainEvery = spawnRainEvery;
            }
            else
            {
                currentSpawnRainEvery += 0.1f;
                if (currentSpawnRainEvery > 5)
                    currentSpawnRainEvery = 5;
            }

            if (lastRainSpawn > currentSpawnRainEvery && currentSpawnRainEvery < 5)
            {
                Vector2 position = new Vector2(Helper.random.Next((int)(Helper.mapPos.X - (Helper.workAreaSize.X / 2)), (int)(Helper.mapPos.X + (Helper.workAreaSize.X / 2))), Helper.mapPos.Y - (Helper.workAreaSize.Y / 2));
                rain.Spawn(position, rainDirection, rainSpeed, rainSize, 0.5f, 1);
                lastRainSpawn = 0;
            }

            rain.Update();

            rain.particlesList.RemoveAll(delegate(ParticlesEnvironment.Particle particle) // Usuńmy cząsteczki, które są za daleko.
            {
                if (particle.position.Y > Helper.mapPos.Y + (Helper.workAreaSize.Y / 2))
                    return true;
                else
                    return false;
            });
        }
    }
}
