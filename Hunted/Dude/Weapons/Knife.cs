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
            coolDownTarget = 500;
        }

        public override bool Use(GameTime gameTime, bool trigger)
        {
            if (!base.Use(gameTime, trigger)) return false;

           // ProjectileController.Instance.Add(ProjectileType.Pistol, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2));

            ProjectileController.Instance.Add(ProjectileType.Knife, owner, Helper.PointOnCircle(ref owner.Position, 75, owner.Rotation - MathHelper.PiOver2), Vector2.Zero);
            ProjectileController.Instance.Add(ProjectileType.Knife, owner, Helper.PointOnCircle(ref owner.Position, 15, owner.Rotation - MathHelper.PiOver2), Vector2.Zero);

            return true;
        }
    }
}
