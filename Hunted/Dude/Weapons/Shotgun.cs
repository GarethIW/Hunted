using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted.Weapons
{
    public class Shotgun : Weapon
    {
        public Shotgun(Dude owner)
            : base(owner)
        {
            clipAmmo = 8;
            isAuto = false;
            coolDownTarget = 1000;
            sortOrder = 2;
        }

        public override void Update(GameTime gameTime)
        {
            if (coolDown > 50 && coolDown < 75) AudioController.PlaySFX("shotgunreload", 0.4f, 0f, 0f, owner.Position);
            
            base.Update(gameTime);
        }

        public override bool Use(GameTime gameTime, Vector2 target, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!base.Use(gameTime, target, trigger, gameCamera, canCollide)) return false;

            if (owner.GetType() == typeof(HeroDude)) owner.Ammo-=5;


            AudioController.PlaySFX("shotgun", 1f, -0.2f,0.2f, owner.Position);
            for (float r = -0.3f; r <= 0.3f; r += 0.15f)
            {
                ProjectileController.Instance.Add(ProjectileType.Shot, owner, muzzlePos, muzzlePos - Helper.PointOnCircle(ref owner.Position, 100, (owner.Rotation + MathHelper.PiOver2 + r + (((float)Helper.Random.NextDouble() * 0.1f) - 0.05f)) - 0.25f), canCollide);
            }
            //ProjectileController.Instance.Add(ProjectileType.Shot, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2-0.25f), canCollide);
            //ProjectileController.Instance.Add(ProjectileType.Shot, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2), canCollide);
            //ProjectileController.Instance.Add(ProjectileType.Shot, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2+0.25f), canCollide);
            //ProjectileController.Instance.Add(ProjectileType.Shot, owner, Helper.PointOnCircle(ref owner.Position, 40, owner.Rotation - MathHelper.PiOver2), owner.Position - Helper.PointOnCircle(ref owner.Position, 100, owner.Rotation + MathHelper.PiOver2+0.5f), canCollide);
            
            return true;
        }
    }
}
