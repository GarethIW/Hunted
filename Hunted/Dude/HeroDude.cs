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
    class HeroDude : Dude
    {
        public HeroDude(Vector2 pos) : base(pos)
        {

        }

        public void LoadContent(ContentManager content, GraphicsDevice gd, LightingEngine le)
        {
            spriteSheet = content.Load<Texture2D>("dude");
            Initialize(gd, le);
        }

        internal void Initialize(GraphicsDevice gd, LightingEngine le)
        {
            HeadTorch = new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None);
            le.LightSources.Add(HeadTorch);
            base.Initialize();
        }

        public override void Update(GameTime gameTime, Map gameMap)
        {
            HeadTorch.Position = Helper.PointOnCircle(ref Position, 40, Rotation - MathHelper.PiOver2);
            HeadTorch.Rotation = Rotation - MathHelper.PiOver2;
            base.Update(gameTime, gameMap);
        }

        public override void Draw(SpriteBatch sb, LightingEngine lightingEngine)
        {
            base.Draw(sb, lightingEngine);
        }

       
    }

    
}
