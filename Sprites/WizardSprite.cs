using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private Vector2 position = new Vector2(200, 200);

        /// Animation Variables
        private bool flipped;
        private double animationTimer;
        private short animationFrame = 1;

        /// <summary>
        /// Loads the sprite texture using the provided ContentManager
        /// </summary>
        /// <param name="content">The ContentManager to load with</param>
        public void LoadContent(ContentManager content)
        {
            idleTexture = content.Load<Texture2D>("wizardidle");
            runTexture = content.Load<Texture2D>("wizardrun");
            texture = idleTexture;
        }

        /// <summary>
        /// Updates the sprite's position based on user input
        /// </summary>
        /// <param name="gameTime">The GameTime</param>
        public void Update(GameTime gameTime)
        {
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            var idle = true;
            var multiplier = 150 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Up
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                idle = false;
                position += new Vector2(0, -1) * multiplier;
                if (texture == idleTexture)
                {
                    texture = runTexture;
                }
            }

            // Down
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                idle = false;
                position += new Vector2(0, 1) * multiplier;
                if (texture == idleTexture)
                {
                    texture = runTexture;
                }
            }

            // Left
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
            {
                idle = false;
                position += new Vector2(-1, 0) * multiplier;
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
                position += new Vector2(1, 0) * multiplier;
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

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit = true;
            }
        }

        /// <summary>
        /// Draws the sprite using the supplied SpriteBatch
        /// </summary>
        /// <param name="gameTime">The game time</param>
        /// <param name="spriteBatch">The spritebatch to render with</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer > 0.15)
            {
                animationFrame++;
                if (texture == idleTexture && animationFrame > 3) animationFrame = 0;
                if (texture == runTexture && animationFrame > 5) animationFrame = 0;
                animationTimer -= 0.15;
            }

            bool isIdle = texture == idleTexture;
            var scale = isIdle ? 32 : 64;
            var source = new Rectangle(animationFrame * scale, isIdle ? 0 : 16, scale, isIdle ? 32 : 48);
            SpriteEffects effect = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(texture, position, source, Color.White, 0.0f, new Vector2(scale/2,isIdle ? 32 : 48), 1.5f, effect, 0);
        }
    }
}
