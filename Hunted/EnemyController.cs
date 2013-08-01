using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted
{
    public class EnemyController
    {
        public static EnemyController Instance;

        public Texture2D SpriteSheet;

        public List<AIDude> Enemies = new List<AIDude>();

        GraphicsDevice graphicsDevice;
        LightingEngine lightingEngine;

        public EnemyController()
        {
            Instance = this;
        }

        public void LoadContent(ContentManager content, GraphicsDevice gd, LightingEngine le)
        {
            SpriteSheet = content.Load<Texture2D>("dude");

            graphicsDevice = gd;
            lightingEngine = le;
        }

        public void Update(GameTime gameTime, Map gameMap, HeroDude gameHero, bool[,] mapFog, Camera gameCamera)
        {
            int count = 0;
            foreach (AIDude e in Enemies.Where(en => (gameHero.Position - en.Position).Length() < 4000f))
            {
                count++;
                e.Update(gameTime, gameMap, gameHero, mapFog, gameCamera);
            }

            // Spawn some new enemies
            if (count < 10)
            {
                Vector2 pos = Helper.RandomPointInCircle(gameHero.Position, 2000f, 4000f);
                if (!gameMap.CheckTileCollision(pos) && pos.X > 0 && pos.X < (gameMap.Width * gameMap.TileWidth) && pos.Y > 0 && pos.Y < (gameMap.Height * gameMap.TileHeight))
                {
                    AIDude newDude = new AIDude(pos);
                    newDude.LoadContent(SpriteSheet, graphicsDevice, lightingEngine, gameHero);
                    newDude.Health = 10 + Helper.Random.Next(30);
                    Enemies.Add(newDude);
                }
            }

            Enemies.RemoveAll(e => !e.Active);
        }

        public void Draw(SpriteBatch sb, LightingEngine lightingEngine, HeroDude gameHero)
        {
            foreach (AIDude e in Enemies.Where(en => (gameHero.Position - en.Position).Length() < 2000f))
            {
                e.Draw(sb, lightingEngine);
            }
        }

        public void DrawLightBlock(SpriteBatch sb, HeroDude gameHero)
        {
            foreach (AIDude e in Enemies.Where(en => (gameHero.Position - en.Position).Length() < 2000f))
            {
                e.DrawLightBlock(sb);
            }
        }

        public void DrawShadows(SpriteBatch sb, LightingEngine lightingEngine, HeroDude gameHero)
        {
            foreach (AIDude e in Enemies.Where(en => (gameHero.Position - en.Position).Length() < 2000f))
            {
                e.DrawShadows(sb, lightingEngine);
            }
        }

        internal void DiscoverGeneral(Vector2 vector2)
        {
            foreach (AIDude e in Enemies.Where(en => en.IsGeneral).OrderBy(en => (en.Position - vector2).Length()))
            {
                if (!e.Discovered)
                {
                    e.Discovered = true;
                    Hud.Instance.Ticker.AddLine("> A General's location has been revealed! (Tab/Back for map)");
                    break;
                }
                else break;
            }
        }
    }
}
