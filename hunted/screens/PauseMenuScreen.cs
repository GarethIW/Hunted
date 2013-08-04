#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace Hunted
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    public class PauseMenuScreen : MenuScreen
    {
        #region Initialization

        PauseBackgroundScreen BGScreen;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public PauseMenuScreen(PauseBackgroundScreen pauseBG)
            : base("Pause", 0)
        {
            BGScreen = pauseBG;
            IsPopup = true;
        }

        public override void LoadContent()
        {
            MenuEntry resumeGameMenuEntry;
            resumeGameMenuEntry = new MenuEntry("Resume");

            MenuEntry saveMenuEntry = new MenuEntry("Save");
            MenuEntry loadMenuEntry = new MenuEntry("Load");
            MenuEntry exitMenuEntry = new MenuEntry("Quit Game");

            // Hook up menu event handlers.
            resumeGameMenuEntry.Selected += ResumeGameMenuEntrySelected;
            saveMenuEntry.Selected += SaveMenuEntrySelected;
            loadMenuEntry.Selected += LoadMenuEntrySelected;
            exitMenuEntry.Selected += ExitMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(saveMenuEntry);
            MenuEntries.Add(loadMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            base.LoadContent();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void ResumeGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            BGScreen.ExitScreen();
            ExitScreen();
        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>

        void SaveMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        void LoadMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void ExitMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //LoadingScreen.Load(ScreenManager, false, e.PlayerIndex, new GameplayScreen(),
              //                 new MainMenuScreen());
            ScreenManager.Game.Exit();
        }


        /// <summary>
        /// When the user cancels the main menu, we exit the game.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            BGScreen.ExitScreen();
            ExitScreen();
        }


        #endregion
    }
}
