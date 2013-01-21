using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    enum TrafficLightState { green = 0, yellow = 1, red = 2, redYellow = 3 } // stan swiatel (zielony, zolty, czerwony, zolty i czerwony)

    class TrafficLightsLogic
    {
        public List<TrafficLightJunction> trafficLightJunctions { get; private set; }
        public Vector2 trafficLightSize { get; private set; }

        public TrafficLightsLogic()
        {
            trafficLightJunctions = new List<TrafficLightJunction>();
        }

        public void SetTrafficLightSize(Vector2 trafficLightSize)
        {
            this.trafficLightSize = trafficLightSize;
        }

        public void AddTrafficLightJunction(TrafficLightJunction trafficLightJunction)
        {
            trafficLightJunctions.Add(trafficLightJunction);
        }

        public void ClearTrafficLightJunctions()
        {
            trafficLightJunctions.Clear();
        }

        public List<TrafficLightObject> GetTrafficLights()
        {
            List<TrafficLightObject> trafficLightObjects = new List<TrafficLightObject>();

            for (int i = 0; i < trafficLightJunctions.Count; ++i)
            {
                TrafficLight trafficLight = trafficLightJunctions[i].trafficLight;

                foreach (TrafficLightObjectInformation tlo in trafficLight.pair1.trafficLightObjects)
                {
                    trafficLightObjects.Add(new TrafficLightObject("light", tlo.position, trafficLightSize, tlo.rotation, i, 1));
                }

                foreach (TrafficLightObjectInformation tlo in trafficLight.pair2.trafficLightObjects)
                {
                    trafficLightObjects.Add(new TrafficLightObject("light", tlo.position, trafficLightSize, tlo.rotation, i, 2));
                }
            }

            return trafficLightObjects;
        }

        public bool IsTrafficLightJunctionInRange(TrafficLightJunction trafficLightJunction)
        {
            Vector2 distance = trafficLightJunction.pos - Helper.busPos;

            if ((Math.Abs(distance.X) <= GameParams.trafficDistanceToDelete)
                 && (Math.Abs(distance.Y) <= GameParams.trafficDistanceToDelete))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<TrafficLightJunction> GetTrafficLightJunctionsInRange()
        {
            List<TrafficLightJunction> trafficLightJunctionsInRange = new List<TrafficLightJunction>();

            foreach (TrafficLightJunction trafficLightJunction in trafficLightJunctions)
            {
                if (IsTrafficLightJunctionInRange(trafficLightJunction))
                    trafficLightJunctionsInRange.Add(trafficLightJunction);
            }

            return trafficLightJunctionsInRange;
        }

        public void GetRedLightRectangles(out List<Rectangle> redLightRectanglesForCars, out List<TrafficLightRectangle> redLightRectanglesForBus)
        {
            redLightRectanglesForBus = new List<TrafficLightRectangle>();
            redLightRectanglesForCars = new List<Rectangle>();

            List<TrafficLightJunction> trafficLightJunctions = GetTrafficLightJunctionsInRange();

            foreach (TrafficLightJunction junction in trafficLightJunctions)
            {
                redLightRectanglesForBus.AddRange(junction.trafficLight.getRedLightForBusRectangles());
                redLightRectanglesForCars.AddRange(junction.trafficLight.getRedLightForCarsRectangles());
            }
        }

        public void Update(TimeSpan framesInterval)
        {
            List<TrafficLightJunction> trafficLightJunctionsInRange = GetTrafficLightJunctionsInRange();

            foreach (TrafficLightJunction trafficLightJunction in trafficLightJunctionsInRange)
            {
                trafficLightJunction.trafficLight.Update(framesInterval);
            }
        }

        public TrafficLightState GetTrafficLightPairState(int junctionIndex, int pairIndex)
        {
            return trafficLightJunctions[junctionIndex].trafficLight.GetTrafficLightPairState(pairIndex);
        }
    }

    struct TrafficLightRectangle
    {
        public Rectangle redLightRectangle; // prostokat do wykrywania czerwonego swiatla
        public Direction direction; // po ktorej stronie skrzyzowania znajduje sie dany prostokat
    }

    class TrafficLightObjectInformation
    {
        public Vector2 position { get; private set; } // pozycja światla
        public float rotation { get; private set; } // rotacja
        protected TrafficLightRectangle redLightForBusRectangle; // prostokat do wykrywania czerwonego swiatla dla autobusu
        protected Rectangle redLightForCarRectangle; // prostokat do wykrywania czerwonego swiatla dla aut

        protected TrafficLightObjectInformation()
        {

        }

        public TrafficLightObjectInformation(Vector2 position, float rotation, TrafficLightRectangle redLightForBusRectangle, Rectangle redLightForCarRectangle)
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

        public TrafficLightRectangle getRedLightForBusRectangle()
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
            pos = new Vector2((int)redLightForBusRectangle.redLightRectangle.X, (int)redLightForBusRectangle.redLightRectangle.Y);
            pos = Helper.ComputeRotation(pos, new Vector2(0, 0), rotation);
            pos += shift;

            if ((rotation / 90) % 2 != 0) // jezeli przy rotacji zmieniamy kierunek z poziomego na pionowy lub odwrotnie
            {
                width = redLightForBusRectangle.redLightRectangle.Height;
                height = redLightForBusRectangle.redLightRectangle.Width;
            }
            else // jezeli nie zmieniamy kierunku
            {
                width = redLightForBusRectangle.redLightRectangle.Width;
                height = redLightForBusRectangle.redLightRectangle.Height;
            }

            pos.Y -= height / 2;
            pos.X -= width / 2;
            redLightForBusRectangle.redLightRectangle = new Rectangle((int)Math.Round(pos.X), (int)Math.Round(pos.Y), width, height);
            redLightForBusRectangle.direction = (Direction)(((int)redLightForBusRectangle.direction + (int)(rotation / 90)) % 4);

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

            this.rotation = ((this.rotation + rotation + 180) / 90) * 90;
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
            width = (int)(GameParams.streetWidth);
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
            width = (int)((GameParams.streetWidth * 2) + GameParams.additionalWidthForBusRedLightRectangle);
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
            redLightForBusRectangle.redLightRectangle = new Rectangle((int)pos.X, (int)pos.Y, width, height);
            redLightForBusRectangle.direction = direction;
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
        public TrafficLightState trafficLightState { get; set; } // stan swiatel
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

        public TrafficLightRectangle[] getRedLightForBusRectangles()
        {
            TrafficLightRectangle[] redLightForBusRectangles = new TrafficLightRectangle[trafficLightObjects.Length];

            for (int i = 0; i < redLightForBusRectangles.Length; ++i)
            {
                redLightForBusRectangles[i] = trafficLightObjects[i].getRedLightForBusRectangle();
            }

            return redLightForBusRectangles;
        }

        public Rectangle[] getRedLightForCarsRectangles()
        {
            Rectangle[] redLightForCarsRectangles = new Rectangle[trafficLightObjects.Length];

            for (int i = 0; i < redLightForCarsRectangles.Length; ++i)
            {
                redLightForCarsRectangles[i] = trafficLightObjects[i].getRedLightForCarRectangle();
            }

            return redLightForCarsRectangles;
        }
    }

    class TrafficLight
    {
        public TrafficLightPair pair1 { get; private set; } // 1 para świateł
        public TrafficLightPair pair2 { get; private set; } // 2 para świateł
        public float trafficLightIntervalBeforeRedYellowStart { get; private set; } // interwał pomiędzy zmianą na czerwone światło dla jednej pary świateł, a uruchomieniem czerwono zółtego na drugiej parze

        // zmienne pomocnicze
        double actualInterval; // pozostaly czas do zmiany stanu swiatel
        bool nextPair1; // czy nastepna para swiatel zmieniona na zielona ma byc para 1

        public TrafficLight(TrafficLightPair pair1, TrafficLightPair pair2, float trafficLightIntervalBeforeRedYellowStart)
        {
            Random rand = new Random();

            this.pair1 = pair1;
            this.pair2 = pair2;
            this.trafficLightIntervalBeforeRedYellowStart = trafficLightIntervalBeforeRedYellowStart;

            // losujemy ktorej pary swiatel stan ma byc pierwszy jako czerwony, a ktorej jako zielony
            if (rand.Next(1) == 0)
            {
                this.pair1.trafficLightState = TrafficLightState.green;
                this.pair2.trafficLightState = TrafficLightState.red;
                actualInterval = this.pair2.redInterval;
            }
            else
            {
                this.pair1.trafficLightState = TrafficLightState.red;
                this.pair2.trafficLightState = TrafficLightState.green;
                actualInterval = this.pair1.redInterval;
            }
        }

        public void Update(TimeSpan framesInterval)
        {
            actualInterval -= framesInterval.TotalSeconds; // zmniejszamy aktualny interwał o czas jaki upłynął

            if (actualInterval < 0) // jezeli jest mniejszy od 0 to zmieniamy aktualny stan
            {
                ChangeLightState();
            }
        }

        public TrafficLightState GetTrafficLightPairState(int pairIndex)
        {
            if (pairIndex == 1)
            {
                return pair1.trafficLightState;
            }
            else
            {
                return pair2.trafficLightState;
            }
        }

        public void ChangeLightState()
        {
            if (pair1.trafficLightState == TrafficLightState.redYellow)
            {
                pair1.NextLightState();
                actualInterval = pair2.redInterval;
            }
            else if (pair1.trafficLightState == TrafficLightState.green)
            {
                pair1.NextLightState();
                actualInterval = GameParams.trafficLightYellowInterval;
            }
            else if (pair1.trafficLightState == TrafficLightState.yellow)
            {
                pair1.NextLightState();
                nextPair1 = false;
                actualInterval = trafficLightIntervalBeforeRedYellowStart;
            }
            else if (pair1.trafficLightState == TrafficLightState.red && pair2.trafficLightState == TrafficLightState.red)
            {
                if (nextPair1)
                {
                    pair1.NextLightState();
                    actualInterval = GameParams.trafficLightRedYellowInterval;
                }
                else
                {
                    pair2.NextLightState();
                    actualInterval = GameParams.trafficLightRedYellowInterval;
                }
            }
            else if (pair2.trafficLightState == TrafficLightState.redYellow)
            {
                pair2.NextLightState();
                actualInterval = pair1.redInterval;
            }
            else if (pair2.trafficLightState == TrafficLightState.green)
            {
                pair2.NextLightState();
                actualInterval = GameParams.trafficLightYellowInterval;
            }
            else if (pair2.trafficLightState == TrafficLightState.yellow)
            {
                pair2.NextLightState();
                nextPair1 = true;
                actualInterval = trafficLightIntervalBeforeRedYellowStart;
            }
        }

        public List<TrafficLightRectangle> getRedLightForBusRectangles()
        {
            List<TrafficLightRectangle> redLightForBusRectangles = new List<TrafficLightRectangle>();

            if (pair1.trafficLightState == TrafficLightState.red)
                redLightForBusRectangles.AddRange(pair1.getRedLightForBusRectangles());

            if (pair2.trafficLightState == TrafficLightState.red)
                redLightForBusRectangles.AddRange(pair2.getRedLightForBusRectangles());

            return redLightForBusRectangles;
        }

        public List<Rectangle> getRedLightForCarsRectangles()
        {
            List<Rectangle> redLightForCarsRectangles = new List<Rectangle>();

            if (pair1.trafficLightState == TrafficLightState.red || pair1.trafficLightState == TrafficLightState.yellow)
                redLightForCarsRectangles.AddRange(pair1.getRedLightForCarsRectangles());

            if (pair2.trafficLightState == TrafficLightState.red || pair2.trafficLightState == TrafficLightState.yellow)
                redLightForCarsRectangles.AddRange(pair2.getRedLightForCarsRectangles());

            return redLightForCarsRectangles;
        }
    }
}
