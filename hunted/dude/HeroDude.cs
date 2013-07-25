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
            SelectedWeapon = 1;
        }

        internal void Initialize(GraphicsDevice gd, LightingEngine le)
        {
            HeadTorch = new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None);
            le.LightSources.Add(HeadTorch);
            base.Initialize();
        }

        public override void Update(GameTime gameTime, Map gameMap)
        {
            base.Update(gameTime, gameMap);

            HeadTorch.Position = Helper.PointOnCircle(ref Position, 30, Rotation - MathHelper.PiOver2);
            HeadTorch.Rotation = Rotation - MathHelper.PiOver2;
            
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
    }

    
}
