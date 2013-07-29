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
    public class Jeep : Vehicle
    {

        public Jeep(Vector2 pos):base(pos)
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
            Lights.Add(new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None));
            Lights.Add(new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None));
            le.LightSources.Add(Lights[0]);
            le.LightSources.Add(Lights[1]);
            base.Initialize();
        }

        public override void Update(GameTime gameTime, Map gameMap)
        {
            CollisionVerts.Clear();
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, -0.44f + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, 0.44f + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, -0.44f + MathHelper.Pi + (Rotation)));
            CollisionVerts.Add(Helper.PointOnCircle(ref Position, 150, 0.44f + MathHelper.Pi + (Rotation)));
            base.Update(gameTime, gameMap);

            //HeadTorch.Position = Helper.PointOnCircle(ref Position, 30, Rotation - MathHelper.PiOver2);
            //HeadTorch.Rotation = Rotation - MathHelper.PiOver2;
            Lights[0].Position = Helper.PointOnCircle(ref Position, 137, (Rotation) - 0.2f);
            Lights[1].Position = Helper.PointOnCircle(ref Position, 137, (Rotation) + 0.2f);
            Lights[0].Rotation = Rotation;
            Lights[1].Rotation = Rotation;
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


    }

    
}
