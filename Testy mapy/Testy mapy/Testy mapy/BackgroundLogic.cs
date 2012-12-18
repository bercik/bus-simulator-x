using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    enum Direction { Up = 0, Right = 1, Down = 2, Left = 3 } // nie zmieniac kolejnosci!

    class Grass
    {
        public string name;
        public Vector2 pos;

        public Grass(string name, Vector2 pos)
        {
            this.name = name;
            this.pos = pos;
        }
    }

    class BackgroundLogic
    {
        Vector2 last_change_pos; // ostatnio zmieniona pozycja w jednostkach mapy (przy przesunięciu trawy tzw. "skoku")
        Vector2 last_pos;
        Vector2 grassSize; // rozmiar trawy
        Vector2 grassOrigin; // srodek trawy
        Point numberOfGrass; // ilość bloków trawy w pionie i poziomie
        Grass[,] grass; // tablica trawy
        int amountOfGrass; // ilość typów trawy

        Random rand; // klasa losująca
        bool first_pos_load = true; // czy to pierwsze załadowanie

        const string grassName = "trawa"; // nazwa plikow graficznych dla trawy (nie wiem po co, tak lubie pisac uniwersalny kod)

        public BackgroundLogic()
        {
            rand = new Random();
        }

        public void CreateGrass(Vector2 mapPos)
        {
            this.last_change_pos = mapPos;

            grass = new Grass[numberOfGrass.X, numberOfGrass.Y];

            for (int i = 0; i < numberOfGrass.X; ++i)
            {
                for (int j = 0; j < numberOfGrass.Y; ++j)
                {
                    RandomGrass(i, j);
                }
            }
        }

        public Vector2 getGrassOrigin()
        {
            return grassOrigin;
        }

        public Vector2 getGrassSize()
        {
            return grassSize;
        }

        private float GetXPosition(int x)
        {
            float leftUpX = last_change_pos.X - (Helper.maxWorkAreaSize.X / 2); // lewy górny kraniec mapy

            return leftUpX + (x * grassSize.X) - (grassSize.X);
        }

        private float GetYPosition(int y)
        {
            float leftUpY = last_change_pos.Y - (Helper.maxWorkAreaSize.Y / 2); // lewy górny kraniec mapy

            return leftUpY + (y * grassSize.Y) - (grassSize.Y);
        }

        private void RandomGrass(int x, int y)
        {
            string name = RandomGrassName();
            grass[x, y] = new Grass(name, new Vector2(GetXPosition(x), GetYPosition(y)));
        }

        private void RandomGrassName(int x, int y)
        {
            grass[x, y].name = RandomGrassName();
        }

        private string RandomGrassName()
        {
            return grassName + rand.Next(0, amountOfGrass).ToString();
        }

        private void RandomGrassColumn(int c)
        {
            for (int i = 0; i < numberOfGrass.Y; ++i)
            {
                RandomGrassName(c, i);
            }
        }

        private void RandomGrassLine(int l)
        {
            for (int i = 0; i < numberOfGrass.X; ++i)
            {
                RandomGrassName(i, l);
            }
        }

        private void MoveGrassNames(Direction direct)
        {
            switch (direct)
            {
                case Direction.Left:
                    for (int i = 0; i < numberOfGrass.X - 1; ++i)
                    {
                        for (int j = 0; j < numberOfGrass.Y; ++j)
                        {
                            grass[i, j].name = grass[i + 1, j].name;
                        }
                    }
                    break;
                case Direction.Right:
                    for (int i = numberOfGrass.X - 1; i > 0; --i)
                    {
                        for (int j = 0; j < numberOfGrass.Y; ++j)
                        {
                            grass[i, j].name = grass[i - 1, j].name;
                        }
                    }
                    break;
                case Direction.Up:
                    for (int i = 0; i < numberOfGrass.Y - 1; ++i)
                    {
                        for (int j = 0; j < numberOfGrass.X; ++j)
                        {
                            grass[j, i].name = grass[j, i + 1].name;
                        }
                    }
                    break;
                case Direction.Down:
                    for (int i = numberOfGrass.Y - 1; i > 0; --i)
                    {
                        for (int j = 0; j < numberOfGrass.X; ++j)
                        {
                            grass[j, i].name = grass[j, i - 1].name;
                        }
                    }
                    break;
            }
        }

        // ustawia pozycje całej trawy do takiej jaka jest na początku (czyli niweluje jakieś przesunięcia związane z zaokrąglaniem)
        private void SetToStartPosition(Location location)
        {
            for (int i = 0; i < numberOfGrass.X; ++i)
            {
                for (int j = 0; j < numberOfGrass.Y; ++j)
                {
                    if (location == Location.horizontal)
                    {
                        grass[i, j].pos.X = GetXPosition(i);
                    }
                    else if (location == Location.vertical)
                    {
                        grass[i, j].pos.Y = GetYPosition(j);
                    }
                }
            }
        }

        private void Move(Direction direct)
        {
            MoveGrassNames(direct);

            Location location = Location.horizontal;

            switch (direct)
            {
                case Direction.Down:
                    location = Location.vertical;
                    RandomGrassLine(0);
                    break;
                case Direction.Up:
                    location = Location.vertical;
                    RandomGrassLine(numberOfGrass.Y - 1);
                    break;
                case Direction.Right:
                    location = Location.horizontal;
                    RandomGrassColumn(0);
                    break;
                case Direction.Left:
                    location = Location.horizontal;
                    RandomGrassColumn(numberOfGrass.X - 1);
                    break;
            }

            SetToStartPosition(location);
        }

        public void UpdatePos(Vector2 pos)
        {
            if (first_pos_load)
            {
                last_change_pos = pos;

                first_pos_load = false;
            }

            Vector2 difference_pos = last_change_pos - pos;

            if (difference_pos.X > grassSize.X)
            {
                last_change_pos.X = pos.X + Math.Abs(pos.X - last_pos.X);

                Move(Direction.Right);
            }
            else if (difference_pos.X < -grassSize.X)
            {
                last_change_pos.X = pos.X - Math.Abs(pos.X - last_pos.X);

                Move(Direction.Left);
            }
            if (difference_pos.Y > grassSize.Y)
            {
                last_change_pos.Y = pos.Y + Math.Abs(pos.Y - last_pos.Y);

                Move(Direction.Down);
            }
            else if (difference_pos.Y < -grassSize.Y)
            {
                last_change_pos.Y = pos.Y - Math.Abs(pos.Y - last_pos.Y);

                Move(Direction.Up);
            }

            last_pos = pos;
        }

        public List<Grass> getGrassToShow()
        {
            // wlasciwa metoda wyznaczajaca pozycje trawy
            List<Grass> grassToShow = new List<Grass>();
            
            for (int i = 0; i < numberOfGrass.X; ++i)
            {
                for (int j = 0; j < numberOfGrass.Y; ++j)
                {
                    Grass g = new Grass(grass[i, j].name, grass[i, j].pos);
                    g.pos = Helper.MapPosToScreenPos(g.pos);
                    grassToShow.Add(g);
                }
            }

            return grassToShow;
        }

        public void SetProperties(Vector2 grassSize, int amountOfGrass)
        {
            this.amountOfGrass = amountOfGrass;
            this.grassSize = grassSize;
            this.grassOrigin = grassSize / 2;
            numberOfGrass.X = (int)((Helper.maxWorkAreaSize.X / (int)grassSize.X) + 3);
            numberOfGrass.Y = (int)((Helper.maxWorkAreaSize.Y / (int)grassSize.Y) + 3);
        }
    }
}
