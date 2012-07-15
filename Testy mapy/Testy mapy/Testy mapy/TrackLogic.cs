using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace Testy_mapy
{
    enum Rotation {rot0 = 0, rot90 = 1, rot180 = 2, rot270 = 3}
    enum Location { vertical, horizontal } // polozenie ulicy (pionowe lub poziome)

    struct Connection
    {
        public readonly Vector2 point1; // punkt wyjscia z jednego skrzyzowania (tylko do odczytu)
        public Vector2 point2; // !!! -||- z drugiego skrzyżowania(jezeli (0,0) to znaczy, ze nie zainicjowano tego punktu)

        public Connection(Vector2 point1)
        {
            this.point1 = point1;
            point2 = Vector2.Zero;
        }

        public Vector2 DifferenceDistance()
        {
            return new Vector2(Math.Abs(point2.X - point1.X), Math.Abs(point2.Y - point1.Y));
        }

        public bool IsPoint1HigherThanPoint2()
        {
            if (point1.X > point2.X)
                return true;
            else if (point1.Y > point2.Y)
                return true;
            else
                return false;
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

            for (int i = 0; i < connections.Length; ++i)
                connections[i] = new Connection(connections[i].point1 + pos);

            this.connections = connections;
        }

        public readonly float rotation; // 0 - 0 stopni, 1 - 90 stopni, 2 - 180 stopni, 3 - 270 stopni
        public readonly Vector2 pos; // pozycja srodka na mapie
        public readonly Vector2 origin; // srodek skrzyzowania
        public readonly Vector2 size; // wielkosc skrzyzowania
        public readonly Connection[] connections; // polaczenia
        public readonly string name; // nazwa
    }

    class Street
    {
        public readonly Vector2 pos;
        public readonly Vector2 origin;
        public readonly Vector2 size;
        public readonly string name;
        public readonly Location location;
        public readonly float rotation;

        public Street(int id, Vector2 pos, Vector2 size, Location location)
        {
            this.name = "street" + id.ToString();
            this.pos = pos;
            this.size = size;
            this.location = location;
            this.rotation = (int)location * 90;
            this.origin = size / 2;
        }
    }

    class TrackLogic
    {
        List<JunctionType> junctionTypes;
        List<Junction> junctions;
        List<Street> streets;
        List<Connection> connections;
        Vector2 streetSize;
        Vector2 streetOrigin;
        int amountOfStreets;
        Random rand;

        public TrackLogic()
        {
            junctions = new List<Junction>();
            junctionTypes = new List<JunctionType>();
            streets = new List<Street>();
            connections = new List<Connection>();

            rand = new Random();
        }

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

        private void GenerateStreet(Connection connection)
        {
            // można dodać metodę pozwalająca generować drogę pod każdym kątem
            Vector2 differenceDistance = connection.DifferenceDistance();
            
            Location location; // polozenie ulic (poziome lub pionowe)
            float startPosition; // startowa pozycja dodawania ulic
            int numberOfStreets; // ilosc ulic pomiedzy polaczeniem

            if ((numberOfStreets = (int)(differenceDistance.X / streetSize.Y)) != 0)
            {
                location = Location.horizontal;
                startPosition = connection.point1.X;
            }
            else if ((numberOfStreets = (int)(differenceDistance.Y / streetSize.Y)) != 0)
            {
                location = Location.vertical;
                startPosition = connection.point1.Y;
            }
            else
            {
                return;
            }

            Vector2 pos = (location == Location.horizontal) ? new Vector2(0, connection.point1.Y) 
                    : new Vector2(connection.point1.X, 0);

            for (int i = 0; i < numberOfStreets; ++i)
            {
                int id = rand.Next(0, amountOfStreets);

                float f_pos = startPosition + streetOrigin.Y + streetSize.Y * i;
                if (location == Location.horizontal)
                    pos.X = f_pos;
                else
                    pos.Y = f_pos;

                streets.Add(new Street(id, pos, streetSize, location));
            }
        }

        private void AddConnections(Junction junction)
        {
            foreach (Connection connection in junction.connections)
            {
                if (!ContainConnection(connection) && connection.point1 != Vector2.Zero && connection.point2 != Vector2.Zero)
                {
                    Connection c = connection;

                    if (connection.IsPoint1HigherThanPoint2())
                    {
                        c = new Connection(connection.point2);
                        c.point2 = connection.point1;
                    }

                    connections.Add(c);
                    GenerateStreet(c);
                }
            }
        }

        private bool ContainConnection(Connection connection)
        {
            for (int i = 0; i < connections.Count; ++i)
            {
                if ((connections[i].point1 == connection.point2 || connections[i].point1 == connection.point1)
                    && (connections[i].point2 == connection.point1 || connections[i].point2 == connection.point2))
                    return true;
            }

            return false;
        }

        public void AddJunction(int id, Vector2 pos, Rotation rotation, Vector2[] points)
        {
            Junction junction = junctionTypes[id].Create(pos, rotation);

            for (int i = 0; i < points.Length; ++i)
            {
                junction.connections[i].point2 = points[i];
            }

            AddConnections(junction);
            junctions.Add(junction);
        }

        public void AddJunctionType(Vector2 size, Direction[] directions)
        {
            junctionTypes.Add(new JunctionType(size, directions, junctionTypes.Count));
        }

        public List<Object> getObjects()
        {
            List<Object> objects = new List<Object>();

            foreach (Street street in streets)
            {
                objects.Add(new Object(street.name, street.pos, street.size, street.rotation, false));
            }
            foreach (Junction junction in junctions)
            {
                objects.Add(new Object(junction.name, junction.pos, junction.size, junction.rotation, false));
            }

            return objects;
        }

        // nalezy wywolac ZAWSZE po wczytaniu tekstur ulic
        public void SetStreetSize(Vector2 streetSize, int amountOfStreets)
        {
            this.streetSize = streetSize;
            this.streetOrigin = streetSize / 2;
            this.amountOfStreets = amountOfStreets;
        }

        // laduje trase z pliku
        public bool LoadTrack(string path)
        {
            path = "tracks\\" + path;

            if (File.Exists(path))
            {
                junctions.Clear();
                streets.Clear();
                connections.Clear();

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
