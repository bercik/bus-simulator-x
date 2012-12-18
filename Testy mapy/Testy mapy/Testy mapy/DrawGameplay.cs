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
        Texture2D stopAreaTexture, stopAreaActiveTexture, waitingAreaTexture, signTexture;
        Texture2D[] pedestrianTexture = new Texture2D[numberOfPedestrians];
        Vector2[] pedestrianTextureOrigin = new Vector2[numberOfPedestrians];

        Vector2 stopAreaTextureOrigin, stopAreaActiveTextureOrigin, waitingAreaTextureOrigin, signTextureOrigin;

        // Constructor.
        public DrawGameplay()
        {
        }

        /// <summary>
        /// Load sprites.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            stopAreaTexture = content.Load<Texture2D>("busstops/busstoparea");
            stopAreaTextureOrigin = new Vector2(stopAreaTexture.Width / 2, stopAreaTexture.Height / 2);

            stopAreaActiveTexture = content.Load<Texture2D>("busstops/busstoparea_active");
            stopAreaActiveTextureOrigin = new Vector2(stopAreaActiveTexture.Width / 2, stopAreaActiveTexture.Height / 2);

            waitingAreaTexture = content.Load<Texture2D>("busstops/busstoparea");
            waitingAreaTextureOrigin = new Vector2(waitingAreaTexture.Width / 2, waitingAreaTexture.Height / 2);

            signTexture = content.Load<Texture2D>("busstops/busstoparea");
            signTextureOrigin = new Vector2(signTexture.Width / 2, signTexture.Height / 2);

            for (int i = 0; i < numberOfPedestrians; i++)
            {
                pedestrianTexture[i] = content.Load<Texture2D>("pedestrians/pedestrian" + i.ToString());
                pedestrianTextureOrigin[i] = new Vector2(pedestrianTexture[i].Width / 2, pedestrianTexture[i].Height / 2);
            }
        }

        /// <summary>
        /// Main function.
        /// </summary>
        public void Draw(GameplayLogic gameplayLogic, SpriteBatch spriteBatch)
        {
            List<Object> objectsList = gameplayLogic.GetAllStopAreas();
            foreach (Object stopArea in objectsList)
                DrawStopArea(spriteBatch, stopArea);

            objectsList = gameplayLogic.GetAllPedestrians();
            foreach (Object pedestrian in objectsList)
                DrawPedestrian(spriteBatch, pedestrian);
        }

        public void DrawPedestrian(SpriteBatch spriteBatch, Object pedestrian)
        {
            Vector2 position = Helper.MapPosToScreenPos(pedestrian.pos);
            Rectangle rect = Helper.CalculateScaleRectangle(position, pedestrian.size);

            spriteBatch.Draw(pedestrianTexture[int.Parse(pedestrian.name)], rect, null, Color.White, MathHelper.ToRadians(pedestrian.rotate), pedestrianTextureOrigin[int.Parse(pedestrian.name)], SpriteEffects.None, 1);
        }

        public void DrawStopArea(SpriteBatch spriteBatch, Object stopArea)
        {
            Vector2 position = Helper.MapPosToScreenPos(stopArea.pos);
            Rectangle rect = Helper.CalculateScaleRectangle(position, stopArea.size);

            if (stopArea.name == "True")
            {
                spriteBatch.Draw(stopAreaActiveTexture, rect, null, Color.White, MathHelper.ToRadians(stopArea.rotate), stopAreaActiveTextureOrigin, SpriteEffects.None, 1);
            }
            else
            {
                spriteBatch.Draw(stopAreaTexture, rect, null, Color.White, MathHelper.ToRadians(stopArea.rotate), stopAreaTextureOrigin, SpriteEffects.None, 1);
            }
        }
    }
}
