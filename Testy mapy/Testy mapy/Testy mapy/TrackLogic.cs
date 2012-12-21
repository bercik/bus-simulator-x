using System;
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
            return IsPoint1HigherThanPoint2(point1, point2);
        }

        public static bool IsPoint1HigherThanPoint2(Vector2 point1, Vector2 point2)
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
        private TrafficLightBasicPair trafficLightsBasicPairs; // standartowe pary swiatel dla danego skrzyzowania

        protected JunctionType()
        {

        }

        public Direction[] GetDirections()
        {
            return directions;
        }

        public Direction[] GetDirectionsAfterRotation(int shift)
        {
            Direction[] directionsAfterRotation = new Direction[directions.Length];

            for (int i = 0; i < directions.Length; ++i)
            {
                directionsAfterRotation[i] = (Direction)((int)(directions[i] + shift) % 4);
            }

            return directionsAfterRotation;
        }

        public JunctionType(Vector2 size, Direction[] directions, int id, Vector2 trafficLightSize)
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

            trafficLightsBasicPairs = new TrafficLightBasicPair(directions, size, trafficLightSize);
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

        public TrafficLightJunction CreateTrafficLight(Vector2 pos, Rotation rotation, int id, int redLightIntervalPair1, int redLightIntervalPair2)
        {
            float f_rotation = (int)rotation * 90;

            TrafficLightObjectInformation[] trafficLightObjectsPair1 = new TrafficLightObjectInformation[trafficLightsBasicPairs.pair1.Length];
            for (int i = 0; i < trafficLightsBasicPairs.pair1.Length; ++i)
            {
                trafficLightObjectsPair1[i] = trafficLightsBasicPairs.pair1[i].CreateNewObjectAfterRotationAndMove(f_rotation, pos);
            }

            TrafficLightObjectInformation[] trafficLightObjectsPair2 = new TrafficLightObjectInformation[trafficLightsBasicPairs.pair2.Length];
            for (int i = 0; i < trafficLightsBasicPairs.pair2.Length; ++i)
            {
                trafficLightObjectsPair2[i] = trafficLightsBasicPairs.pair2[i].CreateNewObjectAfterRotationAndMove(f_rotation, pos);
            }

            TrafficLightPair trafficLightPair1 = new TrafficLightPair(redLightIntervalPair1, trafficLightObjectsPair1, TrafficLightState.green);
            TrafficLightPair trafficLightPair2 = new TrafficLightPair(redLightIntervalPair2, trafficLightObjectsPair2, TrafficLightState.red);

            float trafficLightIntervalBeforeRedYellowStart = 0.0f;

            if (size == new Vector2(300, 300))
                trafficLightIntervalBeforeRedYellowStart = GameParams.trafficLightIntervalBeforeRedYellowStart1;
            else
                trafficLightIntervalBeforeRedYellowStart = GameParams.trafficLightIntervalBeforeRedYellowStart2;

            TrafficLight trafficLight = new TrafficLight(trafficLightPair1, trafficLightPair2, trafficLightIntervalBeforeRedYellowStart);

            return new TrafficLightJunction(id, this.name, pos, this.origin, this.size, f_rotation, ComputeConnections(rotation), trafficLight);
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

        public void AddConnection(Vector2 point1, Vector2 point2)
        {
            for (int i = 0; i < connections.Length; ++i)
            {
                if (connections[i].point1 == point1)
                    connections[i].point2 = point2;
            }
        }
    }

    class TrafficLightJunction : Junction
    {
        public readonly TrafficLight trafficLight; // obiekt swiatel ulicznych

        public TrafficLightJunction(int id, string name, Vector2 pos, Vector2 origin, Vector2 size, float rotation, Connection[] connections, TrafficLight trafficLight)
            : base(id, name, pos, origin, size, rotation, connections)
        {
            this.trafficLight = trafficLight;
        }
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

    class Sidewalk
    {
        public readonly Vector2 pos; // pozycja
        public readonly Vector2 size; // wielkosc calego chodnika
        public readonly Vector2 oneSidewalkSize; // wielkosc tekstury jednego chodnika
        public readonly Vector2 origin; // srodek wzgl lewego gornego rogu
        public readonly int id; // id
        public readonly string name; // nazwa
        public readonly Location location; // polozenie (poziome, pionowe)
        public readonly float rotation; // rotacja

        public Sidewalk(Vector2 pos, Vector2 size, Vector2 oneSidewalkSize, int id, Location location)
        {
            this.pos = pos;
            this.size = size;
            this.oneSidewalkSize = oneSidewalkSize;
            this.origin = size / 2;
            this.id = id;
            this.name = "chodnik" + id.ToString();
            this.location = location;
            this.rotation = (int)location * 90;
        }

        public List<Object> GetSidewalksToShow()
        {
            if (id != -1)
            {
                List<Object> sidewalks = new List<Object>();

                float startPos; // startowa pozycja chodnikow
                int numberOfSidewalks; // ilosc chodnikow

                startPos = (location == Location.horizontal) ? this.pos.X - origin.Y : this.pos.Y - origin.Y;
                numberOfSidewalks = (int)(size.Y / oneSidewalkSize.Y);

                Vector2 oneSidewalkOrigin = oneSidewalkSize / 2; // srodek jednego chodnika
                Vector2 pos = new Vector2(this.pos.X, this.pos.Y); // pozycja dodawanego chodnika

                for (int i = 0; i < numberOfSidewalks; ++i)
                {
                    float f_pos = startPos + oneSidewalkOrigin.Y + oneSidewalkSize.Y * i; // pozycja aktualnego chodnika

                    if (location == Location.horizontal)
                        pos.X = f_pos;
                    else
                        pos.Y = f_pos;

                    sidewalks.Add(new Object(name, pos, oneSidewalkSize, rotation));
                }

                return sidewalks;
            }
            else
            {
                return new List<Object>();
            }
        }
    }

    class TrackLogic
    {
        TrafficLightsLogic trafficLightsLogic;

        List<JunctionType> junctionTypes;
        List<Junction> junctions;
        List<Street> streets;
        List<Sidewalk> sidewalks;
        List<Connection> connections;
        Vector2 streetSize;
        Vector2 streetOrigin;
        Vector2 sidewalkSize;
        Vector2 sidewalkOrigin;
        int amountOfStreets;
        Random rand;

        public TrackLogic()
        {
            trafficLightsLogic = new TrafficLightsLogic();

            junctions = new List<Junction>();
            junctionTypes = new List<JunctionType>();
            streets = new List<Street>();
            sidewalks = new List<Sidewalk>();
            connections = new List<Connection>();

            rand = new Random();
        }

        public void Update(TimeSpan framesInterval)
        {
            trafficLightsLogic.Update(framesInterval);
        }

        private void AddJunction(string s_object)
        {
            string[] split = s_object.Split(';');

            int id = Int32.Parse(split[0]);
            Vector2 pos = new Vector2(float.Parse(split[1]), float.Parse(split[2]));
            Rotation rotation = (Rotation)Int32.Parse(split[3]);
            bool trafficLights = Convert.ToBoolean(Int32.Parse(split[4]));

            if (trafficLights) // jezeli skrzyzowanie zawiera swiatla uliczne
            {
                int redLightIntervalPair1 = Int32.Parse(split[5]);
                if (redLightIntervalPair1 == 0)
                    redLightIntervalPair1 = GameParams.standartRedLightInterval;

                int redLightIntervalPair2 = Int32.Parse(split[6]);
                if (redLightIntervalPair2 == 0)
                    redLightIntervalPair2 = GameParams.standartRedLightInterval;

                AddTrafficLightJunction(id, pos, rotation, redLightIntervalPair1, redLightIntervalPair2);
            }
            else // inaczej
            {
                AddJunction(id, pos, rotation);
            }
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

        private void GenerateSidewalk(Connection connection, int id)
        {
            Vector2 differenceDistance = connection.DifferenceDistance(); // roznica odleglosci

            Location location = (differenceDistance.X != 0) ? Location.horizontal : Location.vertical; // polozenie (poziome lub pionowe)

            Vector2 size = new Vector2(sidewalkSize.X, (location == Location.horizontal) ? differenceDistance.X : differenceDistance.Y);
            
            Vector2 pos1, pos2; // pozycje chodnikow
            float differentPos = GameParams.streetWidth + sidewalkOrigin.X;

            if (location == Location.horizontal)
            {
                pos1 = new Vector2(connection.point1.X + size.Y / 2, connection.point1.Y + differentPos);
                pos2 = new Vector2(connection.point1.X + size.Y / 2, connection.point1.Y - differentPos);
            }
            else
            {
                pos1 = new Vector2(connection.point1.X + differentPos, connection.point1.Y + size.Y / 2);
                pos2 = new Vector2(connection.point1.X - differentPos, connection.point1.Y + size.Y / 2);
            }

            sidewalks.Add(new Sidewalk(pos1, size, sidewalkSize, id, location));
            sidewalks.Add(new Sidewalk(pos2, size, sidewalkSize, id, location));
        }

        // sprawdza czy dany punkt nalezy do skrzyzowania
        private bool ContainEndPoint(Junction junction, Vector2 endPoint, Vector2 lastEndPoint)
        {
            bool containsEndPoint = false;
            bool containsLastEndPoint = false;

            foreach (Connection connection in junction.connections)
            {
                if (!containsEndPoint && connection.point1 == endPoint) // point1 to punkt wyjścia
                    containsEndPoint = true;
                if (!containsLastEndPoint && connection.point1 == lastEndPoint) // point1 to punkt wjscia
                    containsLastEndPoint = true;
            }

            if (containsEndPoint && !containsLastEndPoint)
                return true;
            else
                return false;
        }

        private bool ContainEndPoint(Junction junction, Vector2 endPoint)
        {
            foreach (Connection connection in junction.connections)
            {
                if (connection.point1 == endPoint) // point1 to punkt wyjscia
                    return true;
            }

            return false;
        }

        // pobiera skrzyzowania z obszaru wiekszego od wielkosci ekranu, mniejszego od wielkosci ekranu + zadanego
        private List<Junction> GetJunctionsFromArea(Vector2 size, float carLength)
        {
            List<Junction> junctionsFromArea = new List<Junction>();

            foreach (Junction junction in junctions)
            {
                Vector2 distance = new Vector2();
                distance.X = Math.Abs(Helper.mapPos.X - junction.pos.X) - junction.origin.X - (carLength / 2);
                distance.Y = Math.Abs(Helper.mapPos.Y - junction.pos.Y) - junction.origin.Y - (carLength / 2);

                // sprawdzamy czy dane skrzyzowanie jest w naszym zadanym obszarze dla szerokosci
                if (distance.X > Helper.workAreaOrigin.X && distance.X < Helper.workAreaOrigin.X + size.X
                        && distance.Y < Helper.workAreaOrigin.Y + size.Y)
                {
                    junctionsFromArea.Add(junction);
                    continue;
                }

                // j.w. dla wysokosci
                if (distance.Y > Helper.workAreaOrigin.Y && distance.Y < Helper.workAreaOrigin.Y + size.Y
                        && distance.X < Helper.workAreaOrigin.X + size.X)
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
        private Connection GetConnectionFromPosition(Junction junction, Position position, out int connectionIndex)
        {
            int shift = (int)junction.rotation / 90; // przesuniecie kierunkow
            Direction[] directions = junctionTypes[junction.id].GetDirectionsAfterRotation(shift); // kierunki po rotacji

            for (int i = 0; i < directions.Length; ++i)
            {
                connectionIndex = -1;

                // sprawdzamy czy skrzyzowanie posiada trase wychodzaca w kierunku ekranu
                switch (position)
                {
                    case Position.down:
                        if (directions[i] == Direction.Up)
                        {
                            connectionIndex = i;
                            return junction.connections[i];
                        }
                        break;
                    case Position.up:
                        if (directions[i] == Direction.Down)
                        {
                            connectionIndex = i;
                            return junction.connections[i];
                        }
                        break;
                    case Position.left:
                        if (directions[i] == Direction.Right)
                        {
                            connectionIndex = i;
                            return junction.connections[i];
                        }
                        break;
                    case Position.right:
                        if (directions[i] == Direction.Left)
                        {
                            connectionIndex = i;
                            return junction.connections[i];
                        }
                        break;
                    case Position.upLeft:
                        if (directions[i] == Direction.Right || directions[i] == Direction.Down)
                        {
                            connectionIndex = i;
                            return junction.connections[i];
                        }
                        break;
                    case Position.upRight:
                        if (directions[i] == Direction.Left || directions[i] == Direction.Down)
                        {
                            connectionIndex = i;
                            return junction.connections[i];
                        }
                        break;
                    case Position.downRight:
                        if (directions[i] == Direction.Left || directions[i] == Direction.Up)
                        {
                            connectionIndex = i;
                            return junction.connections[i];
                        }
                        break;
                    case Position.downLeft:
                        if (directions[i] == Direction.Right || directions[i] == Direction.Up)
                        {
                            connectionIndex = i;
                            return junction.connections[i];
                        }
                        break;
                }
            }

            connectionIndex = -1;
            return new Connection();
        }

        private Junction SearchJunctionFromEndPoint(Vector2 endPoint, Vector2 lastEndPoint)
        {
            foreach (Junction junction in junctions)
            {
                if (ContainEndPoint(junction, endPoint, lastEndPoint))
                    return junction;
            }

            return null;
        }

        private int SearchJunctionFromEndPoint(Vector2 endPoint)
        {
            for (int i = 0; i < junctions.Count; ++i)
            {
                if (ContainEndPoint(junctions[i], endPoint))
                    return i;
            }

            return -1;
        }

        private void SearchTwoJunctionsFromEndPoint(Vector2 endPoint, out int id1, out int id2)
        {
            id1 = -1;
            id2 = -1;

            for (int i = 0; i < junctions.Count; ++i)
            {
                if (ContainEndPoint(junctions[i], endPoint))
                {
                    if (id1 == -1)
                    {
                        id1 = i;
                    }
                    else
                    {
                        id2 = i;
                        break;
                    }
                }
            }
        }

        private void AddConnection(string s_object)
        {
            string[] split = s_object.Split(new char[] { ';' });

            Vector2 pos1 = new Vector2(float.Parse(split[0]), float.Parse(split[1]));
            Vector2 pos2 = new Vector2(float.Parse(split[2]), float.Parse(split[3]));
            int id = Int32.Parse(split[4]);

            AddConnection(pos1, pos2, id);
        }

        private Vector2 GetRandomJunctionExitWithout(Junction junction, int connectionIndex)
        {
            if (connectionIndex == 0)
                return junction.connections[1].point1;
            else
                return junction.connections[0].point1;
        }

        public void AddConnection(Vector2 pos1, Vector2 pos2, int sidewalkId)
        {
            if (pos1 == pos2) // jezeli skrzyzowania bezposrednio sie lacza
            {
                int id1, id2;

                SearchTwoJunctionsFromEndPoint(pos1, out id1, out id2);

                junctions[id1].AddConnection(pos1, pos2);
                junctions[id2].AddConnection(pos1, pos2);

                sidewalkId = -1;
            }
            else
            {
                int id = SearchJunctionFromEndPoint(pos1); // szukamy pierwszego skrzyzowania
                junctions[id].AddConnection(pos1, pos2);

                id = SearchJunctionFromEndPoint(pos2); // szukamy drugiego skrzyzowania
                junctions[id].AddConnection(pos2, pos1);
            }

            Connection c; // ustawiamy tak, zeby punkt o nizszych wspolrzednych byl na poczatku

            if (Connection.IsPoint1HigherThanPoint2(pos1, pos2))
            {
                c = new Connection(pos2);
                c.point2 = pos1;
            }
            else
            {
                c = new Connection(pos1);
                c.point2 = pos2;
            }

            connections.Add(c);
            GenerateStreet(c);

            if (sidewalkId != -1)
                GenerateSidewalk(c, sidewalkId);
        }

        public void AddJunction(int id, Vector2 pos, Rotation rotation)
        {
            Junction junction = junctionTypes[id].Create(pos, rotation, id);

            junctions.Add(junction);
        }

        public void AddTrafficLightJunction(int id, Vector2 pos, Rotation rotation, int redLightIntervalPair1, int redLightIntervalPair2)
        {
            TrafficLightJunction trafficLightJunction = junctionTypes[id].CreateTrafficLight(pos, rotation, id, redLightIntervalPair1, redLightIntervalPair2);

            junctions.Add(trafficLightJunction);
            trafficLightsLogic.AddTrafficLightJunction(trafficLightJunction);
        }

        public void AddJunctionType(Vector2 size, Direction[] directions)
        {
            junctionTypes.Add(new JunctionType(size, directions, junctionTypes.Count, trafficLightsLogic.trafficLightSize));
        }

        // zwraca liste obiektow (skrzyzowan i ulic) do dodania do obiektow mapy (w celu pozniejszego wyswietlania)
        public List<Object> GetJunctions()
        {
            List<Object> objects = new List<Object>();

            foreach (Street street in streets)
            {
                objects.Add(new Object(street.name, street.pos, street.size, street.rotation));
            }
            foreach (Junction junction in junctions)
            {
                objects.Add(new Object(junction.name, junction.pos, junction.size, junction.rotation));
            }
            foreach (Sidewalk sidewalk in sidewalks)
            {
                objects.AddRange(sidewalk.GetSidewalksToShow());
            }

            return objects;
        }

        /// <summary>
        /// Zwraca liste swiatel ulicznych do dodania do obiektow mapy (w celu pozniejszego wyswietlenia)
        /// </summary>
        /// <returns></returns>
        public List<TrafficLightObject> GetTrafficLights()
        {
            return trafficLightsLogic.GetTrafficLights();
        }

        public TrafficLightState GetTrafficLightPairState(int junctionIndex, int pairIndex)
        {
            return trafficLightsLogic.GetTrafficLightPairState(junctionIndex, pairIndex);
        }

        // nalezy wywolac ZAWSZE po wczytaniu tekstur ulic
        public void SetStreetSize(Vector2 streetSize, int amountOfStreets)
        {
            this.streetSize = streetSize;
            this.streetOrigin = streetSize / 2;
            this.amountOfStreets = amountOfStreets;
        }

        // nalezy wywolac ZAWSZE po wczytaniu tekstur chodnikow
        public void SetSidewalkSize(Vector2 sidewalkSize)
        {
            this.sidewalkSize = sidewalkSize;
            this.sidewalkOrigin = sidewalkSize / 2;
        }

        // zwraca liste chodnikow
        public List<Sidewalk> GetSidewalks()
        {
            return sidewalks;
        }

        // wywolac ZAWSZE po wczytaniu tekstur swiatel ulicznych
        public void SetTrafficLightSize(Vector2 trafficLightSize)
        {
            trafficLightsLogic.SetTrafficLightSize(trafficLightSize);
        }

        // laduje trase z pliku
        public void LoadTrack(ref StreamReader sr)
        {
            trafficLightsLogic.ClearTrafficLightJunctions();

            junctions.Clear();
            streets.Clear();
            sidewalks.Clear();
            connections.Clear();

            string s_object = sr.ReadLine(); // !!! ta linia to komentarz (DO USUNIECIA) !!!

            while ((s_object = sr.ReadLine()) != null && s_object[0] != '*')
            {
                 AddJunction(s_object);
            }

            s_object = sr.ReadLine(); // !!! ta linia to komentarz (DO USUNIECIA) !!!

            while ((s_object = sr.ReadLine()) != null && s_object[0] != '*')
            {
                AddConnection(s_object);
            }
        }

        // size określa o ile od krawędzi mapy może być oddalone skrzyżowanie
        public void CreateTrack(Vector2 size, float carLength, out Connection connection, out Vector2 origin, out Vector2 randomOutPoint)
        {
            List<Junction> junctionsFromArea = GetJunctionsFromArea(size, carLength);

            if (junctionsFromArea.Count == 0)
            {
                connection = new Connection();
                origin = new Vector2(0, 0);
                randomOutPoint = new Vector2(0, 0);
                return;
            }

            int connectionIndex = -1;

            Junction junction = junctionsFromArea[rand.Next(junctionsFromArea.Count)];

            Position position = GetJunctionPosition(junction.pos, junction.origin);

            Connection newTrack = GetConnectionFromPosition(junction, position, out connectionIndex);

            if (newTrack.IsEmpty())
            {
                int i = 0; // licznik
                while (i < 5) // zwiekszyc jezeli chcemy zwiekszyc szanse na zwrocenie drogi (niekoniecznie zwroconej w strone ekranu)
                {
                    connectionIndex = rand.Next(junction.connections.Length);
                    newTrack = junction.connections[connectionIndex];
                    ++i;

                    if (newTrack.point2 != Vector2.Zero)
                    {
                        connection = newTrack;
                        origin = junction.pos;
                        randomOutPoint = GetRandomJunctionExitWithout(junction, connectionIndex);
                        return;
                    }
                }
            }
            else if (newTrack.point2 != Vector2.Zero)
            {
                connection = newTrack;
                origin = junction.pos;
                randomOutPoint = GetRandomJunctionExitWithout(junction, connectionIndex);
                return;
            }

            connection = new Connection();
            origin = new Vector2(0, 0);
            randomOutPoint = new Vector2(0, 0);
        }

        public void ChangeTrack(Vector2 endPoint, Vector2 lastEndPoint, out Connection connection, out Vector2 origin)
        {
            Junction junction = SearchJunctionFromEndPoint(endPoint, lastEndPoint);

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

        public void GetRedLightRectangles(out List<Rectangle> redLightRectanglesForCars, out List<TrafficLightRectangle> redLightRectanglesForBus)
        {
            trafficLightsLogic.GetRedLightRectangles(out redLightRectanglesForCars, out redLightRectanglesForBus);
        }
    }
}
