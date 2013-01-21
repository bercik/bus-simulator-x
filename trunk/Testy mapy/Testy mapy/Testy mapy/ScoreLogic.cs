using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Testy_mapy
{
    public struct Action
    {
        public string message { get; private set; } // wiadomosc
        public int points { get; private set; } // ilosc punktow

        public Action(int points, string message) : this()
        {
            this.points = points;
            this.message = message;
        }

        public Action(Action action, float multiplier) : this()
        {
            this.points = (int)(action.points * multiplier);
            this.message = action.message + ": " + points.ToString() + " pkt.";
        }
    }

    public static class Score
    {
        private static float totalScore = 0.0f; // całkowity wynik
        private static float addedScore = 0.0f; // liczba dodanych punktów
        private static float removedScore = 0.0f; // liczba odjętych punktów

        private static Dictionary<string, Action> actions; // slownik mozliwych akcjii
        private static List<Action> currentActions; // lista obecnych akcjii

        static Score()
        {
            actions = new Dictionary<string, Action>();
            currentActions = new List<Action>();

            // DODAJ AKCJE TUTAJ:
            actions.Add("killed pedestrian", new Action(-100, "Zabicie pieszego"));
            actions.Add("click pause", new Action(50, "Klikniecie pauzy"));

            // Przed tym komentarzem Robert, po nim Filip.
            actions.Add("pedestrian getting in", new Action(50, "Zabranie pieszego"));
            actions.Add("pedestrian getting out", new Action(50, "Wysadzenie pieszego"));
        }

        public static int GetAddedScore()
        {
            return (int)addedScore;
        }

        public static int GetRemovedScore()
        {
            return (int)removedScore;
        }

        public static int GetTotalScore()
        {
            return (int)totalScore;
        }

        public static void AddAction(string actionName, float multiplier)
        {
            Action action = new Action(actions[actionName], multiplier);

            if (action.points < 0)
                removedScore -= action.points;
            else
                addedScore += action.points;

            totalScore += action.points;

            currentActions.Add(action);
        }

        public static bool GetNextAction(out string currentMessage, out Color currentColor)
        {
            if (currentActions.Count > 0)
            {
                currentMessage = currentActions[0].message;
                currentColor = (currentActions[0].points < 0) ? Color.Red : Color.Green;

                currentActions.RemoveAt(0);

                return true;
            }
            else
            {
                currentMessage = "";
                currentColor = Color.White;

                return false;
            }
        }
    }
}
