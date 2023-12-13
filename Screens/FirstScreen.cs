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
    public class FirstScreen : GameScreen
    {
        private ContentManager _content;
        // Fonts
        private SpriteFont Yoster;

        ExplosionParticleSystem explosions;

        // Dungeon
        Texture2D background;

        // Heros
        private WizardSprite wizard;
        private Texture2D hearts;

        // Projectiles
        private WizardFireballSprite[] wizardProjectiles = new WizardFireballSprite[6];
        private float timeSinceLastFireball = 0.75f;

        // Enemies
        private BrownGoober[] brownGoobers = new BrownGoober[2];
        public SoundEffect fireballHit;

        public override void Activate()
        {
            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            // TODO: Add your initialization logic here
            wizard = new WizardSprite() { position = new Vector2(400, ScreenManager.GraphicsDevice.Viewport.Height - 33) };
            wizardProjectiles[0] = new WizardFireballSprite();
            wizardProjectiles[1] = new WizardFireballSprite();
            wizardProjectiles[2] = new WizardFireballSprite();
            wizardProjectiles[3] = new WizardFireballSprite();
            wizardProjectiles[4] = new WizardFireballSprite();
            wizardProjectiles[5] = new WizardFireballSprite();

            Texture2D enemiesAtlas = _content.Load<Texture2D>("EnemiesAtlas");
            hearts = _content.Load<Texture2D>("Hearts");
            background = _content.Load<Texture2D>("FirstRoom");
            brownGoobers[0] = new BrownGoober(enemiesAtlas, new Vector2(175, 200));
            brownGoobers[1] = new BrownGoober(enemiesAtlas, new Vector2(625, 200));

            explosions = new ExplosionParticleSystem(ScreenManager.Game, 20);
            explosions.Visible = false;
            ScreenManager.Game.Components.Add(explosions);


            // TODO: use this.Content to load your game content here
            Song backgroundMusic = _content.Load<Song>("Game1");

            fireballHit = _content.Load<SoundEffect>("FireballSound");
            wizard.LoadContent(_content);
            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.LoadContent(_content);
            }
            Yoster = _content.Load<SpriteFont>("Yoster");
            MediaPlayer.Play(backgroundMusic);

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
                if (wizard.position.X > 350 && wizard.position.X < 450 && wizard.position.Y < 110 && !brownGoobers[0].Shown && !brownGoobers[1].Shown)
                {
                    ScreenManager.AddScreen(new SecondScreen() { health = wizard.Health }, 0);
                }

                if (wizard.Health <= 0)
                {
                    ScreenManager.AddScreen(new YouLostScreen(), 0);
                } 

                if (wizard.Exit) ScreenManager.Game.Exit();
                wizard.Update(gameTime, ScreenManager.GraphicsDevice);
                brownGoobers[0].Update(gameTime, wizard.position);
                brownGoobers[1].Update(gameTime, wizard.position);

                // Fireball Logic
                bool newFireball = false;
                foreach (WizardFireballSprite fireball in wizardProjectiles)
                {
                    foreach (BrownGoober goober in brownGoobers)
                    {
                        if (fireball.Bounds.CollidesWith(goober.Bounds) && fireball.Shown)
                        {
                            explosions.Visible = true;
                            goober.Hit = true;
                            explosions.PlaceExplosion(fireball.position);
                            fireball.position = new Vector2(-600, -600);
                            fireballHit.Play();
                        }

                        if (wizard.Bounds.CollidesWith(goober.Bounds))
                        {
                            wizard.Hit = true;
                        }
                    }

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
            } else
            {
                ScreenManager.SpriteBatch.Begin();
            }
            ScreenManager.SpriteBatch.Draw(background, new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height), Color.White);
            wizard.Draw(gameTime, ScreenManager.SpriteBatch);
            brownGoobers[0].Draw(gameTime, ScreenManager.SpriteBatch);
            brownGoobers[1].Draw(gameTime, ScreenManager.SpriteBatch);
            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.Draw(gameTime, ScreenManager.SpriteBatch);
            }

            ScreenManager.SpriteBatch.DrawString(Yoster, "ESC to Exit", new Vector2(10, 5), Color.BlanchedAlmond, 0.0f, new Vector2(0), 0.35f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.DrawString(Yoster, $"{(DateTime.UtcNow.Ticks / 1000 / 1000 / 10) - GameData.StartTime}s", new Vector2(25, 425), Color.LightGoldenrodYellow, 0, new Vector2(0), 0.5f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.Draw(hearts, new Vector2(600, 425), new Rectangle(0, 0, 32 * wizard.Health, 32), Color.White);
           
            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
