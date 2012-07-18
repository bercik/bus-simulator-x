using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Testy_mapy
{
    class MapLogic
    {
        Chunk[,] chunks;
        Size chunkSize;
        Size screenSize;
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
        List<Object> junctionsInRange;
        List<Object> objectsUnderBusInRange;
        List<Object> objectsOnBusInRange;

        Size maxObjectSize = new Size(300, 300); // maksymalny możliwy rozmiar obiektu
                                                         // uwzględnij rotację o 45 stopni
                                                         //(czyli przemnoż przez pierwiastek z 2)

        public MapLogic()
        {
            standart_size = new Dictionary<string, Vector2>();
            junctionsInRange = new List<Object>();
            objectsUnderBusInRange = new List<Object>();
            objectsOnBusInRange = new List<Object>();
        }

        private void CreateChunks(int screenWidth, int screenHeight, int mapWidth, int mapHeight)
        {
            screenSize.Width = screenWidth;
            screenSize.Height = screenHeight;
            chunkSize.Width = screenWidth + maxObjectSize.Width;
            chunkSize.Height = screenHeight + maxObjectSize.Height;
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
            bool collide = Convert.ToBoolean(Int32.Parse(split[6]));
            bool underBus = Convert.ToBoolean(Int32.Parse(split[7]));
            SpriteEffects spriteEffects = (SpriteEffects)Int32.Parse(split[8]);

            AddObjectToChunk(name, pos, size, rotate, collide, underBus, spriteEffects);
        }

        private void AddObjectToChunk(string name, Vector2 pos, Vector2 size, float rotate, bool collide,
                bool underBus, SpriteEffects spriteEffects = SpriteEffects.None, bool junction = false)
        {
            if (size.X == 0)
                size.X = standart_size[name].X;
            if (size.Y == 0)
                size.Y = standart_size[name].Y;

            Object o = new Object(name, pos, size, rotate, collide, underBus, spriteEffects);

            int x = (int)pos.X / chunkSize.Width;
            int y = (int)pos.Y / chunkSize.Height;

            
            chunks[x, y].AddObject(o, junction);

            // dla obiektow wiekszych niz zakladane
            bool b_x = false, b_y = false; // czy obiekt jest wiekszy od zakladanej wielkosci (na szerokosc i dlugosc)

            if (o.size.X > maxObjectSize.Width)
            {
                b_x = true;

                if (x + 1 < numberOfChunks.X)
                    chunks[x + 1, y].AddObject(o, junction);
                if (x - 1 > 0)
                    chunks[x - 1, y].AddObject(o, junction);
            }
            if (o.size.Y > maxObjectSize.Height)
            {
                b_y = true;

                if (y + 1 < numberOfChunks.Y)
                    chunks[x, y + 1].AddObject(o, junction);
                if (y - 1 > 0)
                    chunks[x, y - 1].AddObject(o, junction);
            }

            if (b_x || b_y)
            {
                //if (x + 1 < numberOfChunks.X && y + 1 < numberOfChunks.Y)
                //chunks[x + 1, y + 1].AddObject(o, atTheBeginning); // !!!DO EW. USUNIECIA!!! jezeli wyswietlanie obiektow wiekszych od maksymalnego zakladanego bedzie dzialac
            }
        }

        private void ComputePointsOnRotation(Object o, out float x1, out float x2, out float y1, out float y2)
        {
            // metoda obliczajaca wspolrzedne punktow obiektu przy rotacji
            float f_rotate = o.rotate;
            Vector2 new_pos = o.pos - o.origin; // lewy gorny punkt obiektu we wspolrzednych mapy
            Vector2 center = o.pos; // srodek

            if (f_rotate > 180)
                f_rotate -= 180;

            Vector2 pos1 = new Vector2(new_pos.X, new_pos.Y);
            Vector2 pos2 = new Vector2(new_pos.X + o.size.X, new_pos.Y);
            Vector2 pos3 = new Vector2(new_pos.X + o.size.X, new_pos.Y + o.size.Y);
            Vector2 pos4 = new Vector2(new_pos.X, new_pos.Y + o.size.Y);

            if (f_rotate <= 90)
            {
                x1 = ComputeRotationX(pos2, center, f_rotate); // 2
                x2 = ComputeRotationX(pos4, center, f_rotate); // 4
                    
                y1 = ComputeRotationY(pos1, center, f_rotate); // 1
                y2 = ComputeRotationY(pos3, center, f_rotate); // 3
            }
            else
            {
                y1 = ComputeRotationY(pos2, center, f_rotate); // 2
                y2 = ComputeRotationY(pos4, center, f_rotate); // 4

                x1 = ComputeRotationX(pos1, center, f_rotate); // 1
                x2 = ComputeRotationX(pos3, center, f_rotate); // 3
            }
        }

        private void ComputePointsOnRotation(Object o, out MyRectangle myRect)
        {
            myRect = new MyRectangle();

            float f_rotate = o.rotate;
            Vector2 new_pos = o.pos - o.origin; // lewy gorny punkt obiektu we wspolrzednych mapy
            Vector2 center = o.pos; // srodek

            if (f_rotate > 180)
                f_rotate -= 180;

            Vector2 pos1 = new Vector2(new_pos.X, new_pos.Y);
            Vector2 pos2 = new Vector2(new_pos.X + o.size.X, new_pos.Y);
            Vector2 pos3 = new Vector2(new_pos.X + o.size.X, new_pos.Y + o.size.Y);
            Vector2 pos4 = new Vector2(new_pos.X, new_pos.Y + o.size.Y);

            myRect.point1 = ComputeRotation(pos4, center, f_rotate);
            myRect.point2 = ComputeRotation(pos3, center, f_rotate);
            myRect.point3 = ComputeRotation(pos2, center, f_rotate);
            myRect.point4 = ComputeRotation(pos1, center, f_rotate);
        }

        private void GetObjectsInRangeFromChunk(ref List<Object> objects, List<Object> objectsToCheck, int x, int y, Vector2 mapPos)
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
                if (SmallWidth(Math.Abs(mapPos.X - x1)))
                    b_x = true;
                else if (SmallWidth(Math.Abs(mapPos.X - x2)))
                    b_x = true;
                if (SmallHeight(Math.Abs(mapPos.Y - y1)))
                    b_y = true;
                else if (SmallHeight(Math.Abs(mapPos.Y - y2)))
                    b_y = true;


                if (!b_x) // sprawdzamy czy współrzędne X obiektu nie znajdują się pomiędzy ekranem
                {
                    if (CheckCord(mapPos.X, x1, x2, screenSize.Width))
                        b_x = true;
                }
                if (!b_y) // analogicznie jak wyżej dla współrzędnych Y
                {
                    if (CheckCord(mapPos.Y, y1, y2, screenSize.Height))
                        b_y = true;
                }

                if (b_x && b_y)
                {
                    Object oToAdd = new Object(o.name, o.pos, o.size, o.rotate, o.collide, o.underBus, o.spriteEffects);

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

        private float ComputeRotationX(Vector2 point, Vector2 center, float angel)
        {
            return (float)(((point.X - center.X) * Math.Cos(MathHelper.ToRadians(angel))) - ((point.Y - center.Y) * Math.Sin(MathHelper.ToRadians(angel))) + center.X);
        }

        private float ComputeRotationY(Vector2 point, Vector2 center, float angel)
        {
            return (float)(((point.X - center.X) * Math.Sin(MathHelper.ToRadians(angel))) + ((point.Y - center.Y) * Math.Cos(MathHelper.ToRadians(angel))) + center.Y);
        }

        private Vector2 ComputeRotation(Vector2 point, Vector2 center, float angel)
        {
            return new Vector2(ComputeRotationX(point, center, angel), ComputeRotationY(point, center, angel));
        }

        private bool SmallWidth(float a)
        {
            return (Math.Abs(a) <= screenSize.Width / 2);
        }

        private bool SmallHeight(float a)
        {
            return (Math.Abs(a) <= screenSize.Height / 2);
        }

        private void ClearChunks()
        {
            for (int i = 0; i < numberOfChunks.X; ++i)
                for (int j = 0; j < numberOfChunks.Y; ++j)
                    chunks[i, j].Clear();
        }

        private bool CheckCollision(Object o, Vector2 point)
        {
            MyRectangle myRect;
            ComputePointsOnRotation(o, out myRect);

            return myRect.IsInside(point);
        }

        public void AddObjectToChunk(Object o, bool junction)
        {
            AddObjectToChunk(o.name, o.pos, o.size, o.rotate, o.collide, o.underBus, o.spriteEffects, junction);
        }

        public void AddJunctionsToChunks(List<Object> junctions)
        {
            foreach (Object o in junctions)
                AddObjectToChunk(o, true);
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
        public bool LoadMap(string path, int screenWidth, int screenHeight)
        {
            path = "maps\\" + path;

            if (File.Exists(path))
            {
                ClearChunks();

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                string mapSize;
                while ((mapSize = sr.ReadLine()).StartsWith("//")) ;

                int mapWidth = Int32.Parse(mapSize.Substring(0, mapSize.IndexOf(';')));
                int mapHeight = Int32.Parse(mapSize.Substring(mapSize.IndexOf(';') + 1));

                CreateChunks(screenWidth, screenHeight, mapWidth, mapHeight);

                string s_object = "";

                while ((s_object = sr.ReadLine()) != null)
                {
                    if (!s_object.StartsWith("//"))
                        AddObjectToChunk(s_object);
                }

                return true;
            }

            return false;
        }

        // ustawia obiekty do wyswietlenia (pos - pozycja srodka mapy)
        public void SetObjectsInRange(Vector2 mapPos)
        {
            junctionsInRange.Clear();
            objectsOnBusInRange.Clear();
            objectsUnderBusInRange.Clear();

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

                    GetObjectsInRangeFromChunk(ref junctionsInRange, chunks[chunk_x, chunk_y].GetJunctions(), chunk_x, chunk_y, mapPos);
                    GetObjectsInRangeFromChunk(ref objectsOnBusInRange, chunks[chunk_x, chunk_y].GetObjectsOnBus(), chunk_x, chunk_y, mapPos);
                    GetObjectsInRangeFromChunk(ref objectsUnderBusInRange, chunks[chunk_x, chunk_y].GetObjectsUnderBus(), chunk_x, chunk_y, mapPos);
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
                junctionsToShow[i].pos.X -= (mapPos.X - screenSize.Width / 2);
                junctionsToShow[i].pos.Y -= (mapPos.Y - screenSize.Height / 2);
            }

            return junctionsToShow;
        }

        public List<Object> GetObjectsOnBusToShow()
        {
            List<Object> objectsOnBusToShow = new List<Object>(objectsOnBusInRange.Count);

            for (int i = 0; i < objectsOnBusInRange.Count; ++i)
            {
                objectsOnBusToShow.Add((Object)objectsOnBusInRange[i].Clone());
                objectsOnBusToShow[i].pos.X -= (mapPos.X - screenSize.Width / 2);
                objectsOnBusToShow[i].pos.Y -= (mapPos.Y - screenSize.Height / 2);
            }

            return objectsOnBusToShow;
        }

        public List<Object> GetObjectsUnderBusToShow()
        {
            List<Object> objectsUnderBusToShow = new List<Object>(objectsUnderBusInRange.Count);

            for (int i = 0; i < objectsUnderBusInRange.Count; ++i)
            {
                objectsUnderBusToShow.Add((Object)objectsUnderBusInRange[i].Clone());
                objectsUnderBusToShow[i].pos.X -= (mapPos.X - screenSize.Width / 2);
                objectsUnderBusToShow[i].pos.Y -= (mapPos.Y - screenSize.Height / 2);
            }

            return objectsUnderBusToShow;
        }

        // pobiera liste obiektow w zasiegu w wspolrzednych ekranowych
        public List<Object> GetCollisionObjectsInRange()
        {
            List<Object> objectsInRange = new List<Object>();

            objectsInRange.AddRange(objectsOnBusInRange);
            objectsInRange.AddRange(objectsUnderBusInRange);

            return objectsInRange;
        }

        // zwraca true jezeli kolizja wystepuje
        public bool IsCollision(Vector2 point)
        {
            foreach (Object o in GetCollisionObjectsInRange())
            {
                if (o.collide)
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
    }

    class Chunk
    {
        List<Object> junctions;
        List<Object> objectsUnderBus;
        List<Object> objectsOnBus;

        public Chunk()
        {
            junctions = new List<Object>();
            objectsUnderBus = new List<Object>();
            objectsOnBus = new List<Object>();
        }

        public void AddObject(Object o, bool junction = false)
        {
            if (junction)
                junctions.Add(o);
            else if (o.underBus)
                objectsUnderBus.Add(o);
            else
                objectsOnBus.Add(o);
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

        public void Clear()
        {
            junctions.Clear();
            objectsUnderBus.Clear();
            objectsOnBus.Clear();
        }
    }

    class Object : ICloneable
    {
        public string name;
        public float rotate;
        public bool collide; // czy dany obiekt ma wywoływać kolizję
        public bool underBus; // czy dany obiekt ma byc wyswietlany pod autobusem
        public SpriteEffects spriteEffects; // efekty przy wyświetlaniu
        public Vector2 pos;
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

        public Object(string name, Vector2 pos, Vector2 size, float rotate, bool collide,
                bool underBus = false, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            this.name = name;
            this.pos = pos;
            this.size = size;
            this.rotate = rotate;
            this.collide = collide;
            this.spriteEffects = spriteEffects;
            this.underBus = underBus;
        }

        public object Clone()
        {
            Object other = (Object)this.MemberwiseClone();

            return other;
        }
    }
}
