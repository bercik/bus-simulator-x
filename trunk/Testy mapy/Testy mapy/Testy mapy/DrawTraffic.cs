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
        public void LoadContent(ContentManager content, Vector2[] vehicleSizes)
        {
            for (int i = 0; i < GameParams.numberOfCars; i++)
            {
                vehicleTexture[i] = content.Load<Texture2D>("vehicles/vehicle" + i.ToString());
                vehicleTextureOrigin[i] = new Vector2(vehicleTexture[i].Width / 2, vehicleTexture[i].Height / 2);
                vehicleTextureScale[i] = new Vector2(vehicleSizes[i].X / vehicleTexture[i].Width, vehicleSizes[i].Y / vehicleTexture[i].Height);
            }
        }

        public void AddDynamicLights(TrafficLogic trafficLogic, DrawLightmap drawLightmap, EnvironmentSimulation environmentSimulation)
        {
            List<LightObject> list = new List<LightObject>();

            list = trafficLogic.GetHeadLightsPoints(environmentSimulation);
            foreach (LightObject lightObject in list)
                drawLightmap.AddLightObject(lightObject);

            list = trafficLogic.GetTailLightsPoints(environmentSimulation);
            foreach (LightObject lightObject in list)
                drawLightmap.AddLightObject(lightObject);

            list = trafficLogic.GetStopLightsPoints();
            foreach (LightObject lightObject in list)
                drawLightmap.AddLightObject(lightObject);

            list = trafficLogic.GetIndicatorPoints();
            foreach (LightObject lightObject in list)
                drawLightmap.AddLightObject(lightObject);
        }

        /// <summary>
        /// Main function.
        /// </summary>
        public void Draw(TrafficLogic trafficLogic, SpriteBatch spriteBatch)
        {
            List<Object> vehiclesList = trafficLogic.GetAllVehicles();
            foreach (Object vehicle in vehiclesList)
                DrawVehicle(spriteBatch, vehicle);
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
    }
}
