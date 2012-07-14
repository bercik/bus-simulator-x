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

        public void DrawPoints(List<Vector2> points, Vector2 screenPos, Vector2 screenSize)
        {
            spriteBatch.Begin();

            foreach (Vector2 point in points)
            {
                Point p_point = new Point();
                p_point.X = (int)(point.X - (screenPos.X - screenSize.X / 2));
                p_point.Y = (int)(point.Y - (screenPos.Y - screenSize.Y / 2));

                Rectangle rect = new Rectangle(p_point.X, p_point.Y, 4, 4);
                Color color = Color.White;
                Primitives2D.FillRectangle(spriteBatch, rect, color);
            }

            spriteBatch.End();
        }
    }
}
