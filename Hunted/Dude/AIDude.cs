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
        Attacking,
        Investigating
    }

    public class AIDude : Dude
    {
        public AIState State;
        public Vector2 Target;

        public bool BelongsToCompound = false;
        public bool IsGeneral = false;
        public bool Discovered = false;

        BreadCrumb chasePath;
        bool regeneratePath = false;

        double checkLOSTime = 0;
        double genPathTime = 0;
        double checkTorchTime = 0;

        public AIDude(Vector2 pos) : base(pos)
        {
            Target = pos;
            Rotation = (float)Helper.Random.NextDouble() * MathHelper.TwoPi;
            Active = true;
            Dead = false;
            Health = 100f;
            Ammo = 100;
        }

        public void LoadContent(Texture2D sheet, GraphicsDevice gd, LightingEngine le, HeroDude gameHero)
        {
            spriteSheet = sheet;
            Initialize(gd, le);

            Weapons.Add(new Knife(this));
            if (Helper.Random.Next(gameHero.Weapons.Count>1?2:10) == 1)
            {
                Weapons.Add(new Pistol(this));
            }

            if (IsGeneral)
            {
                Weapons.Add(new Rifle(this));
            }
            else
            {
                if (BelongsToCompound)
                {
                    switch (Helper.Random.Next(20))
                    {
                        case 1:
                            Weapons.Add(new Shotgun(this));
                            break;
                        case 2:
                            Weapons.Add(new SMG(this));
                            break;
                        case 3:
                            Weapons.Add(new Rifle(this));
                            break;
                    }
                }
                else
                {
                    switch (Helper.Random.Next(gameHero.Weapons.Count > 1 ? (gameHero.Weapons.Count > 2 ? 30 : 50) : 100))
                    {
                        case 1:
                            Weapons.Add(new Shotgun(this));
                            break;
                        case 2:
                            Weapons.Add(new SMG(this));
                            break;
                        case 3:
                            Weapons.Add(new Rifle(this));
                            break;
                    }
                }
            }
            SelectedWeapon = Weapons.Count - 1;

            

        }

        internal void Initialize(GraphicsDevice gd, LightingEngine le)
        {
            HeadTorch = new LightSource(gd, 500, LightAreaQuality.High, Color.White, BeamStencilType.Narrow, SpotStencilType.None);
            le.LightSources.Add(HeadTorch);
            base.Initialize();
        }

        public void Update(GameTime gameTime, Map gameMap, HeroDude gameHero, bool[,] mapFog, Camera gameCamera)
        {
            checkLOSTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            genPathTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            checkTorchTime += gameTime.ElapsedGameTime.TotalMilliseconds;
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
                        gameHero.HuntedLevel.Seen(gameHero.Position);
                        Target = Position; // Stop dead in tracks
                        if (gameHero.drivingVehicle == null)
                            State = AIState.Chasing; // Begin chasing player
                        else
                        {
                            if (Weapons[SelectedWeapon].GetType() != typeof(Knife)) State = AIState.Attacking;
                        }
                    }
                    

                    // Allow the enemy to "hear" the player if player moves close to enemy
                    if ((gameHero.Speed.Length() > 0f && (gameHero.Position - Position).Length() < 250f) || (gameHero.drivingVehicle != null && (gameHero.Position - Position).Length() < 800f))
                    {
                        gameHero.HuntedLevel.Heard(gameHero.Position, false);
                        LookAt(gameHero.Position);
                    }

                    if ((gameHero.Position - Position).Length() < 800f && 
                        insideBuilding == null && 
                        gameHero.drivingVehicle is Chopper && 
                        ((Chopper)gameHero.drivingVehicle).Height >0f &&
                        !(Weapons[SelectedWeapon] is Knife)) State = AIState.Attacking;

                    break;
                case AIState.Chasing:
                    Target = gameHero.Position;
                    LookAt(gameHero.Position);
                    if(gameHero.drivingVehicle is Chopper) State = AIState.Patrolling;
                    if (((gameHero.Position - Position).Length() < 450f && CheckLineOfSight(gameHero.Position, gameMap)) || ((gameHero.Position - Position).Length() < 800f && gameHero.drivingVehicle is Chopper))
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
                            if (genPathTime > 1000)
                            {
                                genPathTime = 0;
                                regeneratePath = false;
                                chasePath = PathFinder.FindPath(gameMap.AStarWorld, new Point3D((int)(Position.X / gameMap.TileWidth), (int)(Position.Y / gameMap.TileHeight), 0), new Point3D((int)(gameHero.Position.X / gameMap.TileWidth), (int)(gameHero.Position.Y / gameMap.TileHeight), 0));
                                if (chasePath != null) Target = new Vector2((chasePath.position.X * gameMap.TileWidth) + (gameMap.TileWidth / 2), (chasePath.position.Y * gameMap.TileHeight) + (gameMap.TileHeight / 2));
                                else State = AIState.Patrolling;
                            }
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
                    if (gameHero.drivingVehicle is Chopper && insideBuilding != null)
                    {
                        State = AIState.Patrolling;
                        break;
                    }

                    LookAt(gameHero.Position);
                    bool shootUp = (gameHero.drivingVehicle != null && gameHero.drivingVehicle is Chopper && ((Chopper)gameHero.drivingVehicle).Height >0f);
                    Attack(gameTime, gameHero.Position, true, gameCamera, !shootUp);
                    if (Weapons[SelectedWeapon].GetType() != typeof(Knife))
                    {
                        if ((gameHero.Position - Position).Length() > 450f)
                        {
                            if (gameHero.drivingVehicle == null)
                            {
                                Target = gameHero.Position;
                                State = AIState.Chasing;
                            }
                            else State = AIState.Patrolling;
                        }

                        if (Helper.Random.Next(100) == 1)
                        {
                            Vector2 potentialTarget = Helper.RandomPointInCircle(Position, 100, 500);
                            if (!gameMap.CheckCollision(potentialTarget)) Target = potentialTarget;
                        }
                    }
                    else
                    {
                        if (gameHero.drivingVehicle == null)
                        {
                            Target = gameHero.Position;
                            if ((gameHero.Position - Position).Length() < 100f) Target = Position;
                        }
                        else State = AIState.Patrolling;
                    }

                    break;
                case AIState.Investigating:
                    if (Target == Position)
                    {
                        if (chasePath == null || regeneratePath)
                        {
                            if (genPathTime > 1000)
                            {
                                genPathTime = 0;
                                regeneratePath = false;
                                chasePath = PathFinder.FindPath(gameMap.AStarWorld, new Point3D((int)(Position.X / gameMap.TileWidth), (int)(Position.Y / gameMap.TileHeight), 0), new Point3D((int)(gameHero.HuntedLevel.LastKnownPosition.X / gameMap.TileWidth), (int)(gameHero.HuntedLevel.LastKnownPosition.Y / gameMap.TileHeight), 0));
                                if (chasePath != null) Target = new Vector2((chasePath.position.X * gameMap.TileWidth) + (gameMap.TileWidth / 2), (chasePath.position.Y * gameMap.TileHeight) + (gameMap.TileHeight / 2));
                                else State = AIState.Patrolling;
                            }
                        }
                        else
                        {
                            chasePath = chasePath.next;
                            if (chasePath != null) Target = new Vector2((chasePath.position.X * gameMap.TileWidth) + (gameMap.TileWidth / 2), (chasePath.position.Y * gameMap.TileHeight) + (gameMap.TileHeight / 2));
                            else State = AIState.Patrolling;
                        }
                    }
                    if ((Position - gameHero.HuntedLevel.LastKnownPosition).Length() < 200f) State = AIState.Patrolling;
                    
                        
                        if (CheckLineOfSight(gameHero.Position, gameMap))
                        {
                            Target = Position; // Stop dead in tracks
                            State = AIState.Chasing; // Begin chasing player
                        }
                    
                    break;
            }

            if (gameHero.Dead) State = AIState.Patrolling;

            if (Health <= 0 && !Dead)
            {
                deadTime = 5000;
                deadAlpha = 1f;
                Dead = true;
                SpawnDrops(gameMap, gameHero);
                LightingEngine.Instance.RemoveSource(HeadTorch);
                ParticleController.Instance.AddBloodPool(Position);

                if (IsGeneral)
                {
                    Hud.Instance.Ticker.AddLine("> You have eliminated a General! " + (3 - EnemyController.Instance.Enemies.Count(e => e.IsGeneral)) + "/" + (3));

                }
            }

            if (IsGeneral)
            {
                if ((gameHero.Position - Position).Length() < 720f && !Discovered && gameHero.drivingVehicle==null && !LineCollision(gameHero.Position, gameMap, true)) 
                {
                    Discovered = true;
                    Hud.Instance.Ticker.AddLine("> You have found a General!");
                }
            }

            HeadTorch.Position = Helper.PointOnCircle(ref Position, 32, Rotation - MathHelper.PiOver2);
            HeadTorch.Rotation = Rotation - MathHelper.PiOver2;

            if (checkTorchTime > 250)
            {
                checkTorchTime = 0;
                if ((Position.X < gameCamera.Position.X - ((gameCamera.Width / gameCamera.Zoom) / 2) || Position.X > gameCamera.Position.X + ((gameCamera.Width / gameCamera.Zoom) / 2) ||
                   Position.Y < gameCamera.Position.Y - ((gameCamera.Height / gameCamera.Zoom) / 2) || Position.Y > gameCamera.Position.Y + ((gameCamera.Height / gameCamera.Zoom) / 2)) &&
                   LineCollision(gameHero.Position, gameMap, false))
                {
                    HeadTorch.Active = false;
                }
            }
            else if (!Dead) HeadTorch.Active = true;

            if (IsGeneral)
                Animations["head"].XOffset = 4;
            else
                Animations["head"].XOffset = 2;

            if (Dead)
            {
                if (deadTime <= 0)
                {
                    deadAlpha -= 0.01f;
                }
            }

            if (Dead && deadTime <= 0 && deadAlpha <= 0f)
            {
                Active = false;
            }

            base.Update(gameTime, gameMap, mapFog, gameHero);

            
        }

        private void SpawnDrops(Map gameMap, HeroDude gameHero)
        {
            bool hasweapon = false;
            foreach(Weapon w in gameHero.Weapons)
                if(w.GetType()==Weapons[SelectedWeapon].GetType()) hasweapon = true;

            if (hasweapon)
            {
                int drop = Helper.Random.Next(65);
                if (drop < 5) return;
                if (drop >= 5 && drop < 35)
                {
                    if (Helper.Random.Next(2) == 0)
                    {
                        List<Compound> c = gameMap.FindNearestCompounds(Position);
                        if (c[0].Discovered == false) ItemController.Instance.Spawn(ItemType.CompoundMap, Position);
                        else if (BelongsToCompound)
                        {
                            bool found = false;
                            for (int i = 1; i <= 2; i++)
                            {
                                if (c[i].Discovered == false)
                                {
                                    ItemController.Instance.Spawn(ItemType.CompoundMap, Position);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) ItemController.Instance.Spawn(ItemType.Ammo, Position);
                        }
                        else ItemController.Instance.Spawn(ItemType.Health, Position);
                    }
                    else ItemController.Instance.Spawn(ItemType.Health, Position);
                }
                if (drop >= 35 && drop < 58)
                {
                    if (Helper.Random.Next(2) == 0)
                    {
                        List<Compound> c = gameMap.FindNearestCompounds(Position);
                        if (c[0].Discovered == false) ItemController.Instance.Spawn(ItemType.CompoundMap, Position);
                        else if (BelongsToCompound)
                        {
                            bool found = false;
                            for (int i = 1; i <= 2; i++)
                            {
                                if (c[i].Discovered == false)
                                {
                                    ItemController.Instance.Spawn(ItemType.CompoundMap, Position);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) ItemController.Instance.Spawn(ItemType.Ammo, Position);
                        }
                        else ItemController.Instance.Spawn(ItemType.Ammo, Position);
                    }
                    else ItemController.Instance.Spawn(ItemType.Ammo, Position);
                }
                if (drop >= 58 && BelongsToCompound)
                {
                    foreach (AIDude e in EnemyController.Instance.Enemies.Where(en => en.IsGeneral).OrderBy(en => (en.Position - Position).Length()))
                    {
                        if (!e.Discovered && (e.Position - Position).Length() < 20000)
                        {
                            ItemController.Instance.Spawn(ItemType.GeneralMap, Position);
                            break;
                        }
                        else break;
                    }
                }
            }
            else
            {
                if (Weapons[SelectedWeapon] is Pistol) ItemController.Instance.Spawn(ItemType.Pistol, Position);
                if (Weapons[SelectedWeapon] is Shotgun) ItemController.Instance.Spawn(ItemType.Shotgun, Position);
                if (Weapons[SelectedWeapon] is SMG) ItemController.Instance.Spawn(ItemType.SMG, Position);
                if (Weapons[SelectedWeapon] is Rifle) ItemController.Instance.Spawn(ItemType.Rifle, Position);
               
            }
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
                    //if ((Target - Position).Length() > 100f)
                    //{
                    //    Target = Position;
                    //    State = AIState.FollowingPath;
                    //    regeneratePath = true;
                    //}
                    break;
            }

            base.Collided();
        }

        public override void Draw(SpriteBatch sb, LightingEngine lightingEngine)
        {
            if (Dead) return;

            //DrawChasePath(sb, chasePath);
            base.Draw(sb, lightingEngine);

            // Head
            //if(IsGeneral)
            //    sb.Draw(spriteSheet, Position, Animations["head"].CellRect, Color.Red, Rotation, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
            //else
            //    sb.Draw(spriteSheet, Position, Animations["head"].CellRect, Color.LightBlue, Rotation, new Vector2(100, 100) / 2, 1f, SpriteEffects.None, 1);
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
            if ((Position - pos).Length() > 500f) return false;

            if (checkLOSTime > 200)
            {
                checkLOSTime = 0;

                for (float a = (Rotation - MathHelper.PiOver2) - MathHelper.PiOver4; a < (Rotation - MathHelper.PiOver2) + MathHelper.PiOver4; a += 0.1f)
                {
                    for (int r = 0; r < 500; r += 50)
                    {
                        Vector2 checkpos = Helper.PointOnCircle(ref Position, r, a);
                        if (gameMap.CheckTileCollision(checkpos)) break;
                        if ((checkpos - pos).Length() < 50f) return true;
                    }
                }
            }

            return false;
        }

        public override void HitByProjectile(Projectile p)
        {
            base.HitByProjectile(p);

            State = AIState.Chasing;
        }

        internal void InvestigatePosition()
        {
            if (State == AIState.Patrolling)
            {
                regeneratePath = true;
                Target = Position;
                State = AIState.Investigating;
            }
        }
        
    }

    
}
