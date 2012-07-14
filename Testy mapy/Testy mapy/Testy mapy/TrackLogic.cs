using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace Testy_mapy
{
    enum Rotation {rot0 = 0, rot90 = 1, rot180 = 2, rot270 = 3}

    struct Connection
    {
        public readonly Vector2 point1; // punkt wyjscia z jednego skrzyzowania (tylko do odczytu)
        public Vector2 point2; // -||- z drugiego skrzyżowania(jezeli (0,0) to znaczy, ze nie zainicjowano tego punktu)

        public Connection(Vector2 point1)
        {
            this.point1 = point1;
            point2 = Vector2.Zero;
        }
    }

    class JunctionType
    {
        private Vector2 origin; // srodek skrzyzowania
        private string name;
        private Vector2 size;
        private Direction[] directions; // kierunki wychodzenia tras
        private static Vector2[] standartExitsPoint; // standartowe 4 punkty wychodzenia tras względem środka

        protected JunctionType()
        {

        }

        public JunctionType(Vector2 size, Direction[] directions, int id)
        {
            this.size = size;
            this.origin = size / 2;
            this.directions = directions;
            this.name = "junction" + id.ToString();

            if (standartExitsPoint == null)
            {
                standartExitsPoint = new Vector2[4];

                standartExitsPoint[0] = new Vector2(0, -origin.Y); // GORA
                standartExitsPoint[1] = new Vector2(origin.X, 0); // PRAWO
                standartExitsPoint[2] = new Vector2(0, origin.Y); // DOL
                standartExitsPoint[3] = new Vector2(-origin.X, 0); // LEWO
            }
        }

        private Connection[] ComputeConnections(Rotation rotation)
        {
            Connection[] newConnections = new Connection[directions.Length];

            int shift = (int)rotation;

            for (int i = 0; i < directions.Length; ++i)
            {
                directions[i] += shift;

                if ((int)directions[i] > 3)
                    directions[i] -= 4;

                newConnections[i] = new Connection(standartExitsPoint[(int)directions[i]]);
            }

            return newConnections;
        }

        public Junction Create(Vector2 pos, Rotation rotation)
        {
            // raport pelikana film obejrzeć
            return new Junction(this.name, pos, this.origin, this.size, (int)rotation * 90, ComputeConnections(rotation));
        }
    }

    class Junction
    {
        // NIE tworzyć obiektów (wykorzystać do tego funkcje JunctionType.Create())
        public Junction(string name, Vector2 pos, Vector2 origin, Vector2 size, float rotation, Connection[] connections)
        {
            this.name = name;
            this.pos = pos;
            this.origin = origin;
            this.size = size;
            this.rotation = rotation;
            this.connections = connections;
        }

        public readonly float rotation; // 0 - 0 stopni, 1 - 90 stopni, 2 - 180 stopni, 3 - 270 stopni
        public readonly Vector2 pos; // pozycja srodka na mapie
        public readonly Vector2 origin; // srodek skrzyzowania
        public readonly Vector2 size; // wielkosc skrzyzowania
        public readonly Connection[] connections; // polaczenia
        public readonly string name; // nazwa
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
            junctionTypes.Add(new JunctionType(size, directions, junctionTypes.Count));
        }

        public void AddStreetType(Vector2 size)
        {
            streetTypes.Add(size);
        }

        public List<Object> getObjects()
        {
            List<Object> objects = new List<Object>();

            foreach (Junction junction in junctions)
            {
                objects.Add(new Object(junction.name, junction.pos, junction.size, junction.rotation));
            }

            return objects;
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
