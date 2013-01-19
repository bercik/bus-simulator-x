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
        Texture2D exhaustFumesTexture, rainTexture;

        Vector2 exhaustFumesTextureOrigin, exhaustFumesTextureScale, rainTextureOrigin, rainTextureScale;

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

            rainTexture = content.Load<Texture2D>("particles/rain");
            rainTextureOrigin = new Vector2(rainTexture.Width / 2, rainTexture.Height / 2);
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

            string[] split = particle.name.Split(new char[] { '$' });

            float opacity = (float)System.Convert.ToDouble(split[1]);

            if (int.Parse(split[0]) == 0)
            {
                exhaustFumesTextureScale = new Vector2(particle.size.X / exhaustFumesTexture.Width, particle.size.Y / exhaustFumesTexture.Height);

                spriteBatch.Draw(exhaustFumesTexture, position, null, Color.White * opacity, MathHelper.ToRadians(particle.rotate), exhaustFumesTextureOrigin, Helper.GetVectorScale() * exhaustFumesTextureScale, SpriteEffects.None, 1);
            }

            if (int.Parse(split[0]) == 1)
            {
                rainTextureScale = new Vector2(particle.size.X / rainTexture.Width, particle.size.Y / rainTexture.Height);

                spriteBatch.Draw(rainTexture, position, null, Color.White * opacity, MathHelper.ToRadians(particle.rotate), rainTextureOrigin, Helper.GetVectorScale() * rainTextureScale, SpriteEffects.None, 1);
            }
        }
    }
}