using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    public class DrawMap
    {
        Dictionary<string, Texture2D> textures;
        Dictionary<string, Texture2D> grass;
        Dictionary<string, Texture2D> junctions;

        MapLogic mapLogic;
        BackgroundLogic backgroundLogic;
        TrackLogic trackLogic;
        Vector2 pos =  new Vector2(0, 0);

        bool load = false;

        public DrawMap()
        {
            // TODO: Construct any child components here
            mapLogic = new MapLogic();
            backgroundLogic = new BackgroundLogic();
            trackLogic = new TrackLogic();
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
        public void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
        }

        // rysuje obiekty pod autobusem
        public void DrawObjectsUnderBus(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (load)
            {
                List<Object> objectsToShow = mapLogic.GetObjectsUnderBusToShow();

                foreach (Object o in objectsToShow)
                {
                    Rectangle destinationRect = new Rectangle((int)o.pos.X, (int)o.pos.Y, (int)o.size.X, (int)o.size.Y);

                    if (textures.ContainsKey(o.name))
                    {
                        spriteBatch.Draw(textures[o.name], destinationRect, null, Color.White, MathHelper.ToRadians(o.rotate),
                                o.original_origin, o.spriteEffects, 1);
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
                    Rectangle destinationRect = new Rectangle((int)o.pos.X, (int)o.pos.Y, (int)o.size.X, (int)o.size.Y);

                    if (textures.ContainsKey(o.name))
                    {
                        spriteBatch.Draw(textures[o.name], destinationRect, null, Color.White, MathHelper.ToRadians(o.rotate),
                                o.original_origin, o.spriteEffects, 1);
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
                    spriteBatch.Draw(grass[g.name], g.pos, Color.White);
                }
                foreach (Object o in junctionsToShow)
                {
                    Rectangle destinationRect = new Rectangle((int)o.pos.X, (int)o.pos.Y, (int)o.size.X, (int)o.size.Y);

                    if (junctions.ContainsKey(o.name))
                    {
                        spriteBatch.Draw(junctions[o.name], destinationRect, null, Color.White, MathHelper.ToRadians(o.rotate),
                                o.original_origin, o.spriteEffects, 1);
                    }
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
            // ladujemy informacje o obiektach
            mapLogic.LoadObjects("objects.if");

            // ladowanie tekstur (PRZY DODANIU NOWEJ DODAJEMY LINIJKE TYLKO TUTAJ)
            textures = new Dictionary<string, Texture2D>();

            string[] names = mapLogic.GetObjectsNames();

            foreach (string name in names)
                textures.Add(name, content.Load<Texture2D>(name));

            mapLogic.AddStandartObjectsSize(textures);

            // ladowanie tekstur trawy
            grass = new Dictionary<string, Texture2D>();
            grass.Add("trawa0", content.Load<Texture2D>("trawa0"));
            grass.Add("trawa1", content.Load<Texture2D>("trawa1"));
            grass.Add("trawa2", content.Load<Texture2D>("trawa2"));

            Vector2 grassSize = new Vector2(grass[grass.Keys.ToList()[0]].Width, grass[grass.Keys.ToList()[0]].Width); // wielkosc tekstury trawy (MUSI byc jednakowa dla wszystkich tekstur trawy)
            backgroundLogic.SetProperties((int)Helper.screenSize.X, (int)Helper.screenSize.Y, grassSize, grass.Count);

            // ladowanie tekstur i typow skrzyzowan
            Vector2 size;
            Direction[] directions;

            junctions = new Dictionary<string, Texture2D>();

            // kierunki dodawaæ w kolejnoœci: GÓRA, PRAWO, DÓ£, LEWO

            junctions.Add("junction0", content.Load<Texture2D>("junction0"));
            directions = new Direction[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
            size = new Vector2(junctions["junction0"].Width, junctions["junction0"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction1", content.Load<Texture2D>("junction1"));
            directions = new Direction[] { Direction.Right, Direction.Down };
            size = new Vector2(junctions["junction1"].Width, junctions["junction1"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction2", content.Load<Texture2D>("junction2"));
            directions = new Direction[] { Direction.Right, Direction.Down, Direction.Left };
            size = new Vector2(junctions["junction2"].Width, junctions["junction2"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction3", content.Load<Texture2D>("junction3"));
            directions = new Direction[] { Direction.Right, Direction.Down };
            size = new Vector2(junctions["junction3"].Width, junctions["junction3"].Height);
            trackLogic.AddJunctionType(size, directions);

            // ladowanie tekstur ulic
            junctions.Add("street0", content.Load<Texture2D>("street0"));
            junctions.Add("street1", content.Load<Texture2D>("street1"));
            junctions.Add("street2", content.Load<Texture2D>("street2"));

            size = new Vector2(junctions["street0"].Width, junctions["street0"].Height);
            trackLogic.SetStreetSize(size, 3); // !!! zmieniæ przy dodaniu lub usunieciu tekstury ulicy

            // ladowanie tekstur chodnikow
            junctions.Add("chodnik0", content.Load<Texture2D>("chodnik0"));
            junctions.Add("chodnik1", content.Load<Texture2D>("chodnik1"));

            size = new Vector2(junctions["chodnik0"].Width, junctions["chodnik0"].Height);
            trackLogic.SetChodnikSize(size);
        }

        // zwraca czy udalo sie zaladowac mape, startowa pozycja autobusu
        public bool LoadMap(string path, ref Vector2 startPosition)
        {
            path = "maps/" + path;

            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                // pobieramy startowa pozycje autobusu
                string s_startPosition = sr.ReadLine();
                string[] split = s_startPosition.Split(new char[] { ';' });
                startPosition = new Vector2(float.Parse(split[0]), float.Parse(split[1]));

                mapLogic.LoadMap(ref sr);
                trackLogic.LoadTrack(ref sr);
                mapLogic.AddJunctionsToChunks(trackLogic.GetJunctions()); // dodajemy skrzyzowania i drogi jaki obiekty do wyswietlenia

                load = true;
                return true;
            }
            else
            {
                return false;
            }
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

        // size okreœla o ile od krawêdzi mapy mo¿e byæ oddalone skrzy¿owanie
        public void CreateTrack(Vector2 size, out Connection connection, out Vector2 origin)
        {
            trackLogic.CreateTrack(size, out connection, out origin);
        }

        public void ChangeTrack(Vector2 endPoint, Vector2 lastEndPoint, out Connection connection, out Vector2 origin)
        {
            trackLogic.ChangeTrack(endPoint, lastEndPoint, out connection, out origin);
        }

        public void CreateGrass()
        {
            backgroundLogic.CreateGrass();
        }
    }
}
