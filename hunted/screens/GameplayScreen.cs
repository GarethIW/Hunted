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

        RenderTarget2D gameRenderTarget;

        Vector2 mousePos;

        KeyboardState lastKeyboardState;

        LightSource cameraLightSource = new LightSource();

        DateTime TimeOfDay = new DateTime(2013,1,1,8,0,0);

        BlendState lightsBS;

        Texture2D spotTex;
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

            spotTex = content.Load<Texture2D>("spot");

            gameMap = content.Load<Map>("map");

            TerrainGeneration.GenerateTerrain(gameMap);

            gameCamera = new Camera(ScreenManager.GraphicsDevice.Viewport, gameMap);
            gameCamera.ClampRect = new Rectangle(0, 5 * gameMap.TileHeight, gameMap.Width * gameMap.TileWidth, gameMap.Height * gameMap.TileHeight);
            gameCamera.Zoom = 1f;
            //gameCamera.Position = gameHero.Position - (new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height) / 2);
            gameCamera.Position = new Vector2((gameMap.Width * gameMap.TileWidth), (gameMap.Height * gameMap.TileHeight)) / 2;
            gameCamera.Target = gameCamera.Position;
            gameCamera.Update(ScreenManager.GraphicsDevice.Viewport.Bounds);

            cameraLightSource.Type = LightSourceType.Spot;
            lightingEngine.LightSources.Add(cameraLightSource);

            lightsBS = new BlendState()
            {
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.SourceColor,
                ColorBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                AlphaBlendFunction = BlendFunction.Add
                 
            };

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
                lightingEngine.Update(gameTime, ref TimeOfDay);

                cameraLightSource.Position = gameCamera.Position;
                cameraLightSource.Direction = new Vector2(1f, 1f);

                gameCamera.Update(new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height));

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

            //Matrix transform = Matrix.CreateTranslation(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2, 0) * Matrix.CreateRotationZ(-gameCamera.Rotation);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            gameMap.DrawLayer(spriteBatch, "Terrain", gameCamera, lightingEngine);
            gameMap.DrawShadows(spriteBatch, "Wall", gameCamera, lightingEngine);
            gameMap.DrawLayer(spriteBatch, "Wall", gameCamera, lightingEngine);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, lightsBS, SamplerState.PointClamp, null, null, null);
            spriteBatch.Draw(spotTex, new Vector2(spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height) / 2, null, Color.White, 0f, new Vector2(spotTex.Width, spotTex.Height) / 2, 0.75f, SpriteEffects.None, 1);
            spriteBatch.Draw(spotTex, new Vector2(spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height) / 2, null, Color.White, 0f, new Vector2(spotTex.Width, spotTex.Height) / 2, 0.75f, SpriteEffects.None, 1);

            spriteBatch.End();

          
            spriteBatch.Begin();
            //gameHUD.Draw(spriteBatch);
            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition >= 0f)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }

        

       

        #endregion
    }
}
