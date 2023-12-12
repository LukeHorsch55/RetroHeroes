using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite.Sprites;
using MonoGame.Aseprite;
using RetroHeroes.Sprites;
using RetroHeroes.StateManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using SharpDX.MediaFoundation;

namespace RetroHeroes.Screens
{
    public class PowerUpSelectionScreenThree : GameScreen
    {
        private ContentManager _content;
        // Fonts
        private SpriteFont Yoster;

        ExplosionParticleSystem explosions;

        // Dungeon
        TextureAtlas dungeonItemAtlas;
        private Sprite heartPowerup;
        private Sprite tripleShot;
        private Sprite doubleShot;
        Texture2D background;

        // Heros
        private WizardSprite wizard;
        public int health;
        private Texture2D hearts;

        // Projectiles
        private WizardFireballSprite[] wizardProjectiles = new WizardFireballSprite[30];
        private float timeSinceLastFireball = 0.75f;

        // Enemies
        private GreenGoober[] greenGoobers = new GreenGoober[2];
        public SoundEffect fireballHit;
        public SoundEffect powerup;

        public override void Activate()
        {
            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            // TODO: Add your initialization logic here
            wizard = new WizardSprite() { position = new Vector2(400, ScreenManager.GraphicsDevice.Viewport.Height - 33), Health = health };
            for (int i = 0; i < wizardProjectiles.Length; i++)
            {
                wizardProjectiles[i] = new WizardFireballSprite();
            }

            Texture2D enemiesAtlas = _content.Load<Texture2D>("EnemiesAtlas");
            hearts = _content.Load<Texture2D>("Hearts");
            background = _content.Load<Texture2D>("FirstRoom");
            greenGoobers[0] = new GreenGoober(enemiesAtlas, new Vector2(175, 200));
            greenGoobers[1] = new GreenGoober(enemiesAtlas, new Vector2(625, 200));

            explosions = new ExplosionParticleSystem(ScreenManager.Game, 20);
            explosions.Visible = false;
            ScreenManager.Game.Components.Add(explosions);


            // TODO: use this.Content to load your game content here
            Song backgroundMusic = _content.Load<Song>("Game1");
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AsepriteFile aseDungeonItems = AsepriteFile.Load(Directory.GetParent(sCurrentDirectory).Parent.Parent.Parent + "\\Content\\DungeonItems.aseprite");
            dungeonItemAtlas = TextureAtlasProcessor.Process(ScreenManager.GraphicsDevice, aseDungeonItems);
            dungeonItemAtlas.CreateRegion("Hearts", new Rectangle(208, 48, 32, 32));
            dungeonItemAtlas.CreateRegion("TripleShot", new Rectangle(240, 48, 32, 32));
            dungeonItemAtlas.CreateRegion("DoubleShot", new Rectangle(272, 48, 32, 32));
         
            heartPowerup = dungeonItemAtlas.CreateSprite("Hearts");
            tripleShot = dungeonItemAtlas.CreateSprite("TripleShot");
            doubleShot = dungeonItemAtlas.CreateSprite("DoubleShot");

            fireballHit = _content.Load<SoundEffect>("FireballSound");
            powerup = _content.Load<SoundEffect>("powerup");
            wizard.LoadContent(_content);
            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.LoadContent(_content);
            }
            Yoster = _content.Load<SpriteFont>("Yoster");

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
                if (wizard.position.X > 350 && wizard.position.X < 450 && wizard.position.Y < 110)
                {
                    ScreenManager.AddScreen(new FourthScreen() { health = wizard.Health, powerUp = wizard.powerUp }, 0);
                }

                if (wizard.Health <= 0)
                {
                    ScreenManager.AddScreen(new YouLostScreen(), 0);
                }

                if (heartPowerup.IsVisible && wizard.position.X < 220 && wizard.position.X > 150 && wizard.position.Y < 270 && wizard.position.Y > 200)
                {
                    wizard.Health += 3;
                    tripleShot.LayerDepth = -1;
                    heartPowerup.LayerDepth = -1;
                    doubleShot.LayerDepth = -1;
                    tripleShot.IsVisible = false;
                    heartPowerup.IsVisible = false;
                    doubleShot.IsVisible = false;
                    powerup.Play();
                }
                if (tripleShot.IsVisible && wizard.position.X < 422 && wizard.position.X > 352 && wizard.position.Y < 270 && wizard.position.Y > 200)
                {
                    wizard.powerUp = PowerUp.TripleShot;
                    tripleShot.LayerDepth = -1;
                    heartPowerup.LayerDepth = -1;
                    doubleShot.LayerDepth = -1;
                    tripleShot.IsVisible = false;
                    heartPowerup.IsVisible = false;
                    doubleShot.IsVisible = false;
                    powerup.Play();
                }
                if (doubleShot.IsVisible && wizard.position.X < 620 && wizard.position.X > 550 && wizard.position.Y < 270 && wizard.position.Y > 200)
                {
                    wizard.powerUp = PowerUp.DoubleShot;
                    tripleShot.LayerDepth = -1;
                    heartPowerup.LayerDepth = -1;
                    doubleShot.LayerDepth = -1;
                    tripleShot.IsVisible = false;
                    heartPowerup.IsVisible = false;
                    doubleShot.IsVisible = false;
                    powerup.Play();
                }

