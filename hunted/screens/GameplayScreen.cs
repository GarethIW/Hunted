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

        LightingEngine lightingEngine = new LightingEngine();

        RenderTarget2D minimapRT;
        bool[,] mapFog;

        Vector2 mousePos;

        KeyboardState lastKeyboardState;

        //iledLib.LightSource cameraLightSource = new LightSource();

        DateTime TimeOfDay = new DateTime(2013,1,1,8,0,0);

        double mapUpdate = 0;

        LightSource lightSource1;

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

            gameMap = content.Load<Map>("map");

            mapFog = new bool[gameMap.Width, gameMap.Height];

            TerrainGeneration.GenerateTerrain(gameMap);

            gameCamera = new Camera(ScreenManager.GraphicsDevice.Viewport, gameMap);
            gameCamera.ClampRect = new Rectangle(0, 5 * gameMap.TileHeight, gameMap.Width * gameMap.TileWidth, gameMap.Height * gameMap.TileHeight);
            gameCamera.Zoom = 1f;
            //gameCamera.Position = gameHero.Position - (new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height) / 2);
            gameCamera.Position = new Vector2((gameMap.Width * gameMap.TileWidth), (gameMap.Height * gameMap.TileHeight)) / 2;
            gameCamera.Target = gameCamera.Position;
            gameCamera.Update(ScreenManager.GraphicsDevice.Viewport.Bounds);

            //cameraLightSource.Type = LightSourceType.Spot;
            //lightingEngine.LightSources.Add(cameraLightSource);

            minimapRT = new RenderTarget2D(ScreenManager.GraphicsDevice, 200, 200);


            lightSource1 = new LightSource(LightSourceType.Spot, ScreenManager.GraphicsDevice, 300, LightAreaQuality.Middle, new Color(1f, 1f, 1f), lightingEngine.spotBG);

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

            if (IsActive)
            {
                TimeOfDay = TimeOfDay.AddMinutes(gameTime.ElapsedGameTime.TotalSeconds * 50);

                lightingEngine.Update(gameTime, TimeOfDay, ScreenManager.SpriteBatch, ScreenManager.GraphicsDevice);
                lightSource1.Color = Color.White * (1f - (lightingEngine.CurrentSunColor.ToVector3().Z));

                //cameraLightSource.Position = gameCamera.Position;
                //cameraLightSource.Direction = new Vector2(1f, 1f);

                gameCamera.Update(new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height));

                mapUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (mapUpdate >= 100)
                {
                    for (float a = 0.0f; a < MathHelper.TwoPi; a += 0.1f)
                    {
                        for (float r = 0.0f; r < 800f; r += 50f)
                        {
                            Vector2 p = gameCamera.Position + (new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * r);
                            mapFog[(int)(p.X / gameMap.TileWidth), (int)(p.Y / gameMap.TileHeight)] = true;
                        }
                    }
                    ScreenManager.GraphicsDevice.SetRenderTarget(minimapRT);
                    ScreenManager.GraphicsDevice.Clear(Color.Black);
                    ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(0.05f) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(minimapRT.Width / 2, minimapRT.Height / 2, 0));
                    gameMap.DrawMinimap(ScreenManager.SpriteBatch, gameCamera, 0.05f, minimapRT, mapFog);
                    ScreenManager.SpriteBatch.End();
                    ScreenManager.GraphicsDevice.SetRenderTarget(null);
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
                if (keyboardState.IsKeyDown(Keys.Space) && !lastKeyboardState.IsKeyDown(Keys.Space)) TerrainGeneration.GenerateTerrain(gameMap);

                if (input.MouseDragging)
                {
                    gameCamera.Target -= (input.MouseDelta/gameCamera.Zoom);
                }

                if (input.CurrentMouseState.ScrollWheelValue < input.LastMouseState.ScrollWheelValue) gameCamera.Zoom -= (0.1f*gameCamera.Zoom);
                if (input.CurrentMouseState.ScrollWheelValue > input.LastMouseState.ScrollWheelValue) gameCamera.Zoom += (0.1f*gameCamera.Zoom);
                if (gameCamera.Zoom > 1.0f) gameCamera.Zoom = 1.0f;

                mousePos = new Vector2(input.LastMouseState.X, input.LastMouseState.Y);
                mousePos = Vector2.Transform(mousePos, Matrix.Invert(gameCamera.CameraMatrix));

            }

            lastKeyboardState = keyboardState;
            
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            //ScreenManager.GraphicsDevice.SetRenderTarget(gameRenderTarget);
            //ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
            //                                   Color.Black, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            //float zoom = 1f;

            lightingEngine.ShadowMap.StartGeneratingShadowCasteMap(false);
            {
                //this.shadowMap.AddShadowCaster(testTexture, Vector2.Zero, testTexture.Width, testTexture.Height);
                //cat.Draw(spriteBatch, cat.Position, Color.Black, this.shadowMap.PrecisionRatio);
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
                gameMap.DrawLayer(spriteBatch, "Wall", gameCamera, lightingEngine, Color.Black);
                spriteBatch.End();
            }
            lightingEngine.ShadowMap.EndGeneratingShadowCasterMap();

            lightingEngine.ShadowmapResolver.ResolveShadows(lightingEngine.ShadowMap, lightSource1, PostEffect.LinearAttenuation_BlurHigh, 1f, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2));

            ScreenManager.GraphicsDevice.SetRenderTarget(lightingEngine.ScreenLights);
            {
                ScreenManager.GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                {
                    lightSource1.Draw(spriteBatch);
                }
                spriteBatch.End();
            }

            ScreenManager.GraphicsDevice.SetRenderTarget(lightingEngine.ScreenGround);
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            gameMap.DrawLayer(spriteBatch, "Terrain", gameCamera, lightingEngine, Color.White);
            gameMap.DrawShadows(spriteBatch, "Wall", gameCamera, lightingEngine);
            spriteBatch.End();

            // This command impress a texture on another using 2xMultiplicative blend, which is perfect to paste our lights on the underlying image
            lightingEngine.LFX.PrintLightsOverTexture(null, spriteBatch, ScreenManager.GraphicsDevice, lightingEngine.ScreenLights, lightingEngine.ScreenGround, 0.5f * (1f - (lightingEngine.CurrentSunColor.ToVector3().Z)));

            // We re-print the elements not affected by the light (in this case the shadow casters)
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            //gameMap.DrawLayer(spriteBatch, "Terrain", gameCamera, lightingEngine);
            //gameMap.DrawShadows(spriteBatch, "Wall", gameCamera, lightingEngine);
            gameMap.DrawLayer(spriteBatch, "Wall", gameCamera, lightingEngine, Color.White);
            spriteBatch.End();


            spriteBatch.Begin();
            //gameHUD.Draw(spriteBatch);
            spriteBatch.Draw(minimapRT, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 20 - minimapRT.Width, 20), null, Color.White);
            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition >= 0f)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }

        

       

        #endregion
    }
}
