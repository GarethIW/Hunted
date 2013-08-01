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
            coolDownTarget = 200;
            sortOrder = 1;
        }

        public override bool Use(GameTime gameTime, Vector2 target, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!base.Use(gameTime, target, trigger, gameCamera, canCollide)) return false;

            if (owner.GetType() == typeof(HeroDude)) owner.Ammo--;

            AudioController.PlaySFX("pistol", 1f, -0.2f,0.2f, owner.Position);
            ProjectileController.Instance.Add(ProjectileType.Pistol, owner, muzzlePos, target - muzzlePos, canCollide);

            return true;
        }
    }
}
