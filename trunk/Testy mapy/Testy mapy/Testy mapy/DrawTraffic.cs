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
        Texture2D vehicleTexture;

        public DrawTraffic()
        {

        }

        public void LoadContent(ContentManager content)
        {
            vehicleTexture = content.Load<Texture2D>("bus");
        }

        public void Draw(SpriteBatch spriteBatch, Object vehicle)
        {
            Vector2 position = Helper.MapPosToScreenPos(vehicle.pos);
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)vehicle.size.X, (int)vehicle.size.Y);
            spriteBatch.Draw(vehicleTexture, rect, null, Color.White, MathHelper.ToRadians(vehicle.rotate), vehicle.origin, SpriteEffects.None, 1);
        }
    }
}
