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
    public class ItemController
    {
        public static ItemController Instance;

        public Texture2D SpriteSheet;

        public List<Item> Items = new List<Item>();

        GraphicsDevice graphicsDevice;
        LightingEngine lightingEngine;

        Dictionary<ItemType, Rectangle> rectDict = new Dictionary<ItemType, Rectangle>();

        public ItemController()
        {
            Instance = this;
        }

        public void LoadContent(ContentManager content, GraphicsDevice gd, LightingEngine le)
        {
            SpriteSheet = content.Load<Texture2D>("items");

            rectDict.Add(ItemType.Health, new Rectangle(0, 0, 50, 50));
            rectDict.Add(ItemType.Ammo, new Rectangle(50, 0, 50, 50));
            rectDict.Add(ItemType.CompoundMap, new Rectangle(100, 0, 50, 50));
            rectDict.Add(ItemType.GeneralMap, new Rectangle(150, 0, 50, 50));

            graphicsDevice = gd;
            lightingEngine = le;
        }

        public void Update(GameTime gameTime, Map gameMap, HeroDude gameHero, bool[,] mapFog)
        {
            int count = 0;
            foreach (Item i in Items.Where(it => (gameHero.Position - it.Position).Length() < 4000f))
            {
                count++;
                i.Update(gameTime);

                if ((gameHero.Position - i.Position).Length() < 50f)
                {
                    Pickup(i, gameHero, gameMap, mapFog);
                }
            }

            Items.RemoveAll(it => !it.Active);
            
        }

        public void Spawn(ItemType type, Vector2 pos)
        {
            Items.Add(new Item(type, pos));
        }

        void Pickup(Item i, HeroDude gameHero, Map gameMap, bool[,] mapFog)
        {
            switch (i.Type)
            {
                case ItemType.Health:
                    gameHero.Health += 25f;
                    break;
                case ItemType.Ammo:
                    AudioController.PlaySFX("ammo", 1f, 0f, 0f, i.Position);
                    gameHero.Ammo += 10 + Helper.Random.Next(15);
                    break;
                case ItemType.CompoundMap:
                    List<Compound> c = gameMap.FindNearestCompounds(gameHero.Position);
                    if (c[0].Discovered == false)
                    {
                        gameMap.DiscoverCompound(c[0], mapFog);
                        if(!c[0].Bounds.Contains(Helper.VtoP(i.Position/100))) Hud.Instance.Ticker.AddLine("> An enemy compound has been revealed! (Tab/Back for map)");
                        else Hud.Instance.Ticker.AddLine("> This compound has been revealed!");
                    }
                    else 
                    {
                        foreach (Compound nc in c)
                        {
                            if (nc.Discovered == false)
                            {
                                gameMap.DiscoverCompound(nc, mapFog);
                                if (!nc.Bounds.Contains(Helper.VtoP(i.Position / 100))) Hud.Instance.Ticker.AddLine("> An enemy compound has been revealed! (Tab/Back for map)");
                                else Hud.Instance.Ticker.AddLine("> This compound has been revealed!");
                                break;
                            }
                        }
                    }

                    break;
                case ItemType.GeneralMap:
                    EnemyController.Instance.DiscoverGeneral(i.Position);
                    break;
            }

            i.Active = false;
        }

        public void Draw(SpriteBatch sb, LightingEngine lightingEngine, HeroDude gameHero)
        {
            foreach (Item i in Items.Where(it => (gameHero.Position - it.Position).Length() < 2000f))
            {
                sb.Draw(SpriteSheet, i.Position, rectDict[i.Type], lightingEngine.CurrentSunColor, i.Rotation, new Vector2(25, 25), 1f, SpriteEffects.None, 1);
            }
        }

        public void DrawShadows(SpriteBatch sb, LightingEngine lightingEngine, HeroDude gameHero)
        {
            foreach (Item i in Items.Where(it => (gameHero.Position - it.Position).Length() < 2000f))
            {
                for (int s = 1; s < 20; s += 2)
                {
                    Vector2 pos = i.Position + new Vector2(lightingEngine.CurrentShadowVect.X * s, lightingEngine.CurrentShadowVect.Y * s);

                    sb.Draw(SpriteSheet, pos, rectDict[i.Type], Color.Black * 0.03f, i.Rotation, new Vector2(25, 25), 1f, SpriteEffects.None, 1);
                }
            }
        }
    }
}
