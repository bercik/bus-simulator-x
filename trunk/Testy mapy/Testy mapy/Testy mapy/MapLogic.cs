using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Testy_mapy
{
    enum ObjectType { underBus, onBus, junction, trafficLight } // typ obiektu (pod autobusem, nad autobusem, skrzyżowanie, światło uliczne)

    class MapLogic
    {
        Chunk[,] chunks;
        Size chunkSize;
        Point numberOfChunks;
        
        private Vector2 v_mapPos;
        Vector2 mapPos
        {
            get
            {
                return v_mapPos;
            }
            set
            {
                v_mapPos = value;
                Helper.mapPos = v_mapPos;
            }
        }

        Dictionary<string, Vector2> standart_size;
        Dictionary<string, ObjectInformation> objectsInformation;
        List<Object> junctionsInRange;
        List<Object> objectsUnderBusInRange;
        List<Object> objectsOnBusInRange;
        List<Object> trafficLightsInRange;

        Size maxObjectSize = new Size(300, 300); // maksymalny możliwy rozmiar obiektu
                                                         // uwzględnij rotację o 45 stopni
                                                         //(czyli przemnoż przez pierwiastek z 2)

        public MapLogic()
        {
            standart_size = new Dictionary<string, Vector2>();
            objectsInformation = new Dictionary<string, ObjectInformation>();
            junctionsInRange = new List<Object>();
            objectsUnderBusInRange = new List<Object>();
            objectsOnBusInRange = new List<Object>();
            trafficLightsInRange = new List<Object>();
        }

        /// <summary>
        /// Pobiera skrzyzowania do wyswietlenia na minimapie
        /// </summary>
        /// <param name="previewScale"></param>
        /// <returns></returns>
        public List<Object> GetJunctionsForMinimap(float minimapScale)
        {
            Vector2 scaledSize = Helper.screenSize * minimapScale;

            // x i y kawałka na którym się znajdujemy
            int a_x = (int)Helper.mapPos.X / chunkSize.Width;
            int a_y = (int)Helper.mapPos.Y / chunkSize.Height;

            // ilość kawałków jakie należy sprawdzić (nad, pod, w lewo i w prawo od aktualnego) - inaczej przesunięcie (Shift)
            int s_x = (int)Math.Ceiling((scaledSize.X / 2) / chunkSize.Width);
            int s_y = (int)Math.Ceiling((scaledSize.Y / 2) / chunkSize.Height);

            // kawałek od którego będziemy zaczynać
            int ch_x = a_x - s_x;
            if (ch_x < 0)
                ch_x = 0;

            int ch_y = a_y - s_y;
            if (ch_y < 0)
                ch_y = 0;

            List<Object> junctionsForMinimap = new List<Object>();

            for (int x = ch_x; x <= a_x + s_x; ++x)
            {
                for (int y = ch_y; y <= a_y + s_y; ++y)
                {
                    if (x < numberOfChunks.X && y < numberOfChunks.Y)
                    {
                        junctionsForMinimap.AddRange(chunks[x, y].GetJunctions());
                    }
                }
            }

            for (int i = 0; i < junctionsForMinimap.Count; ++i)
            {
                if (!(junctionsForMinimap[i].name.StartsWith("junction") 
                    || junctionsForMinimap[i].name.StartsWith("street"))) // usuwamy wszystko co nie jest skrzyżowaniem lub drogą
                {
                    junctionsForMinimap.RemoveAt(i);
                    --i;
                }
            }

            List<Object> junctionsForMinimapInRange = new List<Object>();
            GetObjectsInRangeFrom(ref junctionsForMinimapInRange, junctionsForMinimap, Helper.mapPos, scaledSize);

            for (int i = 0; i < junctionsForMinimapInRange.Count; ++i)
            {
                junctionsForMinimapInRange[i] = (Object)junctionsForMinimapInRange[i].Clone();
                junctionsForMinimapInRange[i].pos = Helper.MapPosToScreenPos(junctionsForMinimapInRange[i].pos);
            }

            return junctionsForMinimapInRange;
        }

        /// <summary>
        /// Pobiera obiekty do wyświetlenia w podglądzie
        /// </summary>
        /// <param name="previewScale"></param>
        /// <returns></returns>
        public List<Object> GetObjectsToPreview(float previewScale)
        {
            Vector2 scaledSize = Helper.screenSize * previewScale;

            // x i y kawałka na którym się znajdujemy
            int a_x = (int)Helper.mapPos.X / chunkSize.Width;
            int a_y = (int)Helper.mapPos.Y / chunkSize.Height;

            // ilość kawałków jakie należy sprawdzić (nad, pod, w lewo i w prawo od aktualnego) - inaczej przesunięcie (Shift)
            int s_x = (int)Math.Ceiling((scaledSize.X / 2) / chunkSize.Width);
            int s_y = (int)Math.Ceiling((scaledSize.Y / 2) / chunkSize.Height);

            // kawałek od którego będziemy zaczynać
            int ch_x = a_x - s_x;
            if (ch_x < 0)
                ch_x = 0;

            int ch_y = a_y - s_y;
            if (ch_y < 0)
                ch_y = 0;

            List<Object> objectsOnBusToPreview = new List<Object>();
            List<Object> objectsUnderBusToPreview = new List<Object>();
            List<Object> junctionsToPreview = new List<Object>();
            List<Object> trafficLightsToPreview = new List<Object>();

            for (int x = ch_x; x <= a_x + s_x; ++x)
            {
                for (int y = ch_y; y <= a_y + s_y; ++y)
                {
                    if (x < numberOfChunks.X && y < numberOfChunks.Y)
                    {
                        junctionsToPreview.AddRange(chunks[x, y].GetJunctions());
                        objectsUnderBusToPreview.AddRange(chunks[x, y].GetObjectsUnderBus());
                        objectsOnBusToPreview.AddRange(chunks[x, y].GetObjectsOnBus());
                        trafficLightsToPreview.AddRange(chunks[x, y].GetTrafficLights());
                    }
                }
            }

            for (int i = 0; i < junctionsToPreview.Count; ++i)
            {
                if (junctionsToPreview[i].name.StartsWith("chodnik")) // usuwamy chodniki z podglądu mapy
                {
                    junctionsToPreview.RemoveAt(i);
                    --i;
                }
            }

            List<Object> objectsToShow = new List<Object>();
            GetObjectsInRangeFrom(ref objectsToShow, junctionsToPreview, Helper.mapPos, scaledSize);
            GetObjectsInRangeFrom(ref objectsToShow, objectsUnderBusToPreview, Helper.mapPos, scaledSize);
            GetObjectsInRangeFrom(ref objectsToShow, objectsOnBusToPreview, Helper.mapPos, scaledSize);
            GetObjectsInRangeFrom(ref objectsToShow, trafficLightsToPreview, Helper.mapPos, scaledSize);

            for (int i = 0; i < objectsToShow.Count; ++i)
            {
                objectsToShow[i] = (Object)objectsToShow[i].Clone();
                objectsToShow[i].pos = Helper.MapPosToScreenPos(objectsToShow[i].pos);
            }

            return objectsToShow;
        }

        private void CreateChunks(int mapWidth, int mapHeight)
        {
            chunkSize.Width = (int)(Helper.maxWorkAreaSize.X + maxObjectSize.Width);
            chunkSize.Height = (int)(Helper.maxWorkAreaSize.Y + maxObjectSize.Height);
            numberOfChunks.X = (mapWidth / chunkSize.Width) + 1;
            numberOfChunks.Y = (mapHeight / chunkSize.Height) + 1;

            chunks = new Chunk[numberOfChunks.X, numberOfChunks.Y];

            for (int i = 0; i < numberOfChunks.X; ++i)
            {
                for (int y = 0; y < numberOfChunks.Y; ++y)
                {
                    chunks[i, y] = new Chunk();
                }
            }
        }

        private void AddObjectToChunk(string s_object)
        {
            string[] split = s_object.Split(new char[] { ';' });

            string name = split[0];
            Vector2 pos = new Vector2(float.Parse(split[1]), float.Parse(split[2]));
            Vector2 size = new Vector2(Int32.Parse(split[3]), Int32.Parse(split[4]));
            float rotate = float.Parse(split[5]);
            SpriteEffects spriteEffects = (SpriteEffects)Int32.Parse(split[6]);
            ObjectType objectType = objectsInformation[name].underBus ? ObjectType.underBus : ObjectType.onBus;
            
            AddObjectToChunk(name, pos, size, rotate, objectType, spriteEffects);
        }

        private void AddObjectToChunk(string name, Vector2 pos, Vector2 size, float rotate, ObjectType objectType,
                SpriteEffects spriteEffects = SpriteEffects.None)
        {
            if (size.X == 0)
                size.X = standart_size[name].X;
            if (size.Y == 0)
                size.Y = standart_size[name].Y;

            Object o;

            if (standart_size.ContainsKey(name))
                o = new Object(name, pos, size, rotate, standart_size[name], spriteEffects,
                    (objectsInformation.ContainsKey(name) ? objectsInformation[name].collisionRectangles : null));
            else
                o = new Object(name, pos, size, rotate, spriteEffects,
                    (objectsInformation.ContainsKey(name) ? objectsInformation[name].collisionRectangles : null));

            AddObjectToChunk(o, objectType);
        }

        public void AddObjectToChunk(Object o, ObjectType objectType)
        {
            int x = (int)o.pos.X / chunkSize.Width;
            int y = (int)o.pos.Y / chunkSize.Height;

            chunks[x, y].AddObject(o, objectType);

            // dla obiektow wiekszych niz zakladane
            //bool b_x = false, b_y = false; // czy obiekt jest wiekszy od zakladanej wielkosci (na szerokosc i dlugosc)

            if (o.size.X > maxObjectSize.Width)
            {
                //b_x = true;

                if (x + 1 < numberOfChunks.X)
                    chunks[x + 1, y].AddObject(o, objectType);
                if (x - 1 > 0)
                    chunks[x - 1, y].AddObject(o, objectType);
            }
            if (o.size.Y > maxObjectSize.Height)
            {
                //b_y = true;

                if (y + 1 < numberOfChunks.Y)
                    chunks[x, y + 1].AddObject(o, objectType);
                if (y - 1 > 0)
                    chunks[x, y - 1].AddObject(o, objectType);
            }

            /* !!!DO EW. USUNIECIA!!! jezeli wyswietlanie obiektow wiekszych od maksymalnego zakladanego bedzie dzialac
            if (b_x || b_y)
            {
                if (x + 1 < numberOfChunks.X && y + 1 < numberOfChunks.Y)
                    chunks[x + 1, y + 1].AddObject(o, objectType);
            }*/
        }

        private void ComputePointsOnRotation(Object o, out float x1, out float x2, out float y1, out float y2)
        {
            // metoda obliczajaca wspolrzedne punktow obiektu przy rotacji
            float f_rotate = o.rotate;

            if (f_rotate >= 180)
                f_rotate -= 180;

            Vector2 new_pos = o.pos - o.origin; // lewy gorny punkt obiektu we wspolrzednych mapy
            Vector2 center = o.pos; // srodek

            Vector2 pos1 = new Vector2(new_pos.X, new_pos.Y);
            Vector2 pos2 = new Vector2(new_pos.X + o.size.X, new_pos.Y);
            Vector2 pos3 = new Vector2(new_pos.X + o.size.X, new_pos.Y + o.size.Y);
            Vector2 pos4 = new Vector2(new_pos.X, new_pos.Y + o.size.Y);

            if (f_rotate <= 90)
            {
                x1 = Helper.ComputeRotationX(pos2, center, f_rotate); // 2
                x2 = Helper.ComputeRotationX(pos4, center, f_rotate); // 4

                y1 = Helper.ComputeRotationY(pos1, center, f_rotate); // 1
                y2 = Helper.ComputeRotationY(pos3, center, f_rotate); // 3
            }
            else
            {
                y1 = Helper.ComputeRotationY(pos2, center, f_rotate); // 2
                y2 = Helper.ComputeRotationY(pos4, center, f_rotate); // 4

                x1 = Helper.ComputeRotationX(pos1, center, f_rotate); // 1
                x2 = Helper.ComputeRotationX(pos3, center, f_rotate); // 3
            }
        }

        private void GetObjectsInRangeFrom(ref List<Object> objects, List<Object> objectsToCheck, Vector2 mapPos, Vector2 areaSize)
        {
            foreach (Object o in objectsToCheck)
            {
                // jezeli jest juz taki obiekt do wyswietlenia to nie dodajemy go
                if (objects.Contains(o))
                    continue;

                bool b_x = false, b_y = false;
                float x1, x2, y1, y2;

                // metoda obliczajaca wspolrzedne punktow obiektu przy rotacji
                ComputePointsOnRotation(o, out x1, out x2, out y1, out y2);

                // SPRAWDZANIE PUNKTOW
                if (SmallWidth(mapPos.X - x1, areaSize))
                    b_x = true;
                else if (SmallWidth(mapPos.X - x2, areaSize))
                    b_x = true;
                if (SmallHeight(mapPos.Y - y1, areaSize))
                    b_y = true;
                else if (SmallHeight(mapPos.Y - y2, areaSize))
                    b_y = true;


                if (!b_x) // sprawdzamy czy współrzędne X obiektu nie znajdują się pomiędzy ekranem
                {
                    if (CheckCord(mapPos.X, x1, x2, (int)areaSize.X))
                        b_x = true;
                }
                if (!b_y) // analogicznie jak wyżej dla współrzędnych Y
                {
                    if (CheckCord(mapPos.Y, y1, y2, (int)areaSize.Y))
                        b_y = true;
                }

                if (b_x && b_y)
                {
                    Object oToAdd = o;
                    oToAdd.collisionRectangles = o.collisionRectangles;

                    if (standart_size.ContainsKey(o.name))
                        oToAdd.original_origin = standart_size[o.name] / 2;
                    else
                        oToAdd.original_origin = o.origin;

                    //oToAdd.pos += o.origin; //nieaktywne - sprawia, ze wspolrzedne pozycji wyznaczaja srodek obiektu; aktywne - lewy gorny rog

                    objects.Add(oToAdd);
                }
            }
        }

        // p1 - pierwsza współrzędna, p2 - druga współrzędna, screenSize - wielkość ekranu
        private bool CheckCord(float mapPos, float p1, float p2, int screenSize)
        {
            if (p1 > p2) // sprawdzamy i ew. zmieniamy, zeby p1 bylo mniejsze od p2
            {
                float temp = p1;
                p1 = p2;
                p2 = temp;
            }

            return (p1 < mapPos - screenSize / 2 && p2 > mapPos + screenSize / 2);
        }

        private bool SmallWidth(float a, Vector2 areaSize)
        {
            return (Math.Abs(a) <= (int)areaSize.X / 2);
        }

        private bool SmallHeight(float a, Vector2 areaSize)
        {
            return (Math.Abs(a) <= (int)areaSize.Y / 2);
        }

        private void ClearChunks()
        {
            for (int i = 0; i < numberOfChunks.X; ++i)
                for (int j = 0; j < numberOfChunks.Y; ++j)
                    chunks[i, j].Clear();
        }

        private bool CheckCollision(Object o, Vector2 point)
        {
            for (int i = 0; i < o.collisionRectangles.Length; ++i)
            {
                MyRectangle myRect = o.collisionRectangles[i];

                if (myRect.IsInside(point))
                    return true;
            }

            return false;
        }

        private void AddObjectInformation(string s_objectInformation)
        {
            string[] split = s_objectInformation.Split(new char[] { ';' });

            string name = split[0];
            bool collide = Convert.ToBoolean(Int32.Parse(split[1]));
            bool underBus = Convert.ToBoolean(Int32.Parse(split[2]));

            // tutaj ladowanie prostokatow kolizji:
            List<Rectangle> collisionRectangles = new List<Rectangle>();
            int i = 3; // licznik pozycji
            
            while (i < split.Length)
            {
                Rectangle collisionRectangle = new Rectangle();
                collisionRectangle.X = Int32.Parse(split[i]);
                collisionRectangle.Y = Int32.Parse(split[i + 1]);
                collisionRectangle.Width = Int32.Parse(split[i + 2]);
                collisionRectangle.Height = Int32.Parse(split[i + 3]);

                collisionRectangles.Add(collisionRectangle);

                i += 4;
            }

            AddObjectInformation(name, collide, underBus, collisionRectangles);
        }

        private void AddObjectInformation(string name, bool collide, bool underBus, List<Rectangle> collisionRectangles)
        {
            ObjectInformation objectInformation = new ObjectInformation(underBus, collide, collisionRectangles);
            objectsInformation.Add(name, objectInformation);
        }

        public void AddJunctionsToChunks(List<Object> junctions)
        {
            foreach (Object o in junctions)
                AddObjectToChunk(o, ObjectType.junction);
        }

        public void AddTrafficLightsToChunks(List<TrafficLightObject> trafficLights)
        {
            foreach (TrafficLightObject tlo in trafficLights)
                AddObjectToChunk(tlo, ObjectType.trafficLight);
        }

        public void ClearStandartObjectsSize()
        {
            standart_size.Clear();
        }

        // należy wywołać ZAWSZE po wczytaniu tekstur obiektów
        public void AddStandartObjectsSize(Dictionary<string, Microsoft.Xna.Framework.Graphics.Texture2D> textures)
        {
            List<string> keys = textures.Keys.ToList();

            foreach (string key in keys)
            {
                Vector2 size = new Vector2(textures[key].Width, textures[key].Height);
                standart_size.Add(key, size);
            }
        }

        // laduje mape z pliku
        public void LoadMap(ref StreamReader sr)
        {
            ClearChunks();

            string mapSize = sr.ReadLine();

            int mapWidth = Int32.Parse(mapSize.Substring(0, mapSize.IndexOf(';')));
            int mapHeight = Int32.Parse(mapSize.Substring(mapSize.IndexOf(';') + 1));

            CreateChunks(mapWidth, mapHeight);

            string s_object = sr.ReadLine(); // !!! ta linia to komentarz (DO USUNIECIA) !!!

            while ((s_object = sr.ReadLine()) != null && s_object[0] != '*')
            {
                AddObjectToChunk(s_object);
            }
        }

        // laduje informacje o obiektach
        public void LoadObjectsInformation(string path)
        {
            Stream s = TitleContainer.OpenStream("Content/db/" + path);
            StreamReader sr = new StreamReader(s);

            string s_objectInformation = sr.ReadLine(); // !!! pierwsza linijka to komentarz !!! (DO EW. USUNIECIA)

            while ((s_objectInformation = sr.ReadLine()) != null)
            {
                AddObjectInformation(s_objectInformation);
            }
        }

        //zwraca nazwy obiektow
        public string[] GetObjectsNames()
        {
            return objectsInformation.Keys.ToArray();
        }

        // ustawia obiekty do wyswietlenia (pos - pozycja srodka mapy)
        public void SetObjectsInRange(Vector2 mapPos)
        {
            junctionsInRange.Clear();
            objectsOnBusInRange.Clear();
            objectsUnderBusInRange.Clear();
            trafficLightsInRange.Clear();

            // przypisujemy aktualna pozycje mapy
            this.mapPos = mapPos;

            // x i y kawałka na którym się znajdujemy
            int x = (int)mapPos.X / chunkSize.Width;
            int y = (int)mapPos.Y / chunkSize.Height;

            // bezwzgledna pozycja x i y (wobec krawedzi kawałka)
            int abs_x = (int)mapPos.X - chunkSize.Width * x;
            int abs_y = (int)mapPos.Y - chunkSize.Height * y;
            
            // od którego kawałka będziemy rozpoczynać wczytywanie listy obiektów do wyświetlenia
            int ch_x = (abs_x < chunkSize.Width / 2) ? -1 : 0;
            int ch_y = (abs_y < chunkSize.Height / 2) ? -1 : 0;

            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    int chunk_x = j + ch_x + x;
                    if (chunk_x < 0 || chunk_x >= numberOfChunks.X)
                        continue;

                    int chunk_y = i + ch_y + y;
                    if (chunk_y < 0 || chunk_y >= numberOfChunks.Y)
                        continue;

                    GetObjectsInRangeFrom(ref junctionsInRange, chunks[chunk_x, chunk_y].GetJunctions(), mapPos, Helper.workAreaSize);
                    GetObjectsInRangeFrom(ref objectsOnBusInRange, chunks[chunk_x, chunk_y].GetObjectsOnBus(), mapPos, Helper.workAreaSize);
                    GetObjectsInRangeFrom(ref objectsUnderBusInRange, chunks[chunk_x, chunk_y].GetObjectsUnderBus(), mapPos, Helper.workAreaSize);
                    GetObjectsInRangeFrom(ref trafficLightsInRange, chunks[chunk_x, chunk_y].GetTrafficLights(), mapPos, Helper.workAreaSize);
                }
            }
        }

        // pobiera liste obiektow do wyswietlenia przeliczonych na wspolrzedne ekranowe (zapisana po wywolaniu metody SetObjectsInRange)
        public List<Object> GetJunctionsToShow()
        {
            List<Object> junctionsToShow = new List<Object>(junctionsInRange.Count);

            for (int i = 0; i < junctionsInRange.Count; ++i)
            {
                junctionsToShow.Add((Object)junctionsInRange[i].Clone());
                junctionsToShow[i].pos = Helper.MapPosToScreenPos(junctionsToShow[i].pos);
            }

            return junctionsToShow;
        }

        public List<Object> GetObjectsOnBusToShow()
        {
            List<Object> objectsOnBusToShow = new List<Object>(objectsOnBusInRange.Count);

            for (int i = 0; i < objectsOnBusInRange.Count; ++i)
            {
                objectsOnBusToShow.Add((Object)objectsOnBusInRange[i].Clone());
                objectsOnBusToShow[i].pos = Helper.MapPosToScreenPos(objectsOnBusToShow[i].pos);
            }

            return objectsOnBusToShow;
        }

        public List<Object> GetObjectsUnderBusToShow()
        {
            List<Object> objectsUnderBusToShow = new List<Object>(objectsUnderBusInRange.Count);

            for (int i = 0; i < objectsUnderBusInRange.Count; ++i)
            {
                objectsUnderBusToShow.Add((Object)objectsUnderBusInRange[i].Clone());
                objectsUnderBusToShow[i].pos = Helper.MapPosToScreenPos(objectsUnderBusToShow[i].pos);
            }

            return objectsUnderBusToShow;
        }

        public List<TrafficLightObject> GetTrafficLightsToShow()
        {
            List<TrafficLightObject> trafficLightsToShow = new List<TrafficLightObject>(trafficLightsInRange.Count);

            for (int i = 0; i < trafficLightsInRange.Count; ++i)
            {
                trafficLightsToShow.Add((TrafficLightObject)trafficLightsInRange[i].Clone());
                trafficLightsToShow[i].pos = Helper.MapPosToScreenPos(trafficLightsToShow[i].pos);
            }

            return trafficLightsToShow;
        }

        // pobiera liste obiektow w zasiegu w wspolrzednych ekranowych
        public List<Object> GetCollisionObjectsInRange()
        {
            List<Object> collisionObjectsInRange = new List<Object>();

            foreach (Object o in objectsOnBusInRange)
            {
                if (objectsInformation[o.name].collide)
                    collisionObjectsInRange.Add(o);
            }

            foreach (Object o in objectsUnderBusInRange)
            {
                if (objectsInformation[o.name].collide)
                    collisionObjectsInRange.Add(o);
            }

            return collisionObjectsInRange;
        }

        // zwraca true jezeli kolizja wystepuje
        public bool IsCollision(Vector2 point)
        {
            foreach (Object o in GetCollisionObjectsInRange())
            {
                if (CheckCollision(o, point))
                    return true;
            }

            return false;
        }

        // zwraca true jezeli dla ktoregokolwiek punktu kolizja wystepuje
        public bool IsCollision(Vector2[] points)
        {
            foreach (Vector2 point in points)
            {
                if (IsCollision(point))
                    return true;
            }

            return false;
        }

        // funkcja pomocnicza sluzaca do zwrocenia punktow kolizji do ich pozniejszego wyswietlenia na ekranie
        public Vector2[] GetCollisionPointsToDraw()
        {
            List<Vector2> collisionPoints = new List<Vector2>();

            foreach (Object o in GetCollisionObjectsInRange())
            {
                foreach (MyRectangle myRect in o.collisionRectangles)
                {
                    collisionPoints.Add(Helper.MapPosToScreenPos(myRect.point1));
                    collisionPoints.Add(Helper.MapPosToScreenPos(myRect.point2));
                    collisionPoints.Add(Helper.MapPosToScreenPos(myRect.point3));
                    collisionPoints.Add(Helper.MapPosToScreenPos(myRect.point4));
                }
            }

            return collisionPoints.ToArray();
        }
    }

    class Chunk
    {
        List<Object> junctions;
        List<Object> objectsUnderBus;
        List<Object> objectsOnBus;
        List<TrafficLightObject> trafficLights;

        public Chunk()
        {
            junctions = new List<Object>();
            objectsUnderBus = new List<Object>();
            objectsOnBus = new List<Object>();
            trafficLights = new List<TrafficLightObject>();
        }

        public void AddObject(Object o, ObjectType objectType)
        {
            switch (objectType)
            {
                case ObjectType.underBus:
                    objectsUnderBus.Add(o);
                    break;
                case ObjectType.onBus:
                    objectsOnBus.Add(o);
                    break;
                case ObjectType.junction:
                    junctions.Add(o);
                    break;
                case ObjectType.trafficLight:
                    trafficLights.Add((TrafficLightObject)o);
                    break;
            }
        }

        public List<Object> GetJunctions()
        {
            return junctions;
        }

        public List<Object> GetObjectsUnderBus()
        {
            return objectsUnderBus;
        }

        public List<Object> GetObjectsOnBus()
        {
            return objectsOnBus;
        }

        public List<Object> GetTrafficLights()
        {
            return new List<Object>(trafficLights.Cast<Object>());
        }

        public void Clear()
        {
            junctions.Clear();
            objectsUnderBus.Clear();
            objectsOnBus.Clear();
            trafficLights.Clear();
        }
    }

    class Object : ICloneable
    {
        public string name;
        public float rotate;
        public MyRectangle[] collisionRectangles; // prostokaty kolziji
        public SpriteEffects spriteEffects; // efekty przy wyświetlaniu
        public Vector2 pos;
        public Vector2 originalSize { get; private set; } // oryginalny rozmiar obiektu
        public Vector2 origin { get; private set; }
        public Vector2 original_origin { get; set; } // uzywac przy wyswietlaniu
                                                    // (oryginalny srodek dla standartowych rozmiarow tekstury)

        private Vector2 v_size;
        public Vector2 size
        {
            get
            {
                return v_size;
            }
            set
            {
                v_size = value;
                origin = v_size / 2;
            }
        }

        public Vector2 scale { get; private set; } // skala obiektu (wzgledem standartowego rozmiaru)

        protected Object()
        {

        }

        protected Object(string name, Vector2 pos, Vector2 size, float rotate, SpriteEffects spriteEffects)
        {
            this.name = name;
            this.pos = pos;
            this.size = size;
            this.rotate = rotate;
            this.spriteEffects = spriteEffects;
        }

        public Object(string name, Vector2 pos, Vector2 size, float rotate, Vector2 originalSize,
                SpriteEffects spriteEffects = SpriteEffects.None, List<Rectangle> collisionRectangles = null)
            : this(name, pos, size, rotate, spriteEffects)
        {
            if (originalSize == Vector2.Zero)
            {
                this.scale = new Vector2(1, 1);
                this.originalSize = size;
            }
            else
            {
                this.scale = size / originalSize;
                this.originalSize = originalSize;
            }

            ComputeCollisionRectangles(collisionRectangles);
        }

        public Object(string name, Vector2 pos, Vector2 size, float rotate, SpriteEffects spriteEffects = SpriteEffects.None,
                List<Rectangle> collisionRectangles = null)
            : this(name, pos, size, rotate, spriteEffects)
        {
            this.scale = new Vector2(1, 1);

            ComputeCollisionRectangles(collisionRectangles);
        }

        public void ComputeCollisionRectangles(List<Rectangle> collisionRectangles)
        {
            // obliczanie prostokatow kolizji
            if (collisionRectangles == null || collisionRectangles.Count == 0) // jezeli nie zdefiniowano tworzymy standartowy prostokat kolzji
            {
                this.collisionRectangles = new MyRectangle[1];

                Rectangle collisionRectangle = new Rectangle((int)(pos.X - origin.X), (int)(pos.Y - origin.Y), (int)size.X, (int)size.Y);

                this.collisionRectangles[0] = Helper.ComputeRectangleOnRotation(collisionRectangle, pos, rotate);
            }
            else
            {
                this.collisionRectangles = new MyRectangle[collisionRectangles.Count];

                for (int i = 0; i < collisionRectangles.Count; ++i)
                {
                    Rectangle collisionRectangle = collisionRectangles[i];
                    IncludeSpriteEffects(ref collisionRectangle);

                    collisionRectangle.X = (int)((collisionRectangle.X * scale.X) + (pos.X - origin.X));
                    collisionRectangle.Y = (int)((collisionRectangle.Y * scale.Y) + (pos.Y - origin.Y));
                    collisionRectangle.Width = (int)(collisionRectangle.Width * scale.X);
                    collisionRectangle.Height = (int)(collisionRectangle.Height * scale.Y);

                    this.collisionRectangles[i] = Helper.ComputeRectangleOnRotation(collisionRectangle, pos, rotate);
                }
            }
        }

        protected void IncludeSpriteEffects(ref Rectangle collisionRectangle)
        {
            if (this.spriteEffects == SpriteEffects.FlipHorizontally)
            {
                collisionRectangle.X = (int)originalSize.X - collisionRectangle.X - collisionRectangle.Width;
            }
            else if (this.spriteEffects == SpriteEffects.FlipVertically)
            {
                collisionRectangle.Y = (int)originalSize.Y - collisionRectangle.Y - collisionRectangle.Height;
            }
        }

        public object Clone()
        {
            Object other = (Object)this.MemberwiseClone();

            return other;
        }
    }

    class TrafficLightObject : Object
    {
        public int junctionIndex { get; private set; } // indeks skrzyżowania z jakim powiązane jest dane światło
        public int pairIndex { get; private set; } // indeks pary świateł w danym skrzyżowaniu z jakimi powiązane jest dane światło

        public TrafficLightObject(string name, Vector2 pos, Vector2 size, float rotate, int junctionIndex, int pairIndex)
        {
            this.name = name;
            this.pos = pos;
            this.rotate = rotate;
            this.size = size;
            this.junctionIndex = junctionIndex;
            this.pairIndex = pairIndex;
        }
    }

    class ObjectInformation
    {
        public readonly bool underBus; // czy dany typ obiektu jest pod autobusem
        public readonly bool collide; // czy dany typ obiektu wywoluje kolizje
        public readonly List<Rectangle> collisionRectangles; // lista prostokatow kolizjii

        public ObjectInformation(bool underBus, bool collide, List<Rectangle> collisionRectangles)
        {
            this.underBus = underBus;
            this.collide = collide;
            this.collisionRectangles = collisionRectangles;
        }
    }
}