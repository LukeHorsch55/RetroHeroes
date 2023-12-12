﻿using Microsoft.Xna.Framework;
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

namespace RetroHeroes.Screens
{
    public class MainMenuScreen : GameScreen
    {
        private ContentManager _content;
        private Model retroHeroesText;
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), -Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.1f, 100f);

        // Fonts
        private SpriteFont Yoster;
        BasicEffect basicEffect;

        // Dungeon
        TextureAtlas dungeonItemAtlas;
        private Sprite sprite1;
        private Sprite sprite2;
        private Sprite sprite3;
        Texture2D background;

        // Heros
        private WizardSprite wizard;

        // Projectiles
        private WizardFireballSprite[] wizardProjectiles = new WizardFireballSprite[6];
        private float timeSinceLastFireball = 0.75f;

        // Enemies
        private BrownGoober[] brownGoobers = new BrownGoober[2];
        public SoundEffect fireballHit;

        public override void Activate()
        {
            if(_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            // TODO: Add your initialization logic here
            wizard = new WizardSprite() { position = new Vector2(400, 375)};
            wizardProjectiles[0] = new WizardFireballSprite();
            wizardProjectiles[1] = new WizardFireballSprite();
            wizardProjectiles[2] = new WizardFireballSprite();
            wizardProjectiles[3] = new WizardFireballSprite();
            wizardProjectiles[4] = new WizardFireballSprite();
            wizardProjectiles[5] = new WizardFireballSprite();

            Texture2D enemiesAtlas = _content.Load<Texture2D>("EnemiesAtlas");
            Song menuBackground = _content.Load<Song>("MenuMusic");
            background = _content.Load<Texture2D>("MainScreen");
            retroHeroesText = _content.Load<Model>("retroheroes3dtext");
            brownGoobers[0] = new BrownGoober(enemiesAtlas, new Vector2(450, 450));
            brownGoobers[1] = new BrownGoober(enemiesAtlas, new Vector2(650, 250));

            // TODO: use this.Content to load your game content here

            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AsepriteFile aseDungeonItems = AsepriteFile.Load(Directory.GetParent(sCurrentDirectory).Parent.Parent.Parent + "\\Content\\DungeonItems.aseprite");
            dungeonItemAtlas = TextureAtlasProcessor.Process(ScreenManager.GraphicsDevice, aseDungeonItems);
            dungeonItemAtlas.CreateRegion("Crate", new Rectangle(0, 0, 16, 32));
            dungeonItemAtlas.CreateRegion("Barrel", new Rectangle(16, 0, 16, 32));
            dungeonItemAtlas.CreateRegion("Pot", new Rectangle(32, 16, 16, 16));
            dungeonItemAtlas.CreateRegion("PotBroken", new Rectangle(32, 32, 16, 16));
            dungeonItemAtlas.CreateRegion("Chest", new Rectangle(96, 192, 32, 32));
            dungeonItemAtlas.CreateRegion("ChestOpen", new Rectangle(128, 192, 32, 32));
            dungeonItemAtlas.CreateRegion("Key", new Rectangle(33, 65, 16, 16));
            sprite1 = dungeonItemAtlas.CreateSprite("Chest");
            sprite2 = dungeonItemAtlas.CreateSprite("ChestOpen");
            sprite3 = dungeonItemAtlas.CreateSprite("Key");

            fireballHit = _content.Load<SoundEffect>("FireballSound");
            wizard.LoadContent(_content);
            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.LoadContent(_content);
            }
            Yoster = _content.Load<SpriteFont>("Yoster");
            MediaPlayer.Play(menuBackground);

            basicEffect = new BasicEffect(ScreenManager.GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
            };
            basicEffect.Projection = Microsoft.Xna.Framework.Matrix.CreateOrthographicOffCenter(0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height, 0, 0, 1);
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
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            // Wizard Logic
            if (IsActive)
            {
                if (wizard.Exit) ScreenManager.Game.Exit();
                wizard.Update(gameTime, ScreenManager.GraphicsDevice);
                brownGoobers[0].Update(gameTime, wizard.position);
                brownGoobers[1].Update(gameTime, wizard.position);

                if (wizard.position.X > 350 && wizard.position.X < 450 && wizard.position.Y < 110)
                {
                    Debug.WriteLine("going to new screen");
                    // ScreenManager.AddScreen(new FirstScreen(), 0);
                    ScreenManager.AddScreen(new FifthScreen() { health = 10, powerUp = PowerUp.TripleShot }, 0); 
                }

                // Fireball Logic
                bool newFireball = false;
                foreach (WizardFireballSprite fireball in wizardProjectiles)
                {

                    bool before = fireball.Shown;
                    if (!before && timeSinceLastFireball > 0.50 && !newFireball && Mouse.GetState().LeftButton == ButtonState.Pressed)
                    {
                        newFireball = true;
                        fireball.Update(gameTime, wizard.position, ScreenManager.GraphicsDevice);
                        timeSinceLastFireball = 0;
                        break;
                    }

                    if (before)
                    {
                        fireball.Update(gameTime, wizard.position, ScreenManager.GraphicsDevice);
                    }
                }
                timeSinceLastFireball += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);
            var fontSize = Yoster.MeasureString("Retro Heroes");
            var title = new Vector2((ScreenManager.GraphicsDevice.PresentationParameters.Bounds.Width / 2) - fontSize.X / 2, 180f);

            // TODO: Add your drawing code here
            ScreenManager.SpriteBatch.Begin(); // 0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, basicEffect
            ScreenManager.SpriteBatch.Draw(background, new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height), Color.White);
            wizard.Draw(gameTime, ScreenManager.SpriteBatch);
            // brownGoobers[0].Draw(gameTime, ScreenManager.SpriteBatch);
            // brownGoobers[1].Draw(gameTime, ScreenManager.SpriteBatch);
            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.Draw(gameTime, ScreenManager.SpriteBatch);
            }
            // ScreenManager.SpriteBatch.DrawString(Yoster, "Retro Heroes", title, Color.Goldenrod); // Change from title to Vector2.Zero
            ScreenManager.SpriteBatch.DrawString(Yoster, "ESC to Exit", new Vector2(10, 5), Color.BlanchedAlmond, 0.0f, new Vector2(0), 0.35f, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.DrawString(Yoster, "Enter the\n Dungeon", new Vector2(360, 40), Color.DarkRed, 0, new Vector2(0), 0.25f, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.DrawString(Yoster, "WASD to Move", new Vector2(290, 240), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.DrawString(Yoster, "Click to Fire", new Vector2(290, 270), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 0);
            sprite1.Scale = new Vector2(1.5f);
            sprite2.Scale = new Vector2(1.5f);
            sprite3.Scale = new Vector2(4f);
            ScreenManager.SpriteBatch.DrawString(Yoster, GameData.HighScore > 0 ? $"Current Fastest Time: {GameData.HighScore}s" : "No Fastest Time Yet", new Vector2(25, 400), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.DrawString(Yoster, $"Your Current Time: {(DateTime.UtcNow.Ticks / 1000 / 1000 / 10) - GameData.StartTime}s", new Vector2(25, 425), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 1);
            // sprite1.Draw(ScreenManager.SpriteBatch, title - new Vector2(50, 0));
            // sprite2.Draw(ScreenManager.SpriteBatch, title + new Vector2(fontSize.X, 0));
            // sprite3.Draw(ScreenManager.SpriteBatch, new Vector2(title.X + fontSize.X / 2 - sprite3.Width * sprite3.ScaleX / 2, 250));

            // 3d
            ScreenManager.SpriteBatch.End();

            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), ScreenManager.GraphicsDevice.Viewport.AspectRatio, 0.01f, 100f);

            foreach (ModelMesh mesh in retroHeroesText.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    // Add translation to bring the text closer to the camera
                    effect.World = Matrix.CreateTranslation(new Vector3(0, 0, 0.25f)) * Matrix.CreateRotationY( -(float)gameTime.TotalGameTime.TotalSeconds); // * Matrix.CreateRotationX(MathHelper.PiOver4);
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }

            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            ScreenManager.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            //
            base.Draw(gameTime);
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }
    }
}
