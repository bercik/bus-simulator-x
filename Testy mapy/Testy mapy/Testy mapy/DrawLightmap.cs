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
    enum OriginType { Center, CenterDown }

    class LightTexture
    {
        public LightTexture(Texture2D texture, OriginType originType)
        {
            this.texture = texture;
            this.size = new Vector2(texture.Width, texture.Height);

            switch (originType)
            {
                case OriginType.Center:
                    origin = size / 2;
                    break;
                case OriginType.CenterDown:
                    origin = new Vector2(size.X / 2, size.Y);
                    break;
            }
        }

        public Vector2 GetScale(Vector2 newSize)
        {
            return (newSize / this.size);
        }

        public Texture2D texture { get; private set; }
        public Vector2 size { get; private set; }
        public Vector2 origin { get; private set; }
    }

    class DrawLightmap
    {
        RenderTarget2D lightmap;
        GraphicsDevice graphicsDevice;

        Dictionary<string, LightTexture> lightTextures;

        List<LightObject> lightObjects;

        public DrawLightmap(GraphicsDevice graphicsDevice)
        {
            lightObjects = new List<LightObject>();
            lightTextures = new Dictionary<string, LightTexture>();

            this.graphicsDevice = graphicsDevice;

            lightmap = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
        }

        public void LoadContent(ContentManager content)
        {
            lightTextures.Add("light", new LightTexture(content.Load<Texture2D>("light/light"), OriginType.Center));
            lightTextures.Add("spotlight", new LightTexture(content.Load<Texture2D>("light/spotlight"), OriginType.CenterDown));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            graphicsDevice.SetRenderTarget(lightmap); // ustawiamy obiekt renderowania

            graphicsDevice.Clear(Color.Black);

            // rysowanie oświetlenia na teksturę lightmapy:
            spriteBatch.Begin(); // inicjujemy sprite batch

            foreach (LightObject lightObject in lightObjects)
            {
                LightTexture lightTexture = lightTextures[lightObject.name];

                Vector2 pos = Helper.CalculateScalePosition(lightObject.pos);

                spriteBatch.Draw(lightTexture.texture, pos, null, lightObject.color, MathHelper.ToRadians(lightObject.rotate),
                        lightTexture.origin, Helper.GetVectorScale() * lightTexture.GetScale(lightObject.size), SpriteEffects.None, 1.0f);
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
