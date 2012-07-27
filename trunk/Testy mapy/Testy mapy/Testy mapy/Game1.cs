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
        DrawMap drawMap;
        DrawBus drawBus;
        BusLogic busLogic;
        TrafficLogic trafficLogic;
        DrawTraffic drawTraffic;

        bool left, right, brake, accelerate, up, down, prevup, prevdown; //tymczasowe zmienne sluzace do sterowana autobusem

        // licznik FPS:
        int a_fps = 0;
        int fps = 60;
        double time = 0.0f;

        // !!! pomocnicze zmienne do EW. USUNIECIA !!!
        Vector2 startPos = new Vector2(200, 1200); //poczatkowa pozycja
        Texture2D point;
        bool busMode = false; // czy jezdzimy autobusem czy przesuwamy mape
        float scrollingSpeed = 15.0f;
        bool b_release = true;

        // !!! metody pomocnicze do EW. USUNIECIA !!!
        public void DrawPoint(Vector2 pos)
        {
            Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, 4, 4);
            spriteBatch.Draw(point, rect, null, Color.White, 0, new Vector2(2, 2), SpriteEffects.None, 1);
        }

        public void DrawPoints(Vector2[] points)
        {
            foreach (Vector2 point in points)
                DrawPoint(point);
        }

        public void UpdatePos(KeyboardState keybState, GameTime gameTime)
        {
            if (keybState.IsKeyDown(Keys.Up))
                Helper.mapPos += new Vector2(0, -scrollingSpeed);
            if (keybState.IsKeyDown(Keys.Down))
                Helper.mapPos += new Vector2(0, scrollingSpeed);
            if (keybState.IsKeyDown(Keys.Left))
                Helper.mapPos += new Vector2(-scrollingSpeed, 0);
            if (keybState.IsKeyDown(Keys.Right))
                Helper.mapPos += new Vector2(scrollingSpeed, 0);
            if (keybState.IsKeyDown(Keys.B) && b_release)
            {
                busMode = true;
                b_release = false;
            }
            if (keybState.IsKeyUp(Keys.B))
                b_release = true;
            if (keybState.IsKeyDown(Keys.Escape))
                Exit();
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.IsFullScreen = true;
            //graphics.PreferredBackBufferHeight = 768;
            //graphics.PreferredBackBufferWidth = 1024;

            drawMap = new DrawMap();
            drawBus = new DrawBus();
            busLogic = new BusLogic(startPos.X, startPos.Y, 0, 0, new Vector2(50, 150)); //stworz bus logic
            trafficLogic = new TrafficLogic();
            drawTraffic = new DrawTraffic();

            Helper.mapPos = startPos;
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
            drawTraffic.LoadContent(this.Content);
            drawMap.LoadContent(this.Content);

            font = Content.Load<SpriteFont>("font1");
            point = Content.Load<Texture2D>("point");

            drawMap.LoadMap("test.mp", ref busLogic.position);
            startPos = busLogic.position; // !!! do EW. USUNIECIA !!!
            Helper.mapPos = startPos; // !!! TO TEZ
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

            if (busMode)
            {
                if (keybState.IsKeyDown(Keys.Down)) brake = true; else brake = false; //powinien hamowac?

                if (keybState.IsKeyDown(Keys.Up)) accelerate = true; else accelerate = false; //powinien przyspieszac?

                if (keybState.IsKeyDown(Keys.Right)) right = true; else right = false; //skrecamy w prawo?

                if (keybState.IsKeyDown(Keys.Left)) left = true; else left = false; //skrecamy w lewo?

                if (keybState.IsKeyDown(Keys.Space)) busLogic.position = startPos; //przywroc

                if (keybState.IsKeyDown(Keys.Escape)) Exit(); //wyjdz

                if (keybState.IsKeyDown(Keys.B) && b_release)
                {
                    busMode = false; // wylacza jezdzenie autobusem
                    drawMap.CreateGrass();
                    b_release = false;
                }
                if (keybState.IsKeyUp(Keys.B))
                    b_release = true;

                /*---<ZMIANY BIEGOW>--- 
                  byc moze zostanie przeniesione do bus logic zeby nie smiecic, to wszystko zapobiega zmienieniu biegu z kazdym tickiem
                  ma sie zmieniac tylko przy nacisnieciu. NOPE. Po zastanowieniu odgórne funkcje steruj¹ce musz¹ zostaæ tutaj*/
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
                /*---</ZMIANY BIEGOW>---*/

                /*---<LOGIKA AUTOBUSU>---*/
                busLogic.Update(accelerate, brake, left, right, up, down, gameTime.ElapsedGameTime);
                Vector2[] collisionPoints = busLogic.GetCollisionPoints(busLogic.GetDesiredPosition(), busLogic.GetDesiredDirection());
                /*---</LOGIKA AUTOBUSU>---*/

                drawMap.SetPosition(busLogic.CalculateCenter(busLogic.GetDesiredPosition(), busLogic.GetDesiredDirection())); // bedzie busLogic.GetBusPosition() ale obecnie i tak mapa nie dziala
                Helper.mapPos = busLogic.GetBusPosition();

                if (!drawMap.IsCollision(collisionPoints) && !trafficLogic.IsCollision(collisionPoints, busLogic.GetBusPosition()))
                    busLogic.AcceptNewPositionAndDirection();
                else
                {
                    busLogic.Collision();
                    drawMap.SetPosition(busLogic.GetBusPosition());
                }
            }
            else
            {
                UpdatePos(keybState, gameTime);

                drawMap.SetPosition(Helper.mapPos);
            }

            trafficLogic.Update(drawMap, busLogic, gameTime.ElapsedGameTime);

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

            drawMap.DrawTrack(spriteBatch, gameTime);
            drawMap.DrawObjectsUnderBus(spriteBatch, gameTime);

            //<traffic>
            List<Object> indicatorsList = trafficLogic.GetIndicatorPoints();
            foreach (Object indicator in indicatorsList)
                drawTraffic.DrawIndicator(spriteBatch, indicator);
            
            List<Object> vehiclesList = trafficLogic.GetAllVehicles();
            foreach (Object vehicle in vehiclesList)
               drawTraffic.Draw(spriteBatch, vehicle);
            //</traffic>

            Object bus = new Object("bus", busLogic.GetBusPosition(), busLogic.GetSize(), busLogic.GetDirection());
            drawBus.Draw(spriteBatch, bus);

            drawMap.DrawObjectsOnBus(spriteBatch, gameTime);

            // zmienne pomocnicze rysowane na ekranie:
            DrawPoint(Helper.MapPosToScreenPos(Helper.mapPos));

            DrawPoints(trafficLogic.GetPointsToDraw());
            
            DrawPoints(busLogic.GetPointsToDraw());

            DrawPoints(drawMap.GetCollisionPointsToDraw());


            spriteBatch.DrawString(font, "X: " + Helper.mapPos.X, new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(font, "Y: " + Helper.mapPos.Y, new Vector2(0, 30), Color.White);
            spriteBatch.DrawString(font, "Time: " + (float)gameTime.ElapsedGameTime.Milliseconds / 1000, new Vector2(0, 90), Color.White);
            spriteBatch.DrawString(font, "Acc: " + busLogic.GetCurrentAcceleration(), new Vector2(0, 120), Color.White);
            spriteBatch.DrawString(font, "Side acc: " + busLogic.GetSideAcceleration(), new Vector2(0, 150), Color.White);

            spriteBatch.DrawString(font, "Speed: " + busLogic.GetCurrentSpeed(), new Vector2(0, 400), Color.White);
            spriteBatch.DrawString(font, "Gear: " + busLogic.GetCurrentGear(), new Vector2(0, 430), Color.White);

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
