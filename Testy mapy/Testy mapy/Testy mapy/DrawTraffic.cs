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
    class DrawTraffic
    {
        Texture2D vehicleTexture, vehicleTexture0, vehicleTexture1, vehicleTexture2, vehicleTexture3, point, indicatorTexture, tailLightTexture;

        // Constructor.
        public DrawTraffic()
        {
        }

        /// <summary>
        /// Load sprites.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            vehicleTexture0 = content.Load<Texture2D>("vehicle0");
            vehicleTexture1 = content.Load<Texture2D>("vehicle1");
            vehicleTexture2 = content.Load<Texture2D>("vehicle2");
            vehicleTexture3 = content.Load<Texture2D>("vehicle3");
            indicatorTexture = content.Load<Texture2D>("vehicle_indicator");
            tailLightTexture = content.Load<Texture2D>("vehicle_taillight");
            point = content.Load<Texture2D>("point");
        }

        /// <summary>
        /// Main function.
        /// </summary>
        public void Draw(TrafficLogic trafficLogic, SpriteBatch spriteBatch)
        {
            List<Object> vehiclesList = trafficLogic.GetAllVehicles();
            foreach (Object vehicle in vehiclesList)
                DrawVehicle(spriteBatch, vehicle);

            List<Object> indicatorsList = trafficLogic.GetIndicatorPoints();
            foreach (Object indicator in indicatorsList)
                DrawIndicator(spriteBatch, indicator);

            List<Object> tailLightsList = trafficLogic.GetTailLightsPoints();
            foreach (Object tailLight in tailLightsList)
                DrawTailLight(spriteBatch, tailLight);
        }

        /// <summary>
        /// Draw vehicle.
        /// </summary>
        public void DrawVehicle(SpriteBatch spriteBatch, Object vehicle)
        {
            Vector2 position = Helper.MapPosToScreenPos(vehicle.pos);
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)vehicle.size.X, (int)vehicle.size.Y);

            if (vehicle.name == "0")
                vehicleTexture = vehicleTexture0;

            if (vehicle.name == "1")
                vehicleTexture = vehicleTexture1;

            if (vehicle.name == "2")
                vehicleTexture = vehicleTexture2;

            if (vehicle.name == "3")
                vehicleTexture = vehicleTexture3;

            spriteBatch.Draw(vehicleTexture, rect, null, Color.White, MathHelper.ToRadians(vehicle.rotate), vehicle.origin, SpriteEffects.None, 1);
        }

        /// <summary>
        /// Draw indicator.
        /// </summary>
        public void DrawIndicator(SpriteBatch spriteBatch, Object indicator)
        {
            Vector2 position = Helper.MapPosToScreenPos(indicator.pos);
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)indicator.size.X, (int)indicator.size.Y);

            spriteBatch.Draw(indicatorTexture, rect, null, Color.White, MathHelper.ToRadians(indicator.rotate), indicator.origin, SpriteEffects.None, 1);
        }

        /// <summary>
        /// Draw tail light.
        /// </summary>
        public void DrawTailLight(SpriteBatch spriteBatch, Object tailLight)
        {
            Vector2 position = Helper.MapPosToScreenPos(tailLight.pos);
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)tailLight.size.X, (int)tailLight.size.Y);

            spriteBatch.Draw(tailLightTexture, rect, null, Color.White, MathHelper.ToRadians(tailLight.rotate), tailLight.origin, SpriteEffects.None, 1);
        }
    }
}
