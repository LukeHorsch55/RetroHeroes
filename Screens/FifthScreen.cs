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

namespace RetroHeroes.Screens
{
    public class FifthScreen : GameScreen
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
        private GreenGunkShot[] gunkShots1 = new GreenGunkShot[10];
        private GreenGunkShot[] gunkShots2 = new GreenGunkShot[10];
        private float timeSinceLastFireball = 0.75f;

        // Enemies
        private GreenFlyShooter[] flyShooters = new GreenFlyShooter[2];
        private BrownGoober[] brownGoobers = new BrownGoober[3];
        public SoundEffect fireballHit;
        public SoundEffect powerup;

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

            for (int i = 0; i < gunkShots1.Length; i++)
            {
                gunkShots1[i] = new GreenGunkShot();
            }

            for (int i = 0; i < gunkShots1.Length; i++)
            {
                gunkShots2[i] = new GreenGunkShot();
            }

            Texture2D enemiesAtlas = _content.Load<Texture2D>("EnemiesAtlas");
            hearts = _content.Load<Texture2D>("Hearts");
            background = _content.Load<Texture2D>("FourthRoom");
            flyShooters[0] = new GreenFlyShooter(enemiesAtlas, new Vector2(160, 175));
            flyShooters[1] = new GreenFlyShooter(enemiesAtlas, new Vector2(640, 175));
            brownGoobers[0] = new BrownGoober(enemiesAtlas, new Vector2(160, 350));
            brownGoobers[1] = new BrownGoober(enemiesAtlas, new Vector2(640, 350));
            brownGoobers[2] = new BrownGoober(enemiesAtlas, new Vector2(400, 175));

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
            foreach (GreenGunkShot gunk in gunkShots1)
            {
                gunk.LoadContent(_content);
            }

            foreach (GreenGunkShot gunk in gunkShots2)
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
                bool foundAliveGoober = false;
                foreach (BrownGoober goob in brownGoobers)
                {
                    if (goob.Shown) foundAliveGoober = true;
                }
                foreach (GreenFlyShooter goob in flyShooters)
                {
                    if (goob.Shown) foundAliveGoober = true;
                }

                if (wizard.position.X > 350 && wizard.position.X < 450 && wizard.position.Y < 110 && !foundAliveGoober)
                {
                    ScreenManager.AddScreen(new BossScreen() { health = wizard.Health, powerUp = wizard.powerUp }, 0);
                }

                if (wizard.Health <= 0)
                {
                    ScreenManager.AddScreen(new YouLostScreen(), 0);
                }

                if (wizard.Exit) ScreenManager.Game.Exit();
                wizard.Update(gameTime, ScreenManager.GraphicsDevice);

                flyShooters[0].Update(gameTime, wizard.position);
                flyShooters[1].Update(gameTime, wizard.position);
                brownGoobers[0].Update(gameTime, wizard.position);
                brownGoobers[1].Update(gameTime, wizard.position);
                brownGoobers[2].Update(gameTime, wizard.position);

                // Fireball Logic
                bool newFireball = false;

