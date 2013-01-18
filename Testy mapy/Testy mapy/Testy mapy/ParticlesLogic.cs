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
            class Particle
            {
                public Vector2 position;
                public float direction;
                public float speed;
                public float size;                
                public float opacity;
                public int skin;
            }

            public class Wind
            {
                public float strength;
                public float direction;
            }

            public Wind wind = new Wind();
            public float shrinkRate;


            /// <summary>
            /// This functions spawns the particle subjected to the conditions of the environment.
            /// </summary>
            public void Spawn(Vector2 position, float direction, float speed, float size, float opacity)
            {

            }

            List<Particle> particlesList;
        }

        public void Update()
        {

        }
    }
}
