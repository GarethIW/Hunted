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

        EnemyController enemyController;

        LightingEngine lightingEngine = new LightingEngine();

        RenderTarget2D minimapRT;
        bool[,] mapFog;

        Vector2 mousePos;

        KeyboardState lastKeyboardState;

        //iledLib.LightSource cameraLightSource = new LightSource();

        DateTime TimeOfDay = new DateTime(2013,1,1,0,0,0);

        double mapUpdate = 0;

        //LightSource lightSource1;

        Texture2D crosshairTex;
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

            gameMap = content.Load<Map>("map/map");

            mapFog = new bool[gameMap.Width, gameMap.Height];

            gameHero = new HeroDude(new Vector2(50000,50000));
            gameHero.LoadContent(content, ScreenManager.GraphicsDevice, lightingEngine);

            ThreadPool.QueueUserWorkItem(new WaitCallback(GenerateTerrainAsync));

            gameCamera = new Camera(ScreenManager.GraphicsDevice.Viewport, gameMap);
            gameCamera.ClampRect = new Rectangle(0, 0, gameMap.Width * gameMap.TileWidth, gameMap.Height * gameMap.TileHeight);
            gameCamera.Zoom = 1f;
            //gameCamera.Position = gameHero.Position - (new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height) / 2);
            gameCamera.Position = new Vector2((gameMap.Width * gameMap.TileWidth), (gameMap.Height * gameMap.TileHeight)) / 2;
            gameCamera.Target = gameCamera.Position;
            gameCamera.Update(ScreenManager.GraphicsDevice.Viewport.Bounds);

            //cameraLightSource.Type = LightSourceType.Spot;
            //lightingEngine.LightSources.Add(cameraLightSource);

            minimapRT = new RenderTarget2D(ScreenManager.GraphicsDevice, 200, 200);

            crosshairTex = content.Load<Texture2D>("crosshair");

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

                TimeOfDay = TimeOfDay.AddMinutes(gameTime.ElapsedGameTime.TotalSeconds * 50);

                lightingEngine.Update(gameTime, TimeOfDay, ScreenManager.SpriteBatch, ScreenManager.GraphicsDevice);

                gameMap.Update(gameTime);

                enemyController.Update(gameTime, gameMap, gameHero);

                gameHero.Update(gameTime, gameMap);
                //lightSource1.Position = new Vector2(1000, 1000);

                //cameraLightSource.Position = gameCamera.Position;
                //cameraLightSource.Direction = new Vector2(1f, 1f);
                gameCamera.Target = gameHero.Position;
                gameCamera.Update(new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height));

                mapUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (mapUpdate >= 100)
                {
                    for (float a = 0.0f; a < MathHelper.TwoPi; a += 0.1f)
                    {
                        for (float r = 0.0f; r < 800f; r += 50f)
                        {
                            Vector2 p = gameCamera.Position + (new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * r);
                            if(p.X>=0f && p.X<(gameMap.Width * gameMap.TileWidth) && p.Y>=0f && p.Y<(gameMap.Width * gameMap.TileWidth))
                                mapFog[(int)(p.X / gameMap.TileWidth), (int)(p.Y / gameMap.TileHeight)] = true;
                        }
                    }
                    mapUpdate = 0;
                }

                
            }

               
            AudioController.Update(gameTime);
        }




        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
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
                if (keyboardState.IsKeyDown(Keys.Space) && !lastKeyboardState.IsKeyDown(Keys.Space)) ThreadPool.QueueUserWorkItem(new WaitCallback(GenerateTerrainAsync));

                if (input.MouseDragging)
                {
                    gameCamera.Target -= (input.MouseDelta/gameCamera.Zoom);
                }

                if (input.CurrentMouseState.ScrollWheelValue < input.LastMouseState.ScrollWheelValue) gameCamera.Zoom -= (0.1f*gameCamera.Zoom);
                if (input.CurrentMouseState.ScrollWheelValue > input.LastMouseState.ScrollWheelValue) gameCamera.Zoom += (0.1f*gameCamera.Zoom);
                if (gameCamera.Zoom > 1.0f) gameCamera.Zoom = 1.0f;

                mousePos = new Vector2(input.LastMouseState.X, input.LastMouseState.Y);
                mousePos = Vector2.Transform(mousePos, Matrix.Invert(gameCamera.CameraMatrix));

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

                gameHero.LookAt(Helper.PointOnCircle(ref gameHero.Position, 200, Helper.V2ToAngle((gameHero.Position - gameCamera.Position - new Vector2(gameCamera.Width/2, gameCamera.Height/2)) + crosshairPos)));


                
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
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(0.05f) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(minimapRT.Width / 2, minimapRT.Height / 2, 0));
                gameMap.DrawMinimap(spriteBatch, gameCamera, 0.05f, minimapRT, mapFog);
                
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
            gameHero.DrawShadows(spriteBatch, lightingEngine);
            enemyController.DrawShadows(spriteBatch, lightingEngine, gameHero);
            gameHero.Draw(spriteBatch, lightingEngine);
            enemyController.Draw(spriteBatch, lightingEngine, gameHero);
            gameMap.DrawShadows(spriteBatch, "Wall", gameCamera, lightingEngine);
            gameMap.DrawLayer(spriteBatch, "Wall", gameCamera, lightingEngine, Color.White);
            spriteBatch.End();

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            //gameMap.DrawLayer(spriteBatch, "Terrain", gameCamera, lightingEngine, Color.White);
            //gameMap.DrawShadows(spriteBatch, "Wall", gameCamera, lightingEngine);
            //spriteBatch.End();

            lightingEngine.DrawSpots(spriteBatch, gameCamera, gameMap);
           

            spriteBatch.Begin();
            //gameHUD.Draw(spriteBatch);
            spriteBatch.Draw(minimapRT, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 20 - minimapRT.Width, 20), null, Color.White);
            spriteBatch.End();

            

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition >= 0f)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);

            if (IsActive)
            {
                spriteBatch.Begin();
                //gameHUD.Draw(spriteBatch);
                spriteBatch.Draw(crosshairTex, crosshairPos, null, Color.White, 0f, new Vector2(crosshairTex.Width, crosshairTex.Height) / 2, 1f, SpriteEffects.None, 1);
                spriteBatch.End();
            }
        }


        void GenerateTerrainAsync(object si)
        {
            TerrainGeneration.GenerateTerrain(gameMap, lightingEngine, ScreenManager.GraphicsDevice);
        }
       

        #endregion
    }
}
