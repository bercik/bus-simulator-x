using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    class DrawMap
    {
        SpriteFont areaChangeFont; // czcionka u¿ywana do wyœwietlenia nazwy zmienianego obszaru
        SpriteFont mapPreviewFont; // czcionka u¿ywana do wyœwietlenia napisu: "jestem tutaj!"

        Texture2D imHereTexture; // tekstura: "jestem tutaj!"
        Texture2D nextBusStopTexture; // tekstura: "nastêpny przystanek"

        // efekt gradientu na napisie:
        Effect gradientEffect;
        EffectParameter gradientColor;

        // zmienne pomocnicze dla napisu zmiany obszaru:
        Texture2D areasChangeTexture;
        Vector2 areasChangeTexturePosition;

        Dictionary<string, Texture2D> textures;
        Dictionary<string, Texture2D> grass;
        Dictionary<string, Texture2D> junctions;
        Dictionary<string, Texture2D> pedestrians;
        Dictionary<string, Texture2D> trafficLights;

        MapLogic mapLogic;
        BackgroundLogic backgroundLogic;
        TrackLogic trackLogic;
        PedestriansLogic pedestriansLogic;
        AreasLogic areasLogic;
        MinimapLogic minimapLogic;
        Vector2 pos =  new Vector2(0, 0);

        bool load = false;

        public DrawMap(GraphicsDevice graphicsDevice)
        {
            // TODO: Construct any child components here
            mapLogic = new MapLogic();
            backgroundLogic = new BackgroundLogic();
            trackLogic = new TrackLogic();
            pedestriansLogic = new PedestriansLogic();
            areasLogic = new AreasLogic();
            minimapLogic = new MinimapLogic(graphicsDevice);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            // TODO: Add your initialization code here
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime, Vector2[] busCollisionPoints, ref TrafficLogic trafficLogic)
        {
            // TODO: Add your update code here
            pedestriansLogic.Update(gameTime.ElapsedGameTime, busCollisionPoints);
            areasLogic.Update(gameTime.ElapsedGameTime, ref trafficLogic);
            trackLogic.Update(gameTime.ElapsedGameTime);
        }

        /// <summary>
        /// Funkcja pozwalaj¹ca na dodanie pieszego do chodnika
        /// </summary>
        /// <param name="id">Nazwa tekstury.</param>
        /// <param name="position">Aktualna pozycja pieszego.</param>
        /// <param name="rotation">Aktualna rotacja pieszego.</param>
        /// <returns>True je¿eli uda³o siê dodaæ pieszego do chodnika. False je¿eli nie.</returns>
        public bool AddPedestrian(string id, Vector2 position, float rotation)
        {
            return pedestriansLogic.AddPedestrian(id, position, rotation);
        }

        public Texture2D GetMinimapTexture()
        {
            return minimapLogic.GetMinimapTexture();
        }

        public void DrawMinimap(SpriteBatch spriteBatch, float busDirection, Vector2 currentBusStopPosition)
        {
            minimapLogic.DrawMinimap(spriteBatch, mapLogic.GetJunctionsForMinimap(GameParams.minimapScale), junctions, busDirection, currentBusStopPosition);
        }

        public void DrawPreview(SpriteBatch spriteBatch, float previewScale, Vector2 currentBusStopPosition)
        {
            List<Object> objectsToPrewiev = mapLogic.GetObjectsToPreview(previewScale);

            foreach (Object o in objectsToPrewiev)
            {
                Vector2 destinationPos = Helper.CalculateScalePosition(o.pos, previewScale);
                Vector2 v_previewScale = new Vector2(1 / previewScale, 1 / previewScale);

                if (textures.ContainsKey(o.name))
                {
                    spriteBatch.Draw(textures[o.name], destinationPos, null, Color.White, MathHelper.ToRadians(o.rotate),
                            o.original_origin, v_previewScale * o.scale, o.spriteEffects, 1.0f);
                }
                else if (junctions.ContainsKey(o.name))
                {
                    spriteBatch.Draw(junctions[o.name], destinationPos, null, Color.White, MathHelper.ToRadians(o.rotate),
                            o.original_origin, v_previewScale * o.scale, o.spriteEffects, 1.0f);
                }
                else if (trafficLights.ContainsKey(o.name))
                {
                    spriteBatch.Draw(trafficLights[o.name], destinationPos, null, Color.White, MathHelper.ToRadians(o.rotate),
                            o.original_origin, v_previewScale, o.spriteEffects, 1.0f);
                }

                // wyœwietlanie informacji o po³o¿eniu gracza:
                ShowNextBusStop(spriteBatch, previewScale, currentBusStopPosition);
                ShowImHere(spriteBatch, previewScale);
            }
        }

        private void ShowNextBusStop(SpriteBatch spriteBatch, float previewScale, Vector2 currentBusStopPosition)
        {
            Vector2 texturePos = Helper.MapPosToScreenPos(currentBusStopPosition);
            Vector2 textureSize = new Vector2(nextBusStopTexture.Width, nextBusStopTexture.Height);
            Vector2 textureOrigin = textureSize / 2;
            texturePos = Helper.CalculateScalePoint(texturePos, previewScale);
            spriteBatch.Draw(nextBusStopTexture, texturePos, null, Color.White, 0.0f, textureOrigin, 1.0f, SpriteEffects.None, 1.0f);

            Vector2 textPos = texturePos;
            textPos.Y += textureSize.Y + textureOrigin.Y;
            string text = "NASTÊPNY PRZYSTANEK";
            Vector2 textSize = mapPreviewFont.MeasureString(text);
            Vector2 textOrigin = textSize / 2;

            spriteBatch.DrawString(mapPreviewFont, text, textPos, Color.YellowGreen, 0.0f, textOrigin, 1.0f, SpriteEffects.None, 1);
        }

        private void ShowImHere(SpriteBatch spriteBatch, float previewScale)
        {
            Vector2 texturePos = Helper.MapPosToScreenPos(Helper.busPos);
            Vector2 textureSize = new Vector2(imHereTexture.Width, imHereTexture.Height);
            Vector2 textureOrigin = textureSize / 2;
            texturePos = Helper.CalculateScalePoint(texturePos, previewScale);
            spriteBatch.Draw(imHereTexture, texturePos, null, Color.White, 0.0f, textureOrigin, 1.0f, SpriteEffects.None, 1.0f);

            Vector2 textPos = Helper.MapPosToScreenPos(Helper.busPos);
            textPos = Helper.CalculateScalePoint(textPos, previewScale);
            textPos.Y += textureSize.Y + textureOrigin.Y;
            string text = "TU JESTEŒ!";
            Vector2 textSize = mapPreviewFont.MeasureString(text);
            Vector2 textOrigin = textSize / 2;

            spriteBatch.DrawString(mapPreviewFont, text, textPos, Color.OrangeRed, 0.0f, textOrigin, 1.0f, SpriteEffects.None, 1);
        }

        public void DrawAreasChangeInit(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GameTime gameTime)
        {
            string text;
            Color color;

            areasLogic.GetTextAndColorToShow(gameTime.ElapsedGameTime, out text, out color);

            if (text != "")
            {
                // zmiana parametru koloru dla efektu gradientu:
                gradientColor.SetValue(color.ToVector4());

                // ustalanie pozycji tekstu na ekranie (tak aby by³ na samym jego œrodku):
                Vector2 textSize = areaChangeFont.MeasureString(text);
                areasChangeTexturePosition = new Vector2(Helper.screenOrigin.X - (textSize.X / 2), 0);

                // tworzenie obiektu i ustawianie obiektu renderowania:
                RenderTarget2D areaChangeRenderTarget = new RenderTarget2D(graphicsDevice, (int)textSize.X, (int)textSize.Y);

                graphicsDevice.SetRenderTarget(areaChangeRenderTarget); // ustawiamy obiekt renderowania

                graphicsDevice.Clear(Color.Transparent);

                // rysowanie napisu do tekstury:
                spriteBatch.Begin(); // inicjujemy sprite batch

                spriteBatch.DrawString(areaChangeFont, text, new Vector2(0, 0), Color.White);  // rysujemy napis

                spriteBatch.End();  // zakañczamy sprite batch

                graphicsDevice.SetRenderTarget(null); // ustawiamy obiekt renderowania z powrotem na domyœlny (ekran)

                areasChangeTexture = areaChangeRenderTarget;
            }
            else
            {
                areasChangeTexture = null;
            }
        }

        public void DrawAreasChange(SpriteBatch spriteBatch)
        {
            if (areasChangeTexture != null)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied); // musimy zmieniæ tryb spriteBatch, ¿eby ekeft gradientu móg³ dzia³aæ

                // w³¹czanie efektu:
                gradientEffect.CurrentTechnique = gradientEffect.Techniques["Gradient"];
                gradientEffect.CurrentTechnique.Passes[0].Apply();

                spriteBatch.Draw(areasChangeTexture, areasChangeTexturePosition, Color.White);

                spriteBatch.End(); // zakañczamy sprite batch
            }
        }

        // rysuje obiekty pod autobusem
        public void DrawObjectsUnderBus(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (load)
            {
                List<Object> objectsToShow = mapLogic.GetObjectsUnderBusToShow();

                foreach (Object o in objectsToShow)
                {
                    Vector2 destinationPos = Helper.CalculateScalePosition(o.pos);

                    if (textures.ContainsKey(o.name))
                    {
                        spriteBatch.Draw(textures[o.name], destinationPos, null, Color.White, MathHelper.ToRadians(o.rotate),
                                o.original_origin, Helper.GetVectorScale() * o.scale, o.spriteEffects, 1.0f);
                    }
                }
            }
        }

        // rysuje obiekty nad autobusem
        public void DrawObjectsOnBus(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (load)
            {
                List<Object> objectsToShow = mapLogic.GetObjectsOnBusToShow();

                foreach (Object o in objectsToShow)
                {
                    Vector2 destinationPos = Helper.CalculateScalePosition(o.pos);

                    if (textures.ContainsKey(o.name))
                    {
                        spriteBatch.Draw(textures[o.name], destinationPos, null, Color.White, MathHelper.ToRadians(o.rotate), 
                                o.original_origin, Helper.GetVectorScale() * o.scale, o.spriteEffects, 1.0f);
                    }
                }
            }
        }

        // rysuje trasê, trawe i chodniki
        public void DrawTrack(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (load)
            {
                backgroundLogic.UpdatePos(pos);

                List<Grass> grassToShow = backgroundLogic.getGrassToShow();
                List<Object> junctionsToShow = mapLogic.GetJunctionsToShow();

                foreach (Grass g in grassToShow)
                {
                    Vector2 destinationPos = Helper.CalculateScalePosition(g.pos);
                    
                    spriteBatch.Draw(grass[g.name], destinationPos, null, Color.White, 0,
                            backgroundLogic.getGrassOrigin(), Helper.GetVectorScale(), SpriteEffects.None, 1.0f);
                }
                foreach (Object o in junctionsToShow)
                {
                    Vector2 destinationPos = Helper.CalculateScalePosition(o.pos);

                    if (junctions.ContainsKey(o.name))
                    {
                        spriteBatch.Draw(junctions[o.name], destinationPos, null, Color.White, MathHelper.ToRadians(o.rotate),
                                o.original_origin, Helper.GetVectorScale() * o.scale, o.spriteEffects, 1.0f);
                    }
                }
            }
        }

        public void DrawPedestrians(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (load)
            {
                List<Object> pedestriansToShow = pedestriansLogic.GetPedestriansToShow();

                foreach (Object p in pedestriansToShow)
                {
                    Vector2 destinationPos = Helper.CalculateScalePosition(p.pos);

                    spriteBatch.Draw(pedestrians[p.name], destinationPos, null, Color.White, MathHelper.ToRadians(p.rotate),
                            p.origin, Helper.GetVectorScale() * p.scale, p.spriteEffects, 1.0f);

                }
            }
        }

        public void DrawTrafficLights(SpriteBatch spriteBatch, GameTime gameTime)
        {
            List<TrafficLightObject> trafficLightObjects = mapLogic.GetTrafficLightsToShow();

            foreach (TrafficLightObject tlo in trafficLightObjects)
            {
                TrafficLightState trafficLightState = trackLogic.GetTrafficLightPairState(tlo.junctionIndex, tlo.pairIndex);
                string name = tlo.name + ((int)trafficLightState).ToString();

                Vector2 destinationPos = Helper.CalculateScalePosition(tlo.pos);

                if (trafficLights.ContainsKey(name))
                {
                    spriteBatch.Draw(trafficLights[name], destinationPos, null, Color.White, MathHelper.ToRadians(tlo.rotate),
                            tlo.original_origin, Helper.GetVectorScale(), tlo.spriteEffects, 1.0f);
                }
            }
        }

        // funkcja pomocnicza sluzaca do pobrania punktow kolizji do ich pozniejszego wyswietlenia na ekranie
        public Vector2[] GetCollisionPointsToDraw()
        {
            return mapLogic.GetCollisionPointsToDraw();
        }

        public void LoadContent(ContentManager content)
        {
            minimapLogic.LoadContent(content);

            // zmienna pomocnicza:
            Vector2 size;

            // ladujemy efekt:
            gradientEffect = content.Load<Effect>("effects/gradient");
            gradientColor = gradientEffect.Parameters["gradientColor"];

            // ladujemy informacje o obiektach
            mapLogic.LoadObjectsInformation("objects.if");

            // ladujemy czcionki:
            areaChangeFont = content.Load<SpriteFont>("fonts/areaChangeFont");
            mapPreviewFont = content.Load<SpriteFont>("fonts/mapPreviewFont");

            // ladujemy teksture: "jestem tutaj!"
            imHereTexture = content.Load<Texture2D>("help/imHere");
            nextBusStopTexture = content.Load<Texture2D>("help/nextBusStop");

            // ladowanie tekstur obiektow (PRZY DODANIU NOWEJ DODAJEMY LINIJKE TYLKO TUTAJ)
            textures = new Dictionary<string, Texture2D>();

            string[] names = mapLogic.GetObjectsNames();
            string objectTexturesDirectory = "objects/";

            foreach (string name in names)
                textures.Add(name, content.Load<Texture2D>(objectTexturesDirectory + name));

            mapLogic.AddStandartObjectsSize(textures);

            // ladowanie tekstur trawy
            grass = new Dictionary<string, Texture2D>();
            grass.Add("trawa0", content.Load<Texture2D>("grass/trawa0"));
            grass.Add("trawa1", content.Load<Texture2D>("grass/trawa1"));
            grass.Add("trawa2", content.Load<Texture2D>("grass/trawa2"));

            size = new Vector2(grass[grass.Keys.ToList()[0]].Width, grass[grass.Keys.ToList()[0]].Height); // wielkosc tekstury trawy (MUSI byc jednakowa dla wszystkich tekstur trawy)
            backgroundLogic.SetProperties(size, grass.Count);

            // ladowanie tekstur swiatel
            trafficLights = new Dictionary<string, Texture2D>();

            trafficLights.Add("light", content.Load<Texture2D>("traffic_lights/light"));
            trafficLights.Add("light0", content.Load<Texture2D>("traffic_lights/light0"));
            trafficLights.Add("light1", content.Load<Texture2D>("traffic_lights/light1"));
            trafficLights.Add("light2", content.Load<Texture2D>("traffic_lights/light2"));
            trafficLights.Add("light3", content.Load<Texture2D>("traffic_lights/light3"));

            size = new Vector2(trafficLights[trafficLights.Keys.ToList()[0]].Width, trafficLights[trafficLights.Keys.ToList()[0]].Height);
            trackLogic.SetTrafficLightSize(size);

            // ladowanie tekstur i typow skrzyzowan
            Direction[] directions;

            junctions = new Dictionary<string, Texture2D>();

            // kierunki dodawaæ w kolejnoœci: GÓRA, PRAWO, DÓ£, LEWO
            junctions.Add("junction0", content.Load<Texture2D>("junctions/junction0"));
            directions = new Direction[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
            size = new Vector2(junctions["junction0"].Width, junctions["junction0"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction1", content.Load<Texture2D>("junctions/junction1"));
            directions = new Direction[] { Direction.Right, Direction.Down };
            size = new Vector2(junctions["junction1"].Width, junctions["junction1"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction2", content.Load<Texture2D>("junctions/junction2"));
            directions = new Direction[] { Direction.Right, Direction.Down, Direction.Left };
            size = new Vector2(junctions["junction2"].Width, junctions["junction2"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction3", content.Load<Texture2D>("junctions/junction3"));
            directions = new Direction[] { Direction.Right, Direction.Down };
            size = new Vector2(junctions["junction3"].Width, junctions["junction3"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction4", content.Load<Texture2D>("junctions/junction4"));
            directions = new Direction[] { Direction.Right, Direction.Down, Direction.Left };
            size = new Vector2(junctions["junction4"].Width, junctions["junction4"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction5", content.Load<Texture2D>("junctions/junction5"));
            directions = new Direction[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
            size = new Vector2(junctions["junction5"].Width, junctions["junction5"].Height);
            trackLogic.AddJunctionType(size, directions);

            // ladowanie tekstur ulic
            junctions.Add("street0", content.Load<Texture2D>("streets/street0"));
            junctions.Add("street1", content.Load<Texture2D>("streets/street1"));
            junctions.Add("street2", content.Load<Texture2D>("streets/street2"));

            size = new Vector2(junctions["street0"].Width, junctions["street0"].Height);
            trackLogic.SetStreetSize(size, 3); // !!! zmieniæ przy dodaniu lub usunieciu tekstury ulicy

            // ladowanie tekstur chodnikow
            junctions.Add("chodnik0", content.Load<Texture2D>("sidewalks/chodnik0"));
            junctions.Add("chodnik1", content.Load<Texture2D>("sidewalks/chodnik1"));
            junctions.Add("chodnik2", content.Load<Texture2D>("sidewalks/chodnik2"));
            junctions.Add("chodnik3", content.Load<Texture2D>("sidewalks/chodnik3"));

            int[] frequences = new int[] { 1, 3, 5, 12 }; // dodac lub usunac przy dodaniu lub usunieciu jednego typu chodnika
            pedestriansLogic.SetFrequencyOfOccurrencePedestrians(frequences);

            size = new Vector2(junctions["chodnik0"].Width, junctions["chodnik0"].Height);
            float sidewalkHeight = size.Y;
            trackLogic.SetSidewalkSize(size);

            // ladowanie tekstur pieszych
            pedestrians = new Dictionary<string, Texture2D>();
            pedestrians.Add("died_pedestrian", content.Load<Texture2D>("pedestrians/died_pedestrian"));
            pedestrians.Add("pedestrian0", content.Load<Texture2D>("pedestrians/pedestrian0"));
            pedestrians.Add("pedestrian1", content.Load<Texture2D>("pedestrians/pedestrian1"));
            pedestrians.Add("pedestrian2", content.Load<Texture2D>("pedestrians/pedestrian2"));

            size = new Vector2(pedestrians["pedestrian0"].Width, pedestrians["pedestrian0"].Height);
            pedestriansLogic.SetProperties(size, 3, sidewalkHeight); // zmodyfikowac przy dodaniu lub usunieciu pieszych
        }

        // zwraca czy udalo sie zaladowac mape, startowa pozycja autobusu, startowa rotacja autobusu
        public void LoadMap(string path, ref Vector2 startPosition, ref float startRotation)
        {
            Stream s = TitleContainer.OpenStream("Content/maps/" + path);

            Encoding enc = Encoding.GetEncoding("Windows-1250"); // dziêki temu mo¿emy odczytywaæ polskie znaki
            StreamReader sr = new StreamReader(s, enc);

            // pobieramy startowa pozycje autobusu i jego rotacje
            string s_startPosition = sr.ReadLine();
            string[] split = s_startPosition.Split(new char[] { ';' });
            startPosition = new Vector2(float.Parse(split[0]), float.Parse(split[1]));
            startRotation = float.Parse(sr.ReadLine()); // pobieramy startow¹ rotacjê

            mapLogic.LoadMap(ref sr);
            trackLogic.LoadTrack(ref sr);
            areasLogic.LoadAreas(ref sr);
            mapLogic.AddJunctionsToChunks(trackLogic.GetJunctions()); // dodajemy skrzyzowania, drogi i chodniki jako obiekty do wyswietlenia
            mapLogic.AddTrafficLightsToChunks(trackLogic.GetTrafficLights()); // dodajemy swiatla jako obiekty do wyswietlenia
            pedestriansLogic.SetSidewalks(trackLogic.GetSidewalks()); // ustawiamy skrzyzowania w klasie pedestriansLogic

            load = true;
        }

        public void SetPosition(Vector2 pos)
        {
            this.pos = pos;

            mapLogic.SetObjectsInRange(pos);
        }

        public bool IsCollision(Vector2 point)
        {
            return mapLogic.IsCollision(point);
        }

        public bool IsCollision(Vector2[] points)
        {
            return mapLogic.IsCollision(points);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">Okreœla o ile od krawêdzi mapy mo¿e najdalej byæ skrzy¿owanie</param>
        /// <param name="carLength">D³ugoœæ auta (czyli wspó³rzêdna Y wielkoœci lub Height)</param>
        /// <param name="connection">Po³¹czenie miêdzy skrzy¿owaniami</param>
        /// <param name="origin">Œrodek jednego z skrzy¿owañ</param>
        /// <param name="randomOutPoint">Losowy punkt wyjœcia inny od tego z connection z danego skrzy¿owania</param>
        public void CreateTrack(Vector2 size, float carLength, out Connection connection, out Vector2 origin, out Vector2 randomOutPoint)
        {
            trackLogic.CreateTrack(size, carLength, out connection, out origin, out randomOutPoint);
        }

        public void ChangeTrack(Vector2 endPoint, Vector2 lastEndPoint, out Connection connection, out Vector2 origin)
        {
            trackLogic.ChangeTrack(endPoint, lastEndPoint, out connection, out origin);
        }

        public void CreateGrass(Vector2 mapPos)
        {
            backgroundLogic.CreateGrass(mapPos);
        }

        public void GetRedLightRectangles(out List<Rectangle> redLightRectanglesForCars, out List<TrafficLightRectangle> redLightRectanglesForBus)
        {
            trackLogic.GetRedLightRectangles(out redLightRectanglesForCars, out redLightRectanglesForBus);
        }
    }
}
