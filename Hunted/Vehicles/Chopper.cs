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
    public class Chopper : Vehicle
    {
        public float Height = 0f;

        float bladesRot = 0f;
        float bladesSpeed = 0f;

        bool takingOff = false;
        bool landing = false;

        float maxCameraScale = 0.6f;

        public Chopper(Vector2 pos)
            : base(pos)
        {
            Health = 100f;
        }


        public void LoadContent(Texture2D sheet, GraphicsDevice gd, LightingEngine le)
        {
            spriteSheet = sheet;
            Initialize(gd, le);
        }

        internal void Initialize(GraphicsDevice gd, LightingEngine le)
        {
            //HeadTorch = new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None);
            //le.LightSources.Add(HeadTorch);
            //Lights.Add(new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None));
            //Lights.Add(new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None));
            //le.LightSources.Add(Lights[0]);
            //le.LightSources.Add(Lights[1]);

            turnSpeed = 0.1f;

            base.Initialize();
        }

        public override void Update(GameTime gameTime, Map gameMap, HeroDude gameHero, Camera gameCamera)
        {
            CollisionVerts.Clear();
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 100, -0.44f + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 100, 0.44f + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 200, MathHelper.Pi + (Rotation)));
            //CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, 0.44f + MathHelper.Pi + (Rotation)));
            base.Update(gameTime, gameMap, gameHero, gameCamera);

            //HeadTorch.Position = Helper.PointOnCircle(ref Position, 30, Rotation - MathHelper.PiOver2);
            //HeadTorch.Rotation = Rotation - MathHelper.PiOver2;
            //Lights[0].Position = Helper.PointOnCircle(ref Position, 137, (Rotation) - 0.2f);
            //Lights[1].Position = Helper.PointOnCircle(ref Position, 137, (Rotation) + 0.2f);
            //Lights[0].Rotation = Rotation;
            //Lights[1].Rotation = Rotation;

            if (linearSpeed > 0f) linearSpeed -= decelerate;
            if (linearSpeed < 0f) linearSpeed += decelerate;

            linearSpeed = MathHelper.Clamp(linearSpeed, -maxSpeed, maxSpeed);
            Vector2 moveVect = Helper.AngleToVector(Rotation, 100f);
            moveVect.Normalize();

            if (!turning)
            {
                turnAmount = MathHelper.Lerp(turnAmount, 0f, 0.1f);
            }

            if ((turnAmount > 0f && turnAmount < 0.001f) || (turnAmount < 0f && turnAmount > -0.001f)) turnAmount = 0f;

            Rotation += MathHelper.Clamp(turnAmount * 0.05f, -0.025f, 0.025f);

            Speed = moveVect * linearSpeed;

            if (takingOff && bladesSpeed>0.4f)
            {
                linearSpeed = 0f;
                Speed = Vector2.Zero;
                Height = MathHelper.Lerp(Height, 1f, 0.02f);
                if (Height > 0.99f) { Height = 1f; takingOff = false; }
            }

            if (landing)
            {
                linearSpeed = 0f;
                Speed = Vector2.Zero;

                Height = MathHelper.Lerp(Height, 0f, 0.02f);
                if (Height < 0.01f) { Height = 0f; landing = false; }

            }

            turning = false;

            if (gameHero.drivingVehicle == this) bladesSpeed = MathHelper.Lerp(bladesSpeed, 0.5f, 0.01f);
            if (gameHero.drivingVehicle == null) bladesSpeed = MathHelper.Lerp(bladesSpeed, 0f, 0.01f);
            bladesRot += bladesSpeed;

            if (gameHero.drivingVehicle == this)
            {
                gameCamera.ZoomTarget = 1f - ((maxCameraScale / 1f) * Height);
            }
        }

        public override void Draw(SpriteBatch sb, LightingEngine lightingEngine)
        {  
            sb.Draw(spriteSheet, Position, new Rectangle(200,0,300,400), lightingEngine.CurrentSunColor, Rotation+MathHelper.PiOver2, new Vector2(150,125), 1f, SpriteEffects.None, 1);
            sb.Draw(spriteSheet, Position, new Rectangle(500, 0, 400, 400), lightingEngine.CurrentSunColor, bladesRot, new Vector2(400, 400) / 2, 1f, SpriteEffects.None, 1);
        }

        public override void DrawShadows(SpriteBatch sb, LightingEngine lightingEngine)
        {
            for (int i = 1; i < 40; i += 2)
            {
                Vector2 pos = Position + new Vector2(lightingEngine.CurrentShadowVect.X * i, lightingEngine.CurrentShadowVect.Y * i);

                sb.Draw(spriteSheet, pos, new Rectangle(200, 0, 300, 400), Color.Black * 0.03f, Rotation + MathHelper.PiOver2, new Vector2(150, 125), 1f, SpriteEffects.None, 1);
                sb.Draw(spriteSheet, pos, new Rectangle(500, 0, 400, 400), Color.Black * 0.03f, bladesRot, new Vector2(400, 400) / 2, 1f, SpriteEffects.None, 1);

            }
        }

        public override void DrawLightBlock(SpriteBatch sb)
        {
            // Arms
            sb.Draw(spriteSheet, Position, new Rectangle(200, 0, 300, 400), Color.Black, Rotation + MathHelper.PiOver2, new Vector2(150, 125), 1f, SpriteEffects.None, 1);
        }

        internal void DrawInAir(SpriteBatch sb, LightingEngine lightingEngine)
        {
            sb.Draw(spriteSheet, Position, new Rectangle(200, 0, 300, 400), lightingEngine.CurrentSunColor, Rotation + MathHelper.PiOver2, new Vector2(150, 125), 1f + (maxCameraScale * Height), SpriteEffects.None, 1);
            sb.Draw(spriteSheet, Position, new Rectangle(500, 0, 400, 400), lightingEngine.CurrentSunColor, bladesRot, new Vector2(400, 400) / 2, 1f + (maxCameraScale * Height), SpriteEffects.None, 1);
        }
        internal void DrawShadowsInAir(SpriteBatch sb, LightingEngine lightingEngine)
        {
            
            for (int i = 1; i < 40; i += 2)
            {
                Vector2 pos = Position + ((lightingEngine.CurrentShadowVect * 750f) * Height) + new Vector2(lightingEngine.CurrentShadowVect.X * i, lightingEngine.CurrentShadowVect.Y * i);

                sb.Draw(spriteSheet, pos, new Rectangle(200, 0, 300, 400), Color.Black * 0.03f, Rotation + MathHelper.PiOver2, new Vector2(150, 125), 1f, SpriteEffects.None, 1);
                sb.Draw(spriteSheet, pos, new Rectangle(500, 0, 400, 400), Color.Black * 0.01f, bladesRot, new Vector2(400, 400) / 2, 1f, SpriteEffects.None, 1);

            }
        }

        internal override void Accelerate(float max)
        {
            if (Height < 1f && !landing) takingOff = true;
            else if(!landing)
            {
                if (linearSpeed < (maxSpeed * max)) linearSpeed += acceleration;
            }

            base.Accelerate(max);
        }

        internal override void Brake()
        {
            if (Height < 1f && !landing) takingOff = true;
            else if (!landing)
            {
                if (linearSpeed > (-(maxSpeed))) linearSpeed -= acceleration;
            }

            base.Brake();
        }

        internal override void Turn(float p)
        {
            if (Height <= 0f) return;

            if (turnAmount > 0f && p < 0f) turnAmount = 0f;
            if (turnAmount < 0f && p > 0f) turnAmount = 0f;
            turnAmount += (turnSpeed * p);

            turning = true;

            base.Turn(p);
        }

        internal void Land(Map gameMap)
        {
            if (landing || takingOff) return;

            bool found = false;
            for (float a = 0f; a < MathHelper.TwoPi; a += 0.5f)
            {
                for (int r = 0; r < 300; r += 20)
                {
                    Vector2 pos = Helper.PointOnCircle(ref Position, r, a);
                    if (gameMap.CheckTileCollision(pos)) found = true;
                    foreach (Vehicle v in VehicleController.Instance.Vehicles) if (v != this && Helper.IsPointInShape(pos, v.CollisionVerts)) found = true;
                }
            }

            if(!found) landing = true;
        }
    }

    
}
