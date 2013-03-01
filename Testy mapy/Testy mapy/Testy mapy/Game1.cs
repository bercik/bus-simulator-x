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

        Effect lightEffect; // efekt œwiat³a
        EffectParameter globalLightColor; // globalny kolor œwiat³a
        EffectParameter lightmapTexture; // tekstura lightmapy

        RenderTarget2D scene;

        HUD hud;

        DrawMap drawMap;
        DrawBus drawBus;

        BusLogic busLogic;
        TrafficLogic trafficLogic;
        DrawTraffic drawTraffic;
        CollisionsLogic collisionsLogic;
        GameplayLogic gameplayLogic;
        DrawGameplay drawGameplay;
        ParticlesLogic particlesLogic;
        DrawParticles drawParticles;
        DrawLightmap drawLightmap;

        EnvironmentSimulation environmentSimulation;

        static class FPSCounter
        {
            public static int fps;

            static int frameRate = 0;
            static int frameCounter = 0;
            static TimeSpan elapsedTime = TimeSpan.Zero;

            public static void Update(GameTime gameTime)
            {
                elapsedTime += gameTime.ElapsedGameTime;

                if (elapsedTime > TimeSpan.FromSeconds(1))
                {
                    elapsedTime -= TimeSpan.FromSeconds(1);
                    frameRate = frameCounter;
                    frameCounter = 0;

                    fps = frameRate;
                }
            }

            public static void IncFrameCounter()
            {
                frameCounter++;
            }
        }

        // !!! pomocnicze zmienne do EW. USUNIECIA !!!
        Vector2 startPos = new Vector2(0, 0); //poczatkowa pozycja
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
            float modifier = 1.0f;

            if (keybState.IsKeyDown(Keys.LeftShift))
                modifier = 0.05f;

            if (keybState.IsKeyDown(Keys.Up))
                Helper.mapPos += new Vector2(0, -scrollingSpeed * modifier);
            if (keybState.IsKeyDown(Keys.Down))
                Helper.mapPos += new Vector2(0, scrollingSpeed * modifier);
            if (keybState.IsKeyDown(Keys.Left))
                Helper.mapPos += new Vector2(-scrollingSpeed * modifier, 0);
            if (keybState.IsKeyDown(Keys.Right))
                Helper.mapPos += new Vector2(scrollingSpeed * modifier, 0);
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.IsFullScreen = true;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1024;
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

            // inicjujemy klasê Helper
            Helper.mapPos = startPos;

            scene = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

            hud = new HUD();
            environmentSimulation = new EnvironmentSimulation(00, 19);

            drawMap = new DrawMap(graphics.GraphicsDevice);
            drawBus = new DrawBus();

            busLogic = new BusLogic(startPos.X, startPos.Y, 0, 0, new Vector2(50, 150));
            trafficLogic = new TrafficLogic();
            drawTraffic = new DrawTraffic();
            collisionsLogic = new CollisionsLogic();
            gameplayLogic = new GameplayLogic();
            drawGameplay = new DrawGameplay();
            particlesLogic = new ParticlesLogic();
            drawParticles = new DrawParticles();
            drawLightmap = new DrawLightmap(GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // ³adowanie dŸwiêków
            Sound.LoadContent(this.Content);

            // ³adujemy efekty:
            lightEffect = Content.Load<Effect>("effects/light");
            globalLightColor = lightEffect.Parameters["globalLightColor"];
            lightmapTexture = lightEffect.Parameters["lightmap"];

            // ustawianie wielkoœci ekranu w klasie Helper
            Helper.SetScreenSize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            Helper.SetScale(1.5f);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            // ³adowanie interfejsu u¿ytkownika:
            hud.LoadContent(this.Content);

            // ³adowanie klas rysuj¹cych
            drawBus.LoadContent(this.Content, busLogic.GetSize());
            drawGameplay.LoadContent(this.Content, gameplayLogic.GetStopAreaSize(), gameplayLogic.GetSignSize());

            drawTraffic.LoadContent(this.Content, trafficLogic.GetVehicleTypesSizes());
            drawParticles.LoadContent(this.Content);

            drawMap.LoadContent(this.Content);
            drawLightmap.LoadContent(this.Content);

            font = Content.Load<SpriteFont>("fonts/font1");
            point = Content.Load<Texture2D>("help/point");

            Vector2 busPosition = new Vector2(0, 0);
            float busRotation = 0.0f;
            drawMap.LoadMap("test.mp", ref busPosition, ref busRotation);
            busLogic.SetDirection(busRotation);
            busLogic.SetPosition(busPosition);
            drawMap.CreateGrass(busLogic.GetBusPosition());

            // £adowanie plików map w celu wygenerowania przystanków.
            gameplayLogic.LoadMapFile("test.mp");
            gameplayLogic.LoadOrderFile("order_test.ord");

            startPos = busLogic.GetRealPosition(); // !!! do EW. USUNIECIA !!!
            Helper.mapPos = startPos; // !!! TO TEZ

            Sound.PlayBackgroundSong();
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

            // Obs³uga klawiatury.
            KeyboardState keybState = Keyboard.GetState();

            // Obs³uga myszy.
            MouseState mouseState = Mouse.GetState();

            // Aktualizacja InputLogic. Przed t¹ akcj¹ nie powinno byæ ¿adnych innych update'ów.
            InputLogic.Update(keybState, mouseState);

            // Zaktualizuj Helper. Przed t¹ akcj¹ nie powinno byæ ¿adnych innych update'ów oprócz InputLogic.
            Helper.Update(gameTime);

            // Zaktualizuj logikê FPS counter.
            FPSCounter.Update(gameTime);

            if (!previewMode) // je¿eli nie jesteœmy obecnie w podgl¹dzie mapy to wszystko dzia³a normalnie
            {
                if (!InputLogic.pauseButton.state)
                {
                    if (busMode)
                    {
                        if (keybState.IsKeyDown(Keys.Space)) busLogic.SetPosition(startPos);          // Przywróæ na pozycje pocz¹tkow¹.

                        // Logika autobusu.
                        busLogic.Update(InputLogic.accelerateButton.state, InputLogic.brakeButton.state, InputLogic.leftTurnButton.state, InputLogic.rightTurnButton.state, InputLogic.gearUpButton.state, InputLogic.gearDownButton.state, InputLogic.doorsButton.state, InputLogic.lightsButton.state);

                        // Ustawienia mapy i klasy pomocniczej.
                        drawMap.SetPosition(busLogic.GetBusPosition(), environmentSimulation.EnableLampadaire());
                        Helper.mapPos = busLogic.GetBusPosition();
                        Helper.busPos = busLogic.GetBusPosition();

                        // Kolizje.
                        collisionsLogic.HandleCollisions(trafficLogic, busLogic, gameplayLogic);
                    }
                    else
                    {
                        UpdatePos(keybState, gameTime);

                        drawMap.SetPosition(Helper.mapPos, environmentSimulation.EnableLampadaire());
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

                    // ob³usga skali HUDU:
                    if (keybState.IsKeyDown(Keys.OemPlus))
                        hud.scale += 0.01f;

                    if (keybState.IsKeyDown(Keys.OemMinus))
                        hud.scale -= 0.01f;

                    trafficLogic.Update(drawMap, busLogic);
                    gameplayLogic.Update(busLogic, drawMap, gameTime.ElapsedGameTime);
                    drawMap.Update(gameTime, busLogic.GetCollisionPoints(), ref trafficLogic, ref environmentSimulation);
                    environmentSimulation.Update(gameTime.ElapsedGameTime, ref particlesLogic);
                    globalLightColor.SetValue(environmentSimulation.GetGlobalLightColor());
                    particlesLogic.Update(trafficLogic, busLogic);
                }
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

            base.Draw(gameTime);

            // TODO: Add your drawing code here

            if (previewMode) // je¿eli rysujemy podgl¹d mapy
            {
                spriteBatch.Begin();

                drawMap.DrawPreview(spriteBatch, previewScale, gameplayLogic.GetCurrentBusStopPosition());

                spriteBatch.DrawString(font, "Preview scale: " + Math.Round(previewScale, 3), new Vector2(0, 0), Color.White);
            }
            else
            {
                //drawing first lightmap:                

                // Latarnie uliczne:
                drawMap.AddDynamicLampadaireLights(ref drawLightmap);

                // Œwiat³a samochodów.
                drawTraffic.AddDynamicLights(trafficLogic, drawLightmap, environmentSimulation);

                // Œwiat³a autobusu.
                drawBus.AddDynamicLights(busLogic, drawLightmap);

                // Œwiat³a gameplay logic.
                drawGameplay.AddDynamicLights(gameplayLogic, drawLightmap);

                // Œwiat³a uliczne:
                drawMap.AddDynamicTrafficLights(ref drawLightmap);

                drawLightmap.Draw(spriteBatch);
                lightmapTexture.SetValue(drawLightmap.GetLightmapTexture());

                // drawing minimap:
                drawMap.DrawMinimap(spriteBatch, busLogic.GetCurrentDirection(), gameplayLogic.GetCurrentBusStopPosition());
                drawMap.DrawAreasChangeInit(GraphicsDevice, spriteBatch, gameTime);

                DrawScene(spriteBatch, gameTime);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend); // musimy zmieniæ tryb spriteBatch, ¿eby ekeft œwiat³a móg³ dzia³aæ
                // turning on the light effect
                lightEffect.CurrentTechnique = lightEffect.Techniques["Light"];
                lightEffect.CurrentTechnique.Passes[0].Apply();

                spriteBatch.Draw(scene, new Vector2(0, 0), Color.White);

                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                // zmienne pomocnicze rysowane na ekranie:
                DrawPoint(Helper.MapPosToScreenPos(Helper.mapPos));

                /*punkty*/
                if (InputLogic.debugButton.state)
                {
                    List<MyRectangle> trafficLightsForCars;
                    List<TrafficLightRectangle> trafficLightsForBus;

                    drawMap.GetRedLightRectangles(out trafficLightsForCars, out trafficLightsForBus);

                    foreach (MyRectangle myRect in trafficLightsForCars)
                    {
                        DrawPoint(Helper.MapPosToScreenPos(myRect.point1));
                        DrawPoint(Helper.MapPosToScreenPos(myRect.point2));
                        DrawPoint(Helper.MapPosToScreenPos(myRect.point3));
                        DrawPoint(Helper.MapPosToScreenPos(myRect.point4));
                    }



                    DrawPoints(trafficLogic.GetPointsToDraw());
                    DrawPoints(busLogic.GetPointsToDraw());
                    DrawPoints(gameplayLogic.GetPointsToDraw());

                    DrawPoints(drawMap.GetCollisionPointsToDraw());
                }
                /*/punkty*/

                // rysujemy nazwe zmienionego obszaru (jezeli obszar sie zmienil)
                spriteBatch.End();
                drawMap.DrawAreasChange(spriteBatch);
                spriteBatch.Begin();

                DrawHud(spriteBatch, gameTime.ElapsedGameTime);

                if (InputLogic.debugButton.state)
                {
                    spriteBatch.DrawString(font, "FPS: " + FPSCounter.fps, new Vector2(0, 60), Color.White);

                    spriteBatch.DrawString(font, "X: " + Helper.mapPos.X, new Vector2(0, 90), Color.White);
                    spriteBatch.DrawString(font, "Y: " + Helper.mapPos.Y, new Vector2(0, 120), Color.White);
                    spriteBatch.DrawString(font, "Time: " + (float)gameTime.ElapsedGameTime.Milliseconds / 1000, new Vector2(0, 150), Color.White);
                    spriteBatch.DrawString(font, "Acc: " + Math.Round(busLogic.GetCurrentAcceleration(), 2), new Vector2(0, 180), Color.White);
                    spriteBatch.DrawString(font, "Side acc: " + Math.Round(busLogic.GetSideAcceleration(), 2), new Vector2(0, 210), Color.White);

                    spriteBatch.DrawString(font, "Scale: " + Helper.GetScale(), new Vector2(0, 270), Color.White);
                    spriteBatch.DrawString(font, "HUD Scale: " + hud.scale.ToString("0.00"), new Vector2(0, 300), Color.White);
                    spriteBatch.DrawString(font, "GL (R): " + environmentSimulation.GetGlobalLightColor().X.ToString(), new Vector2(0, 330), Color.White);
                    spriteBatch.DrawString(font, "GL (GB): " + environmentSimulation.GetGlobalLightColor().Y.ToString(), new Vector2(0, 360), Color.White);
                    spriteBatch.DrawString(font, "Cars: " + trafficLogic.vehicles.Count, new Vector2(0, 390), Color.White);
                    spriteBatch.DrawString(font, "Fumes: " + particlesLogic.CountFumes(), new Vector2(0, 420), Color.White);
                }
            }

            FPSCounter.IncFrameCounter();

            spriteBatch.End();
        }

        protected void DrawScene(SpriteBatch spriteBatch, GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(scene); // ustawiamy obiekt renderowania

            GraphicsDevice.Clear(Color.Transparent);

            // rysowanie:
            spriteBatch.Begin(); // inicjujemy sprite batch

            // track, objects under bus, pedestrians
            drawMap.DrawTrack(spriteBatch, gameTime);
            drawMap.DrawObjectsUnderBus(spriteBatch, gameTime);
            drawMap.DrawPedestrians(spriteBatch, gameTime);

            // Gameplay.
            drawGameplay.Draw(gameplayLogic, spriteBatch);

            // Traffic.
            drawTraffic.Draw(trafficLogic, spriteBatch);

            // Bus.
            drawBus.Draw(busLogic, spriteBatch);

            // Something very nice and useful - most likely map object wchich are supposed to be above the bus.
            drawMap.DrawObjectsOnBus(spriteBatch, gameTime);

            // traffic lights
            drawMap.DrawTrafficLights(spriteBatch, gameTime);

            // Particles.
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            drawParticles.Draw(particlesLogic, spriteBatch);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null); // ustawiamy obiekt renderowania z powrotem na domyœlny (ekran)
        }

        protected void DrawHud(SpriteBatch spriteBatch, TimeSpan frameInterval)
        {
            InformationForHud infoForHud = new InformationForHud();
            infoForHud.busSpeed = busLogic.GetCurrentSpeed();
            infoForHud.busRpm = busLogic.GetCurrentAcceleration();
            infoForHud.busGear = busLogic.GetCurrentGear();
            infoForHud.busPosition = busLogic.GetBusPosition();
            infoForHud.busStopPosition = gameplayLogic.GetCurrentBusStopPosition();

            infoForHud.numberOfPedestriansInTheBus = gameplayLogic.NumberOfPedestriansInTheBus();
            infoForHud.doorOpen = busLogic.DoorsAreOpen();
            if (gameplayLogic.ArePedestriansGettingIn())
            {
                infoForHud.pedestrianState = PedestrianState.GettingIn;
            }
            else if (gameplayLogic.ArePedestriansGettingOff())
            {
                infoForHud.pedestrianState = PedestrianState.GettingOff;
            }
            else
            {
                infoForHud.pedestrianState = PedestrianState.Nothing;
            }

            infoForHud.currentTime = environmentSimulation.GetCurrentTime();

            hud.Draw(spriteBatch, GraphicsDevice, frameInterval, infoForHud, drawMap.GetMinimapTexture(), InputLogic.pauseButton.state);
        }
    }
}
