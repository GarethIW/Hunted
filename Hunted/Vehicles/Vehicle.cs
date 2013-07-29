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
    public class Vehicle
    {
        public Vector2 Position;
        public Vector2 Speed;
        public float Rotation;

        public bool Active;
        public bool Dead;

        public float Health;

        public List<LightSource> Lights = new List<LightSource>();

        public List<Vector2> CollisionVerts = new List<Vector2>();

        internal float maxSpeed = 12f;
        internal float linearSpeed = 0f;
        internal float decelerate = 0.025f;
        internal float acceleration = 0.1f;
        internal float turnSpeed = 0.025f;
        internal float turnAmount = 0f;

        bool turning = false;

        internal Texture2D spriteSheet;

        internal Dictionary<string, SpriteAnimation> Animations = new Dictionary<string, SpriteAnimation>();

        internal List<Weapon> Weapons = new List<Weapon>();
        internal int SelectedWeapon = 0;

        

        public Vehicle(Vector2 pos)
        {
            Position = pos;
            Active = true;
        }

        
        //public void LoadContent(Texture2D spriteSheet)
        //{
        //    Initialize();
        //}

        internal virtual void Initialize()
        {
            
            
        }

        public virtual void Update(GameTime gameTime, Map gameMap)
        {
            DoCollisions(gameMap);
            Position += Speed;

            Position.X = MathHelper.Clamp(Position.X, 50, (gameMap.Width * gameMap.TileWidth) -50);
            Position.Y = MathHelper.Clamp(Position.Y, 50, (gameMap.Height * gameMap.TileHeight) -50);

            if (Speed.Length() > 0f)
            {
               
            }
            else
            {
               
            }

            if(linearSpeed>0f) linearSpeed -= decelerate;
            if (linearSpeed < 0f) linearSpeed += decelerate;

            linearSpeed = MathHelper.Clamp(linearSpeed, -(maxSpeed/2), maxSpeed);
            Vector2 moveVect = Helper.AngleToVector(Rotation, 100f);
            moveVect.Normalize();

            if (!turning)
            {
                //if (turnAmount > 0f) turnAmount -= 0.25f;
                //if (turnAmount < 0f) turnAmount += 0.25f;
                //turnAmount = 0f;

                turnAmount = MathHelper.Lerp(turnAmount, 0f, 0.1f);
            }

            if ((turnAmount > 0f && turnAmount < 0.001f) || (turnAmount < 0f && turnAmount > -0.001f)) turnAmount = 0f;

            if (linearSpeed >= 0.1f || linearSpeed <= -0.1f)
                Rotation += MathHelper.Clamp((linearSpeed / 100f) * turnAmount,-0.025f,0.025f);

            Speed = moveVect * linearSpeed;

            turning = false;

            Health = MathHelper.Clamp(Health, 0f, 100f);

            foreach (Dude d in EnemyController.Instance.Enemies)
            {
                if (Helper.IsPointInShape(d.Position, this.CollisionVerts) && d.Health>=0f)
                {
                    Health -= 0.5f;
                    d.HitByVehicle(this);
                }
            }

            if (Health < 50f)
            {
                maxSpeed = 10f;
            }
            if (Health < 20f)
            {
                maxSpeed = (Health * 4f) / 10f;
            }
            if (Health <= 0f)
            {
                maxSpeed = 0f;
            }
        }

        public virtual void Move(Vector2 amount)
        {
            if (amount.Length() > 0f)
            {
                amount.Normalize();
                Speed = amount * maxSpeed;
            }
        }

        public virtual void LookAt(Vector2 target)
        {
            Rotation = Helper.TurnToFace(Position, target, Rotation, 1f, 0.25f);
        }

        public virtual void Attack(GameTime gameTime, bool trigger, Camera gameCamera)
        {
            Weapons[SelectedWeapon].Use(gameTime, trigger, gameCamera);
        }

        public virtual void Draw(SpriteBatch sb, LightingEngine lightingEngine)
        {  
            //sb.Draw(spriteSheet, Position, , lightingEngine.CurrentSunColor, Rotation, new Vector2(100,100)/2, 1f, SpriteEffects.None, 1);
            
        }
        
        public virtual void DrawShadows(SpriteBatch sb, LightingEngine lightingEngine)
        {
            //for (int i = 1; i < 20; i += 2)
            //{
            //    Vector2 pos = Position + new Vector2(lightingEngine.CurrentShadowVect.X * i, lightingEngine.CurrentShadowVect.Y * i);

            //    sb.Draw(spriteSheet, pos, Animations["arms"].CellRect, Color.Black * 0.03f, Rotation, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
            //}
        }

        public virtual void DrawLightBlock(SpriteBatch sb)
        {
            // Arms
            //sb.Draw(spriteSheet, Position, Animations["arms"].CellRect, Color.Black, Rotation, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
        }

        public virtual void Collided() 
        {
            Health -= ((float)Math.Abs(linearSpeed) / 2f);
            linearSpeed = 0f;
            Speed = Vector2.Zero;
        }

        public virtual void HitByProjectile(Projectile p)
        {
            Health -= p.Damage/5f;
            //AudioController.PlaySFX("hit", 0.5f, -0.4f, 0.4f, Position);
            if (p.Type != ProjectileType.Knife)
            {
                //ParticleController.Instance.AddGSW(p);
            }
            else
            {
                //ParticleController.Instance.AddKnifeWound(p);
            }
        }


        internal void Accelerate(float max)
        {
            if (linearSpeed < (maxSpeed * max)) linearSpeed += acceleration;
            
        }

        internal void Brake()
        {
            linearSpeed -= acceleration * 2f;
        }

        internal void Turn(float p)
        {
            if (turnAmount > 0f && p < 0f) turnAmount = 0f;
            if (turnAmount < 0f && p > 0f) turnAmount = 0f;
            turnAmount += (turnSpeed * p);
            turning = true;
        }

        void DoCollisions(Map gameMap)
        {
            bool Collision = false;
            Vector2 test = Speed;
            test.Normalize();
            float rot = Helper.V2ToAngle(test);

            if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, rot))) Collision = true;
            if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, rot -0.2f))) Collision = true;
            if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, rot + 0.2f))) Collision = true;
            foreach (Vehicle veh in VehicleController.Instance.Vehicles)
            {
                if (veh == this) continue;
                if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, rot), veh.CollisionVerts)) Collision = true;
                if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, rot - 0.2f), veh.CollisionVerts)) Collision = true;
                if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, rot + 0.2f), veh.CollisionVerts)) Collision = true;
            }
            if (Collision)
            {
                Collided();
            }

            //bool LCollision = false;
            //bool RCollision = false;
            //bool UCollision = false;
            //bool DCollision = false;

            //if (Speed.X > 0f)
            //{
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, 0f))) RCollision = true;
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, -0.2f))) RCollision = true;
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, 0.2f))) RCollision = true;
            //    foreach (Vehicle veh in VehicleController.Instance.Vehicles)
            //    {
            //        if (veh == this) continue;
            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, 0f), veh.CollisionVerts)) RCollision = true;
            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, -0.2f), veh.CollisionVerts)) RCollision = true;
            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, 0.2f), veh.CollisionVerts)) RCollision = true;
            //    }
            //}

            //if (Speed.X < 0f)
            //{
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, MathHelper.Pi))) LCollision = true;
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, MathHelper.Pi - 0.2f))) LCollision = true;
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, MathHelper.Pi + 0.2f))) LCollision = true;
            //    foreach (Vehicle veh in VehicleController.Instance.Vehicles)
            //    {
            //        if (veh == this) continue;

            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, MathHelper.Pi), veh.CollisionVerts)) LCollision = true;
            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, MathHelper.Pi - 0.2f), veh.CollisionVerts)) LCollision = true;
            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, MathHelper.Pi + 0.2f), veh.CollisionVerts)) LCollision = true;
            //    }
            //}

            //if (Speed.Y < 0f)
            //{
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, (MathHelper.PiOver2 + MathHelper.Pi)))) UCollision = true;
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, (MathHelper.PiOver2 + MathHelper.Pi) - 0.2f))) UCollision = true;
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, (MathHelper.PiOver2 + MathHelper.Pi) + 0.2f))) UCollision = true;
            //    foreach (Vehicle veh in VehicleController.Instance.Vehicles)
            //    {
            //        if (veh == this) continue;

            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, (MathHelper.PiOver2 + MathHelper.Pi)), veh.CollisionVerts)) UCollision = true;
            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, (MathHelper.PiOver2 + MathHelper.Pi) - 0.2f), veh.CollisionVerts)) UCollision = true;
            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, (MathHelper.PiOver2 + MathHelper.Pi) + 0.2f), veh.CollisionVerts)) UCollision = true;
            //    }
            //}

            //if (Speed.Y > 0f)
            //{
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, MathHelper.PiOver2))) DCollision = true;
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, MathHelper.PiOver2 - 0.2f))) DCollision = true;
            //    if (gameMap.CheckCollision(Helper.PointOnCircle(ref Position, 135, MathHelper.PiOver2 + 0.2f))) DCollision = true;
            //    foreach (Vehicle veh in VehicleController.Instance.Vehicles)
            //    {
            //        if (veh == this) continue;

            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, MathHelper.PiOver2), veh.CollisionVerts)) DCollision = true;
            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, MathHelper.PiOver2 - 0.2f), veh.CollisionVerts)) DCollision = true;
            //        if (Helper.IsPointInShape(Helper.PointOnCircle(ref Position, 135, MathHelper.PiOver2 + 0.2f), veh.CollisionVerts)) DCollision = true;
            //    }
            //}

            //if (Speed.X > 0f && RCollision) Speed.X = 0f;
            //if (Speed.X < 0f && LCollision) Speed.X = 0f;
            //if (Speed.Y > 0f && DCollision) Speed.Y = 0f;
            //if (Speed.Y < 0f && UCollision) Speed.Y = 0f;

            //if (UCollision || DCollision || LCollision || RCollision)
            //    Collided();
        }



    }

    
}
