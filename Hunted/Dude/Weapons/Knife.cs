using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hunted.Weapons
{
    public class Knife : Weapon
    {
        public Knife(Dude owner)
            : base(owner)
        {
            clipAmmo = 1;
            isAuto = true;
            coolDown = 500;
        }

        public override bool Use(GameTime gameTime, bool trigger)
        {
            if (!base.Use(gameTime, trigger)) return false;

           // ProjectileController.Instance.Add(ProjectileType.Pistol, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2));

            return true;
        }
    }
}
