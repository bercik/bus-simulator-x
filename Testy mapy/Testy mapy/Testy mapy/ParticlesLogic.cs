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
                public Particle(Vector2 position, float direction, float speed, Vector2 size, float opacity, int skin)
                {
                    this.position = position;
                    this.direction = direction;
                    this.speed = speed;
                    this.size = size;
                    this.skin = skin;
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
                public Wind(float strength, float direction)
                {
                    this.strength = strength;
                    this.direction = direction;
                }

                public float strength;
                public float direction;
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
                Vector2 vector = ToMathVector(10, 45);
                vector = ToPhysVector(new Vector2(-7, -7));

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
                    particle.speed *= constAcc;

                    // Move the particle.
                    particle.Move();
                }
            }
        }

        // Define environments here as normal variables.
        ParticlesEnvironment exhaustFumes = new ParticlesEnvironment((float)1.02, (float)0.09, new ParticlesEnvironment.Wind((float)0.5, 45), 1);
        float lastFumesSpawn = 0;
        float spawnFumesEvery = (float)0.1;
        Vector2 fumeSize = new Vector2(10, 10);
        float fumesSpeed = 10;

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
                list.Add(new Object(particle.skin.ToString(), particle.position, particle.size, particle.direction));
            }

            return list;
        }

        public void Update(TrafficLogic trafficLogic)
        {
            // Increase last spawned counter.
            lastFumesSpawn += Helper.timeCoherenceMultiplier;

            // If it is time to spawn.
            if (lastFumesSpawn > spawnFumesEvery)
            {
                // Get the information about the exhaust pipes.
                List<Vector4> spawnPoints = trafficLogic.GetExhaustPipePositions();

                // Spawn at each location.
                foreach (Vector4 spawnPoint in spawnPoints)
                {
                    // Calculate the direction (car direction - 180).
                    float direction = spawnPoint.Z - 180;
                    if (direction < 0)
                        direction += 360;
                    
                    // Spawn.
                    exhaustFumes.Spawn(new Vector2(spawnPoint.X, spawnPoint.Y), direction, fumesSpeed - spawnPoint.W, fumeSize, 1, 0);  
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
        }
    }
}