                if (flyShooters[1].Health >= 0)
                {
                    bool newGunkShot = false;
                    foreach (GreenGunkShot gunk in gunkShots2)
                    {
                        bool gunkShown = gunk.Shown;
                        if (gunkShown) gunk.Update(gameTime, flyShooters[1].position, wizard.position, ScreenManager.GraphicsDevice);
                        if (!gunkShown && flyShooters[1].timeSinceLastShot > 1.25 && !newGunkShot)
                        {
                            newGunkShot = true;
                            gunk.Update(gameTime, flyShooters[1].position, wizard.position, ScreenManager.GraphicsDevice);
                            flyShooters[1].timeSinceLastShot = 0;
                            break;
                        }

                        if (gunk.Shown && gunk.Bounds.CollidesWith(wizard.Bounds))
                        {
                            wizard.Hit = true;
                        }
                    }
                    flyShooters[1].timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (flyShooters[0].Health >= 0)
                {
                    bool newGunkShot = false;
                    foreach (GreenGunkShot gunk in gunkShots1)
                    {
                        bool gunkShown = gunk.Shown;
                        if (gunkShown) gunk.Update(gameTime, flyShooters[0].position, wizard.position, ScreenManager.GraphicsDevice);
                        if (!gunkShown && flyShooters[0].timeSinceLastShot > 1.25 && !newGunkShot)
                        {
                            newGunkShot = true;
                            gunk.Update(gameTime, flyShooters[0].position, wizard.position, ScreenManager.GraphicsDevice);
                            flyShooters[0].timeSinceLastShot = 0;
                            break;
                        }
                        if (gunk.Shown && gunk.Bounds.CollidesWith(wizard.Bounds))
                        {
                            wizard.Hit = true;
                        }
                    }
                    flyShooters[0].timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                for (int i = 0; i < wizardProjectiles.Length; i++)
                {
                    // Fly 0
                    if (wizardProjectiles[i].Bounds.CollidesWith(flyShooters[0].Bounds) && wizardProjectiles[i].Shown)
                    {
                        explosions.Visible = true;
                        explosions.PlaceExplosion(wizardProjectiles[i].position);
                        flyShooters[0].Hit = true;
                        wizardProjectiles[i].position = new Vector2(-600, -600);
                        fireballHit.Play();
                    }

                    if (wizard.Bounds.CollidesWith(flyShooters[0].Bounds))
                    {
                        wizard.Hit = true;
                    }

                    // Fly 1
                    if (wizardProjectiles[i].Bounds.CollidesWith(flyShooters[1].Bounds) && wizardProjectiles[i].Shown)
                    {
                        explosions.Visible = true;
                        explosions.PlaceExplosion(wizardProjectiles[i].position);
                        flyShooters[1].Hit = true;
                        wizardProjectiles[i].position = new Vector2(-600, -600);
                        fireballHit.Play();
                    }

                    if (wizard.Bounds.CollidesWith(flyShooters[1].Bounds))
                    {
                        wizard.Hit = true;
                    }

                    foreach (BrownGoober goober in brownGoobers)
                    {
                        if (wizardProjectiles[i].Bounds.CollidesWith(goober.Bounds) && wizardProjectiles[i].Shown)
                        {
                            explosions.Visible = true;
                            goober.Hit = true;
                            explosions.PlaceExplosion(wizardProjectiles[i].position);
                            wizardProjectiles[i].position = new Vector2(-600, -600);
                            fireballHit.Play();
                        }

                        if (wizard.Bounds.CollidesWith(goober.Bounds))
                        {
                            wizard.Hit = true;
                        }
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

            foreach (GreenGunkShot shot in gunkShots1)
            {
                shot.Draw(gameTime, ScreenManager.SpriteBatch);
            }

            foreach (GreenGunkShot shot in gunkShots2)
            {
                shot.Draw(gameTime, ScreenManager.SpriteBatch);
            }

            ScreenManager.SpriteBatch.DrawString(Yoster, "ESC to Exit", new Vector2(10, 5), Color.BlanchedAlmond, 0.0f, new Vector2(0), 0.35f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.DrawString(Yoster, $"{(DateTime.UtcNow.Ticks / 1000 / 1000 / 10) - GameData.StartTime}s", new Vector2(25, 425), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.Draw(hearts, new Vector2(600, 425), new Rectangle(0, 0, 32 * wizard.Health, 32), Color.White);
            flyShooters[0].Draw(gameTime, ScreenManager.SpriteBatch);
            flyShooters[1].Draw(gameTime, ScreenManager.SpriteBatch);
            brownGoobers[0].Draw(gameTime, ScreenManager.SpriteBatch);
            brownGoobers[1].Draw(gameTime, ScreenManager.SpriteBatch);
            brownGoobers[2].Draw(gameTime, ScreenManager.SpriteBatch);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
