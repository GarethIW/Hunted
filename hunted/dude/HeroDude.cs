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
    public class HuntedLevel
    {
        public float Level;
        public Vector2 LastKnownPosition;
        public double TimeSinceLastSeen;
        
        double updateTime;

        public void Update(GameTime gameTime)
        {
            TimeSinceLastSeen += gameTime.ElapsedGameTime.TotalMilliseconds;
            updateTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (updateTime >= 1000)
            {
                updateTime = 0;
                if (TimeSinceLastSeen > 5000)
                    Level -= 1f;
            }

            Level = MathHelper.Clamp(Level, 0f, 100f);
        }

        public void Seen(Vector2 pos)
        {
            if (TimeSinceLastSeen > 1000)
            {
                TimeSinceLastSeen = 0;
                LastKnownPosition = pos + new Vector2(-100f + ((float)Helper.Random.NextDouble() * 200f), -100f + ((float)Helper.Random.NextDouble() * 200f));
                Level += 2f;
            }
        }

        public void Heard(Vector2 pos, bool wasGunshot)
        {
            if (TimeSinceLastSeen > 10000)
            {
                LastKnownPosition = pos + new Vector2(-300f + ((float)Helper.Random.NextDouble() * 600f), -300f + ((float)Helper.Random.NextDouble() * 600f));
                Level += 1f;
            }

            if (wasGunshot && TimeSinceLastSeen > 1000)
            {
                TimeSinceLastSeen = 0;
                LastKnownPosition = pos + new Vector2(-200f + ((float)Helper.Random.NextDouble() * 400f), -200f + ((float)Helper.Random.NextDouble() * 400f));
                Level += 1f;
            }
        }
    }

    public class HeroDude : Dude
    {
        public HuntedLevel HuntedLevel = new HuntedLevel();

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
            //Weapons.Add(new Pistol(this));
            //Weapons.Add(new Shotgun(this));
            //Weapons.Add(new SMG(this));
            //Weapons.Add(new Rifle(this));            
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

            HuntedLevel.Update(gameTime);

            HeadTorch.Position = Helper.PointOnCircle(ref Position, 32, Rotation - MathHelper.PiOver2);
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
                ParticleController.Instance.AddBloodPool(Position);
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
                    Weapons.Clear();
                    Weapons.Add(new Knife(this));  
                    SelectedWeapon = 0;
                    HuntedLevel.Level = 0f;
                    EnemyController.Instance.ClearSpawn(gameMap.HeroSpawn);
                    VehicleController.Instance.ClearSpawn(gameMap.HeroSpawn);
                    Ammo = 0;
                }
            }

            if (!(drivingVehicle is Chopper))
            {
                foreach (Compound c in gameMap.Compounds)
                    foreach (Building b in c.Buildings)
                        if (b.Type == BuildingType.Building)
                        {
                            Point pos = Helper.VtoP(Position / 100);
                            if (b.Rect.Contains(pos) && b.RoofFade > 0.1f) b.RoofFade -= 0.01f;
                            else if (b.RoofFade < 1f) b.RoofFade += 0.01f;
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
            else
            {
                foreach(Weapon w in Weapons)
                    if(w.sortOrder==weapon) SelectedWeapon = Weapons.IndexOf(w);
            }
        }

        internal void GiveWeapon(ItemType itemType)
        {
            int weapon = 0;

            switch(itemType)
            {
                case ItemType.Pistol:
                    if (Weapons.Count(w => w is Pistol) == 0)
                    {
                        Weapons.Add(new Pistol(this));
                        weapon = 1;
                    }
                        break;
                case ItemType.Shotgun:
                        if (Weapons.Count(w => w is Shotgun) == 0) { Weapons.Add(new Shotgun(this)); weapon = 2; }
                        break;
                case ItemType.SMG:
                        if (Weapons.Count(w => w is SMG) == 0) { Weapons.Add(new SMG(this)); weapon = 3; }
                        break;
                case ItemType.Rifle:
                        if (Weapons.Count(w => w is Rifle) == 0) { Weapons.Add(new Rifle(this)); weapon = 4; }
                        break;
            }

            Weapons = Weapons.OrderBy(w => w.sortOrder).ToList();

            if(weapon!=0)
                foreach (Weapon w in Weapons)
                    if (w.sortOrder == weapon) SelectedWeapon = Weapons.IndexOf(w);
            
        }
    }

    
}
