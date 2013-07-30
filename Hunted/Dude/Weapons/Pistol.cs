using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted.Weapons
{
    public class Pistol : Weapon
    {
        public Pistol(Dude owner)
            : base(owner)
        {
            clipAmmo = 30;
            isAuto = false;
            coolDownTarget = 500;
        }

        public override bool Use(GameTime gameTime, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!base.Use(gameTime, trigger, gameCamera, canCollide)) return false;

            AudioController.PlaySFX("pistol", 1f, -0.2f,0.2f, owner.Position);
            ProjectileController.Instance.Add(ProjectileType.Pistol, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2), canCollide);

            return true;
        }
    }
}
