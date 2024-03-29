﻿using Hunted.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted
{
    public class Jeep : Vehicle
    {
        SoundEffectInstance engineIdleSound;

        public Jeep(Vector2 pos):base(pos)
        {
            Health = 100f;
        }


        public void LoadContent(Texture2D sheet, GraphicsDevice gd, LightingEngine le)
        {
            spriteSheet = sheet;
            Initialize(gd, le);

            engineSound = AudioController.effects["engine"].CreateInstance();
            engineIdleSound = AudioController.effects["truck"].CreateInstance();

            engineIdleSound.Volume = 0f;
            engineIdleSound.IsLooped = true;
            engineSound.Volume = 0f;
            engineSound.IsLooped = true;
            
        }

        internal void Initialize(GraphicsDevice gd, LightingEngine le)
        {
            //HeadTorch = new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None);
            //le.LightSources.Add(HeadTorch);
            Lights.Add(new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None));
            Lights.Add(new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None));
            le.LightSources.Add(Lights[0]);
            le.LightSources.Add(Lights[1]);

            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, -0.44f + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, 0.44f + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, -0.44f + MathHelper.Pi + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, 0.44f + MathHelper.Pi + (Rotation)));

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
                turnAmount = MathHelper.Lerp(turnAmount, 0f, 0.1f);
            }

            if ((turnAmount > 0f && turnAmount < 0.001f) || (turnAmount < 0f && turnAmount > -0.001f)) turnAmount = 0f;

            if (linearSpeed >= 0.1f || linearSpeed <= -0.1f)
                Rotation += MathHelper.Clamp((linearSpeed / 100f) * turnAmount, -0.025f, 0.025f);

            Speed = moveVect * linearSpeed;

            turning = false;

            foreach (Dude d in EnemyController.Instance.Enemies)
            {
                if (Helper.IsPointInShape(d.Position, this.CollisionVerts) && d.Health >= 0f && !d.Dead)
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
                limitedSpeed = (Health * 4f) / 10f;
            }
            if (Health <= 0f)
            {
                limitedSpeed = 0f;
            }

            //HeadTorch.Position = Helper.PointOnCircle(ref Position, 30, Rotation - MathHelper.PiOver2);
            //HeadTorch.Rotation = Rotation - MathHelper.PiOver2;
            if (Active)
            {
                Lights[0].Position = Helper.PointOnCircle(ref Position, 137, (Rotation) - 0.2f);
                Lights[1].Position = Helper.PointOnCircle(ref Position, 137, (Rotation) + 0.2f);
                Lights[0].Rotation = Rotation;
                Lights[1].Rotation = Rotation;
            }

            if (gameHero.drivingVehicle == this)
            {
                engineSound.Play();
                engineIdleSound.Play();

                if (maxSpeed > 0f)
                    gameCamera.ZoomTarget = 1f - ((0.5f / maxSpeed) * (float)Math.Abs(linearSpeed));
                else gameCamera.ZoomTarget = 1f;

                if (Health > 0f)
                {
                    engineIdleSound.Volume = 1f - ((1f / 13f) * (float)Math.Abs(linearSpeed));
                    engineSound.Volume = ((1f / 13f) * (float)Math.Abs(linearSpeed));
                    engineSound.Pitch = -0.5f + (((1f / 12f) * (float)Math.Abs(linearSpeed)));
                    engineIdleSound.Volume = MathHelper.Clamp(engineIdleSound.Volume, 0f, 0.5f);
                }
                else
                {
                    engineIdleSound.Volume = 0f;
                    engineSound.Volume = 0f;
                }
            }
            else
            {
                engineSound.Stop();
                engineIdleSound.Stop();
            }

        }

        public override void Draw(SpriteBatch sb, LightingEngine lightingEngine)
        {  
            sb.Draw(spriteSheet, Position, new Rectangle(0,0,200,300), lightingEngine.CurrentSunColor, Rotation+MathHelper.PiOver2, new Vector2(200,300)/2, 1f, SpriteEffects.None, 1);
            
        }

        public override void DrawShadows(SpriteBatch sb, LightingEngine lightingEngine)
        {
            for (int i = 1; i < 40; i += 2)
            {
                Vector2 pos = Position + new Vector2(lightingEngine.CurrentShadowVect.X * i, lightingEngine.CurrentShadowVect.Y * i);

                sb.Draw(spriteSheet, pos, new Rectangle(0, 0, 200, 300), Color.Black * 0.03f, Rotation + MathHelper.PiOver2, new Vector2(200, 300) / 2, 1f, SpriteEffects.None, 1);
            }
        }



        public override void DrawLightBlock(SpriteBatch sb)
        {
            // Arms
            sb.Draw(spriteSheet, Position, new Rectangle(0, 0, 200, 300), Color.Black, Rotation + MathHelper.PiOver2, new Vector2(200, 300) / 2, 1f, SpriteEffects.None, 1);
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
            linearSpeed -= acceleration * 2f;

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

    }

    
}