                if (wizard.Exit) ScreenManager.Game.Exit();
                wizard.Update(gameTime, ScreenManager.GraphicsDevice);

                // Fireball Logic
                bool newFireball = false;
                for (int i = 0; i < wizardProjectiles.Length; i++ )
                {
                    bool before = wizardProjectiles[i].Shown;
                    if (before && wizard.powerUp != PowerUp.TripleShot)
                    {
                        wizardProjectiles[i].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice);
                        continue;
                    }
                    switch(wizard.powerUp)
                    {
                        case PowerUp.None:
                            if (!before && timeSinceLastFireball > 0.50 && !newFireball && Mouse.GetState().LeftButton == ButtonState.Pressed)
                            {
                                newFireball = true;
                                wizardProjectiles[i].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice);
                                timeSinceLastFireball = 0;
                                break;
                            }
                            break;
                        case PowerUp.TripleShot:
                            if (!before && timeSinceLastFireball > 0.50 && !newFireball && Mouse.GetState().LeftButton == ButtonState.Pressed)
                            {
                                if (!wizardProjectiles[i].Shown && !wizardProjectiles[i + 1 >= 30 ? 0 : i + 1].Shown && !wizardProjectiles[i + 2 >= 30 ? 0 : i + 2].Shown)
                                {
                                    newFireball = true;
                                    wizardProjectiles[i].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice);
                                    timeSinceLastFireball = 0;
                                    wizardProjectiles[i + 1 >= 30 ? 0 : i + 1].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice, 15f);
                                    wizardProjectiles[i + 2 >= 30 ? 0 : i + 2].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice, -15f);

                                }
                            }
                            if (before)
                            {
                                wizardProjectiles[i].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice);
                            }
                            break;
                        case PowerUp.DoubleShot:
                            if (!before && timeSinceLastFireball > 0.50 && !newFireball && Mouse.GetState().LeftButton == ButtonState.Pressed)
                            {
                                newFireball = true;
                                wizardProjectiles[i].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice);
                                timeSinceLastFireball = 0;
                                if (i + 1 >= 30)
                                {
                                    wizardProjectiles[0].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice, doubleShot: true);
                                }
                                else
                                {
                                    wizardProjectiles[i + 1].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice, doubleShot: true);
                                }
                            }
                            break;
                    }
                }
                timeSinceLastFireball += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }
        private bool _shaking = false;
        public float _shakeTime = 0f;
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);

            // TODO: Add your drawing code here
            if (wizard.Hit)
            {
                _shaking = true;
                Matrix shakeTransform = Matrix.Identity;
                if (_shaking)
                {
                    _shakeTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    shakeTransform = Matrix.CreateTranslation(2 * MathF.Sin(_shakeTime), 2 * MathF.Cos(_shakeTime), 0);
                    if (_shakeTime > 500) _shaking = false;
                }
                ScreenManager.SpriteBatch.Begin(transformMatrix: shakeTransform);
            }
            else
            {
                ScreenManager.SpriteBatch.Begin();
            }
            ScreenManager.SpriteBatch.Draw(background, new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height), Color.White);

            heartPowerup.Draw(ScreenManager.SpriteBatch, new Vector2(150, 200));
            tripleShot.Draw(ScreenManager.SpriteBatch, new Vector2(352, 200));
            doubleShot.Draw(ScreenManager.SpriteBatch, new Vector2(550, 200));
            wizard.Draw(gameTime, ScreenManager.SpriteBatch);

            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.Draw(gameTime, ScreenManager.SpriteBatch);
            }

            ScreenManager.SpriteBatch.DrawString(Yoster, "ESC to Exit", new Vector2(10, 5), Color.BlanchedAlmond, 0.0f, new Vector2(0), 0.35f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.DrawString(Yoster, $"{(DateTime.UtcNow.Ticks / 1000 / 1000 / 10) - GameData.StartTime}s", new Vector2(25, 425), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.Draw(hearts, new Vector2(600, 425), new Rectangle(0, 0, 32 * wizard.Health, 32), Color.White);

            heartPowerup.Scale = new Vector2(3f);
            tripleShot.Scale = new Vector2(3f);
            doubleShot.Scale = new Vector2(2.5f);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
