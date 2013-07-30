using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

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

        public override bool Use(GameTime gameTime, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!base.Use(gameTime, trigger, gameCamera, canCollide)) return false;

           // ProjectileController.Instance.Add(ProjectileType.Pistol, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2));
            AudioController.PlaySFX("sword", 1f, -0.3f, 0.3f, owner.Position);

            ProjectileController.Instance.Add(ProjectileType.Knife, owner, Helper.PointOnCircle(ref owner.Position, 75, owner.Rotation - MathHelper.PiOver2), Vector2.Zero, true);
            ProjectileController.Instance.Add(ProjectileType.Knife, owner, Helper.PointOnCircle(ref owner.Position, 15, owner.Rotation - MathHelper.PiOver2), Vector2.Zero, true);

            return true;
        }
    }
}
