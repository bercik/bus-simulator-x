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
    public static class Sound
    {
        static Song backgroundSong;

        public static void LoadContent(ContentManager content)
        {
            backgroundSong = content.Load<Song>("sounds/background_song");
        }

        public static void PlayBackgroundSong()
        {
            MediaPlayer.IsRepeating = true;
            //MediaPlayer.Play(backgroundSong);
        }
    }
}
