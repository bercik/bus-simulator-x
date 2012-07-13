using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class BusLogic
    {
        private Vector2 position;
        private float direction;
        private float speed;
        private float sideAcc;
        private float width;
        private float height;

        public BusLogic(float x, float y)
        {
            this.position.X = x;
            this.position.Y = y;
        }

        public Vector2 GetBusPosition()
        {
            Vector2 busPosition;

            busPosition.X = position.X + (height * float(Math.Sin(direction)) / 2);
            busPosition.Y = ;

            return busPosition;
        }
    }
}
