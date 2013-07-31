using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TiledLib;

namespace Hunted
{
    public enum ProjectileType
    {
        Knife,
        Pistol,
        SMG,
        Shot,
        Grenade,
        Rocket
    }

    public class Projectile
    {
        public ProjectileType Type;
        public Dude Owner;
        public float Damage;

        public Vector2 Position;
        public Vector2 Velocity;
        public bool Active;
        public bool CanCollide;
        public double Life;
        public float RotationSpeed;
        public float Rotation;
        public Rectangle SourceRect; 
    }

    public class ProjectileController
    {
        public static ProjectileController Instance;

        public List<Projectile> Projectiles;
        public Random Rand = new Random();

        public Texture2D _texProjectiles;

        public ProjectileController()
        {
            Instance = this;

            Projectiles = new List<Projectile>();
        }

        public void LoadContent(ContentManager content)
        {
            _texProjectiles = content.Load<Texture2D>("projectiles");
        }

        public void Update(GameTime gameTime, Map gameMap, HeroDude gameHero)
        {
            foreach (Projectile p in Projectiles.Where(part => part.Active))
            {
                p.Life -= gameTime.ElapsedGameTime.TotalMilliseconds;
                p.Position += p.Velocity;
                p.Rotation += p.RotationSpeed;

                if (p.CanCollide && gameMap.CheckCollision(p.Position,false) == true)
                {
                    p.Velocity = Vector2.Zero;
                    p.RotationSpeed = 0f;
                }

                

                foreach (Vehicle v in VehicleController.Instance.Vehicles)
                {
                    if (Helper.IsPointInShape(p.Position, v.CollisionVerts))
                    {
                        v.HitByProjectile(p);
                        p.Active = false;
                    }
                }
                if (!p.Active) continue;

                if (p.Owner.GetType() == typeof(HeroDude))
                {
                    foreach (AIDude d in EnemyController.Instance.Enemies.Where(dude => dude.Active))
                    {
                        if ((d.Position - p.Position).Length() < 60f)
                        {
                            d.HitByProjectile(p);
                            p.Active = false;
                        }
                    }
                }
                if (!p.Active) continue;


                if (p.Owner.GetType() == typeof(AIDude))
                {
                    if ((gameHero.Position - p.Position).Length() < 60f)
                    {
                        gameHero.HitByProjectile(p);
                        
                    }
                }

                if (p.Life <= 0)
                {
                    p.Active = false;
                }
               
            }



            Projectiles.RemoveAll(part => !part.Active);
        }

        public void Draw(SpriteBatch sb)
        {


            foreach (Projectile p in Projectiles.Where(part => part.Active))
            {
                sb.Draw(_texProjectiles, 
                        p.Position,
                        p.SourceRect, Color.White, p.Rotation, new Vector2(p.SourceRect.Width / 2, p.SourceRect.Height / 2), 1f, SpriteEffects.None, 1);
            }

          
        }

        public void Add(ProjectileType type, Dude owner, Vector2 position, Vector2 direction, bool canCollide)
        {
            direction.Normalize();

            switch (type)
            {
                case ProjectileType.Knife:
                    Add(position, Vector2.Zero, 10, true, new Rectangle(8, 0, 1, 1), 0f, 15f, owner, type);
                    break;
                case ProjectileType.Pistol:
                    Add(position, direction * 20f, 2000, canCollide, new Rectangle(0, 0, 2, 4), Helper.V2ToAngle(direction) + MathHelper.PiOver2, 5f, owner, type);
                    break;
            }
        }

        public void Add(Vector2 spawnPos, Vector2 velocity, float life, bool canCollide, Rectangle sourcerect, float rot, float damage, Dude owner, ProjectileType type)
        {
            Projectile p = new Projectile();
            p.Type = type;
            p.Owner = owner;
            p.Damage = damage;
            p.Position = spawnPos;
            p.Velocity = velocity;
            p.Life = life;
            p.CanCollide = canCollide;
            p.SourceRect = sourcerect;
            p.Active = true;
            //p.RotationSpeed = rot;
            p.Rotation = rot;
            Projectiles.Add(p);
        }


        internal void Reset()
        {
            Projectiles.Clear();
        }
    }
}
