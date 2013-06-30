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

namespace Hunted
{
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public bool Active;
        public bool AffectedByGravity;
        public bool CanCollide;
        public float Alpha;
        public double Life;
        public float RotationSpeed;
        public float Rotation;
        public Color Color;
        public Rectangle SourceRect; 
    }

    public class ParticleController
    {
        const int MAX_PARTICLES = 3000;

        public Particle[] Particles;
        public Random Rand = new Random();

        public Texture2D _texParticles;

        public ParticleController()
        {
            Particles = new Particle[MAX_PARTICLES];
        }

        public void LoadContent(ContentManager content)
        {
            _texParticles = content.Load<Texture2D>("particles");

            for (int i = 0; i < MAX_PARTICLES; i++)
                Particles[i] = new Particle();
        }

        public void Update(GameTime gameTime)
        {
            foreach (Particle p in Particles)
            {
                p.Life -= gameTime.ElapsedGameTime.TotalMilliseconds;
                p.Position += p.Velocity;
                p.Rotation += p.RotationSpeed;

                if (p.AffectedByGravity) p.Velocity.Y += 0.1f;

                if (p.CanCollide && GameManager.Map.CheckCollision(p.Position) != null && GameManager.Map.CheckCollision(p.Position)==true)
                {
                    p.Velocity = Vector2.Zero;
                    p.RotationSpeed = 0f;
                }

                if (p.Life <= 0)
                {
                    p.Alpha -= 0.01f;
                    if (p.Alpha < 0.05f) p.Active = false;
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
         

            foreach (Particle p in Particles)
            {
                sb.Draw(_texParticles, 
                        p.Position,
                        p.SourceRect, p.Color * p.Alpha, p.Rotation, new Vector2(p.SourceRect.Width / 2, p.SourceRect.Height / 2), 1f, SpriteEffects.None, 1);
            }

          
        }

        public void Add(Vector2 spawnPos, Vector2 velocity, float life, bool affectedbygravity, bool canCollide, Rectangle sourcerect, float rot, Color col)
        {
            foreach (Particle p in Particles)
                if (!p.Active)
                {
                    p.Position = spawnPos;
                    p.Velocity = velocity;
                    p.Life = life;
                    p.AffectedByGravity = affectedbygravity;
                    p.CanCollide = canCollide;
                    p.SourceRect = sourcerect;
                    p.Alpha = 1f;
                    p.Active = true;
                    p.RotationSpeed = rot;
                    p.Color = col;
                    p.Rotation = rot;
                    break;
                }
        }

        

        public void AddGSW(Vector2 pos, Vector2 velocity)
        {
            Vector2 tempV = pos;
            float amount = 0f;
            while (amount<1f)
            {
                Add(tempV, velocity + new Vector2((float)(Rand.NextDouble()*2)-1f,(float)(Rand.NextDouble()*2)-1f), 3000, true, true, new Rectangle(0, 0, 8, 8), 0f, Color.White);
                tempV = Vector2.Lerp(pos, pos+velocity, amount);
                amount += 0.1f;
            }
        }

        public void AddMetalDebris(Vector2 pos, Vector2 velocity)
        {
            Vector2 tempV = pos;
            float amount = 0f;
            while (amount < 1f)
            {
                float grey = (float)Rand.NextDouble();
                Add(tempV, velocity + new Vector2((float)(Rand.NextDouble() * 2) - 1f, (float)(Rand.NextDouble() * 2) - 1f), 3000, true, true, new Rectangle(8, 0, 8, 8), 0f, 
                    new Color(grey, grey, grey));
                tempV = Vector2.Lerp(pos, pos + velocity, amount);
                amount += 0.2f;
            }
            tempV = pos;
            //amount = 0f;
            //while (amount < 1f)
            //{
                Add(tempV, (velocity * 2f) + new Vector2((float)(Rand.NextDouble()) - 0.5f, (float)(Rand.NextDouble()) - 0.5f), 100, false, true, new Rectangle(8, 0, 8, 8), 0f,
                    new Color(0.9f+((float)Rand.NextDouble()*0.1f), (float)Rand.NextDouble()*0.5f + 0.5f, 0f));
                tempV = Vector2.Lerp(pos, pos + velocity, amount);
                amount += 0.3f;
            //}
        }

        public void AddSpark(Vector2 pos, Vector2 velocity)
        {
            Add(pos, (velocity * 2f) + new Vector2((float)(Rand.NextDouble()) - 0.5f, (float)(Rand.NextDouble()) - 0.5f), 100, true, true, new Rectangle(8, 0, 8, 8), 0f,
                   new Color(0.9f + ((float)Rand.NextDouble() * 0.1f), (float)Rand.NextDouble() * 0.5f + 0.5f, 0f));
        }

        public void AddExplosion(Vector2 pos)
        {
            Vector2 tempV = pos;
            float amount = 0f;
            Vector2 velocity = new Vector2(0, 0f);
            while (amount < 1f)
            {
                Add(tempV, (velocity) + new Vector2((float)(Rand.NextDouble() * 8) - 4f, (float)(Rand.NextDouble() * 8) - 4f), 1000, true, false, new Rectangle(8, 0, 8, 8), 0f, 
                    new Color(0.9f+((float)Rand.NextDouble()*0.1f), (float)Rand.NextDouble()*0.5f + 0.5f, 0f));
                tempV = Vector2.Lerp(pos, pos + velocity, amount);
                amount += 0.01f;
            }
        }

        public void AddGibs(Vector2 pos)
        {
            Add(pos + new Vector2(0, -8), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(16, 0, 8, 8), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(0, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(56, 0, 8, 8), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-8, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(24, 0, 8, 8), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(8, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(32, 0, 8, 8), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-8, 8), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(40, 0, 8, 8), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(8, 8), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(48, 0, 8, 8), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
        }

        public void AddChopperGibs(Vector2 pos)
        {
            Add(pos + new Vector2(-32, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(64, 0, 18, 18), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(0, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(87, 0, 18, 18), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(32, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(111, 0,18, 18), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(0, -10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(64, 45, 64, 8), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
        }

        public void AddBoatGibs(Vector2 pos)
        {
            Add(pos + new Vector2(-32, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(64, 64, 32, 16), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(32, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(64, 81, 32, 16), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-16, -16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(96, 64, 32, 16), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(16, -16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(96, 81, 32, 16), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
        }

        public void AddSpiderGibs(Vector2 pos)
        {
            Add(pos + new Vector2(-16, -16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(128, 64, 16, 16), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(16, -16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(142, 64, 16, 16), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-16, 16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(160, 64, 16, 16), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(16, 16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(176, 64, 16, 16), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(0, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(128, 100, 28, 28), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

        }

        public void AddJeepGibs(Vector2 pos)
        {
            Add(pos + new Vector2(-16, -16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(128, 0, 32, 24), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(16, -16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(160, 0, 32, 24), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-16, 16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(128, 50, 14, 14), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(16, 16), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(142, 50, 14, 14), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(0, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 5000, true, true, new Rectangle(172, 52, 18, 10), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

        }

        public void AddHeroGibs(Vector2 pos)
        {
            Add(pos + new Vector2(0, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(64, 128, 26, 15), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(0, -10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(91, 128, 37, 23), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

            Add(pos + new Vector2(10, 10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(68, 148, 9, 9), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-10, 10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(68, 148, 9, 9), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

            Add(pos + new Vector2(10, 15), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(82, 150, 5, 5), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-10, 15), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(82, 150, 5, 5), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

            Add(pos + new Vector2(10, 10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(64, 169, 4, 23), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-10, 10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(64, 169, 4, 23), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

            Add(pos + new Vector2(-10, 15), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(72, 174, 4, 23), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(10, 15), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(72, 174, 4, 23), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

            Add(pos + new Vector2(-10, 20), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(114, 187, 14, 5), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(10, 20), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(114, 187, 14, 5), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
        }

        public void AddBossGibs(Vector2 pos)
        {
            Add(pos + new Vector2(0, 0), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(192, 128, 44, 29), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(0, -10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(192, 162, 46, 31), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

            Add(pos + new Vector2(10, 10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(128, 192, 43, 37), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-10, 10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(128, 232, 54, 25), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

            Add(pos + new Vector2(10, 15), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(192, 192, 41, 38), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-10, 15), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(206, 232, 50,24), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);

            Add(pos + new Vector2(10, 10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(238, 128, 18, 18), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(-10, 10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(238, 128, 18, 18), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
            Add(pos + new Vector2(0, 10), new Vector2((float)(Rand.NextDouble() * 4) - 2f, (float)(Rand.NextDouble() * 4) - 2f), 20000, true, true, new Rectangle(238, 128, 18, 18), (float)(Rand.NextDouble() / 10) - 0.05f, Color.White);
        }

        internal void Reset()
        {
            foreach (Particle p in Particles)
            {
                p.Active = false;
            }
        }
    }
}
