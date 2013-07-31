using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted.Weapons
{
    public class SMG : Weapon
    {
        int roundsCount = 0;
        int AIroundscount = 0;

        public SMG(Dude owner)
            : base(owner)
        {
            clipAmmo = 50;
            isAuto = true;
            coolDownTarget = 30;


        }

        public override bool Use(GameTime gameTime, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!base.Use(gameTime, trigger, gameCamera, canCollide)) return false;

            if(owner is AIDude) AIroundscount++;
            roundsCount++;
            if (roundsCount == 2)
            {
                roundsCount = 0;
                if (owner.GetType() == typeof(HeroDude)) owner.Ammo--;
            }

            if (AIroundscount >= 20)
            {
                AIroundscount = 0;
                coolDownTarget = 1000;
            }
            else coolDownTarget = 30;


            AudioController.PlaySFX("smg", 1f, -0.2f,0.2f, owner.Position);
            ProjectileController.Instance.Add(ProjectileType.SMG, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2 + (((float)Helper.Random.NextDouble() * 0.02f) - 0.01f)), canCollide);

            return true;
        }
    }
}
