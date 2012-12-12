using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    enum TrafficLightState { green = 0, yellow = 1, red = 2, redYellow = 3 } // stan swiatel (zielony, zolty, czerwony, zolty i czerwony)

    class TrafficLightObjectInformation
    {
        public Vector2 position { get; private set; } // pozycja światla
        public float rotation { get; private set; } // rotacja
        protected Rectangle redLightForBusRectangle; // prostokat do wykrywania czerwonego swiatla dla autobusu
        protected Rectangle redLightForCarRectangle; // prostokat do wykrywania czerwonego swiatla dla aut

        protected TrafficLightObjectInformation()
        {

        }

        public TrafficLightObjectInformation(Vector2 position, float rotation, Rectangle redLightForBusRectangle, Rectangle redLightForCarRectangle)
        {
            this.position = position;
            this.rotation = rotation;
            this.redLightForBusRectangle = redLightForBusRectangle;
            this.redLightForCarRectangle = redLightForCarRectangle;
        }

        // wywoływać do obliczenia parametrów przy dodawaniu nowego typu skrzyżowań
        public TrafficLightObjectInformation(Direction direction, Vector2 junctionSize, Vector2 trafficLightSize)
        {
            ComputeParameters(direction, junctionSize, trafficLightSize);
        }

        public Rectangle getRedLightForBusRectangle()
        {
            return redLightForBusRectangle;
        }

        public Rectangle getRedLightForCarRectangle()
        {
            return redLightForCarRectangle;
        }

        // obraca współrzędne o zadany kąt i przesuwa o dany wektor
        protected void RotateAndMove(float rotation, Vector2 shift)
        {
            // zmienne pomocnicze:
            Vector2 pos;
            int width, height;

            // pozycja światła
            position = Helper.ComputeRotation(position, new Vector2(0, 0), rotation);
            position += shift;

            // pozycja prostokątu do wykrywania czerwonego światła dla autobusu
            pos = new Vector2((int)redLightForBusRectangle.X, (int)redLightForBusRectangle.Y);
            pos = Helper.ComputeRotation(pos, new Vector2(0, 0), rotation);
            pos += shift;

            if ((rotation / 90) % 2 != 0) // jezeli przy rotacji zmieniamy kierunek z poziomego na pionowy lub odwrotnie
            {
                width = redLightForBusRectangle.Height;
                height = redLightForBusRectangle.Width;
            }
            else // jezeli nie zmieniamy kierunku
            {
                width = redLightForBusRectangle.Width;
                height = redLightForBusRectangle.Height;
            }

            pos.Y -= height / 2;
            pos.X -= width / 2;
            redLightForBusRectangle = new Rectangle((int)Math.Round(pos.X), (int)Math.Round(pos.Y), width, height);

            // pozycja prostokątu do wykrywania czerwonego światła dla aut
            pos = new Vector2((int)redLightForCarRectangle.X, (int)redLightForCarRectangle.Y);
            pos = Helper.ComputeRotation(pos, new Vector2(0, 0), rotation);
            pos += shift;

            if ((rotation / 90) % 2 != 0) // jezeli przy rotacji zmieniamy kierunek z poziomego na pionowy lub odwrotnie
            {
                width = redLightForCarRectangle.Height;
                height = redLightForCarRectangle.Width;
            }
            else // jezeli nie zmieniamy kierunku
            {
                width = redLightForCarRectangle.Width;
                height = redLightForCarRectangle.Height;
            }

            pos.Y -= height / 2;
            pos.X -= width / 2;
            redLightForCarRectangle = new Rectangle((int)Math.Round(pos.X), (int)Math.Round(pos.Y), width, height);

            this.rotation = ((this.rotation + rotation) / 90) * 90;
        }

        /// <summary>
        /// Tworzy nowy obiekt TrafficLightObjectInformation po rotacji obecnego
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public TrafficLightObjectInformation CreateNewObjectAfterRotationAndMove(float rotation, Vector2 shift)
        {
            TrafficLightObjectInformation trafficLightObjectInformation = new TrafficLightObjectInformation(this.position, this.rotation, this.redLightForBusRectangle, this.redLightForCarRectangle);

            trafficLightObjectInformation.RotateAndMove(rotation, shift);

            return trafficLightObjectInformation;
        }

        /// <summary>
        /// liczy wszystkie parametry na podstawie woltu skrzyzowania i jego wielkosci
        /// </summary>
        /// <param name="direction">wlot skrzyzowania</param>
        /// <param name="junctionSize">wielkosc skrzyzowania</param>
        protected void ComputeParameters(Direction direction, Vector2 junctionSize, Vector2 trafficLightSize)
        {
            // zmienne pomocnicze:
            float x, y;
            int width, height;
            Vector2 pos;

            // obliczamy rotacje
            rotation = (int)direction * 90.0f;

            // obliczamy pozycje srodka swiatla
            Vector2 junctionOrigin = junctionSize / 2;

            x = -GameParams.streetWidth - GameParams.lightDistanceFromStreet - trafficLightSize.X / 2;
            y = -junctionOrigin.Y - trafficLightSize.Y / 2;
            position = Helper.ComputeRotation(new Vector2(x, y), new Vector2(0, 0), rotation);

            // obliczamy pozycje prostokata dla aut
            width = (int)(GameParams.streetWidth / 2);
            height = GameParams.redLightRectangleHeight;
            x = -width / 2;
            y = -junctionOrigin.Y;
            pos = Helper.ComputeRotation(new Vector2(x, y), new Vector2(0, 0), rotation);
            if ((int)direction % 2 != 0)
            {
                int temp = width;
                width = height;
                height = temp;
            }
            redLightForCarRectangle = new Rectangle((int)pos.X, (int)pos.Y, width, height);

            // obliczamy pozycje prostokata dla autobusu
            width = (int)(GameParams.streetWidth + GameParams.additionalWidthForBusRedLightRectangle);
            height = GameParams.redLightRectangleHeight;
            x = 0;
            y = -junctionOrigin.Y;
            pos = Helper.ComputeRotation(new Vector2(x, y), new Vector2(0, 0), rotation);
            if ((int)direction % 2 != 0)
            {
                int temp = width;
                width = height;
                height = temp;
            }
            redLightForBusRectangle = new Rectangle((int)pos.X, (int)pos.Y, width, height);
        }
    }

    class TrafficLightBasicPair
    {
        public TrafficLightObjectInformation[] pair1 { get; private set; } // pierwsza para swiatel (pierwsze napotkane wyjscie liczac od gory w prawo)
        public TrafficLightObjectInformation[] pair2 { get; private set; } // druga para swiatel

        protected TrafficLightBasicPair()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directions">kierunki wychodzenia tras z skrzyzowania</param>
        /// <param name="size">rozmiar skrzyzowania</param>
        public TrafficLightBasicPair(Direction[] directions, Vector2 size, Vector2 trafficLightSize)
        {
            CreatePairs(directions, size, trafficLightSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directions">kierunki wychodzenia tras z skrzyzowaia</param>
        /// <param name="size">rozmiar skrzyzowania</param>
        private void CreatePairs(Direction[] directions, Vector2 junctionSize, Vector2 trafficLightSize)
        {
            Direction search = (Direction)((int)(directions[0] + 2) % 4); // szukane drugie wyjscie (przeciwne do pierwszego)
            int searchIndex = -1; // indeks znalezionego drugiego wyjscia

            // znajdujemy pierwszą parę świateł
            for (int i = 1; i < directions.Length; ++i)
            {
                if (directions[i] == search)
                {
                    pair1 = new TrafficLightObjectInformation[2];
                    pair1[0] = new TrafficLightObjectInformation(directions[0], junctionSize, trafficLightSize);
                    pair1[1] = new TrafficLightObjectInformation(directions[i], junctionSize, trafficLightSize);
                    searchIndex = i;
                    break;
                }
            }

            if (searchIndex == -1)
            {
                pair1 = new TrafficLightObjectInformation[1];
                pair1[0] = new TrafficLightObjectInformation(directions[0], junctionSize, trafficLightSize);
            }

            // znajdujemy drugą parę świateł
            List<int> pair2Index = new List<int>(2); // lista z indeksami kierunków drugiej pary świateł
            for (int j = 1; j < directions.Length; ++j)
            {
                if (j != searchIndex)
                {
                    pair2Index.Add(j); // jezeli ten kierunek nie byl wczesniej przypisany to dodajemy
                }
            }

            pair2 = new TrafficLightObjectInformation[pair2Index.Count];
            pair2[0] = new TrafficLightObjectInformation(directions[pair2Index[0]], junctionSize, trafficLightSize);
            if (pair2Index.Count > 1)
                pair2[1] = new TrafficLightObjectInformation(directions[pair2Index[1]], junctionSize, trafficLightSize);
        }
    }

    class TrafficLightPair
    {
        public TrafficLightState trafficLightState { get; private set; } // stan swiatel
        public TrafficLightObjectInformation[] trafficLightObjects { get; private set; } // 1 lub 2 obiekty swiatel przechowujace prostokaty do wykrywania czerwonego swiatla
        public float redInterval { get; private set; } // czas trwania czerwonego swiatla

        public TrafficLightPair(float redInterval, TrafficLightObjectInformation[] trafficLightObjects, TrafficLightState trafficLightState)
        {
            this.redInterval = redInterval;
            this.trafficLightObjects = trafficLightObjects;
            this.trafficLightState = trafficLightState;
        }

        /// <summary>
        /// włącza następny stan świateł
        /// </summary>
        public void NextLightState()
        {
            trafficLightState = (TrafficLightState)((int)(trafficLightState + 1) % 4);
        }
    }

    class TrafficLight
    {
        public TrafficLightPair pair1 { get; private set; } // 1 para świateł
        public TrafficLightPair pair2 { get; private set; } // 2 para świateł

        public TrafficLight(TrafficLightPair pair1, TrafficLightPair pair2)
        {
            this.pair1 = pair1;
            this.pair2 = pair2;
        }
    }
}
