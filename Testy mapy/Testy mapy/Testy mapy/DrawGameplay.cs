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
    class DrawGameplay
    {
        const int numberOfPedestrians = 3;
        Texture2D stopAreaTexture, stopAreaActiveTexture, signTexture, deadPedestrianTexture;
        
        Texture2D[] pedestrianTexture = new Texture2D[numberOfPedestrians];
        Vector2[] pedestrianTextureOrigin = new Vector2[numberOfPedestrians];
        Vector2[] pedestrianTextureScale = new Vector2[numberOfPedestrians];

        Vector2 stopAreaTextureOrigin, stopAreaTextureScale,
                stopAreaActiveTextureOrigin, stopAreaActiveTextureScale,
                signTextureOrigin, signTextureScale,
                deadPedestrianTextureOrigin, deadPedestrianTextureScale;

        // Constructor.
        public DrawGameplay()
        {
        }

        /// <summary>
        /// Load sprites.
        /// </summary>
        public void LoadContent(ContentManager content, Vector2 pedestrianSize, Vector2 stopAreaSize, Vector2 signSize)
        {
            stopAreaTexture = content.Load<Texture2D>("busstops/busstoparea");
            stopAreaTextureOrigin = new Vector2(stopAreaTexture.Width / 2, stopAreaTexture.Height / 2);
            stopAreaTextureScale = new Vector2(stopAreaSize.X / stopAreaTexture.Width, stopAreaSize.Y / stopAreaTexture.Height);

            stopAreaActiveTexture = content.Load<Texture2D>("busstops/busstoparea_active");
            stopAreaActiveTextureOrigin = new Vector2(stopAreaActiveTexture.Width / 2, stopAreaActiveTexture.Height / 2);
            stopAreaActiveTextureScale = new Vector2(stopAreaSize.X / stopAreaActiveTexture.Width, stopAreaSize.Y / stopAreaActiveTexture.Height);

            signTexture = content.Load<Texture2D>("busstops/busstopsign");
            signTextureOrigin = new Vector2(signTexture.Width / 2, signTexture.Height / 2);
            signTextureScale = new Vector2(signSize.X / signTexture.Width, signSize.Y / signTexture.Height);


            deadPedestrianTexture = content.Load<Texture2D>("pedestrians/died_pedestrian0");
            deadPedestrianTextureOrigin = new Vector2(deadPedestrianTexture.Width / 2, deadPedestrianTexture.Height / 2);
            deadPedestrianTextureScale = new Vector2(pedestrianSize.X / deadPedestrianTexture.Width, pedestrianSize.Y / deadPedestrianTexture.Height);

            for (int i = 0; i < numberOfPedestrians; i++)
            {
                pedestrianTexture[i] = content.Load<Texture2D>("pedestrians/pedestrian" + i.ToString());
                pedestrianTextureOrigin[i] = new Vector2(pedestrianTexture[i].Width / 2, pedestrianTexture[i].Height / 2);
                pedestrianTextureScale[i] = new Vector2(pedestrianSize.X / pedestrianTexture[i].Width, pedestrianSize.Y / pedestrianTexture[i].Height);
            }
        }

        /// <summary>
        /// Main function.
        /// </summary>
        public void Draw(GameplayLogic gameplayLogic, SpriteBatch spriteBatch)
        {
            List<Object> objectsList = gameplayLogic.GetStopAreasToDraw();
            foreach (Object stopArea in objectsList)
                DrawStopArea(spriteBatch, stopArea);

            objectsList = gameplayLogic.GetPedestriansToDraw();
            foreach (Object pedestrian in objectsList)
                DrawPedestrian(spriteBatch, pedestrian);

            objectsList = gameplayLogic.GetSignsToDraw();
            foreach (Object sign in objectsList)
                DrawSign(spriteBatch, sign);
        }

        public void DrawPedestrian(SpriteBatch spriteBatch, Object pedestrian)
        {
            Vector2 position = Helper.MapPosToScreenPos(pedestrian.pos);
            position = Helper.CalculateScalePosition(position);

            if (int.Parse(pedestrian.name) == -1)
            {
                spriteBatch.Draw(deadPedestrianTexture, position, null, Color.White, MathHelper.ToRadians(pedestrian.rotate), deadPedestrianTextureOrigin, Helper.GetVectorScale() * deadPedestrianTextureScale, SpriteEffects.None, 1);
            }
            else
            {
                spriteBatch.Draw(pedestrianTexture[int.Parse(pedestrian.name)], position, null, Color.White, MathHelper.ToRadians(pedestrian.rotate), pedestrianTextureOrigin[int.Parse(pedestrian.name)], Helper.GetVectorScale() * pedestrianTextureScale[int.Parse(pedestrian.name)], SpriteEffects.None, 1);
            }
        }

        public void DrawStopArea(SpriteBatch spriteBatch, Object stopArea)
        {
            Vector2 position = Helper.MapPosToScreenPos(stopArea.pos);
            position = Helper.CalculateScalePosition(position);

            if (stopArea.name == "True")
            {
                spriteBatch.Draw(stopAreaActiveTexture, position, null, Color.White, MathHelper.ToRadians(stopArea.rotate), stopAreaActiveTextureOrigin, Helper.GetVectorScale() * stopAreaActiveTextureScale, SpriteEffects.None, 1);
            }
            else
            {
                spriteBatch.Draw(stopAreaTexture, position, null, Color.White, MathHelper.ToRadians(stopArea.rotate), stopAreaTextureOrigin, Helper.GetVectorScale() * stopAreaTextureScale, SpriteEffects.None, 1);
            }
        }

        public void DrawSign(SpriteBatch spriteBatch, Object sign)
        {
            Vector2 position = Helper.MapPosToScreenPos(sign.pos);
            position = Helper.CalculateScalePosition(position);

            spriteBatch.Draw(signTexture, position, null, Color.White, MathHelper.ToRadians(sign.rotate), signTextureOrigin, Helper.GetVectorScale() * signTextureScale, SpriteEffects.None, 1);
        }
    }
}
