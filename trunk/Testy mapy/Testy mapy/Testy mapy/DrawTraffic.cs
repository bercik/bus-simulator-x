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
        Texture2D indicatorTexture, tailLightTexture;
        Vector2 indicatorTextureOrigin, indicatorTextureScale, tailLightTextureOrigin, tailLightTextureScale;
        Texture2D[] vehicleTexture = new Texture2D[GameParams.numberOfCars];
        Vector2[] vehicleTextureOrigin = new Vector2[GameParams.numberOfCars];
        Vector2[] vehicleTextureScale = new Vector2[GameParams.numberOfCars];

        // Constructor.
        public DrawTraffic()
        {
        }

        /// <summary>
        /// Load sprites.
        /// </summary>
        public void LoadContent(ContentManager content, Vector2[] vehicleSizes, Vector2 indicatorSize, Vector2 tailLightSize)
        {
            for (int i = 0; i < GameParams.numberOfCars; i++)
            {
                vehicleTexture[i] = content.Load<Texture2D>("vehicles/vehicle" + i.ToString());
                vehicleTextureOrigin[i] = new Vector2(vehicleTexture[i].Width / 2, vehicleTexture[i].Height / 2);
                vehicleTextureScale[i] = new Vector2(vehicleSizes[i].X / vehicleTexture[i].Width, vehicleSizes[i].Y / vehicleTexture[i].Height);
            }

            indicatorTexture = content.Load<Texture2D>("vehicles/vehicle_indicator");
            indicatorTextureOrigin = new Vector2(indicatorTexture.Width / 2, indicatorTexture.Height / 2);
            indicatorTextureScale = new Vector2(indicatorSize.X / indicatorTexture.Width, indicatorSize.Y / indicatorTexture.Height);

            tailLightTexture = content.Load<Texture2D>("vehicles/vehicle_taillight");
            tailLightTextureOrigin = new Vector2(tailLightTexture.Width / 2, tailLightTexture.Height / 2);
            tailLightTextureScale = new Vector2(tailLightSize.X / tailLightTexture.Width, tailLightSize.Y / tailLightTexture.Height);
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
            position = Helper.CalculateScalePosition(position);

            spriteBatch.Draw(vehicleTexture[Int32.Parse(vehicle.name)], position, null, Color.White, MathHelper.ToRadians(vehicle.rotate), vehicleTextureOrigin[Int32.Parse(vehicle.name)], Helper.GetVectorScale() * vehicleTextureScale[Int32.Parse(vehicle.name)], SpriteEffects.None, 1);
        }

        /// <summary>
        /// Draw indicator.
        /// </summary>
        public void DrawIndicator(SpriteBatch spriteBatch, Object indicator)
        {
            Vector2 position = Helper.MapPosToScreenPos(indicator.pos);
            position = Helper.CalculateScalePosition(position);

            spriteBatch.Draw(indicatorTexture, position, null, Color.White, MathHelper.ToRadians(indicator.rotate), indicatorTextureOrigin, Helper.GetVectorScale() * indicatorTextureScale, SpriteEffects.None, 1);
        }

        /// <summary>
        /// Draw tail light.
        /// </summary>
        public void DrawTailLight(SpriteBatch spriteBatch, Object tailLight)
        {
            Vector2 position = Helper.MapPosToScreenPos(tailLight.pos);
            position = Helper.CalculateScalePosition(position);

            spriteBatch.Draw(tailLightTexture, position, null, Color.White, MathHelper.ToRadians(tailLight.rotate), tailLightTextureOrigin, Helper.GetVectorScale() * tailLightTextureScale, SpriteEffects.None, 1);
        }
    }
}
