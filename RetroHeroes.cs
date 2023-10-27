using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite.Sprites;
using RetroHeroes.Screens;
using RetroHeroes.Sprites;
using RetroHeroes.StateManagement;
using SharpDX.Direct3D9;
using System;
using System.Diagnostics;
using System.IO;

namespace RetroHeroes
{
    public class RetroHeroes : Game
    {
        private GraphicsDeviceManager graphics;
        private ScreenManager screenManager;

        public RetroHeroes()
        {
            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(Environment.CurrentDirectory + "\\speedrecord.txt");
                //Read the first line of text
                line = sr.ReadLine();
                Debug.WriteLine(line);
                GameData.HighScore = Convert.ToInt64(line);
                Debug.WriteLine(GameData.HighScore);
                //Continue to read until you reach end of file
                while (line != null)
                {
                    //write the line to console window
                    Console.WriteLine(line);
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                GameData.HighScore = 0;
                // Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                graphics = new GraphicsDeviceManager(this);
                Content.RootDirectory = "Content";
                IsMouseVisible = true;

                var screenFactory = new ScreenFactory();
                Services.AddService(typeof(IScreenFactory), screenFactory);

                GameData.StartTime = DateTime.UtcNow.Ticks / 1000 / 1000 / 10;

                screenManager = new ScreenManager(this);
                Components.Add(screenManager);

                AddInitialScreens();
            }
        }

        private void AddInitialScreens()
        {
            screenManager.AddScreen(new MainMenuScreen(), null);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent() {}

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}