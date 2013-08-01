using Hunted.Weapons;
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
    public class HeroDude : Dude
    {

        public HeroDude(Vector2 pos) : base(pos)
        {
            Health = 100f;
            Ammo = 100;
        }

        public void LoadContent(ContentManager content, GraphicsDevice gd, LightingEngine le)
        {
            spriteSheet = content.Load<Texture2D>("dude");
            Initialize(gd, le);

            Weapons.Add(new Knife(this));
            Weapons.Add(new Pistol(this));
            Weapons.Add(new Shotgun(this));
            Weapons.Add(new SMG(this));
            Weapons.Add(new Rifle(this));            
            SelectedWeapon = 0;
        }

        internal void Initialize(GraphicsDevice gd, LightingEngine le)
        {
            HeadTorch = new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None);
            le.LightSources.Add(HeadTorch);
            base.Initialize();
        }

        public override void Update(GameTime gameTime, Map gameMap, bool[,] mapFog)
        {
            base.Update(gameTime, gameMap, mapFog);

            HeadTorch.Position = Helper.PointOnCircle(ref Position, 30, Rotation - MathHelper.PiOver2);
            HeadTorch.Rotation = Rotation - MathHelper.PiOver2;

            foreach (Compound c in gameMap.Compounds)
            {
                if (!c.Discovered && (c.Position-Position).Length()<1000f)
                {
                    gameMap.DiscoverCompound(c, mapFog);
                    Hud.Instance.Ticker.AddLine("> This compound has been revealed!");
                }
            }

            if (drivingVehicle!=null)
            {
                HeadTorch.Active = false;

                Position = drivingVehicle.Position;
                Rotation = drivingVehicle.Rotation + MathHelper.PiOver2;
            }
            else if(!Dead) HeadTorch.Active = true;

            if (Health <= 0 && !Dead)
            {
                deadTime = 5000;
                deadAlpha = 1f;
                Dead = true;
                HeadTorch.Active = false;
            }

            if (Dead)
            {
                if (deadTime < 3000)
                {
                    deadAlpha -= 0.01f;
                }
                if (deadTime < 1000)
                {
                    Position = gameMap.HeroSpawn;
                    Camera.Instance.Position = Position;
                    Camera.Instance.Target = Position;
                    Health = 100f;
                    HeadTorch.Active = true;
                    Dead = false;
                }
            }

            if (!Dead && deadAlpha < 1f) deadAlpha += 0.01f;
        }

        public override void Draw(SpriteBatch sb, LightingEngine lightingEngine)
        {
            

            base.Draw(sb, lightingEngine);
        }



        internal void SelectWeapon(int weapon, bool increment)
        {
            if (increment)
            {
                SelectedWeapon += weapon;
                if (SelectedWeapon == Weapons.Count) SelectedWeapon = 0;
                if (SelectedWeapon == -1) SelectedWeapon = Weapons.Count - 1;
            }
            else SelectedWeapon = weapon;
        }

        internal void GiveWeapon(ItemType itemType)
        {
            switch(itemType)
            {
                case ItemType.Pistol:
                    if (Weapons.Count(w => w is Pistol) == 0) Weapons.Add(new Pistol(this));
                        break;
                case ItemType.Shotgun:
                        if (Weapons.Count(w => w is Shotgun) == 0) Weapons.Add(new Shotgun(this));
                        break;
                case ItemType.SMG:
                        if (Weapons.Count(w => w is SMG) == 0) Weapons.Add(new SMG(this));
                        break;
                case ItemType.Rifle:
                        if (Weapons.Count(w => w is Rifle) == 0) Weapons.Add(new Rifle(this));
                        break;
            }

            Weapons = Weapons.OrderBy(w => w.sortOrder).ToList();
            
        }
    }

    
}
