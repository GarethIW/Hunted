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
    public class Dude
    {
        public Vector2 Position;
        public Vector2 Speed;
        public float Rotation;

        public bool Active;
        public bool Dead;

        public float Health;

        public int Ammo;

        public Vehicle drivingVehicle = null;

        public LightSource HeadTorch;

        internal float maxSpeed = 5f;

        internal Texture2D spriteSheet;

        internal Dictionary<string, SpriteAnimation> Animations = new Dictionary<string, SpriteAnimation>();

        internal List<Weapon> Weapons = new List<Weapon>();
        internal int SelectedWeapon = 0;

        internal double deadTime;
        internal float deadAlpha;

        public Dude(Vector2 pos)
        {
            Position = pos;
        }

        
        //public void LoadContent(Texture2D spriteSheet)
        //{
        //    Initialize();
        //}

        internal virtual void Initialize()
        {
            Animations.Add("feet", new SpriteAnimation(4, 100, 0, 0, new Rectangle(0,0,100,100), true, true));
            Animations.Add("arms", new SpriteAnimation(4, 100, 0, 1, new Rectangle(0,0,100,100), true, true));
            Animations.Add("head", new SpriteAnimation(2, 100, 0, 2, new Rectangle(0, 0, 100, 100), false, false));
            Animations.Add("hands", new SpriteAnimation(1, 100, 0, 3, new Rectangle(0, 0, 100, 100), true, false));
            Animations.Add("gun", new SpriteAnimation(1, 100, 0, 4, new Rectangle(0, 0, 100, 100), true, false));
            Active = true;
            Dead = false;
        }

        public virtual void Update(GameTime gameTime, Map gameMap, bool[,] mapFog)
        {
            if (!Dead)
            {

                DoCollisions(gameMap);
                Position += Speed;

                Position.X = MathHelper.Clamp(Position.X, 50, (gameMap.Width * gameMap.TileWidth) - 50);
                Position.Y = MathHelper.Clamp(Position.Y, 50, (gameMap.Height * gameMap.TileHeight) - 50);

                if (Speed.Length() > 0f)
                {
                    Animations["feet"].Update(gameTime);
                    Animations["arms"].Update(gameTime);
                    Animations["head"].Update(gameTime);
                    
                    

                    if (Animations["feet"].CurrentFrame == 0 || Animations["feet"].CurrentFrame == 3)
                    {
                        // Footsteps
                        Tile t = ((TileLayer)gameMap.GetLayer("Terrain")).Tiles[(int)(Position.X / gameMap.TileWidth), (int)(Position.Y / gameMap.TileWidth)];
                        if (t.Properties.Contains("fstep")) AudioController.PlaySFX("fstep-" + t.Properties["fstep"], 0.1f, -0.3f, 0.3f, Position);
                    }
                }
                else
                {
                    Animations["feet"].Reset();
                    Animations["arms"].Reset();
                    Animations["head"].Reset();
                }

                Animations["hands"].CellRect.X = 100 * Animations["hands"].CurrentFrame;
                Animations["gun"].CellRect.X = 100 * Animations["gun"].CurrentFrame;

                Speed = Vector2.Zero;

                foreach (Weapon w in Weapons) w.Update(gameTime);


                if (Weapons[SelectedWeapon] is Knife)
                {
                    if (Weapons[SelectedWeapon].coolDown > 0 && Weapons[SelectedWeapon].coolDown < 100) Animations["hands"].CurrentFrame = 1;
                    else if (Weapons[SelectedWeapon].coolDown >= 100 && Weapons[SelectedWeapon].coolDown < 200) Animations["hands"].CurrentFrame = 2;
                    else if (Weapons[SelectedWeapon].coolDown >= 200 && Weapons[SelectedWeapon].coolDown < 300) Animations["hands"].CurrentFrame = 3;
                    else Animations["hands"].CurrentFrame = 0;

                    Animations["gun"].CurrentFrame = 0;

                }
                else
                {
                    Animations["hands"].CurrentFrame = 3;
                    Animations["gun"].CurrentFrame = Weapons[SelectedWeapon].sortOrder;
                }

            }

            Health = MathHelper.Clamp(Health, 0f, 100f);
            Ammo = (int)MathHelper.Clamp(Ammo, 0, 100);

            if (Dead)
            {
                deadTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
                
            }
            
        }

        public virtual void Move(Vector2 amount)
        {
            if (Dead || !Active) return;

            if (amount.Length() > 0f)
            {
                amount.Normalize();
                Speed = amount * maxSpeed;
            }
        }

        public virtual void EnterVehicle(Map gameMap)
        {
            if (Dead || !Active) return;


            if (drivingVehicle == null)
            {
                foreach (Vehicle v in VehicleController.Instance.Vehicles)
                {
                    if ((v.Position - Position).Length() < 200f)
                    {
                        drivingVehicle = v;
                        break;
                    }
                }
            }
            else
            {
                // Exit vehicle
                //bool found = false;
                if (drivingVehicle is Chopper && ((Chopper)drivingVehicle).Height > 0f) ((Chopper)drivingVehicle).Land(gameMap);
                else
                {
                    for (float a = 0f; a < MathHelper.TwoPi; a += 0.5f)
                    {
                        Vector2 pos = Helper.PointOnCircle(ref drivingVehicle.Position, 200, a);
                        if (!gameMap.CheckTileCollision(pos) && !Helper.IsPointInShape(pos, drivingVehicle.CollisionVerts))
                        {
                            if (drivingVehicle is Boat || !LineCollision(pos, gameMap))
                            {
                                Position = pos;
                                drivingVehicle = null;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public virtual void LookAt(Vector2 target)
        {
            if (Dead || !Active) return;

            Rotation = Helper.TurnToFace(Position, target, Rotation, 1f, 0.25f);
        }

        public virtual void Attack(GameTime gameTime, Vector2 target, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (Dead || !Active) return;

            Weapons[SelectedWeapon].Use(gameTime, target, trigger, gameCamera, canCollide);
        }

        public virtual void Draw(SpriteBatch sb, LightingEngine lightingEngine)
        {
            if (Dead) return;


            if (drivingVehicle != null) return;
            // Feet
            sb.Draw(spriteSheet, Position, Animations["feet"].CellRect, lightingEngine.CurrentSunColor, (Speed.Length()>0f)?Helper.V2ToAngle(Speed)+MathHelper.PiOver2:Rotation, new Vector2(100,100)/2, 1f, SpriteEffects.None, 1);
            // Hands
            sb.Draw(spriteSheet, Position, Animations["hands"].CellRect, lightingEngine.CurrentSunColor, Rotation-0.15f, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
            // Gun
            sb.Draw(spriteSheet, Position, Animations["gun"].CellRect, lightingEngine.CurrentSunColor, Rotation - 0.15f, new Vector2(50, 60), 1f, SpriteEffects.None, 1);
            // Arms
            sb.Draw(spriteSheet, Position, Animations["arms"].CellRect, lightingEngine.CurrentSunColor, Rotation - 0.15f, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
            // Head
            sb.Draw(spriteSheet, Position, Animations["head"].CellRect, lightingEngine.CurrentSunColor, Rotation - 0.15f, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
        }
        
        public virtual void DrawShadows(SpriteBatch sb, LightingEngine lightingEngine)
        {
            if (Dead) return;

            if (drivingVehicle != null) return;

            for (int i = 1; i < 20; i += 2)
            {

                Vector2 pos = Position + new Vector2(lightingEngine.CurrentShadowVect.X * i, lightingEngine.CurrentShadowVect.Y * i);

                sb.Draw(spriteSheet, pos, Animations["hands"].CellRect, Color.Black * 0.03f, Rotation - 0.15f, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
                sb.Draw(spriteSheet, pos, Animations["gun"].CellRect, Color.Black * 0.03f, Rotation - 0.15f, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
                sb.Draw(spriteSheet, pos, Animations["arms"].CellRect, Color.Black * 0.03f, Rotation - 0.15f, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
            }
        }

        public virtual void DrawLightBlock(SpriteBatch sb)
        {
            if (Dead) return;

            if (drivingVehicle != null) return;
            // Arms
            sb.Draw(spriteSheet, Position, Animations["gun"].CellRect, Color.Black, Rotation - 0.15f, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
            sb.Draw(spriteSheet, Position, Animations["hands"].CellRect, Color.Black, Rotation - 0.15f, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
            sb.Draw(spriteSheet, Position, Animations["arms"].CellRect, Color.Black, Rotation - 0.15f, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
        }

        public virtual void Collided() { }

        public virtual void HitByProjectile(Projectile p)
        {
            if (Dead || !Active) return;

            if (drivingVehicle != null) return;

            Health -= (p.Owner.GetType() == typeof(HeroDude)) ? p.Damage : p.Damage / 2;
            AudioController.PlaySFX("hit", 0.5f, -0.4f, 0.4f, Position);
            if (p.Type != ProjectileType.Knife)
            {
                ParticleController.Instance.AddGSW(p);
            }
            else if(p.Type == ProjectileType.SMG)
            {
                ParticleController.Instance.AddSMGWound(p);
            }
            else
            {
                ParticleController.Instance.AddKnifeWound(p);
            }
            p.Active = false;
        }

        public virtual void HitByVehicle(Vehicle v)
        {
            if (Dead || !Active) return;

            Health -= 1 + ((float)Math.Abs(v.linearSpeed)) / 2f;
            Speed = v.Speed;
            AudioController.PlaySFX("hit", 0.5f, -0.4f, 0.4f, Position);
            ParticleController.Instance.AddVehicleWound(this);
        }

        internal bool LineCollision(Vector2 testPos, Map gameMap)
        {
            Vector2 testVector = Position;
            Vector2 testLine = (testPos - Position);

            while ((testVector - testPos).Length() > 30)
            {
                if (gameMap.CheckCollision(testVector)) return true;
                testVector += (testLine / 50f);
            }

            return false;
        }

        void DoCollisions(Map gameMap)
        {
            bool LCollision = false;
            bool RCollision = false;
            bool UCollision = false;
            bool DCollision = false;

            if (Speed.X > 0f)
            {
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, 0f))) RCollision = true;
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, -0.4f))) RCollision = true;
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, 0.4f))) RCollision = true;
                foreach (Vehicle veh in VehicleController.Instance.Vehicles)
                {
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, 0f), veh.CollisionVerts)) RCollision = true;
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, -0.4f), veh.CollisionVerts)) RCollision = true;
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, 0.4f), veh.CollisionVerts)) RCollision = true;
                }
            }

            if (Speed.X < 0f)
            {
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, MathHelper.Pi))) LCollision = true;
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, MathHelper.Pi - 0.4f))) LCollision = true;
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, MathHelper.Pi + 0.4f))) LCollision = true;
                foreach (Vehicle veh in VehicleController.Instance.Vehicles)
                {
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, MathHelper.Pi), veh.CollisionVerts)) LCollision = true;
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, MathHelper.Pi - 0.4f), veh.CollisionVerts)) LCollision = true;
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, MathHelper.Pi + 0.4f), veh.CollisionVerts)) LCollision = true;
                }
            }

            if (Speed.Y < 0f)
            {
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, (MathHelper.PiOver2 + MathHelper.Pi)))) UCollision = true;
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, (MathHelper.PiOver2 + MathHelper.Pi) - 0.4f))) UCollision = true;
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, (MathHelper.PiOver2 + MathHelper.Pi) + 0.4f))) UCollision = true;
                foreach (Vehicle veh in VehicleController.Instance.Vehicles)
                {
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, (MathHelper.PiOver2 + MathHelper.Pi)), veh.CollisionVerts)) UCollision = true;
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, (MathHelper.PiOver2 + MathHelper.Pi) - 0.4f), veh.CollisionVerts)) UCollision = true;
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50,(MathHelper.PiOver2 + MathHelper.Pi) + 0.4f), veh.CollisionVerts)) UCollision = true;
                }
            }

            if (Speed.Y > 0f)
            {
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, MathHelper.PiOver2))) DCollision = true;
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, MathHelper.PiOver2 - 0.4f))) DCollision = true;
                if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 50, MathHelper.PiOver2 + 0.4f))) DCollision = true;
                foreach (Vehicle veh in VehicleController.Instance.Vehicles)
                {
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, MathHelper.PiOver2), veh.CollisionVerts)) DCollision = true;
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50, MathHelper.PiOver2 - 0.4f), veh.CollisionVerts)) DCollision = true;
                    if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 50,MathHelper.PiOver2 + 0.4f), veh.CollisionVerts)) DCollision = true;
                }
            }

            if (Speed.X > 0f && RCollision) Speed.X = 0f;
            if (Speed.X < 0f && LCollision) Speed.X = 0f;
            if (Speed.Y > 0f && DCollision) Speed.Y = 0f;
            if (Speed.Y < 0f && UCollision) Speed.Y = 0f;

            if (UCollision || DCollision || LCollision || RCollision) Collided();
        }


    }

    
}
