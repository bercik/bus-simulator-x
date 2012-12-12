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
    class DrawTraffic
    {
        Texture2D point, indicatorTexture, tailLightTexture;
        Texture2D[] vehicleTexture = new Texture2D[5];

        // Constructor.
        public DrawTraffic()
        {
        }

        /// <summary>
        /// Load sprites.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            vehicleTexture[0] = content.Load<Texture2D>("vehicle0");
            vehicleTexture[1] = content.Load<Texture2D>("vehicle1");
            vehicleTexture[2] = content.Load<Texture2D>("vehicle2");
            vehicleTexture[3] = content.Load<Texture2D>("vehicle3");
            vehicleTexture[4] = content.Load<Texture2D>("vehicle4");
            indicatorTexture = content.Load<Texture2D>("vehicle_indicator");
            tailLightTexture = content.Load<Texture2D>("vehicle_taillight");
            point = content.Load<Texture2D>("help/point");
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
            Rectangle rect = Helper.CalculateScaleRectangle(position, vehicle.size);

            spriteBatch.Draw(vehicleTexture[Int32.Parse(vehicle.name)], rect, null, Color.White, MathHelper.ToRadians(vehicle.rotate), vehicle.origin, SpriteEffects.None, 1);
        }

        /// <summary>
        /// Draw indicator.
        /// </summary>
        public void DrawIndicator(SpriteBatch spriteBatch, Object indicator)
        {
            Vector2 position = Helper.MapPosToScreenPos(indicator.pos);
            Rectangle rect = Helper.CalculateScaleRectangle(position, indicator.size);

            spriteBatch.Draw(indicatorTexture, rect, null, Color.White, MathHelper.ToRadians(indicator.rotate), indicator.origin, SpriteEffects.None, 1);
        }

        /// <summary>
        /// Draw tail light.
        /// </summary>
        public void DrawTailLight(SpriteBatch spriteBatch, Object tailLight)
        {
            Vector2 position = Helper.MapPosToScreenPos(tailLight.pos);
            Rectangle rect = Helper.CalculateScaleRectangle(position, tailLight.size);

            spriteBatch.Draw(tailLightTexture, rect, null, Color.White, MathHelper.ToRadians(tailLight.rotate), tailLight.origin, SpriteEffects.None, 1);
        }
    }
}
