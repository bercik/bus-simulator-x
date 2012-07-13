using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Testy_mapy
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        DrawMap mapa;

        int a_fps = 0;
        int fps = 60;
        double time = 0.0f;
        Vector2 pos = new Vector2(400, 240);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.IsFullScreen = true;
            //graphics.PreferredBackBufferHeight = 768;
            //graphics.PreferredBackBufferWidth = 1024;

            mapa = new DrawMap(this);

            Components.Add(mapa);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("font1");

            mapa.LoadMap("test.mp");
            mapa.LoadTrack("test.tc");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            // obsluga klawiatury
            KeyboardState keybState = Keyboard.GetState();
            if (keybState.IsKeyDown(Keys.Down))
                pos.Y += 5.0f;
            if (keybState.IsKeyDown(Keys.Up))
                pos.Y -= 5.0f;
            if (keybState.IsKeyDown(Keys.Right))
                pos.X += 5.0f;
            if (keybState.IsKeyDown(Keys.Left))
                pos.X -= 5.0f;
            if (keybState.IsKeyDown(Keys.Space))
                pos = new Vector2(400, 240);

            if (keybState.IsKeyDown(Keys.Escape))
                Exit();
            
            if (keybState.IsKeyDown(Keys.L))
                mapa.LoadMap("test.mp");

            pos += new Vector2(0, 0);

            mapa.SetPosition(pos);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            base.Draw(gameTime);

            // TODO: Add your drawing code here

            // zmienne pomocnicze rysowane na ekranie:
            spriteBatch.DrawString(font, "X: " + pos.X, new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(font, "Y: " + pos.Y, new Vector2(0, 30), Color.White);

            // licznik FPS
            time += gameTime.ElapsedGameTime.TotalMilliseconds;
            ++a_fps;

            spriteBatch.DrawString(font, "FPS: " + fps, new Vector2(0, 60), Color.White);

            if (time > 1000)
            {
                time -= 1000;
                fps = a_fps;
                a_fps = 0;
            }

            spriteBatch.End();
        }
    }
}
