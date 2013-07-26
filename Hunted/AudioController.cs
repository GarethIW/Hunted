using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using TiledLib;


namespace Hunted
{
    public static class AudioController
    {
        public static float sfxvolume = 1f;
        public static float musicvolume = 0.5f;

        public static Random randomNumber = new Random();

        public static Dictionary<string, SoundEffect> effects;

        public static Dictionary<string, SoundEffectInstance> songs;

        static string playingTrack = "";
        static bool isPlaying;

        public static string currentlyPlaying = "";

        public static int currentTrack = 0;

        public static void LoadContent(ContentManager content)
        {
            effects = new Dictionary<string, SoundEffect>();

            effects.Add("explode", content.Load<SoundEffect>("audio/sfx/explode"));
            effects.Add("hit", content.Load<SoundEffect>("audio/sfx/hit"));
            effects.Add("machinegun", content.Load<SoundEffect>("audio/sfx/machinegun"));
            effects.Add("mortar", content.Load<SoundEffect>("audio/sfx/mortar"));
            effects.Add("pistol", content.Load<SoundEffect>("audio/sfx/pistol"));
            effects.Add("shotgun", content.Load<SoundEffect>("audio/sfx/shotgun"));
            effects.Add("smg", content.Load<SoundEffect>("audio/sfx/smg"));
            effects.Add("sniper", content.Load<SoundEffect>("audio/sfx/sniper"));
            effects.Add("sword", content.Load<SoundEffect>("audio/sfx/sword"));
            effects.Add("fstep-grass", content.Load<SoundEffect>("audio/sfx/fstep-grass"));
            effects.Add("fstep-sand", content.Load<SoundEffect>("audio/sfx/fstep-sand"));
            effects.Add("fstep-dirt", content.Load<SoundEffect>("audio/sfx/fstep-dirt"));
            effects.Add("fstep-concrete", content.Load<SoundEffect>("audio/sfx/fstep-concrete"));
            effects.Add("ammo", content.Load<SoundEffect>("audio/sfx/ammo"));
            effects.Add("ticker", content.Load<SoundEffect>("audio/sfx/typewriter"));
           


            songs = new Dictionary<string, SoundEffectInstance>();
            //songs.Add("0", content.Load<SoundEffect>("music/1").CreateInstance());
            
        }

        public static void LoadMusic(string piece, ContentManager content)
        {
            //if (currentlyPlaying.ToLower() == piece.ToLower()) return;
            //currentlyPlaying = piece;

            //if (!MediaPlayer.GameHasControl) return;

            //if (MediaPlayer.State != MediaState.Stopped) MediaPlayer.Stop();
            ////if (musicInstance != null)
            ////{
            ////    musicInstance.Dispose();
            ////}

            //musicInstance = content.Load<Song>("audio/music/" + piece);
            //MediaPlayer.IsRepeating = true;
            //// MediaPlayer.Volume = musicvolume;
            //MediaPlayer.Play(musicInstance);

            //if (!OptionsMenuScreen.music) MediaPlayer.Pause();
        }

        public static void PlayMusic()
        {
            PlayMusic(currentTrack.ToString());
            currentTrack++;
            if (currentTrack == 5) currentTrack = 0;
        }

        public static void PlayMusic(string track)
        {
            playingTrack = track;
            isPlaying = true;
            songs[track].IsLooped = true;
            songs[track].Volume = 0f;
            songs[track].Play();
        }

        public static void StopMusic()
        {

            isPlaying = false;
        }

        public static void ToggleMusic()
        {

            //if (OptionsMenuScreen.music)
            //{
            //    MediaPlayer.Resume();
            //}
            //else
            //    MediaPlayer.Pause();
        }

        public static void PlaySFX(string name)
        {
            //if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, 0f, 0f);
        }
        public static void PlaySFX(string name, float pitch)
        {
            //if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, pitch, 0f);
        }
        public static void PlaySFX(string name, float volume, float pitch, float pan)
        {
           // if (OptionsMenuScreen.sfx)
            if (pan < -1f || pan > 1f) return;
            volume = MathHelper.Clamp(volume, 0f, 1f);
            effects[name].Play(volume * sfxvolume, pitch, pan);
        }
        public static void PlaySFX(string name, float minpitch, float maxpitch)
        {
           // if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), 0f);
        }

        internal static void PlaySFX(string name, float volume, float minpitch, float maxpitch, Vector2 Position)
        {
            Vector2 screenPos = Vector2.Transform(Position, Camera.Instance.CameraMatrix);
            float dist = (Camera.Instance.Position - Position).Length();
            if (dist < 2000f)
            {
                float pan = MathHelper.Clamp((screenPos.X - (Camera.Instance.Width / 2)) / (Camera.Instance.Width / 2), -1f, 1f);
                effects[name].Play(((1f/2000f) * (2000f-dist)) * volume * sfxvolume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), pan);
            }
        }


        public static void Update(GameTime gameTime)
        {

            if (playingTrack == "") return;

            if(isPlaying)
                if (songs[playingTrack].Volume < musicvolume) songs[playingTrack].Volume += 0.01f;

             if (!isPlaying)
                 if (songs[playingTrack].Volume > 0) songs[playingTrack].Volume -= 0.01f;
                 else songs[playingTrack].Stop();

            // if (MediaPlayer.Volume > musicvolume) MediaPlayer.Volume = musicvolume;
        }

        public static void Unload()
        {

        }



       
    }
}
