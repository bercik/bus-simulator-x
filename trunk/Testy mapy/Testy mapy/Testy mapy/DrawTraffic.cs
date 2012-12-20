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
        const int numberOfCars = 12;
        Texture2D indicatorTexture, tailLightTexture;
        Vector2 indicatorTextureOrigin, tailLightTextureOrigin;
        Texture2D[] vehicleTexture = new Texture2D[numberOfCars];
        Vector2[] vehicleTextureOrigin = new Vector2[numberOfCars];

        // Constructor.
        public DrawTraffic()
        {
        }

        /// <summary>
        /// Load sprites.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            for (int i=0; i<numberOfCars; i++)
            {
                vehicleTexture[i] = content.Load<Texture2D>("vehicles/vehicle" + i.ToString());
                vehicleTextureOrigin[i] = new Vector2(vehicleTexture[i].Width / 2, vehicleTexture[i].Height / 2);
            }

            indicatorTexture = content.Load<Texture2D>("vehicles/vehicle_indicator");
            indicatorTextureOrigin = new Vector2(indicatorTexture.Width / 2, indicatorTexture.Height / 2);
            tailLightTexture = content.Load<Texture2D>("vehicles/vehicle_taillight");
            tailLightTextureOrigin = new Vector2(tailLightTexture.Width / 2, tailLightTexture.Height / 2);
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

            spriteBatch.Draw(vehicleTexture[Int32.Parse(vehicle.name)], rect, null, Color.White, MathHelper.ToRadians(vehicle.rotate), vehicleTextureOrigin[Int32.Parse(vehicle.name)], SpriteEffects.None, 1);
        }

        /// <summary>
        /// Draw indicator.
        /// </summary>
        public void DrawIndicator(SpriteBatch spriteBatch, Object indicator)
        {
            Vector2 position = Helper.MapPosToScreenPos(indicator.pos);
            Rectangle rect = Helper.CalculateScaleRectangle(position, indicator.size);

            spriteBatch.Draw(indicatorTexture, rect, null, Color.White, MathHelper.ToRadians(indicator.rotate), indicatorTextureOrigin, SpriteEffects.None, 1);
        }

        /// <summary>
        /// Draw tail light.
        /// </summary>
        public void DrawTailLight(SpriteBatch spriteBatch, Object tailLight)
        {
            Vector2 position = Helper.MapPosToScreenPos(tailLight.pos);
            Rectangle rect = Helper.CalculateScaleRectangle(position, tailLight.size);

            spriteBatch.Draw(tailLightTexture, rect, null, Color.White, MathHelper.ToRadians(tailLight.rotate), tailLightTextureOrigin, SpriteEffects.None, 1);
        }
    }
}
