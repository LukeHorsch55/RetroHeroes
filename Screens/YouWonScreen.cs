using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Sprites;
using RetroHeroes.Sprites;
using RetroHeroes.StateManagement;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using System.Diagnostics;
using Microsoft.Xna.Framework.Media;
using SharpDX;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using System.Windows.Forms;
using Matrix = Microsoft.Xna.Framework.Matrix;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using System.Reflection;
using SharpDX.MediaFoundation;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace RetroHeroes.Screens
{
    public class YouWonScreen : GameScreen
    {
        private ContentManager _content;

        // Fonts
        private SpriteFont Yoster;

        public override void Activate()
        {
            if(_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");



            Song menuBackground = _content.Load<Song>("MenuMusic");

            // TODO: use this.Content to load your game content here

            
            Yoster = _content.Load<SpriteFont>("Yoster");
            MediaPlayer.Play(menuBackground);

        
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void Unload()
        {
            _content.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            var kbState = Keyboard.GetState();

            if (kbState.IsKeyDown(Keys.Escape))
            {
                ScreenManager.Game.Exit();
            }

            if (kbState.IsKeyDown(Keys.R))
            {
                ScreenManager.AddScreen(new MainMenuScreen(), 0);
                GameData.StartTime = DateTime.UtcNow.Ticks / 1000 / 1000 / 10;
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.DimGray, 0, 0);
            var fontSize = Yoster.MeasureString("You Won!");
            var title = new Vector2((ScreenManager.GraphicsDevice.PresentationParameters.Bounds.Width / 2) - fontSize.X / 2, 180f);

            // TODO: Add your drawing code here
            ScreenManager.SpriteBatch.Begin(); // 0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, basicEffect
            
            ScreenManager.SpriteBatch.DrawString(Yoster, "You Won!", title, Color.Gold); // Change from title to Vector2.Zero
            ScreenManager.SpriteBatch.DrawString(Yoster, "ESC to Exit", new Vector2(10, 5), Color.BlanchedAlmond, 0.0f, new Vector2(0), 0.35f, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.DrawString(Yoster, "R to Restart", new Vector2(290, 240), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 0);
            
            if (GameData.HighScore == GameData.FinishedScore)
            {
                ScreenManager.SpriteBatch.DrawString(Yoster, "New Record of " + GameData.FinishedScore + " seconds!", new Vector2(150, 280), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 0);
            }
            else
            {
                ScreenManager.SpriteBatch.DrawString(Yoster, "No New Record, Your time was " + GameData.FinishedScore + " seconds", new Vector2(50, 280), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 0);
            }

            ScreenManager.SpriteBatch.End();
         
            base.Draw(gameTime);
        }
    }
}
