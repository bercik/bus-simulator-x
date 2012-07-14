using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    enum Direction { Up = 0, Right = 1, Down = 2, Left = 3 }

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
        Vector2 last_change_pos;
        Vector2 last_pos;
        Vector2 grassSize;
        Vector2 screenSize;
        Point numberOfGrass;
        Grass[,] grass;
        int amountOfGrass;
        Random rand;
        bool first_pos_load = true;
        const string grassName = "trawa"; // nazwa plikow graficznych dla trawy (nie wiem po co, tak lubie pisac uniwersalny kod)

        public BackgroundLogic()
        {
            rand = new Random();
        }

        private void CreateGrass()
        {
            grass = new Grass[numberOfGrass.X, numberOfGrass.Y];

            for (int i = 0; i < numberOfGrass.X; ++i)
            {
                for (int j = 0; j < numberOfGrass.Y; ++j)
                {
                    RandomGrass(i, j);
                }
            }
        }

        private void RandomGrass(int x, int y)
        {
            string name = RandomGrassName();
            grass[x, y] = new Grass(name, new Vector2(x * grassSize.X - grassSize.X, y * grassSize.Y - grassSize.Y));
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

        private void Move(Direction direct)
        {
            MoveGrassNames(direct);

            for (int i = 0; i < numberOfGrass.X; ++i)
            {
                for (int j = 0; j < numberOfGrass.Y; ++j)
                {
                    Vector2 pos = new Vector2();

                    switch (direct)
                    {
                        case Direction.Down:
                            pos.Y = -grassSize.Y;
                            RandomGrassLine(0);
                            break;
                        case Direction.Up:
                            pos.Y = grassSize.Y;
                            RandomGrassLine(numberOfGrass.Y - 1);
                            break;
                        case Direction.Right:
                            pos.X = -grassSize.X;
                            RandomGrassColumn(0);
                            break;
                        case Direction.Left:
                            pos.X = grassSize.X;
                            RandomGrassColumn(numberOfGrass.X - 1);
                            break;
                    }

                    grass[i, j].pos += pos;
                }
            }
        }

        public void UpdatePos(Vector2 pos)
        {
            if (first_pos_load)
            {
                last_change_pos = pos;
                last_pos = pos;

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

            // wlasciwa metoda wyznaczajaca pozycje trawy
            for (int i = 0; i < numberOfGrass.X; ++i)
            {
                for (int j = 0; j < numberOfGrass.Y; ++j)
                {
                    grass[i, j].pos += last_pos - pos;
                }
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
                    grassToShow.Add(grass[i, j]);
                }
            }

            return grassToShow;
        }

        public void SetProperties(int screenWidth, int screenHeight, Vector2 grassSize, int amountOfGrass)
        {
            this.amountOfGrass = amountOfGrass;
            this.grassSize = grassSize;
            this.screenSize = new Vector2(screenWidth, screenHeight);
            numberOfGrass.X = screenWidth / (int)grassSize.X + ((screenWidth % (int)grassSize.X == 0) ? 2 : 3);
            numberOfGrass.Y = screenHeight / (int)grassSize.Y + ((screenHeight % (int)grassSize.Y == 0) ? 2 : 3);

            CreateGrass();
        }
    }
}
