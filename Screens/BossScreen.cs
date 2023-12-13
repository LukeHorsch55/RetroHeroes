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
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace RetroHeroes.Screens
{
    public class BossScreen : GameScreen
    {
        private ContentManager _content;
        // Fonts
        private SpriteFont Yoster;

        ExplosionParticleSystem explosions;

        // Dungeon
        Texture2D background;

        // Heros
        private WizardSprite wizard;
        public int health;
        private Texture2D hearts;
        public PowerUp powerUp;

        // Projectiles
        private WizardFireballSprite[] wizardProjectiles = new WizardFireballSprite[30];
        private GreenGunkShot[] gunkShots = new GreenGunkShot[20];
        private float timeSinceLastFireball = 0.75f;

        // Enemies
        private FinalBoss finalBoss;
        public SoundEffect fireballHit;
        public SoundEffect powerup;
        private Texture2D greenHearts;

        public override void Activate()
        {
            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            // TODO: Add your initialization logic here
            wizard = new WizardSprite() { position = new Vector2(400, ScreenManager.GraphicsDevice.Viewport.Height - 33), Health = health, powerUp = powerUp };
            for (int i = 0; i < wizardProjectiles.Length; i++)
            {
                wizardProjectiles[i] = new WizardFireballSprite();
            }

            for (int i = 0; i < gunkShots.Length; i++)
            {
                gunkShots[i] = new GreenGunkShot();
            }

            Texture2D enemiesAtlas = _content.Load<Texture2D>("EnemiesAtlas");
            hearts = _content.Load<Texture2D>("Hearts");
            greenHearts = _content.Load<Texture2D>("GreenHearts");
            background = _content.Load<Texture2D>("FourthRoom");
            finalBoss = new FinalBoss(enemiesAtlas, new Vector2(385, 150));

            explosions = new ExplosionParticleSystem(ScreenManager.Game, 20);
            explosions.Visible = false;
            ScreenManager.Game.Components.Add(explosions);

            fireballHit = _content.Load<SoundEffect>("FireballSound");
            powerup = _content.Load<SoundEffect>("powerup");
            wizard.LoadContent(_content);
            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.LoadContent(_content);
            }
            foreach (GreenGunkShot gunk in gunkShots)
            {
                gunk.LoadContent(_content);
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
                long time = (DateTime.UtcNow.Ticks / 1000 / 1000 / 10) - GameData.StartTime;
                GameData.FinishedScore = time;
                if (!finalBoss.Shown && (GameData.HighScore == 0 || GameData.HighScore > (DateTime.UtcNow.Ticks / 1000 / 1000 / 10) - GameData.StartTime))
                {
                    GameData.HighScore = time;
                    try
                    {
                        if (GameData.HighScore == 0)
                        {
                            File.Create(Environment.CurrentDirectory + "\\speedrecord.txt");
                        }
                        //Pass the filepath and filename to the StreamWriter Constructor
                        StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + "\\speedrecord.txt");

                        //Write a line of text
                        sw.WriteLine(time);
                        //Close the file
                        sw.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }

                if (!finalBoss.Shown)
                {
                    ScreenManager.AddScreen(new YouWonScreen(), 0);
                }

                if (wizard.Health <= 0)
                {
                    ScreenManager.AddScreen(new YouLostScreen(), 0);
                }

                if (wizard.Exit) ScreenManager.Game.Exit();
                wizard.Update(gameTime, ScreenManager.GraphicsDevice);

                finalBoss.Update(gameTime, wizard.position);

                // Fireball Logic
                bool newFireball = false;

                if (finalBoss.Health >= 0)
                {
                    bool newGunkShot = false;
                    for (int i = 0; i < gunkShots.Length; i++ )
                    {
                        bool gunkShown = gunkShots[i].Shown;
                        if (gunkShown) gunkShots[i].Update(gameTime, finalBoss.position, wizard.position, ScreenManager.GraphicsDevice);
                        if (finalBoss.timeSinceLastShot > 1f && !newGunkShot && !gunkShots[i].Shown && !gunkShots[i + 1 >= 20 ? 0 : i + 1].Shown && !gunkShots[i + 2 >= 20 ? 0 : i + 2].Shown)
                        {
                            newFireball = true;
                            gunkShots[i].Update(gameTime, finalBoss.position, wizard.position, ScreenManager.GraphicsDevice);
                            finalBoss.timeSinceLastShot = 0;
                            gunkShots[i + 1 >= 20 ? 0 : i + 1].Update(gameTime, finalBoss.position, wizard.position, ScreenManager.GraphicsDevice, 30f);
                            gunkShots[i + 2 >= 20 ? 0 : i + 2].Update(gameTime, finalBoss.position, wizard.position, ScreenManager.GraphicsDevice, -30f);
                            newGunkShot = true;
                        }
                        if (gunkShown && gunkShots[i].Bounds.CollidesWith(wizard.Bounds))
                        {
                            wizard.Hit = true;
                        }
                    }
                    finalBoss.timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                for (int i = 0; i < wizardProjectiles.Length; i++)
                {
   
                    if (wizardProjectiles[i].Bounds.CollidesWith(finalBoss.Bounds) && wizardProjectiles[i].Shown)
                    {
                        explosions.Visible = true;
                        explosions.PlaceExplosion(wizardProjectiles[i].position);
                        finalBoss.Hit = true;
                        wizardProjectiles[i].position = new Vector2(-600, -600);
                        fireballHit.Play();
                    }

                    if (wizard.Bounds.CollidesWith(finalBoss.Bounds))
                    {
                        wizard.Hit = true;
                    }

                    bool before = wizardProjectiles[i].Shown;
                    if (before && wizard.powerUp != PowerUp.TripleShot)
                    {
                        wizardProjectiles[i].Update(gameTime, wizard.position, ScreenManager.GraphicsDevice);
                        continue;
                    }
                    switch (wizard.powerUp)
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

            wizard.Draw(gameTime, ScreenManager.SpriteBatch);

            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.Draw(gameTime, ScreenManager.SpriteBatch);
            }

            foreach (GreenGunkShot shot in gunkShots)
            {
                shot.Draw(gameTime, ScreenManager.SpriteBatch);
            }


            ScreenManager.SpriteBatch.DrawString(Yoster, "ESC to Exit", new Vector2(10, 5), Color.BlanchedAlmond, 0.0f, new Vector2(0), 0.35f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.DrawString(Yoster, $"{(DateTime.UtcNow.Ticks / 1000 / 1000 / 10) - GameData.StartTime}s", new Vector2(25, 425), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.Draw(hearts, new Vector2(600, 425), new Rectangle(0, 0, 32 * wizard.Health, 32), Color.White);
            ScreenManager.SpriteBatch.Draw(greenHearts, new Vector2(50, 5), new Rectangle(0, 0, 16 * finalBoss.Health, 32), Color.White);
            finalBoss.Draw(gameTime, ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
