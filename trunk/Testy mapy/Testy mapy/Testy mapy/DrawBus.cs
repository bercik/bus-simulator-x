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
    class DrawBus
    {
        Texture2D busTexture;
        Vector2 busTextureOrigin, busTextureScale;

        public DrawBus()
        {
        }

        public void AddDynamicLights(BusLogic busLogic, DrawLightmap drawLightmap)
        {
            List<LightObject> list = new List<LightObject>();
 
            list = busLogic.GetReversingLightsPoints();
            foreach (LightObject lightObject in list)
                drawLightmap.AddLightObject(lightObject);

            list = busLogic.GetStopLightsPoints();
            foreach (LightObject lightObject in list)
                drawLightmap.AddLightObject(lightObject);

            list = busLogic.GetTailLightsPoints();
            foreach (LightObject lightObject in list)
                drawLightmap.AddLightObject(lightObject);

            list = busLogic.GetHeadLightsPoints();
            foreach (LightObject lightObject in list)
                drawLightmap.AddLightObject(lightObject);           
        }

        public void LoadContent(ContentManager content, Vector2 busSize)
        {
            busTexture = content.Load<Texture2D>("vehicles/bus");
            busTextureOrigin = new Vector2(busTexture.Width / 2, busTexture.Height / 2);
            busTextureScale = new Vector2(busSize.X / busTexture.Width, busSize.Y / busTexture.Height);
        }

        public void Draw(BusLogic busLogic, SpriteBatch spriteBatch)
        {
            Object bus = new Object("bus", busLogic.GetBusPosition(), busLogic.GetSize(), busLogic.GetDirection());
            DrawTheBus(spriteBatch, bus);
        }

        private void DrawTheBus(SpriteBatch spriteBatch, Object bus)
        {
            Vector2 position = Helper.MapPosToScreenPos(bus.pos);
            position = Helper.CalculateScalePosition(position);            

            spriteBatch.Draw(busTexture, position, null, Color.White, MathHelper.ToRadians(bus.rotate), busTextureOrigin, Helper.GetVectorScale() * busTextureScale, SpriteEffects.None, 1);
        }
    }
}