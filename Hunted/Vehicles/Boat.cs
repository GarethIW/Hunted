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
    public class Boat : Vehicle
    {

        public Boat(Vector2 pos):base(pos)
        {
            Health = 100f;
            acceleration = 0.2f;
            //decelerate = 0.01f;
            maxSpeed = 15f;
            turnSpeed = 0.005f;
        }


        public void LoadContent(Texture2D sheet, GraphicsDevice gd, LightingEngine le)
        {
            spriteSheet = sheet;
            Initialize(gd, le);

            engineSound = AudioController.effects["boat"].CreateInstance();
        }

        internal void Initialize(GraphicsDevice gd, LightingEngine le)
        {
            //HeadTorch = new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None);
            //le.LightSources.Add(HeadTorch);
            Lights.Add(new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None));
            //Lights.Add(new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None));
            le.LightSources.Add(Lights[0]);
            //le.LightSources.Add(Lights[1]);
            base.Initialize();
        }

        public override void Update(GameTime gameTime, Map gameMap, HeroDude gameHero, Camera gameCamera)
        {
            CollisionVerts.Clear();
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, -0.44f + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, 0.44f + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, -0.44f + MathHelper.Pi + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, 0.44f + MathHelper.Pi + (Rotation)));

            base.Update(gameTime, gameMap, gameHero, gameCamera);

            if (linearSpeed > 0f) linearSpeed -= decelerate;
            if (linearSpeed < 0f) linearSpeed += decelerate;

            linearSpeed = MathHelper.Clamp(linearSpeed, -(limitedSpeed / 2), limitedSpeed);
            Vector2 moveVect = Helper.AngleToVector(Rotation, 100f);
            moveVect.Normalize();

            if (!turning)
            {
                turnAmount = MathHelper.Lerp(turnAmount, 0f, 0.05f);
            }

            if ((turnAmount > 0f && turnAmount < 0.001f) || (turnAmount < 0f && turnAmount > -0.001f)) turnAmount = 0f;

            if (linearSpeed >= 0.1f || linearSpeed <= -0.1f)
                Rotation += MathHelper.Clamp((linearSpeed / 100f) * turnAmount, -0.025f, 0.025f);

            Speed = moveVect * linearSpeed;

            turning = false;

            foreach (Dude d in EnemyController.Instance.Enemies)
            {
                if (Helper.IsPointInShape(d.Position, this.CollisionVerts) && d.Health >= 0f)
                {
                    Health -= 0.5f;
                    d.HitByVehicle(this);
                }
            }

            if (Health < 50f)
            {
                limitedSpeed = 10f;
            }
            if (Health < 20f)
            {
                limitedSpeed = 5f;
            }
            if (Health <= 0f)
            {
                limitedSpeed = 2f;
            }

            //HeadTorch.Position = Helper.PointOnCircle(ref Position, 30, Rotation - MathHelper.PiOver2);
            //HeadTorch.Rotation = Rotation - MathHelper.PiOver2;
            Lights[0].Position = Helper.PointOnCircle(ref Position, 145, (Rotation));
            //Lights[1].Position = Helper.PointOnCircle(ref Position, 137, (Rotation) + 0.2f);
            Lights[0].Rotation = Rotation;
            //Lights[1].Rotation = Rotation;

            if (gameHero.drivingVehicle == this)
            {
                engineSound.Play();

                if (maxSpeed > 0f)
                    gameCamera.ZoomTarget = 1f - ((0.5f / maxSpeed) * (float)Math.Abs(linearSpeed));
                else gameCamera.ZoomTarget = 1f;

                if (Health > 0f)
                {
                    engineSound.Volume = MathHelper.Clamp(0.2f + ((1f / 18f) * (float)Math.Abs(linearSpeed)), 0f, 1f);
                    engineSound.Pitch = -0.3f + (((0.6f / 18f) * (float)Math.Abs(linearSpeed)));
                }
                else
                {
                    engineSound.Volume = 0f;
                }
            }
            else
            {
                engineSound.Stop();
            }

            
        }

        public override void Draw(SpriteBatch sb, LightingEngine lightingEngine)
        {  
            sb.Draw(spriteSheet, Position, new Rectangle(0,400,200,300), lightingEngine.CurrentSunColor, Rotation+MathHelper.PiOver2, new Vector2(200,300)/2, 1f, SpriteEffects.None, 1);
            
        }

        public override void DrawShadows(SpriteBatch sb, LightingEngine lightingEngine)
        {
            for (int i = 1; i < 40; i += 2)
            {
                Vector2 pos = Position + new Vector2(lightingEngine.CurrentShadowVect.X * i, lightingEngine.CurrentShadowVect.Y * i);

                sb.Draw(spriteSheet, pos, new Rectangle(0, 400, 200, 300), Color.Black * 0.03f, Rotation + MathHelper.PiOver2, new Vector2(200, 300) / 2, 1f, SpriteEffects.None, 1);
            }
        }



        public override void DrawLightBlock(SpriteBatch sb)
        {
            // Arms
            sb.Draw(spriteSheet, Position, new Rectangle(0, 400, 200, 300), Color.Black, Rotation + MathHelper.PiOver2, new Vector2(200, 300) / 2, 1f, SpriteEffects.None, 1);
        }

        public override void Collided()
        {
            if (linearSpeed > 1f)
                AudioController.PlaySFX("crash", 0.3f, 0.3f, 0f);

            Health -= ((float)Math.Abs(linearSpeed) / 2f);
            linearSpeed = 0f;
            Speed = Vector2.Zero;

            base.Collided();
        }

        internal override void Accelerate(float max)
        {
            if (linearSpeed < (limitedSpeed * max)) linearSpeed += acceleration;

            base.Accelerate(max);
        }

        internal override void Brake()
        {
            linearSpeed -= acceleration * 0.5f;

            base.Brake();
        }

        internal override void Turn(float p)
        {
            if (turnAmount > 0f && p < 0f) turnAmount = 0f;
            if (turnAmount < 0f && p > 0f) turnAmount = 0f;
            turnAmount += (turnSpeed * p);
            turning = true;

            base.Turn(p);
        }

        internal override void DoCollisions(Map gameMap)
        {
            bool Collision = false;
            if (Speed == Vector2.Zero) return;
            Vector2 test = Speed;
            test.Normalize();
            float rot = Helper.V2ToAngle(test);

            if (gameMap.GetTile(Helper.PointOnCircle(ref Position, 135, rot), "Terrain")!=null && gameMap.GetTile(Helper.PointOnCircle(ref Position, 135, rot), "Terrain").Properties.Contains("CanBoat"))
            {
                if (gameMap.GetTile(Helper.PointOnCircle(ref Position, 135, rot), "Water") == null) linearSpeed = MathHelper.Lerp(linearSpeed, 0f, 0.05f);
                if (gameMap.GetTile(Helper.PointOnCircle(ref Position, 50, rot), "Water") == null) linearSpeed = MathHelper.Lerp(linearSpeed, 0f, 1f);
            }
            else Collision = true;
            if (gameMap.GetTile(Helper.PointOnCircle(ref Position, 135, rot-0.2f), "Terrain") != null && gameMap.GetTile(Helper.PointOnCircle(ref Position, 135, rot - 0.2f), "Terrain").Properties.Contains("CanBoat"))
            {
                if (gameMap.GetTile(Helper.PointOnCircle(ref Position, 135, rot-0.2f), "Water") == null) linearSpeed = MathHelper.Lerp(linearSpeed, 0f, 0.05f);
                
            }
            else Collision = true;
            if (gameMap.GetTile(Helper.PointOnCircle(ref Position, 135, rot+0.2f), "Terrain") != null && gameMap.GetTile(Helper.PointOnCircle(ref Position, 135, rot + 0.2f), "Terrain").Properties.Contains("CanBoat"))
            {
                if (gameMap.GetTile(Helper.PointOnCircle(ref Position, 135, rot+0.2f), "Water") == null) linearSpeed = MathHelper.Lerp(linearSpeed, 0f, 0.05f);
            }
            else Collision = true;
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
        }

    }

    
}
