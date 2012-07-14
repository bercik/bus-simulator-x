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
        DrawBus drawBus;
        BusLogic busLogic;

        bool left, right, brake, accelerate, up, down, prevup, prevdown; //tymczasowe zmienne sluzace do sterowana autobusem

        int a_fps = 0;
        int fps = 60;
        double time = 0.0f;
        Vector2 pos = new Vector2(1000, 600); //poczatkowa pozycja

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.IsFullScreen = true;
            //graphics.PreferredBackBufferHeight = 768;
            //graphics.PreferredBackBufferWidth = 1024;

            mapa = new DrawMap(this);
            drawBus = new DrawBus();
            busLogic = new BusLogic(pos.X, pos.Y, 0, 0, 50, 100); //stworz bus logic

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
            // ustawianie wielkoœci ekranu w klasie Helper
            Helper.SetScreenSize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            drawBus.LoadContent(this.Content);

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

            if (keybState.IsKeyDown(Keys.Down)) brake = true; else brake = false; //powinien hamowac?

            if (keybState.IsKeyDown(Keys.Up)) accelerate = true; else accelerate = false; //powinien przyspieszac?

            if (keybState.IsKeyDown(Keys.Right)) right = true; else right = false; //skrecamy w prawo?

            if (keybState.IsKeyDown(Keys.Left)) left = true; else left = false; //skrecamy w lewo?

            if (keybState.IsKeyDown(Keys.Space)) busLogic.position = pos; //przywroc

            if (keybState.IsKeyDown(Keys.Escape)) Exit(); //wyjdz

            /*ZMIANY BIEGOW byc moze zostanie przeniesione do bus logic zeby nie smiecic, to wszystko zapobiega zmienieniu biegu z kazdym tickiem
             ma sie zmieniac tylko przy nacisnieciu*/
            if (keybState.IsKeyDown(Keys.A) && prevup)
            {
                up = true;
                prevup = false;
            }
            else
                up = false;

            if (keybState.IsKeyUp(Keys.A))
                prevup = true;

            if (keybState.IsKeyDown(Keys.Z) && prevdown)
            {
                down = true;
                prevdown = false;
            }
            else
                down = false;

            if (keybState.IsKeyUp(Keys.Z))
                prevdown = true;
            /*ZMIANY BIEGOW END*/
            
            if (keybState.IsKeyDown(Keys.L))
                mapa.LoadMap("test.mp");

            busLogic.Update(accelerate, brake, left, right, up, down, gameTime.ElapsedGameTime);

            mapa.SetPosition(busLogic.position); // bedzie busLogic.GetBusPosition() ale obecnie i tak mapa nie dziala

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
            spriteBatch.DrawString(font, "X: " + busLogic.position.X, new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(font, "Y: " + busLogic.position.Y, new Vector2(0, 30), Color.White);
            spriteBatch.DrawString(font, "Time: " + (float)gameTime.ElapsedGameTime.Milliseconds / 1000, new Vector2(0, 90), Color.White);
            spriteBatch.DrawString(font, "Speed: " + busLogic.GetCurrentSpeed(), new Vector2(0, 120), Color.White);
            spriteBatch.DrawString(font, "Acc: " + busLogic.GetCurrentAcceleration(), new Vector2(0, 150), Color.White);
            spriteBatch.DrawString(font, "Gear: " + busLogic.GetCurrentGear(), new Vector2(0, 180), Color.White);

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
