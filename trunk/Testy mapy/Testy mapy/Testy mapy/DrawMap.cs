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
    public class DrawMap : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        Dictionary<string, Texture2D> textures;
        Dictionary<string, Texture2D> grass;
        Dictionary<string, Texture2D> junctions;

        MapLogic mapLogic;
        BackgroundLogic backgroundLogic;
        TrackLogic trackLogic;
        Vector2 pos =  new Vector2(0, 0);

        bool load = false;

        public DrawMap(Game game)
            : base(game)
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
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (load)
            {
                backgroundLogic.UpdatePos(pos);

                List<Grass> grassToShow = backgroundLogic.getGrassToShow();
                List<Object> objectsToShow = mapLogic.GetObjectsToShow();

                spriteBatch.Begin();

                foreach (Grass g in grassToShow)
                {
                    spriteBatch.Draw(grass[g.name], g.pos, Color.White);
                }

                foreach (Object o in objectsToShow)
                {
                    Rectangle destinationRect = new Rectangle((int)o.pos.X, (int)o.pos.Y, (int)o.size.X, (int)o.size.Y);
                    
                    if (junctions.ContainsKey(o.name))
                    {
                        spriteBatch.Draw(junctions[o.name], destinationRect, null, Color.White, MathHelper.ToRadians(o.rotate),
                                o.original_origin, o.spriteEffects, 1);
                    }
                    else if (textures.ContainsKey(o.name))
                    {
                        spriteBatch.Draw(textures[o.name], destinationRect, null, Color.White, MathHelper.ToRadians(o.rotate),
                                o.original_origin, o.spriteEffects, 1);
                    }
                }

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            // ladowanie tekstur (PRZY DODANIU NOWEJ DODAJEMY LINIJKE TYLKO TUTAJ)
            textures = new Dictionary<string, Texture2D>();
            textures.Add("beczka", Game.Content.Load<Texture2D>("beczka"));
            textures.Add("pole", Game.Content.Load<Texture2D>("pole"));
            textures.Add("przystanek", Game.Content.Load<Texture2D>("przystanek"));
            textures.Add("budynek0", Game.Content.Load<Texture2D>("budynek0"));
            textures.Add("znak_uwaga_zakret_w_lewo", Game.Content.Load<Texture2D>("znak_uwaga_zakret_w_lewo"));
            textures.Add("drzewo", Game.Content.Load<Texture2D>("drzewo"));

            mapLogic.AddStandartObjectsSize(textures);

            // ladowanie tekstur trawy
            grass = new Dictionary<string, Texture2D>();
            grass.Add("trawa0", Game.Content.Load<Texture2D>("trawa0"));
            grass.Add("trawa1", Game.Content.Load<Texture2D>("trawa1"));
            grass.Add("trawa2", Game.Content.Load<Texture2D>("trawa2"));

            Vector2 grassSize = new Vector2(grass[grass.Keys.ToList()[0]].Width, grass[grass.Keys.ToList()[0]].Width); // wielkosc tekstury trawy (MUSI byc jednakowa dla wszystkich tekstur trawy)
            backgroundLogic.SetProperties(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, grassSize, grass.Count);

            // ladowanie tekstur i typow skrzyzowan
            Vector2 size;
            Direction[] directions;

            junctions = new Dictionary<string, Texture2D>();

            // kierunki dodawaæ w kolejnoœci: GÓRA, PRAWO, DÓ£, LEWO

            junctions.Add("junction0", Game.Content.Load<Texture2D>("junction0"));
            directions = new Direction[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
            size = new Vector2(junctions["junction0"].Width, junctions["junction0"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction1", Game.Content.Load<Texture2D>("junction1"));
            directions = new Direction[] { Direction.Right, Direction.Down };
            size = new Vector2(junctions["junction1"].Width, junctions["junction1"].Height);
            trackLogic.AddJunctionType(size, directions);

            junctions.Add("junction2", Game.Content.Load<Texture2D>("junction2"));
            directions = new Direction[] { Direction.Right, Direction.Down, Direction.Left };
            size = new Vector2(junctions["junction2"].Width, junctions["junction2"].Height);
            trackLogic.AddJunctionType(size, directions);

            // ladowanie tekstur ulic
            junctions.Add("street0", Game.Content.Load<Texture2D>("street0"));
            junctions.Add("street1", Game.Content.Load<Texture2D>("street1"));
            junctions.Add("street2", Game.Content.Load<Texture2D>("street2"));

            size = new Vector2(junctions["street0"].Width, junctions["street0"].Height);
            trackLogic.SetStreetSize(size, 3); // !!! zmieniæ przy dodaniu lub usunieciu tekstury ulicy

            base.LoadContent();
        }

        public void LoadMap(string path)
        {
            load = mapLogic.LoadMap(path, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
        }

        public void LoadTrack(string path)
        {
            if (trackLogic.LoadTrack(path))
            {
                mapLogic.AddObjectsToChunks(trackLogic.getObjects(), true);
            }
        }

        public void SetPosition(Vector2 pos)
        {
            this.pos = pos;

            mapLogic.SetOBjectsInRange(pos);
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
        public Connection CreateTrack(Vector2 size)
        {
            return trackLogic.CreateTrack(size);
        }

        public Connection ChangeTrack(Vector2 endPoint)
        {
            return trackLogic.ChangeTrack(endPoint);
        }
    }
}
