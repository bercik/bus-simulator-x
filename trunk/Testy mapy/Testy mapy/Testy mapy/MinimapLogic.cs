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
    class MinimapLogic
    {
        protected GraphicsDevice graphicsDevice;
        protected RenderTarget2D minimap; // minimapa

        // bus:
        Texture2D busTexture;
        Vector2 busOrigin; // srodek tekstury autobusu na minimapie
        float busScale = 3.5f; // skala autobusu na minimapie

        Texture2D busStopTexture;
        Vector2 busStopOrigin; // srodek tekstury przystanku na minimapie
        float busStopScale = 3.0f; // skala przystanku na minimapie

        public MinimapLogic(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            minimap = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
        }

        public void LoadContent(ContentManager content)
        {
            busTexture = content.Load<Texture2D>("help/bus_minimap");
            busOrigin = new Vector2(busTexture.Width / 2, busTexture.Height / 2);

            busStopTexture = content.Load<Texture2D>("help/bus_stop_minimap");
            busStopOrigin = new Vector2(busStopTexture.Width / 2, busStopTexture.Height / 2);
        }

        public void DrawMinimap(SpriteBatch spriteBatch, List<Object> junctions, Dictionary<string, Texture2D> junctionsTexture, float busDirection, Vector2 currentBusStopPosition)
        {
            // Set the device to the render target
            graphicsDevice.SetRenderTarget(minimap);

            graphicsDevice.Clear(Color.Black);

            // drawing junctions
            spriteBatch.Begin();

            Vector2 v_minimapScale = new Vector2(1 / GameParams.minimapScale, 1 / GameParams.minimapScale);

            foreach (Object o in junctions)
            {
                Vector2 destinationPos = Helper.CalculateScalePosition(o.pos, GameParams.minimapScale);

                if (junctionsTexture.ContainsKey(o.name))
                {
                    spriteBatch.Draw(junctionsTexture[o.name], destinationPos, null, Color.White, MathHelper.ToRadians(o.rotate),
                            o.original_origin, v_minimapScale * o.scale, o.spriteEffects, 1.0f);
                }
            }

            Vector2 busPos = Helper.MapPosToScreenPos(Helper.busPos);
            busPos = Helper.CalculateScalePoint(busPos, GameParams.minimapScale);
            spriteBatch.Draw(busTexture, busPos, null, Color.White, MathHelper.ToRadians(busDirection), busOrigin, busScale * v_minimapScale, SpriteEffects.None, 1.0f);

            Vector2 busStopPos = Helper.MapPosToScreenPos(currentBusStopPosition);
            busStopPos = Helper.CalculateScalePoint(busStopPos, GameParams.minimapScale);
            spriteBatch.Draw(busStopTexture, busStopPos, null, Color.White, 0.0f, busStopOrigin, busStopScale * v_minimapScale, SpriteEffects.None, 1.0f);

            spriteBatch.End();

            // Reset the device to the back buffer
            graphicsDevice.SetRenderTarget(null);
        }

        public Texture2D GetMinimapTexture()
        {
            return (Texture2D)minimap;
        }
    }
}
