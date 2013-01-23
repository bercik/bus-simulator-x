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
    class DrawLightmap
    {
        RenderTarget2D lightmap;
        GraphicsDevice graphicsDevice;

        Dictionary<string, Texture2D> lightTextures;

        List<LightObject> lightObjects;

        public DrawLightmap(GraphicsDevice graphicsDevice)
        {
            lightObjects = new List<LightObject>();
            lightTextures = new Dictionary<string, Texture2D>();

            this.graphicsDevice = graphicsDevice;

            lightmap = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
        }

        public void LoadContent(ContentManager content)
        {
            lightTextures.Add("light", content.Load<Texture2D>("light/light"));
            lightTextures.Add("spotlight", content.Load<Texture2D>("light/spotlight"));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            graphicsDevice.SetRenderTarget(lightmap); // ustawiamy obiekt renderowania

            graphicsDevice.Clear(Color.Black);

            // rysowanie oświetlenia na teksturę lightmapy:
            spriteBatch.Begin(); // inicjujemy sprite batch

            foreach (LightObject lightObject in lightObjects)
            {
                spriteBatch.Draw(lightTextures[lightObject.name], lightObject.pos, null, lightObject.color, lightObject.rotate,
                        lightObject.original_origin, Helper.GetVectorScale(), SpriteEffects.None, 1.0f);
            }

            lightObjects.Clear();

            spriteBatch.End();  // zakańczamy sprite batch

            graphicsDevice.SetRenderTarget(null); // ustawiamy obiekt renderowania z powrotem na domyślny (ekran)
        }

        public Texture2D GetLightmapTexture()
        {
            return lightmap;
        }

        public void AddLightObject(LightObject newLightObject)
        {
            lightObjects.Add(newLightObject);
        }
    }
}
