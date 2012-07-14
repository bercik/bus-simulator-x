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
        Texture2D busTexture;

        public DrawBus()
        {

        }

        public void LoadContent(ContentManager content)
        {
            busTexture = content.Load<Texture2D>("bus");
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 mapPosition, Object bus)
        {
            Vector2 position = Helper.MapPosToScreenPos(mapPosition, bus.pos);
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)bus.size.X, (int)bus.size.Y);
            spriteBatch.Draw(busTexture, rect, null, Color.White, MathHelper.ToRadians(bus.rotate), bus.origin, SpriteEffects.None, 1);
        }
    }
}
