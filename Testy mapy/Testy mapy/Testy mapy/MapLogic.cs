using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    struct Size
    {
        public int Width;
        public int Height;

        public Size(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
        }
    }

    class MapLogic
    {
        Chunk[,] chunks;
        Size chunkSize;
        Size screenSize;
        Point numberOfChunks;
        Dictionary<string, Vector2> standart_size;

        Size maxObjectSize = new Size(300, 300); // maksymalny możliwy rozmiar obiektu
                                                         // uwzględnij rotację o 45 stopni
                                                         //(czyli przemnoż przez pierwiastek z 2)

        public MapLogic()
        {
            standart_size = new Dictionary<string, Vector2>();
        }

        private void CreateChunks(int screenWidth, int screenHeight, int mapWidth, int mapHeight)
        {
            screenSize.Width = screenWidth;
            screenSize.Height = screenHeight;
            chunkSize.Width = screenWidth + maxObjectSize.Width;
            chunkSize.Height = screenHeight + maxObjectSize.Height;
            numberOfChunks.X = (mapWidth / chunkSize.Width);
            numberOfChunks.Y = (mapHeight / chunkSize.Height);

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

            AddObjectToChunk(name, pos, size, rotate, false);
        }

        private void AddObjectToChunk(string name, Vector2 pos, Vector2 size, float rotate, bool atTheBeginning)
        {
            if (size.X == 0)
                size.X = standart_size[name].X;
            if (size.Y == 0)
                size.Y = standart_size[name].Y;

            Object o = new Object(name, pos, size, rotate);

            int x = (int)pos.X / chunkSize.Width;
            int y = (int)pos.Y / chunkSize.Height;

            chunks[x, y].AddObject(o, atTheBeginning);
        }

        private void GetObjectsToShowFromChunk(ref List<Object> objectsToShow, int x, int y, Vector2 pos)
        {
            foreach (Object o in chunks[x, y].GetObjects())
            {
                bool b_x = false, b_y = false;

                // metoda obliczajaca wspolrzedne punktow obiektu przy rotacji
                float f_rotate = o.rotate;
                Vector2 new_pos = o.pos - o.origin;
                Vector2 center = new_pos + o.origin;

                if (f_rotate > 180)
                    f_rotate -= 180;

                float x1, x2, y1, y2;
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


                // SPRAWDZANIE PUNKTOW
                Vector2 differenceSize;

                if (standart_size.ContainsKey(o.name))
                    differenceSize = o.size - standart_size[o.name];
                else
                    differenceSize = new Vector2(0, 0);

                if (SmallWidth(Math.Abs(pos.X - x1) - differenceSize.X))
                    b_x = true;
                else if (SmallWidth(Math.Abs(pos.X - x2) - differenceSize.X))
                    b_x = true;
                if (SmallHeight(Math.Abs(pos.Y - y1) - differenceSize.Y))
                    b_y = true;
                else if (SmallHeight(Math.Abs(pos.Y - y2) - differenceSize.Y))
                    b_y = true;

                if (b_x && b_y)
                {
                    Object oToAdd = new Object(o.name, new Vector2(0, 0), o.size, o.rotate);
                    oToAdd.pos.X = o.pos.X - (pos.X - screenSize.Width / 2);
                    oToAdd.pos.Y = o.pos.Y - (pos.Y - screenSize.Height / 2);
                    if (standart_size.ContainsKey(o.name))
                        oToAdd.original_origin = standart_size[o.name] / 2;
                    else
                        oToAdd.original_origin = o.origin;
                    //oToAdd.pos += o.origin; //nieaktywne - sprawia, ze wspolrzedne pozycji wyznaczaja srodek obiektu; aktywne - lewy gorny rog
                    objectsToShow.Add(oToAdd);
                }
            }
        }

        private float ComputeRotationX(Vector2 point, Vector2 center, float angel)
        {
            return (float)(((point.X - center.X) * Math.Cos(MathHelper.ToRadians(angel))) - ((point.Y - center.Y) * Math.Sin(MathHelper.ToRadians(angel))) + center.X);
        }

        private float ComputeRotationY(Vector2 point, Vector2 center, float angel)
        {
            return (float)(((point.X - center.X) * Math.Sin(MathHelper.ToRadians(angel))) + ((point.Y - center.Y) * Math.Cos(MathHelper.ToRadians(angel))) + center.Y);
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

        public void AddObjectToChunk(Object o, bool atTheBeginning)
        {
            AddObjectToChunk(o.name, o.pos, o.size, o.rotate, atTheBeginning);
        }

        public void AddObjectsToChunks(List<Object> objects, bool atTheBeginning)
        {
            foreach (Object o in objects)
                AddObjectToChunk(o, atTheBeginning);
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

                string mapSize = sr.ReadLine();
                int mapWidth = Int32.Parse(mapSize.Substring(0, mapSize.IndexOf(';')));
                int mapHeight = Int32.Parse(mapSize.Substring(mapSize.IndexOf(';') + 1));

                CreateChunks(screenWidth, screenHeight, mapWidth, mapHeight);

                string s_object = "";

                while ((s_object = sr.ReadLine()) != null)
                {
                    AddObjectToChunk(s_object);
                }

                return true;
            }

            return false;
        }

        // pobiera liste obiektow do wyswietlenia (z odpowiednimi wspolrzednymi ekranowymi)
        public List<Object> GetOBjectsToShow(Vector2 pos)
        {
            List<Object> objectsToShow = new List<Object>();

            // x i y kawałka na którym się znajdujemy
            int x = (int)pos.X / chunkSize.Width;
            int y = (int)pos.Y / chunkSize.Height;

            // bezwzgledna pozycja x i y (wobec krawedzi kawałka)
            int abs_x = (int)pos.X - chunkSize.Width * x; 
            int abs_y = (int)pos.Y - chunkSize.Height * y;
            
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

                    GetObjectsToShowFromChunk(ref objectsToShow, chunk_x, chunk_y, pos);
                }
            }

            return objectsToShow;
        }
    }

    class Chunk
    {
        List<Object> objects;

        public Chunk()
        {
            objects = new List<Object>();
        }

        public void AddObject(Object o, bool atTheBeginning)
        {
            if (atTheBeginning)
                objects.Insert(0, o);
            else
                objects.Add(o);
        }

        public List<Object> GetObjects()
        {
            return objects;
        }

        public void Clear()
        {
            objects.Clear();
        }
    }

    class Object
    {
        public string name;
        public float rotate;
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

        public Object(string name, Vector2 pos, Vector2 size, float rotate)
        {
            this.name = name;
            this.pos = pos;
            this.size = size;
            this.rotate = rotate;
        }
    }
}
