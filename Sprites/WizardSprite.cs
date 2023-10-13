using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollisionExample.Collisions;
using System.Security.Cryptography.X509Certificates;
using SharpDX.Direct3D9;
using Microsoft.Xna.Framework.Audio;

namespace RetroHeroes.Sprites
{
    public class WizardSprite
    {
        private KeyboardState previousKeyboardState;
        private KeyboardState keyboardState;

        private Texture2D idleTexture;
        private Texture2D runTexture;
        private Texture2D texture;
        public bool Exit = false;

        public Vector2 position = new Vector2(200, 200);

        /// Health State
        public BoundingRectangle Bounds;
        public bool Hit = false;
        public double LastHit;
        public int Health = 10;

        /// Animation Variables
        private bool flipped;
        private double animationTimer;
        private short animationFrame = 1;

        public SoundEffect wizardHitByGoober;

        /// <summary>
        /// Loads the sprite texture using the provided ContentManager
        /// </summary>
        /// <param name="content">The ContentManager to load with</param>
        public void LoadContent(ContentManager content)
        {
            wizardHitByGoober = content.Load<SoundEffect>("BrownGoobPlayerHit");
            idleTexture = content.Load<Texture2D>("wizardidle");
            runTexture = content.Load<Texture2D>("wizardrun");
            texture = idleTexture;
        }

        /// <summary>
        /// Updates the sprite's position based on user input
        /// </summary>
        /// <param name="gameTime">The GameTime</param>
        public void Update(GameTime gameTime, GraphicsDevice gd)
        {
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            var idle = true;
            var multiplier = 150 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var velocity = new Vector2();

            // Up
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                idle = false;
                velocity += new Vector2(0, -1);
                if (texture == idleTexture)
                {
                    texture = runTexture;
                }
            }

            // Down
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                idle = false;
                velocity += new Vector2(0, 1);
                if (texture == idleTexture)
                {
                    texture = runTexture;
                }
            }

            // Left
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
            {
                idle = false;
                velocity += new Vector2(-1, 0);
                if (texture == idleTexture)
                {
                    texture = runTexture;
                }
                flipped = true;
            }

            // Right
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                idle = false;
                velocity += new Vector2(1, 0);
                if (texture == idleTexture)
                {
                    texture = runTexture;
                }
                flipped = false;
            }

            if (idle)
            {
                texture = idleTexture;
            }

            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed && ms.X > position.X && flipped == true)
            {
                flipped = false;
            }

            if (ms.LeftButton == ButtonState.Pressed && ms.X < position.X && flipped == false)
            {
                flipped = true;
            }

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit = true;
            }


            if (velocity != Vector2.Zero)
            {
                velocity.Normalize();
                velocity = velocity * multiplier;
            }

            if (!(position.X + velocity.X < 50 || position.X + velocity.X > gd.Viewport.Width - 50))
            {
                position += new Vector2(velocity.X, 0);
            }

            if (!(position.Y + velocity.Y < 100 || position.Y + velocity.Y > gd.Viewport.Height - 33))
            {
                position += new Vector2(0, velocity.Y);
            }
            Bounds = new BoundingRectangle(position.X - 32, position.Y - 32, 32, 48);
        }

        /// <summary>
        /// Draws the sprite using the supplied SpriteBatch
        /// </summary>
        /// <param name="gameTime">The game time</param>
        /// <param name="spriteBatch">The spritebatch to render with</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
            LastHit += gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer > 0.15)
            {
                if (Hit && LastHit > 0.75)
                {
                    wizardHitByGoober.Play(volume: 1f, pitch: 0.5f, pan: 0.5f);
                    Hit = false;
                    Health--;
                    LastHit = 0f;
                }

                animationFrame++;
                if (texture == idleTexture && animationFrame > 3) animationFrame = 0;
                if (texture == runTexture && animationFrame > 5) animationFrame = 0;
                animationTimer -= 0.15;
            }

            bool isIdle = texture == idleTexture;
            var scale = isIdle ? 32 : 64;
            var source = new Rectangle((isIdle && animationFrame > 3 ? animationFrame % 3 : animationFrame) * scale, isIdle ? 0 : 16, scale, isIdle ? 32 : 48);
            SpriteEffects effect = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(texture, position, source, Hit ? Color.Red : Color.White, 0.0f, new Vector2(scale/2,isIdle ? 32 : 48), 1.5f, effect, 0);
        }
    }
}
