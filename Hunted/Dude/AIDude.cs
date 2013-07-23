using Hunted.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;
using TiledLib.AStar;

namespace Hunted
{
    public enum AIState
    {
        Patrolling,
        Chasing,
        FollowingPath,
        Fleeing,
        Attacking
    }

    public class AIDude : Dude
    {
        public AIState State;
        public Vector2 Target;

        BreadCrumb chasePath;
        bool regeneratePath = false;

        public AIDude(Vector2 pos) : base(pos)
        {
            Target = pos;
            Rotation = (float)Helper.Random.NextDouble() * MathHelper.TwoPi;
            Active = true;
            Dead = false;
            Health = 100f;
        }

        public void LoadContent(Texture2D sheet, GraphicsDevice gd, LightingEngine le)
        {
            spriteSheet = sheet;
            Initialize(gd, le);

            Weapons.Add(new Knife(this));
            if (Helper.Random.Next(2) == 1)
            {
                Weapons.Add(new Pistol(this));
                SelectedWeapon = 1;
            }
        }

        internal void Initialize(GraphicsDevice gd, LightingEngine le)
        {
            HeadTorch = new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None);
            le.LightSources.Add(HeadTorch);
            base.Initialize();
        }

        public void Update(GameTime gameTime, Map gameMap, HeroDude gameHero)
        {
            // Moving
            if ((Position - Target).Length() > 10f)
            {
                Vector2 dir = Target - Position;
                Move(dir);
                //if(State==AIState.Patrolling) 
                if(State!=AIState.Attacking) LookAt(Target);
            }
            else
            {
                Target = Position;
                // Reached target
                switch (State)
                {
                    case AIState.Patrolling:
                        if (Helper.Random.Next(500) == 1)
                        {
                            Vector2 potentialTarget = Helper.RandomPointInCircle(Position, 100, 500);
                            if (!gameMap.CheckCollision(potentialTarget)) Target = potentialTarget;
                        }
                        break;
                }
            }

            switch (State)
            {
                case AIState.Patrolling:
                    // Chase player if in LOS
                    if (CheckLineOfSight(gameHero.Position, gameMap))
                    {
                        Target = Position; // Stop dead in tracks
                        State = AIState.Chasing; // Begin chasing player
                    }

                    // Allow the enemy to "hear" the player if player moves close to enemy
                    if (gameHero.Speed.Length() > 0f && (gameHero.Position - Position).Length() < 250f)
                        LookAt(gameHero.Position);

                    break;
                case AIState.Chasing:
                    Target = gameHero.Position;
                    LookAt(gameHero.Position);
                    if ((gameHero.Position - Position).Length() < 350f && CheckLineOfSight(gameHero.Position, gameMap))
                    {
                        Target = Position;
                        State = AIState.Attacking;
                    }
                    break;                    
                case AIState.FollowingPath:
                    if (Target == Position)
                    {
                        if (chasePath == null || regeneratePath)
                        {
                            regeneratePath = false;
                            //chasePath = PathFinder.FindPath(gameMap.AStarWorld, new Point3D((int)(Position.X / gameMap.TileWidth), (int)(Position.Y / gameMap.TileHeight), 0), new Point3D((int)(gameHero.Position.X / gameMap.TileWidth), (int)(gameHero.Position.Y / gameMap.TileHeight), 0));
                            if (chasePath != null) Target = new Vector2((chasePath.position.X * gameMap.TileWidth) + (gameMap.TileWidth / 2), (chasePath.position.Y * gameMap.TileHeight) + (gameMap.TileHeight / 2));
                            else State = AIState.Patrolling;
                        }
                        else
                        {
                            chasePath = chasePath.next;
                            if (chasePath != null) Target = new Vector2((chasePath.position.X * gameMap.TileWidth) + (gameMap.TileWidth / 2), (chasePath.position.Y * gameMap.TileHeight) + (gameMap.TileHeight / 2));
                            else State = AIState.Chasing;
                        }
                    }
                    if (CheckLineOfSight(gameHero.Position, gameMap))
                    {
                        Target = Position; // Stop dead in tracks
                        State = AIState.Chasing; // Begin chasing player
                    }
                    break;
                case AIState.Attacking:
                    LookAt(gameHero.Position);
                    Attack(gameTime, true);
                    if (Weapons[SelectedWeapon].GetType() != typeof(Knife))
                    {
                        if ((gameHero.Position - Position).Length() > 400f)
                        {
                            Target = gameHero.Position;
                            State = AIState.Chasing;
                        }

                        if (Helper.Random.Next(100) == 1)
                        {
                            Vector2 potentialTarget = Helper.RandomPointInCircle(Position, 100, 500);
                            if (!gameMap.CheckCollision(potentialTarget)) Target = potentialTarget;
                        }
                    }
                    else
                    {
                        Target = gameHero.Position;
                        if ((gameHero.Position - Position).Length() < 80f) Target = Position;
                    }

                    break;
            }
                   

            

            base.Update(gameTime, gameMap);

            HeadTorch.Position = Helper.PointOnCircle(ref Position, 30, Rotation - MathHelper.PiOver2);
            HeadTorch.Rotation = Rotation - MathHelper.PiOver2;
        }

        public override void Collided()
        {
            switch (State)
            {
                case AIState.Patrolling:
                    Target = Position;
                    break;
                case AIState.Chasing:
                    Target = Position;
                    State = AIState.FollowingPath;
                    break;
                case AIState.FollowingPath:
                    Target = Position;
                    regeneratePath = true;
                    break;
                case AIState.Attacking:
                    Target = Position;
                    State = AIState.FollowingPath;
                    regeneratePath = true;
                    break;
            }

            base.Collided();
        }

        public override void Draw(SpriteBatch sb, LightingEngine lightingEngine)
        {
            //DrawChasePath(sb, chasePath);
            base.Draw(sb, lightingEngine);
        }

        void DrawChasePath(SpriteBatch sb, BreadCrumb p)
        {
            if(p!=null)
            {
                sb.Draw(spriteSheet, new Vector2((p.position.X * 100) + (100 / 2), (p.position.Y * 100) + (100 / 2)), new Rectangle(50,50,10,10), Color.Red, (float)Helper.Random.NextDouble(), new Vector2(5,5)/2, 1f, SpriteEffects.None, 1);
                DrawChasePath(sb, p.next);
            }
        }

        bool CheckLineOfSight(Vector2 pos, Map gameMap)
        {
            for (float a = (Rotation-MathHelper.PiOver2) - MathHelper.PiOver4; a < (Rotation-MathHelper.PiOver2) + MathHelper.PiOver4; a += 0.05f)
            {
                for (int r = 0; r < 500; r += 50)
                {
                    Vector2 checkpos = Helper.PointOnCircle(ref Position, r, a);
                    if (gameMap.CheckTileCollision(checkpos)) break;
                    if ((checkpos - pos).Length() < 20f) return true;
                }
            }

            return false;
        }

        
    }

    
}
