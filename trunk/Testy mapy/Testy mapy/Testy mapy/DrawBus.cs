using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
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
        Texture2D busTexture, tailLightTexture;

        public DrawBus()
        {

        }

        public void LoadContent(ContentManager content)
        {
            busTexture = content.Load<Texture2D>("bus");
            tailLightTexture = content.Load<Texture2D>("vehicle_taillight");
        }

        public void Draw(BusLogic busLogic, SpriteBatch spriteBatch)
        {
            Object bus = new Object("bus", busLogic.GetBusPosition(), busLogic.GetSize(), busLogic.GetDirection());
            DrawTheBus(spriteBatch, bus);

            List<Object> tailLights = busLogic.GetTailLightsPoints();

            foreach (Object tailLight in tailLights)
                DrawTailLight(spriteBatch, tailLight);
        }

        private void DrawTheBus(SpriteBatch spriteBatch, Object bus)
        {
            Vector2 position = Helper.MapPosToScreenPos(bus.pos);
            Vector2 size = bus.size;
            //Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)bus.size.X, (int)bus.size.Y);

            Rectangle rect = Helper.CalculateScaleRectangle(position, size);

            spriteBatch.Draw(busTexture, rect, null, Color.White, MathHelper.ToRadians(bus.rotate), bus.origin, SpriteEffects.None, 1);
        }

        public void DrawTailLight(SpriteBatch spriteBatch, Object tailLight)
        {
            Vector2 position = Helper.MapPosToScreenPos(tailLight.pos);
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)tailLight.size.X, (int)tailLight.size.Y);

            spriteBatch.Draw(tailLightTexture, rect, null, Color.White, MathHelper.ToRadians(tailLight.rotate), tailLight.origin, SpriteEffects.None, 1);
        }
    }
}