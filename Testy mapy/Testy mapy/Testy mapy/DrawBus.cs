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
        Texture2D busTexture, tailLightTexture;
        Vector2 busTextureOrigin, busTextureScale, tailLightTextureOrigin, tailLightTextureScale;

        public DrawBus()
        {

        }

        public void LoadContent(ContentManager content, Vector2 busSize, Vector2 tailLightSize)
        {
            busTexture = content.Load<Texture2D>("vehicles/bus");
            busTextureOrigin = new Vector2(busTexture.Width / 2, busTexture.Height / 2);
            busTextureScale = new Vector2(busSize.X / busTexture.Width, busSize.Y / busTexture.Height);

            tailLightTexture = content.Load<Texture2D>("vehicles/vehicle_taillight");
            tailLightTextureOrigin = new Vector2(tailLightTexture.Width / 2, tailLightTexture.Height / 2);
            tailLightTextureScale = new Vector2(tailLightSize.X / tailLightTexture.Width, tailLightSize.Y / tailLightTexture.Height);
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
            position = Helper.CalculateScalePosition(position);            

            spriteBatch.Draw(busTexture, position, null, Color.White, MathHelper.ToRadians(bus.rotate), busTextureOrigin, Helper.GetVectorScale() * busTextureScale, SpriteEffects.None, 1);
        }

        public void DrawTailLight(SpriteBatch spriteBatch, Object tailLight)
        {
            Vector2 position = Helper.MapPosToScreenPos(tailLight.pos);
            position = Helper.CalculateScalePosition(position);

            spriteBatch.Draw(tailLightTexture, position, null, Color.White, MathHelper.ToRadians(tailLight.rotate), tailLightTextureOrigin, Helper.GetVectorScale() * tailLightTextureScale, SpriteEffects.None, 1);
        }
    }
}