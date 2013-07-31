using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted.Weapons
{
    public class Rifle : Weapon
    {
        int roundsCount = 0;

        public Rifle(Dude owner)
            : base(owner)
        {
            clipAmmo = 50;
            isAuto = true;
            coolDownTarget = 75;
        }

        public override bool Use(GameTime gameTime, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!trigger)
            {
                roundsCount = 0;
                coolDownTarget = 100;
            }


            if (!base.Use(gameTime, trigger, gameCamera, canCollide)) return false;

            roundsCount++;
            if (roundsCount == 3)
            {
                roundsCount = 0;
                coolDownTarget = 600;
            }
            else coolDownTarget = 75;

            if (owner.GetType() == typeof(HeroDude)) owner.Ammo--;


            AudioController.PlaySFX("rifle", 1f, 0f,0.3f, owner.Position);
            ProjectileController.Instance.Add(ProjectileType.Rifle, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2), canCollide);

            return true;
        }
    }
}
