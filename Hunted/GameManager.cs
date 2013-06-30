using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted
{
    public static class GameManager
    {
        public static Map Map;
        public static Camera Camera;

        public static SpriteFont Font;

        //public static SpawnerController SpawnerController;
        //public static GravPadController GravPadController;
        //public static EnemyController EnemyController;
        //public static ProjectileManager ProjectileManager;
        //public static ParticleController ParticleController;
        //public static WaterController WaterController;

        //public static Hero Hero;

        //public static HUD HUD;

        public static int CurrentScene;
        public static double SceneTime;
        public static Matrix SceneMatrix;
    }
}
