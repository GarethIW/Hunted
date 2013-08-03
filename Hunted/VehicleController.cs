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
    public class VehicleController
    {
        public static VehicleController Instance;

        public Texture2D SpriteSheet;

        public List<Vehicle> Vehicles = new List<Vehicle>();

        GraphicsDevice graphicsDevice;
        LightingEngine lightingEngine;


        public VehicleController()
        {
            Instance = this;
        }

        public void LoadContent(ContentManager content, GraphicsDevice gd, LightingEngine le)
        {
            SpriteSheet = content.Load<Texture2D>("vehicles");

            graphicsDevice = gd;
            lightingEngine = le;
        }

        public void Update(GameTime gameTime, Map gameMap, HeroDude gameHero, Camera gameCamera)
        {
            int count = 0;
            foreach (Vehicle v in Vehicles.Where(veh => (gameHero.Position - veh.Position).Length() < 4000f))
            {
                count++;
                v.Update(gameTime, gameMap, gameHero, gameCamera);

                if ((gameHero.Position - v.Position).Length() < 50f)
                {
                   
                }
            }

            Vehicles.RemoveAll(it => !it.Active);
            
        }

        public void Spawn(ItemType type, Vector2 pos)
        {
            //Items.Add(new Item(type, pos));
        }



        public void Draw(SpriteBatch sb, LightingEngine lightingEngine, HeroDude gameHero)
        {
            foreach (Vehicle v in Vehicles.Where(veh => (gameHero.Position - veh.Position).Length() < 2000f && !(veh is Chopper)))
            {
                
                v.Draw(sb, lightingEngine);
            }

            // Draw helis on top of other vehicles because of rotor blades
            foreach (Vehicle v in Vehicles.Where(veh => (gameHero.Position - veh.Position).Length() < 2000f && veh is Chopper))
            {
                if (((Chopper)v).Height > 0f) continue;
                v.Draw(sb, lightingEngine);
            }

            
        }

        public void DrawLightBlock(SpriteBatch sb, HeroDude gameHero)
        {
            foreach (Vehicle v in Vehicles.Where(veh => (gameHero.Position - veh.Position).Length() < 2000f))
            {
                if (v is Chopper && ((Chopper)v).Height > 0f) continue;

                v.DrawLightBlock(sb);
            }
        }

        public void DrawShadows(SpriteBatch sb, LightingEngine lightingEngine, HeroDude gameHero)
        {
            foreach (Vehicle v in Vehicles.Where(veh => (gameHero.Position - veh.Position).Length() < 2000f))
            {
                if (v is Chopper && ((Chopper)v).Height > 0f) continue;

                v.DrawShadows(sb, lightingEngine);
            }
        }

        internal void DrawHelis(SpriteBatch sb, LightingEngine lightingEngine, HeroDude gameHero)
        {
            foreach (Vehicle v in Vehicles.Where(veh => (gameHero.Position - veh.Position).Length() < 2000f))
            {
                if (v is Chopper && ((Chopper)v).Height > 0f)
                {
                    ((Chopper)v).DrawInAir(sb, lightingEngine);
                }
            }
        }

        internal void DrawHeliShadows(SpriteBatch sb, LightingEngine lightingEngine, HeroDude gameHero)
        {
            foreach (Vehicle v in Vehicles.Where(veh => (gameHero.Position - veh.Position).Length() < 2000f))
            {
                if (v is Chopper && ((Chopper)v).Height > 0f)
                {
                    ((Chopper)v).DrawShadowsInAir(sb, lightingEngine);
                }
            }
        }

        internal bool CheckVehicleCollision(Vector2 pos)
        {

            foreach (Vehicle v in Vehicles.Where(veh=>(veh.Position - pos).Length()<500f))
            {
                if(Helper.IsPointInShape(pos, v.CollisionVerts)) return true;
            }

            return false;
        }

        internal void ClearSpawn(Vector2 pos)
        {
            foreach (Vehicle v in Vehicles.Where(e => (pos - e.Position).Length() < 800f).ToList())
            {
                foreach(LightSource l in v.Lights) LightingEngine.Instance.RemoveSource(l);
                v.Active = false;
            }
        }
    }
}
