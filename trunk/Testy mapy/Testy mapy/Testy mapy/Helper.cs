using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    static class Helper
    {
        public static Vector2 screenSize;

        public static void SetScreenSize(float width, float height)
        {
            screenSize = new Vector2(width, height);
        }

        public static Vector2 MapPosToScreenPos(Vector2 currentMapPos, Vector2 pos)
        {
            return pos - (currentMapPos - screenSize / 2);
        }
    }
}
