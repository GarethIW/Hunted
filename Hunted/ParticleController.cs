﻿using System;
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
    public enum SpecialParticle
    {
        None,
        Blood
    }

    public class Particle
    {
        public SpecialParticle Special;
        public Vector2 Position;
        public Vector2 Velocity;
        public bool Active;
        public bool CanCollide;
        public float Alpha;
        public double Life;
        public float RotationSpeed;
        public float Rotation;
        public float Scale;
        public Color Color;
        public Rectangle SourceRect; 
    }

    public class ParticleController
    {
        public static ParticleController Instance;

        public List<Particle> Particles;
        public Random Rand = new Random();

        public Texture2D _texParticles;

        public ParticleController()
        {
            Instance = this;

            Particles = new List<Particle>();
        }

        public void LoadContent(ContentManager content)
        {
            _texParticles = content.Load<Texture2D>("particles");
        }

        public void Update(GameTime gameTime, Map gameMap)
        {
            foreach (Particle p in Particles.Where(part => part.Active))
            {
                p.Life -= gameTime.ElapsedGameTime.TotalMilliseconds;
                p.Position += p.Velocity;
                p.Rotation += p.RotationSpeed;

                if (p.CanCollide && gameMap.CheckCollision(p.Position) == true)
                {
                    p.Velocity = Vector2.Zero;
                    p.RotationSpeed = 0f;
                }

                if (p.Life <= 0)
                {
                    p.Alpha -= 0.01f;
                    if (p.Alpha < 0.05f) p.Active = false;
                }

                switch (p.Special)
                {
                    case SpecialParticle.Blood:
                        if (p.Scale > 0.25f) p.Scale -= 0.01f;
                        break;
                }
            }

            Particles.RemoveAll(part => !part.Active);
        }

        public void Draw(SpriteBatch sb)
        {
         

            foreach (Particle p in Particles.Where(part => part.Active))
            {
                sb.Draw(_texParticles, 
                        p.Position,
                        p.SourceRect, p.Color * p.Alpha, p.Rotation, new Vector2(p.SourceRect.Width / 2, p.SourceRect.Height / 2), p.Scale, SpriteEffects.None, 1);
            }

          
        }

        public void Add(Vector2 spawnPos, Vector2 velocity, float life, bool canCollide, Rectangle sourcerect, float rot, Color col, SpecialParticle special)
        {
            Particle p = new Particle();
            p.Special = special;
            p.Position = spawnPos;
            p.Velocity = velocity;
            p.Life = life;
            p.CanCollide = canCollide;
            p.SourceRect = sourcerect;
            p.Alpha = 1f;
            p.Active = true;
            p.RotationSpeed = rot;
            p.Color = col;
            p.Rotation = rot;
            p.Scale = 1f;
            Particles.Add(p);
        }


        internal void Reset()
        {
            Particles.Clear();
        }
    }
}
