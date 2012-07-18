﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace Testy_mapy
{
    enum Rotation {rot0 = 0, rot90 = 1, rot180 = 2, rot270 = 3}

    public struct Connection
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

        // zwraca true jezeli obydwa punkty maja (0,0)
        public bool IsEmpty()
        {
            if (point1 == Vector2.Zero && point2 == Vector2.Zero)
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
        private Vector2[] standartExitsPoint; // standartowe 4 punkty wychodzenia tras względem środka

        protected JunctionType()
        {

        }

        public Direction[] GetDirections()
        {
            return directions;
        }

        public JunctionType(Vector2 size, Direction[] directions, int id)
        {
            this.size = size;
            this.origin = size / 2;
            this.directions = directions;
            this.name = "junction" + id.ToString();

            standartExitsPoint = new Vector2[4];
            standartExitsPoint[0] = new Vector2(0, -origin.Y); // GORA
            standartExitsPoint[1] = new Vector2(origin.X, 0); // PRAWO
            standartExitsPoint[2] = new Vector2(0, origin.Y); // DOL
            standartExitsPoint[3] = new Vector2(-origin.X, 0); // LEWO
        }

        private Connection[] ComputeConnections(Rotation rotation)
        {
            Connection[] newConnections = new Connection[directions.Length];

            int shift = (int)rotation;

            for (int i = 0; i < directions.Length; ++i)
            {
                int index = (int)directions[i] + shift;

                if (index > 3)
                    index -= 4;

                newConnections[i] = new Connection(standartExitsPoint[index]);
            }

            return newConnections;
        }

        public Junction Create(Vector2 pos, Rotation rotation, int id)
        {
            // raport pelikana film obejrzeć
            return new Junction(id, this.name, pos, this.origin, this.size, (int)rotation * 90, ComputeConnections(rotation));
        }
    }

    class Junction
    {
        // NIE tworzyć obiektów (wykorzystać do tego funkcje JunctionType.Create())
        public Junction(int id, string name, Vector2 pos, Vector2 origin, Vector2 size, float rotation, Connection[] connections)
        {
            this.id = id;
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
        public readonly int id; // typ skrzyzowania
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
                // id 0 to typowa trasa. ustalamy ile razy czesciej ma sie losowac wlasnie ona
                int id = rand.Next(0, amountOfStreets + 1);
                if (id == amountOfStreets)
                    id = 0;

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

        // sprawdza czy dany punkt nalezy do skrzyzowania
        private bool ContainEndPoint(Junction junction, Vector2 endPoint)
        {
            foreach (Connection connection in junction.connections)
            {
                if (connection.point1 == endPoint) // point1 to punkt wyjścia
                    return true;
            }

            return false;
        }

        // pobiera skrzyzowania z obszaru wiekszego od wielkosci ekranu, mniejszego od wielkosci ekranu + zadanego
        private List<Junction> GetJunctionsFromArea(Vector2 size)
        {
            List<Junction> junctionsFromArea = new List<Junction>();

            foreach (Junction junction in junctions)
            {
                Vector2 distance = new Vector2();
                distance.X = Math.Abs(Helper.mapPos.X - junction.pos.X) - junction.origin.X;
                distance.Y = Math.Abs(Helper.mapPos.Y - junction.pos.Y) - junction.origin.Y;

                Vector2 halfScreenSize = Helper.screenSize / 2;

                // sprawdzamy czy dane skrzyzowanie jest w naszym zadanym obszarze dla szerokosci
                if (distance.X > halfScreenSize.X && distance.X < halfScreenSize.X + size.X
                        && distance.Y < halfScreenSize.Y + size.Y)
                {
                    junctionsFromArea.Add(junction);
                    continue;
                }

                // j.w. dla wysokosci
                if (distance.Y > halfScreenSize.Y && distance.Y < halfScreenSize.Y + size.Y
                        && distance.X < halfScreenSize.X + size.X)
                {
                    junctionsFromArea.Add(junction);
                    continue;
                }
            }

            return junctionsFromArea;
        }

        // zwraca umiejscowienie skrzyzowania wzgledem obszaru ekranu
        private Position GetJunctionPosition(Vector2 junctionPos, Vector2 junctionOrigin)
        {
            Position position_x, position_y; // pozycja wzgledem obszaru ekranu

            Vector2 distance = Helper.mapPos - junctionPos; // dystans pomiedzy srodkiem mapy i srodkiem skrzyzowania
            Vector2 halfScreenSize = Helper.screenSize / 2;
            bool d_x = false, d_y = false; // czy skrzyzowanie wykracza poza ekran na szerokosc (d_x) i wysokosc (d_y)

            if (Math.Abs(distance.X) > halfScreenSize.X + junctionOrigin.X)
                d_x = true;
            if (Math.Abs(distance.Y) > halfScreenSize.Y + junctionOrigin.Y)
                d_y = true;

            if (distance.X > 0)
                position_x = Position.left;
            else
                position_x = Position.right;

            if (distance.Y > 0)
                position_y = Position.up;
            else
                position_y = Position.down;

            if (d_x && d_y)
            {
                Position position;

                if (position_y == Position.up)
                    position = position_x + 1;
                else
                    position = position_x - 1;

                return position;
            }
            else if (d_x)
            {
                return position_x;
            }
            else
            {
                return position_y;
            }
        }

        /* zwraca polaczenie zgodnie z polozeniem skrzyzowania wzgledem ekranu (zwraca polaczenie w strone ekranu)
         jezeli takiego polaczenia nie ma zwraca puste polaczenie */
        private Connection GetConnectionFromPosition(Junction junction, Position position)
        {
            Direction[] directions = junctionTypes[junction.id].GetDirections(); // kierunki bez rotacji

            int shift = (int)junction.rotation / 90; // przesuniecie kierunkow

            for (int i = 0; i < directions.Length; ++i)
            {
                // obliczamy kierunki wychodzenia tras z skrzyzowania po rotacji
                Direction newDirection = directions[i] + shift;

                if ((int)newDirection > 3)
                    newDirection -= 4;

                // sprawdzamy czy skrzyzowanie posiada trase wychodzaca w kierunku ekranu
                switch (position)
                {
                    case Position.down:
                        if (newDirection == Direction.Up)
                            return junction.connections[i];
                        break;
                    case Position.up:
                        if (newDirection == Direction.Down)
                            return junction.connections[i];
                        break;
                    case Position.left:
                        if (newDirection == Direction.Right)
                            return junction.connections[i];
                        break;
                    case Position.right:
                        if (newDirection == Direction.Left)
                            return junction.connections[i];
                        break;
                    case Position.upLeft:
                        if (newDirection == Direction.Right || newDirection == Direction.Down)
                            return junction.connections[i];
                        break;
                    case Position.upRight:
                        if (newDirection == Direction.Left || newDirection == Direction.Down)
                            return junction.connections[i];
                        break;
                    case Position.downRight:
                        if (newDirection == Direction.Left || newDirection == Direction.Up)
                            return junction.connections[i];
                        break;
                    case Position.downLeft:
                        if (newDirection == Direction.Right || newDirection == Direction.Up)
                            return junction.connections[i];
                        break;
                }
            }

            return new Connection();
        }

        private Junction SearchJunctionFromEndPoint(Vector2 endPoint)
        {
            foreach (Junction junction in junctions)
            {
                if (ContainEndPoint(junction, endPoint))
                    return junction;
            }

            return null;
        }

        public void AddJunction(int id, Vector2 pos, Rotation rotation, Vector2[] points)
        {
            Junction junction = junctionTypes[id].Create(pos, rotation, id);

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

        // zwraca liste obiektow (skrzyzowan i ulic) do dodania do obiektow mapy (w celu pozniejszego wyswietlania)
        public List<Object> GetJunctions()
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
                    if (!s_object.StartsWith("//"))
                        AddJunction(s_object);
                }

                return true;
            }

            return false;
        }

        // size określa o ile od krawędzi mapy może być oddalone skrzyżowanie
        public Connection CreateTrack(Vector2 size)
        {
            List<Junction> junctionsFromArea = GetJunctionsFromArea(size);

            if (junctionsFromArea.Count == 0)
                return new Connection();
                
            Junction junction = junctionsFromArea[rand.Next(junctionsFromArea.Count)];

            Position position = GetJunctionPosition(junction.pos, junction.origin);

            Connection newTrack = GetConnectionFromPosition(junction, position);

            if (newTrack.IsEmpty())
            {
                int i = 0; // licznik
                while (i < 5) // zwiekszyc jezeli chcemy zwiekszyc szanse na zwrocenie drogi (niekoniecznie zwroconej w strone ekranu)
                {
                    newTrack = junction.connections[rand.Next(junction.connections.Length)];
                    ++i;

                    if (newTrack.point2 != Vector2.Zero)
                    {
                        return newTrack;
                    }
                }
            }
            else if (newTrack.point2 != Vector2.Zero)
            {
                return newTrack;
            }

            return new Connection();
        }

        public void ChangeTrack(Vector2 endPoint, out Connection connection, out Vector2 origin)
        {
            Junction junction = SearchJunctionFromEndPoint(endPoint);

            if (junction != null)
            {
                Connection[] possibleConnections = new Connection[junction.connections.Length - 1];
                int i = 0; // licznik

                foreach (Connection c in junction.connections)
                {
                    if (c.point1 != endPoint && c.point2 != new Vector2(0, 0))
                    {
                        possibleConnections[i] = c;
                        ++i;
                    }
                }

                connection = possibleConnections[rand.Next(possibleConnections.Length)];
                origin = junction.pos;
            }
            else
            {
                connection = new Connection();
                origin = Vector2.Zero;
            }
        }
    }
}
