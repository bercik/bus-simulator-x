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
        Texture2D vehicleTexture, vehicleTexture0, vehicleTexture1, vehicleTexture2, point;

        public DrawTraffic()
        {

        }

        public void LoadContent(ContentManager content)
        {
            vehicleTexture0 = content.Load<Texture2D>("vehicle0");
            vehicleTexture1 = content.Load<Texture2D>("vehicle1");
            vehicleTexture2 = content.Load<Texture2D>("vehicle2");
            point = content.Load<Texture2D>("point");
        }

        public void Draw(SpriteBatch spriteBatch, Object vehicle)
        {
            Vector2 position = Helper.MapPosToScreenPos(vehicle.pos);
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)vehicle.size.X, (int)vehicle.size.Y);

            if (vehicle.name == "0")
                vehicleTexture = vehicleTexture0;

            if (vehicle.name == "1")
                vehicleTexture = vehicleTexture1;

            if (vehicle.name == "2")
                vehicleTexture = vehicleTexture2;

            spriteBatch.Draw(vehicleTexture, rect, null, Color.White, MathHelper.ToRadians(vehicle.rotate), vehicle.origin, SpriteEffects.None, 1);


        }
    }
}
