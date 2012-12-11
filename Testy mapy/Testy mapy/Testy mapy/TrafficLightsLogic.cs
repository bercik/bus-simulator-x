using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    enum TrafficLightState { green = 0, yellow = 1, red = 2, redYellow = 3 } // stan swiatel (zielony, zolty, czerwony, zolty i czerwony)

    class TrafficLightBasicObjectInformation
    {
        Vector2 position; // pozycja światla
        float rotation; // rotacja
        Rectangle redLightForBusRectangle; // prostokat do wykrywania czerwonego swiatla dla autobusu
        Rectangle redLightForCarRectangle; // prostokat do wykrywania czerwonego swiatla dla aut

        protected TrafficLightBasicObjectInformation()
        {

        }

        public TrafficLightBasicObjectInformation(Direction direction, Vector2 junctionSize, Vector2 trafficLightSize)
        {
            ComputeParameters(direction, junctionSize, trafficLightSize);
        }

        /// <summary>
        /// liczy wszystkie parametry na podstawie woltu skrzyzowania i jego wielkosci
        /// </summary>
        /// <param name="direction">wlot skrzyzowania</param>
        /// <param name="junctionSize">wielkosc skrzyzowania</param>
        private void ComputeParameters(Direction direction, Vector2 junctionSize, Vector2 trafficLightSize)
        {
            // zmienne pomocnicze:
            float x, y;
            int width, height;
            Vector2 pos;

            // obliczamy rotacje
            rotation = (int)direction * 90.0f;

            // obliczamy pozycje srodka swiatla
            Vector2 junctionOrigin = junctionSize / 2;

            x = -GameParams.streetWidth / 2 - GameParams.lightDistanceFromStreet - trafficLightSize.X / 2;
            y = -junctionOrigin.Y - trafficLightSize.Y / 2;
            position = Helper.ComputeRotation(new Vector2(x, y), new Vector2(0, 0), rotation);

            // obliczamy pozycje prostokata dla aut
            width = (int)(GameParams.streetWidth / 2);
            height = GameParams.redLightRectangleHeight;
            x = -width / 2;
            y = -junctionOrigin.Y;
            pos = Helper.ComputeRotation(new Vector2(x, y), new Vector2(0, 0), rotation);

            if ((int)direction % 2 == 0) // jezeli kierunek to gora lub dol
            {
                pos.X -= width / 2;
                pos.Y -= height / 2;

                redLightForCarRectangle = new Rectangle((int)Math.Round(pos.X), (int)Math.Round(pos.Y), width, height);
            }
            else // inaczej jzezeli kierunek to prawo lub lewo zamieniamy wysokosc z szerokoscia
            {
                pos.Y -= width / 2;
                pos.X -= height / 2;

                redLightForCarRectangle = new Rectangle((int)Math.Round(pos.X), (int)Math.Round(pos.Y), height, width);
            }

            // obliczamy pozycje prostokata dla autobusu
            width = (int)(GameParams.streetWidth + GameParams.additionalWidthForBusRedLightRectangle);
            height = GameParams.redLightRectangleHeight;
            x = 0;
            y = -junctionOrigin.Y;
            pos = Helper.ComputeRotation(new Vector2(x, y), new Vector2(0, 0), rotation);

            if ((int)direction % 2 == 0) // jezeli kierunek to gora lub dol
            {
                pos.X -= width / 2;
                pos.Y -= height / 2;

                redLightForBusRectangle = new Rectangle((int)Math.Round(pos.X), (int)Math.Round(pos.Y), width, height);
            }
            else // inaczej jzezeli kierunek to prawo lub lewo zamieniamy wysokosc z szerokoscia
            {
                pos.Y -= width / 2;
                pos.X -= height / 2;

                redLightForBusRectangle = new Rectangle((int)Math.Round(pos.X), (int)Math.Round(pos.Y), height, width);
            }
        }
    }

    class TrafficLightBasicPair
    {
        TrafficLightBasicObjectInformation[] pair1; // pierwsza para swiatel (pierwsze napotkane wyjscie liczac od gory w prawo)
        TrafficLightBasicObjectInformation[] pair2; // druga para swiatel

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
                    pair1 = new TrafficLightBasicObjectInformation[2];
                    pair1[0] = new TrafficLightBasicObjectInformation(directions[0], junctionSize, trafficLightSize);
                    pair1[1] = new TrafficLightBasicObjectInformation(directions[i], junctionSize, trafficLightSize);
                    searchIndex = i;
                    break;
                }
            }

            if (searchIndex == -1)
            {
                pair1 = new TrafficLightBasicObjectInformation[1];
                pair1[0] = new TrafficLightBasicObjectInformation(directions[0], junctionSize, trafficLightSize);
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

            pair2 = new TrafficLightBasicObjectInformation[pair2Index.Count];
            pair2[0] = new TrafficLightBasicObjectInformation(directions[pair2Index[0]], junctionSize, trafficLightSize);
            if (pair2Index.Count > 1)
                pair2[1] = new TrafficLightBasicObjectInformation(directions[pair2Index[1]], junctionSize, trafficLightSize);
        }
    }

    class TrafficLightObjectInformation
    {
        Rectangle redLightForBusRectangle; // prostokat do wykrywania czerwonego swiatla dla autobusu
        Rectangle redLightForCarRectangle; // prostokat do wykrywania czerwonego swiatla dla aut
    }

    class TrafficLightPair
    {
        TrafficLightState trafficLightState; // stan swiatel
        TrafficLightObjectInformation[] trafficLightObjects; // 1 lub 2 obiekty swiatel przechowujace prostokaty do wykrywania czerwonego swiatla
        float redInterval; // czas trwania czerwonego swiatla
    }

    class TrafficLights
    {
        TrafficLightPair pair1; // 1 para świateł
        TrafficLightPair pair2; // 2 para świateł
    }
}
