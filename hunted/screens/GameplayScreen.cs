#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using TiledLib;
using System.Collections.Generic;
#endregion

namespace Hunted
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        Map gameMap;
        Camera gameCamera;
        HeroDude gameHero;
        Hud gameHud;

        EnemyController enemyController;
        ParticleController particleController;
        ProjectileController projectileController;
        ItemController itemController;
        VehicleController vehicleController;

        LightingEngine lightingEngine = new LightingEngine();

        RenderTarget2D minimapRT;
        bool[,] mapFog;

        Vector2 mousePos;

        KeyboardState lastKeyboardState;

        //iledLib.LightSource cameraLightSource = new LightSource();

        DateTime TimeOfDay = new DateTime(2013,1,1,8,0,0);
        DateTime StartTime = new DateTime(2013, 1, 1, 0, 0, 0);
        int gameDay = 1;

        double mapUpdate = 0;

        //LightSource lightSource1;

        Texture2D crosshairTex;
        Texture2D mapIcons;
        Vector2 crosshairPos;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            IsStubbourn = true;

        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            AudioController.LoadContent(content);

            //gameFont = content.Load<SpriteFont>("menufont");

            lightingEngine.LoadContent(content, ScreenManager.GraphicsDevice, ScreenManager.SpriteBatch);

            enemyController = new EnemyController();
            enemyController.LoadContent(content, ScreenManager.GraphicsDevice, lightingEngine);
            particleController = new ParticleController();
            particleController.LoadContent(content);
            projectileController = new ProjectileController();
            projectileController.LoadContent(content);
            itemController = new ItemController();
            itemController.LoadContent(content, ScreenManager.GraphicsDevice, lightingEngine);
            vehicleController = new VehicleController();
            vehicleController.LoadContent(content, ScreenManager.GraphicsDevice, lightingEngine);

            gameMap = content.Load<Map>("map/map");

            gameHud = new Hud();
            gameHud.LoadContent(content);

            mapFog = new bool[gameMap.Width, gameMap.Height];

            ThreadPool.QueueUserWorkItem(new WaitCallback(GenerateTerrainAsync));

            gameHero = new HeroDude(gameMap.HeroSpawn);
            gameHero.LoadContent(content, ScreenManager.GraphicsDevice, lightingEngine);

            gameCamera = new Camera(ScreenManager.GraphicsDevice.Viewport, gameMap);
            gameCamera.ClampRect = new Rectangle(0, 0, gameMap.Width * gameMap.TileWidth, gameMap.Height * gameMap.TileHeight);
            gameCamera.ZoomTarget = 1f;
            //gameCamera.Position = gameHero.Position - (new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height) / 2);
            gameCamera.Position = gameHero.Position;
            gameCamera.Target = gameCamera.Position;
            gameCamera.Update(ScreenManager.GraphicsDevice.Viewport.Bounds);

            //cameraLightSource.Type = LightSourceType.Spot;
            //lightingEngine.LightSources.Add(cameraLightSource);

            minimapRT = new RenderTarget2D(ScreenManager.GraphicsDevice, 200, 200);

            crosshairTex = content.Load<Texture2D>("crosshair");
            mapIcons = content.Load<Texture2D>("mapicons");

            //lightSource1 = new LightSource(ScreenManager.GraphicsDevice, 600, LightAreaQuality.Low, new Color(1f, 1f, 1f), BeamStencilType.Wide, SpotStencilType.None);

            //lightingEngine.LightSources.Add(lightSource1);
            
            

            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive && !TerrainGeneration.Generating)
            {
                ScreenManager.Game.IsMouseVisible = false;

                TimeOfDay = TimeOfDay.AddMinutes(gameTime.ElapsedGameTime.TotalSeconds);
                gameDay = 1 + ((TimeOfDay - StartTime).Days);

                lightingEngine.Update(gameTime, TimeOfDay, ScreenManager.SpriteBatch, ScreenManager.GraphicsDevice);

                gameMap.Update(gameTime);

                enemyController.Update(gameTime, gameMap, gameHero, mapFog, gameCamera);
                projectileController.Update(gameTime, gameMap, gameHero);
                particleController.Update(gameTime, gameMap);
                itemController.Update(gameTime, gameMap, gameHero, mapFog);
                vehicleController.Update(gameTime, gameMap, gameHero, gameCamera);

                gameHero.Update(gameTime, gameMap, mapFog);
                //lightSource1.Position = new Vector2(1000, 1000);

                //cameraLightSource.Position = gameCamera.Position;
                //cameraLightSource.Direction = new Vector2(1f, 1f);
                gameCamera.Target = gameHero.Position;
                if (gameHero.drivingVehicle == null) gameCamera.ZoomTarget = 1f;
                if (gameCamera.Zoom > 1.0f) gameCamera.Zoom = 1.0f;
                gameCamera.Update(new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height));

                mapUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
                float revealRadius = 800f;
                if (gameHero.drivingVehicle != null)
                {
                    if (gameHero.drivingVehicle.GetType() == typeof(Jeep)) revealRadius = 1600f;
                    if (gameHero.drivingVehicle.GetType() == typeof(Chopper)) revealRadius = 2000f;
                    if (gameHero.drivingVehicle.GetType() == typeof(Boat)) revealRadius = 2500f;
                }
                if (mapUpdate >= 100)
                {
                    for (float a = 0.0f; a < MathHelper.TwoPi; a += 0.05f)
                    {
                        for (float r = 0.0f; r < revealRadius; r += 50f)
                        {
                            Vector2 p = gameCamera.Position + (new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * r);
                            if(p.X>=0f && p.X<(gameMap.Width * gameMap.TileWidth) && p.Y>=0f && p.Y<(gameMap.Width * gameMap.TileWidth))
                                mapFog[(int)(p.X / gameMap.TileWidth), (int)(p.Y / gameMap.TileHeight)] = true;
                        }
                    }
                    mapUpdate = 0;
                }

                gameHud.Update(gameTime, gameHero, TimeOfDay, gameDay);
                
            }

               
            AudioController.Update(gameTime);
        }




        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

        

            // Look up inputs for the active player profile.
            int playerIndex = 0;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            PlayerIndex player;
            if (input.IsPauseGame(ControllingPlayer))
            {
                PauseBackgroundScreen pauseBG = new PauseBackgroundScreen();
                ScreenManager.AddScreen(pauseBG, ControllingPlayer);
                ScreenManager.AddScreen(new PauseMenuScreen(pauseBG), ControllingPlayer);
            }
        
            if(IsActive)
            {
                if ((keyboardState.IsKeyDown(Keys.Tab) && !lastKeyboardState.IsKeyDown(Keys.Tab)) || gamePadState.IsButtonDown(Buttons.Back)) ScreenManager.AddScreen(new MapScreen(gameMap, mapFog, gameHero, mapIcons), null);
                if (keyboardState.IsKeyDown(Keys.Space) && !lastKeyboardState.IsKeyDown(Keys.Space)) ThreadPool.QueueUserWorkItem(new WaitCallback(GenerateTerrainAsync));

                if (keyboardState.IsKeyDown(Keys.D1) && !lastKeyboardState.IsKeyDown(Keys.D1)) gameHero.SelectWeapon(0, false);
                if (keyboardState.IsKeyDown(Keys.D2) && !lastKeyboardState.IsKeyDown(Keys.D2)) gameHero.SelectWeapon(1, false);
                if (keyboardState.IsKeyDown(Keys.D3) && !lastKeyboardState.IsKeyDown(Keys.D3)) gameHero.SelectWeapon(2, false);
                if (keyboardState.IsKeyDown(Keys.D4) && !lastKeyboardState.IsKeyDown(Keys.D4)) gameHero.SelectWeapon(3, false);
                if (keyboardState.IsKeyDown(Keys.D5) && !lastKeyboardState.IsKeyDown(Keys.D5)) gameHero.SelectWeapon(4, false);

                if (input.IsNewButtonPress(Buttons.Y, null, out player)) gameHero.SelectWeapon(1, true);
                if (input.IsNewButtonPress(Buttons.RightShoulder, null, out player) || input.IsNewButtonPress(Buttons.DPadDown, null, out player)) gameHero.SelectWeapon(1, true);
                if (input.IsNewButtonPress(Buttons.LeftShoulder, null, out player) || input.IsNewButtonPress(Buttons.DPadUp, null, out player)) gameHero.SelectWeapon(-1, true);

                if ((keyboardState.IsKeyDown(Keys.E) && !lastKeyboardState.IsKeyDown(Keys.E)) || input.IsNewButtonPress(Buttons.A, null, out player))
                {
                    gameHero.EnterVehicle(gameMap);
                }


                if (input.MouseDragging)
                {
                    gameCamera.Target -= (input.MouseDelta/gameCamera.Zoom);
                }

                //if (input.CurrentMouseState.ScrollWheelValue < input.LastMouseState.ScrollWheelValue) gameCamera.Zoom -= (0.1f*gameCamera.Zoom);
                //if (input.CurrentMouseState.ScrollWheelValue > input.LastMouseState.ScrollWheelValue) gameCamera.Zoom += (0.1f*gameCamera.Zoom);
                

                if (input.CurrentMouseState.ScrollWheelValue < input.LastMouseState.ScrollWheelValue) gameHero.SelectWeapon(-1,true);
                if (input.CurrentMouseState.ScrollWheelValue > input.LastMouseState.ScrollWheelValue) gameHero.SelectWeapon(1, true);

                mousePos = new Vector2(input.LastMouseState.X, input.LastMouseState.Y);
                mousePos = Vector2.Transform(mousePos, Matrix.Invert(gameCamera.CameraMatrix));

               

               

                if (gameHero.drivingVehicle == null)
                {
                    if (input.CurrentGamePadStates[0].ThumbSticks.Left.Length() > 0.2f)
                    {
                        gameHero.Move(new Vector2(input.CurrentGamePadStates[0].ThumbSticks.Left.X, -input.CurrentGamePadStates[0].ThumbSticks.Left.Y));
                    }
                    if (input.CurrentGamePadStates[0].ThumbSticks.Right.Length() > 0.2f)
                    {
                        gameHero.LookAt(Helper.PointOnCircle(ref gameHero.Position, 200, Helper.V2ToAngle(new Vector2(input.CurrentGamePadStates[0].ThumbSticks.Right.X, -input.CurrentGamePadStates[0].ThumbSticks.Right.Y))));
                    }
                    else
                    {
                        if (input.CurrentGamePadStates[0].ThumbSticks.Left.Length() > 0.2f)
                        {
                            gameHero.LookAt(Helper.PointOnCircle(ref gameHero.Position, 200, Helper.V2ToAngle(new Vector2(input.CurrentGamePadStates[0].ThumbSticks.Left.X, -input.CurrentGamePadStates[0].ThumbSticks.Left.Y))));
                        }
                    }

                    crosshairPos = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);

                    Vector2 keyboardStick = Vector2.Zero;
                    if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.W)) keyboardStick.Y = -1f;
                    if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.S)) keyboardStick.Y = 1f;
                    if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.A)) keyboardStick.X = -1f;
                    if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.D)) keyboardStick.X = 1f;
                    if (keyboardStick.Length() > 0f) gameHero.Move(keyboardStick);
                    gameHero.Attack(gameTime, gameCamera.Position + (crosshairPos - new Vector2(gameCamera.Width / 2, gameCamera.Height / 2)), input.CurrentMouseState.LeftButton == ButtonState.Pressed, gameCamera, true);

                    gameHero.LookAt(gameCamera.Position + (crosshairPos - new Vector2(gameCamera.Width / 2, gameCamera.Height / 2)));//Helper.PointOnCircle(ref gameHero.Position, 200, Helper.V2ToAngle(((gameHero.Position - gameCamera.Position) ) + (crosshairPos- new Vector2(gameCamera.Width / 2, gameCamera.Height / 2)))));
                }

                // driving controls
                if (gameHero.drivingVehicle != null)
                {
                    if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.W)) gameHero.drivingVehicle.Accelerate(1f);
                    if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.S)) gameHero.drivingVehicle.Brake();
                    if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.A)) gameHero.drivingVehicle.Turn(-1f);
                    if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.D)) gameHero.drivingVehicle.Turn(1f);
                }

                


            }

            lastKeyboardState = keyboardState;
            
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (TerrainGeneration.Generating) return;

            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            //ScreenManager.GraphicsDevice.SetRenderTarget(gameRenderTarget);
            //ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
            //                                   Color.Black, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            //float zoom = 1f;

            if (mapUpdate == 0)
            {
                ScreenManager.GraphicsDevice.SetRenderTarget(minimapRT);
                ScreenManager.GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameHero.Position.X, -(int)gameHero.Position.Y, 0) * Matrix.CreateScale(0.05f) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(minimapRT.Width / 2, minimapRT.Height / 2, 0));
                gameMap.DrawMinimap(spriteBatch, gameCamera, 0.05f, minimapRT, mapFog, gameHero.Position);
                
                //ScreenManager.SpriteBatch.Draw(lightingEngine.BeamStencils[BeamStencilType.Narrow], Vector2.Zero, null, Color.White);
                ScreenManager.SpriteBatch.End();
                ScreenManager.GraphicsDevice.SetRenderTarget(null);
            }

            // Draw shadow casters
            LightingEngine.Instance.ShadowMap.StartGeneratingShadowCasteMap(false);
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
                gameMap.DrawLayer(spriteBatch, "Wall", gameCamera, lightingEngine, Color.LightGray);
                gameHero.DrawLightBlock(spriteBatch);
                enemyController.DrawLightBlock(spriteBatch, gameHero);
                vehicleController.DrawLightBlock(spriteBatch, gameHero);
                spriteBatch.End();
            }
            LightingEngine.Instance.ShadowMap.EndGeneratingShadowCasterMap();

            lightingEngine.DrawPhase1(spriteBatch, gameCamera);

            // Draw stuff that the light can cast onto
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            gameMap.DrawLayer(spriteBatch, "Terrain", gameCamera, lightingEngine, Color.White);
            spriteBatch.End();

            lightingEngine.DrawPhase2(spriteBatch);

           
            // We re-print the elements not affected by the light (in this case the shadow casters)
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            itemController.DrawShadows(spriteBatch, lightingEngine, gameHero);
            enemyController.DrawShadows(spriteBatch, lightingEngine, gameHero);
            gameHero.DrawShadows(spriteBatch, lightingEngine);
            vehicleController.DrawShadows(spriteBatch, lightingEngine, gameHero);
                
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            particleController.Draw(spriteBatch, ParticleBlendMode.Alpha, lightingEngine);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            particleController.Draw(spriteBatch, ParticleBlendMode.Additive, lightingEngine);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            itemController.Draw(spriteBatch, lightingEngine, gameHero);
            enemyController.Draw(spriteBatch, lightingEngine, gameHero);
            gameHero.Draw(spriteBatch, lightingEngine);
            vehicleController.Draw(spriteBatch, lightingEngine, gameHero);
            gameMap.DrawShadows(spriteBatch, "Wall", gameCamera, lightingEngine);
            gameMap.DrawLayer(spriteBatch, "Wall", gameCamera, lightingEngine, Color.White);
            
            projectileController.Draw(spriteBatch);
            spriteBatch.End();

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            //gameMap.DrawLayer(spriteBatch, "Terrain", gameCamera, lightingEngine, Color.White);
            //gameMap.DrawShadows(spriteBatch, "Wall", gameCamera, lightingEngine);
            //spriteBatch.End();



            lightingEngine.DrawSpots(spriteBatch, gameCamera, gameMap);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            gameMap.DrawRoofLayer(spriteBatch, gameCamera, lightingEngine, Color.White, gameHero.Position);
            vehicleController.DrawHeliShadows(spriteBatch, lightingEngine, gameHero);      
            vehicleController.DrawHelis(spriteBatch, lightingEngine, gameHero);
            spriteBatch.End();

            spriteBatch.Begin();
            //gameHUD.Draw(spriteBatch);
            spriteBatch.Draw(minimapRT, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 20 - minimapRT.Width, 20), null, Color.White * 0.75f);
            spriteBatch.Draw(mapIcons, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 20 - (minimapRT.Width/2), 20 + (minimapRT.Height/2)), new Rectangle(0, 0, 12, 13), Color.White, gameHero.Rotation - MathHelper.PiOver2, new Vector2(6, 6), 1f, SpriteEffects.None, 1);
            gameHud.Draw(spriteBatch);
            spriteBatch.End();

            

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition >= 0f)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);

            if (gameHero.deadAlpha<1f)
            {
                ScreenManager.FadeBackBufferToBlack(1f - gameHero.deadAlpha);
            }

            if (IsActive && gameHero.drivingVehicle==null)
            {
                spriteBatch.Begin();
                //gameHUD.Draw(spriteBatch);
                spriteBatch.Draw(crosshairTex, crosshairPos, new Rectangle(gameHero.SelectedWeapon==0?48:0,0,48,48), Color.White, 0f, new Vector2(crosshairTex.Width/4, crosshairTex.Height/2), 1f, SpriteEffects.None, 1);
                spriteBatch.End();
            }
        }


        void GenerateTerrainAsync(object si)
        {
            TerrainGeneration.GenerateTerrain(gameMap, lightingEngine, ScreenManager.GraphicsDevice);
            gameHero.Position = gameMap.HeroSpawn;
            gameCamera.Position = gameHero.Position;
            gameCamera.Target = gameCamera.Position;

            
            List<Compound> possibleComps = new List<Compound>();

            // Spawn vehicles
            foreach (Compound c in gameMap.Compounds)
            {
                foreach (Building b in c.Buildings)
                {
                    if (b.Type == BuildingType.Carpark)
                    {
                        Jeep j = new Jeep((new Vector2(b.Rect.Center.X, b.Rect.Center.Y) * new Vector2(gameMap.TileWidth, gameMap.TileHeight)) + new Vector2(50, 50));
                        j.Rotation = (float)Helper.Random.NextDouble() * MathHelper.TwoPi;
                        j.LoadContent(vehicleController.SpriteSheet, ScreenManager.GraphicsDevice, lightingEngine);
                        vehicleController.Vehicles.Add(j);
                        //gameHero.Position = j.Position + new Vector2(300, 0);
                    }

                    if (b.Type == BuildingType.Helipad)
                    {
                        Chopper chop = new Chopper((new Vector2(b.Rect.Center.X, b.Rect.Center.Y) * new Vector2(gameMap.TileWidth, gameMap.TileHeight)) + new Vector2(50, 50));
                        chop.Rotation = (float)Helper.Random.NextDouble() * MathHelper.TwoPi;
                        chop.LoadContent(vehicleController.SpriteSheet, ScreenManager.GraphicsDevice, lightingEngine);
                        vehicleController.Vehicles.Add(chop);
                        //gameHero.Position = chop.Position + new Vector2(300, 0);
                    }
                }
            }

            // Spawn enemies
            foreach (Compound c in gameMap.Compounds)
            {
                if (c.Buildings.Count > 0)
                {
                    int bnum = c.Buildings.Count;
                    foreach (Building b in c.Buildings) if (b.Type != BuildingType.Building) bnum--;
                    if(bnum>0) possibleComps.Add(c);
                }

                for (int y = c.Bounds.Top; y < c.Bounds.Bottom; y++)
                {
                    for (int x = c.Bounds.Left; x < c.Bounds.Right; x++)
                    {
                        if (((TileLayer)gameMap.GetLayer("Wall")).Tiles[x, y] != null) continue;
                        Vector2 pos = new Vector2((x * gameMap.TileWidth) + 50, (y * gameMap.TileHeight) + 50);
                        bool found = false;

                        if (vehicleController.CheckVehicleCollision(pos)) found = true;
                        foreach (AIDude d in enemyController.Enemies)
                        {
                            if ((d.Position - pos).Length() < 700) found = true;
                        }

                        if (!found)
                        {
                            AIDude newDude = new AIDude(pos);
                            newDude.BelongsToCompound = true;
                            newDude.LoadContent(enemyController.SpriteSheet, ScreenManager.GraphicsDevice, lightingEngine, gameHero);
                            newDude.Health = 10 + Helper.Random.Next(30);
                            enemyController.Enemies.Add(newDude);
                        }

                    }
                }
            }

            // Spawn Generals
            for (int i = 0; i < 3; i++)
            {

                while (true)
                {
                    Compound c = possibleComps[Helper.Random.Next(possibleComps.Count)];
                    bool found = false;
                    foreach (AIDude d in enemyController.Enemies.Where(en => en.IsGeneral)) if ((d.Position - c.Position).Length() <= 20000) found = true;
                    if (!found)
                    {
                        Building b;
                        while (true)
                        {
                            b = c.Buildings[Helper.Random.Next(c.Buildings.Count)];
                            if (b.Type == BuildingType.Building) break;
                        }
                        Vector2 pos = (new Vector2(b.Rect.Center.X, b.Rect.Center.Y) * new Vector2(gameMap.TileWidth, gameMap.TileHeight)) + new Vector2(50, 50);
                        AIDude newDude = new AIDude(pos);
                        newDude.LoadContent(enemyController.SpriteSheet, ScreenManager.GraphicsDevice, lightingEngine, gameHero);
                        newDude.Health = 50 + Helper.Random.Next(30);
                        newDude.BelongsToCompound = true;
                        newDude.IsGeneral = true;
                        enemyController.Enemies.Add(newDude);
                        possibleComps.Remove(c);
                        break;
                    }
                }
            }

            

            foreach (Jetty jetty in gameMap.Jetties)
            {
                Boat b = new Boat(jetty.BoatPosition);
                b.Rotation = jetty.BoatRotation;
                b.LoadContent(vehicleController.SpriteSheet, ScreenManager.GraphicsDevice, lightingEngine);
                vehicleController.Vehicles.Add(b);
            }

            gameHud.Ticker.AddLine("> Explore area and eliminate resistance");

            TerrainGeneration.Generating = false;
        }
       

        #endregion
    }
}
