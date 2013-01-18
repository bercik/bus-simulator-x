using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Testy_mapy
{
    class DrawParticles
    {
        Texture2D exhaustFumesTexture;

        Vector2 exhaustFumesTextureOrigin, exhaustFumesTextureScale;

        // Constructor.
        public DrawParticles()
        {
        }

        /// <summary>
        /// Load sprites.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            exhaustFumesTexture = content.Load<Texture2D>("particles/exhaust_fumes");
            exhaustFumesTextureOrigin = new Vector2(exhaustFumesTexture.Width / 2, exhaustFumesTexture.Height / 2);
        }

        /// <summary>
        /// Main function.
        /// </summary>
        public void Draw(ParticlesLogic particlesLogic, SpriteBatch spriteBatch)
        {
            List<Object> objectsList = particlesLogic.GetParticlesToDraw();
            foreach (Object particle in objectsList)
                DrawParticle(spriteBatch, particle);
        }

        public void DrawParticle(SpriteBatch spriteBatch, Object particle)
        {
            Vector2 position = Helper.MapPosToScreenPos(particle.pos);
            position = Helper.CalculateScalePosition(position);

            if (int.Parse(particle.name) == 0)
            {
                exhaustFumesTextureScale = new Vector2(particle.size.X / exhaustFumesTexture.Width, particle.size.Y / exhaustFumesTexture.Height);

                spriteBatch.Draw(exhaustFumesTexture, position, null, Color.White, MathHelper.ToRadians(particle.rotate), exhaustFumesTextureOrigin, Helper.GetVectorScale() * exhaustFumesTextureScale, SpriteEffects.None, 1);
            }
        }
    }
}