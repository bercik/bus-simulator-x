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
        CollisionsLogic collisionsLogic;
        GameplayLogic gameplaylogic;

        bool left, right, brake, accelerate, up, down, prevup, prevdown; // Zmienne s³u¿¹ce do sterowana autobusem.

        // licznik FPS:
        int a_fps = 0;
        int fps = 60;
        double time = 0.0f;

        // !!! pomocnicze zmienne do EW. USUNIECIA !!!
        Vector2 startPos = new Vector2(200, 1200); //poczatkowa pozycja
        Texture2D point; // tekstura punktu
        
        float scrollingSpeed = 10.0f; // predkosc przewijania mapy

        // BUS MODE:
        bool busMode = true; // czy jezdzimy autobusem czy przesuwamy mape
        bool b_release = true; // czy zwolniono klawisz B

        // MAP PREVIEW:
        bool m_release = true; // czy zwolniono klawisz M
        bool previewMode = false; // czy aktualnie jesteœmy w trybie podgl¹du mapy
        float previewScale = 7.0f; // skala przy podgl¹dzie
        readonly float maxPreviewScale = 10.0f; // maksymalna skala podgl¹du mapy
        readonly float minPreviewScale = 4.0f; // minimalna skala podgl¹du mapy

        // !!! metody pomocnicze do EW. USUNIECIA !!!
        public void DrawPoint(Vector2 pos)
        {
            Vector2 newPos = Helper.CalculateScalePoint(pos);
            Rectangle rect = new Rectangle((int)Math.Round(newPos.X), (int)Math.Round(newPos.Y), 4, 4);

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
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.IsFullScreen = true;
            //graphics.PreferredBackBufferHeight = 768;
            //graphics.PreferredBackBufferWidth = 1024;

            // inicjujemy klasê Helper
            Helper.mapPos = startPos;

            drawMap = new DrawMap();
            drawBus = new DrawBus();
            
            busLogic = new BusLogic(startPos.X, startPos.Y, 0, 0, new Vector2(50, 150));
            trafficLogic = new TrafficLogic();
            drawTraffic = new DrawTraffic();
            collisionsLogic = new CollisionsLogic();
            gameplaylogic = new GameplayLogic();
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
            Helper.SetScale(2.0f);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            drawBus.LoadContent(this.Content);
            drawTraffic.LoadContent(this.Content);
            drawMap.LoadContent(this.Content);

            font = Content.Load<SpriteFont>("fonts/font1");
            point = Content.Load<Texture2D>("help/point");

            Vector2 busPosition = new Vector2(0, 0);
            float busRotation = 0.0f;
            drawMap.LoadMap("test.mp", ref busPosition, ref busRotation);
            busLogic.SetDirection(busRotation);
            busLogic.SetPosition(busPosition);
            drawMap.CreateGrass(busLogic.GetBusPosition());

            gameplaylogic.LoadMapFile("busstops_test.mp");

            startPos = busLogic.GetRealPosition(); // !!! do EW. USUNIECIA !!!
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
            // Obs³uga klawiatury.
            KeyboardState keybState = Keyboard.GetState();

            if (!previewMode) // je¿eli nie jesteœmy obecnie w podgl¹dzie mapy to wszystko dzia³a normalnie
            {
                if (busMode)
                {
                    if (keybState.IsKeyDown(Keys.Down)) brake = true; else brake = false;         // Powinien hamowac?

                    if (keybState.IsKeyDown(Keys.Up)) accelerate = true; else accelerate = false; // Powinien przyspieszac?

                    if (keybState.IsKeyDown(Keys.Right)) right = true; else right = false;        // Skrêcamy w prawo?

                    if (keybState.IsKeyDown(Keys.Left)) left = true; else left = false;           // Skrêcamy w lewo?

                    if (keybState.IsKeyDown(Keys.Space)) busLogic.SetPosition(startPos);          // Przywróæ na pozycje pocz¹tkow¹.

                    /* <ZMIANY BIEGOW>
                      To wszystko zapobiega zmienieniu biegu z ka¿dym tickiem, w koñcu chcemy ¿eby zosta³ zmieniony tylko przy wciœniêciu przycisku
                    */
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
                    /* </ZMIANY BIEGOW> */

                    // Logika autobusu.
                    busLogic.Update(accelerate, brake, left, right, up, down, gameTime.ElapsedGameTime);

                    // Ustawienia mapy i klasy pomocniczej.
                    drawMap.SetPosition(busLogic.GetBusPosition());
                    Helper.mapPos = busLogic.GetBusPosition();
                    Helper.busPos = busLogic.GetBusPosition();

                    // Kolizje.
                    collisionsLogic.HandleCollisions(trafficLogic, busLogic);
                }
                else
                {
                    UpdatePos(keybState, gameTime);

                    drawMap.SetPosition(Helper.mapPos);
                }

                // obs³uga BUS MODE i MAP PREVIEW
                if (keybState.IsKeyDown(Keys.B) && b_release)
                {
                    if (!busMode)
                        drawMap.CreateGrass(Helper.mapPos);

                    busMode = !busMode;
                    b_release = false;
                }
                if (keybState.IsKeyUp(Keys.B))
                    b_release = true;

                if (keybState.IsKeyDown(Keys.M) && m_release) // w³¹cza/wy³¹cza podgl¹d mapy
                {
                    previewMode = !previewMode;
                    m_release = false;
                }
                if (keybState.IsKeyUp(Keys.M))
                    m_release = true;

                // obs³uga skali mapy:
                if (keybState.IsKeyDown(Keys.PageUp))
                    Helper.SetScale(Helper.GetScale() + 0.01f);

                if (keybState.IsKeyDown(Keys.PageDown))
                    Helper.SetScale(Helper.GetScale() - 0.01f);

                if (keybState.IsKeyDown(Keys.Delete))
                    Helper.SetScale(1.0f);

                trafficLogic.Update(drawMap, busLogic, gameTime.ElapsedGameTime);
                drawMap.Update(gameTime, busLogic.GetCollisionPoints(), ref trafficLogic);
            }
            else
            {
                UpdatePos(keybState, gameTime);

                // obs³uga MAP PREVIEW:
                if (keybState.IsKeyDown(Keys.M) && m_release) // w³¹cza/wy³¹cza podgl¹d mapy
                {
                    previewMode = !previewMode;
                    m_release = false;
                }
                if (keybState.IsKeyUp(Keys.M))
                    m_release = true;

                // obs³uga skali podgl¹du mapy:
                if (keybState.IsKeyDown(Keys.PageUp))
                    previewScale += 0.05f;

                if (keybState.IsKeyDown(Keys.PageDown))
                    previewScale -= 0.05f;

                if (keybState.IsKeyDown(Keys.Delete))
                    previewScale = 7.0f;

                if (previewScale < minPreviewScale)
                    previewScale = minPreviewScale;
                else if (previewScale > maxPreviewScale)
                    previewScale = maxPreviewScale;
            }

            if (keybState.IsKeyDown(Keys.Escape)) // wy³¹cza grê
                Exit();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);

            spriteBatch.Begin();

            base.Draw(gameTime);

            // TODO: Add your drawing code here

            if (previewMode) // je¿eli rysujemy podgl¹d mapy
            {
                drawMap.DrawPreview(spriteBatch, previewScale);

                spriteBatch.DrawString(font, "Preview scale: " + Math.Round(previewScale, 3), new Vector2(0, 0), Color.White);
            }
            else
            {
                // track, objects under bus, pedestrians
                drawMap.DrawTrack(spriteBatch, gameTime);
                drawMap.DrawObjectsUnderBus(spriteBatch, gameTime);
                drawMap.DrawPedestrians(spriteBatch, gameTime);

                // Traffic.
                drawTraffic.Draw(trafficLogic, spriteBatch);

                // Bus.
                drawBus.Draw(busLogic, spriteBatch);

                // Something very nice and useful - most likely map object wchich are supposed to be above the bus.
                drawMap.DrawObjectsOnBus(spriteBatch, gameTime);

                // traffic lights
                drawMap.DrawTrafficLights(spriteBatch, gameTime);

                // zmienne pomocnicze rysowane na ekranie:
                DrawPoint(Helper.MapPosToScreenPos(Helper.mapPos));

                DrawPoints(trafficLogic.GetPointsToDraw());

                DrawPoints(busLogic.GetPointsToDraw());

                DrawPoints(drawMap.GetCollisionPointsToDraw());

                // rysujemy nazwe zmienionego obszaru (jezeli obszar sie zmienil)
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied); // musimy zmieniæ tryb spriteBatch
                drawMap.DrawAreasChange(spriteBatch, gameTime);
                spriteBatch.End();
                spriteBatch.Begin();

                spriteBatch.DrawString(font, "X: " + Helper.mapPos.X, new Vector2(0, 0), Color.White);
                spriteBatch.DrawString(font, "Y: " + Helper.mapPos.Y, new Vector2(0, 30), Color.White);
                spriteBatch.DrawString(font, "Time: " + (float)gameTime.ElapsedGameTime.Milliseconds / 1000, new Vector2(0, 90), Color.White);
                spriteBatch.DrawString(font, "Acc: " + Math.Round(busLogic.GetCurrentAcceleration(), 2), new Vector2(0, 120), Color.White);
                spriteBatch.DrawString(font, "Side acc: " + Math.Round(busLogic.GetSideAcceleration(), 2), new Vector2(0, 150), Color.White);

                spriteBatch.DrawString(font, "Scale: " + Helper.GetScale(), new Vector2(0, 180), Color.White);

                spriteBatch.DrawString(font, "Speed: " + Math.Round(busLogic.GetCurrentSpeed(), 0), new Vector2(0, 400), Color.White);
                spriteBatch.DrawString(font, "Gear: " + busLogic.GetCurrentGear(), new Vector2(0, 430), Color.White);
            }

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
