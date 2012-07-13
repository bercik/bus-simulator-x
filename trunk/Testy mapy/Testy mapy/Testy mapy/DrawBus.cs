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
        SpriteBatch spriteBatch;

        public DrawBus()
        {

        }

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public void DrawPoints(List<Vector2> points)
        {
            spriteBatch.Begin();

            foreach (Vector2 point in points)
            {
                Rectangle rect = new Rectangle((int)point.X - 2, (int)point.Y - 2, 4, 4);
                Primitives2D.FillRectangle(spriteBatch, rect, Color.White);
            }

            spriteBatch.End();
        }
    }
}
