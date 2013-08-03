using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted.Weapons
{
    public class Weapon
    {
        internal int clipAmmo;
        internal int maxClipAmmo;
        internal int maxAmmo;

        internal double coolDownTarget;
        internal double coolDown;
        internal bool isAuto;
        internal bool triggerHeld;

        internal Dude owner;

        internal int sortOrder;

        internal Vector2 muzzlePos;

        public Weapon(Dude own)
        {
            owner = own;

            triggerHeld = false;
            coolDown = 0;
        }

        public virtual void Update(GameTime gameTime)
        {
            coolDown += gameTime.ElapsedGameTime.TotalMilliseconds;

            muzzlePos = Helper.PointOnCircle(ref owner.Position, 65, (owner.Rotation - MathHelper.PiOver2) + 0.26f);
        }

        public virtual bool Use(GameTime gameTime, Vector2 target, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!trigger)
            {
                triggerHeld = false;
                return false;
            }

            if (triggerHeld && !isAuto && (owner.GetType() == typeof(HeroDude))) return false;
            triggerHeld = true;

            if (coolDown < coolDownTarget * ((owner is HeroDude)?1f:2f)) return false;
            coolDown = 0;

            if (this.GetType() != typeof(Knife))
            {
                //if (clipAmmo <= 0) return false;
                if (owner.Ammo <= 0 && owner.GetType() == typeof(HeroDude)) return false;

                //if (owner.GetType() == typeof(HeroDude)) clipAmmo--;
            }

            //if ((owner.GetType() == typeof(AIDude)))
            //{
                

            
            //}

            if (owner is HeroDude && !(this is Knife)) EnemyController.Instance.HeroFiredShot((HeroDude)owner);

            return true;
        }

       
    }
}
