using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace Testy_mapy
{
    enum Rotation {rot0 = 0, rot90 = 90, rot180 = 180, rot270 = 270}

    struct Connection
    {
        public readonly Vector2 point1;
        public Vector2 point2; // jezeli (0,0) to znaczy, ze nie zainicjowano tego punktu

        public Connection(Vector2 point1)
        {
            this.point1 = point1;
            point2 = Vector2.Zero;
        }
    }

    class JunctionType
    {
        public Vector2 origin;
        public Connection[] connections; // względem środka
        protected int connectionsCount;

        protected JunctionType()
        {

        }

        public JunctionType(Vector2 size, Direction[] directions)
        {
            origin = size / 2;
            connectionsCount = directions.Length;
            connections = new Connection[connectionsCount];

            for (int i = 0; i < directions.Length; ++i)
            {
                switch (directions[i])
                {
                    case Direction.Up:
                        connections[i] = new Connection(new Vector2(0, -origin.Y));
                        break;
                    case Direction.Right:
                        connections[i] = new Connection(new Vector2(origin.X, 0));
                        break;
                    case Direction.Down:
                        connections[i] = new Connection(new Vector2(0, origin.Y));
                        break;
                    case Direction.Left:
                        connections[i] = new Connection(new Vector2(-origin.X, 0));
                        break;
                }
            }
        }

        public Junction Create(Vector2 pos, Rotation rotation)
        {
            Junction junction = new Junction(); // raport pelikana film obejrzeć
            
            junction.connectionsCount = this.connectionsCount;
            junction.connections = this.connections;
            junction.origin = this.origin;
            junction.pos = pos;
            junction.rotation = rotation;

            return junction;
        }
    }

    class Junction : JunctionType
    {
        public Junction() { } // NIE tworzyć obiektów (wykorzystać do tego funkcje JunctionType.Create())

        public Rotation rotation;
        public Vector2 pos;
    }

    class TrackLogic
    {
        List<Junction> junctions;
        List<JunctionType> junctionTypes;
        List<Vector2> streetTypes;

        private void AddJunction(string s_object)
        {
            string[] split = s_object.Split(';');

            int id = Int32.Parse(split[0]);
            Vector2 pos = new Vector2(float.Parse(split[1]), float.Parse(split[2]));
            Rotation rotation = (Rotation)Int32.Parse(split[3]);
            List<Vector2> points = new List<Vector2>();

            for (int i = 4; i < split.Length; i += 2)
                points.Add(new Vector2(float.Parse(split[i]), float.Parse(split[i + 1])));

            AddJunction(id, pos, rotation, points.ToArray());
        }

        public void AddJunction(int id, Vector2 pos, Rotation rotation, Vector2[] points)
        {
            junctions.Add(junctionTypes[id].Create(pos, rotation));
        }

        public TrackLogic()
        {
            junctions = new List<Junction>();
            junctionTypes = new List<JunctionType>();
            streetTypes = new List<Vector2>();
        }

        public void AddJunctionType(Vector2 size, Direction[] directions)
        {
            junctionTypes.Add(new JunctionType(size, directions));
        }

        public void AddStreetType(Vector2 size)
        {
            streetTypes.Add(size);
        }

        // laduje trase z pliku
        public bool LoadTrack(string path)
        {
            path = "tracks\\" + path;

            if (File.Exists(path))
            {
                junctions.Clear();

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                string s_object = "";

                while ((s_object = sr.ReadLine()) != null)
                {
                    AddJunction(s_object);
                }

                return true;
            }

            return false;
        }
    }
}
