using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.Diagnostics;
using CollisionExample.Collisions;

namespace RetroHeroes.Sprites
{
    public class GreenFlyShooter
    {
        private Texture2D texture;
        public bool Exit = false;

        public Vector2 position = new Vector2(650, 400);

        /// Animation Variables
        private bool flipped;
        private double animationTimer;
        private short animationFrame = 1;
        public float timeSinceLastShot = 0f;

        // Collision
        public BoundingCircle Bounds;
        public bool Hit = false;
        public int Health = 5;
        public bool Shown = true;

        public GreenFlyShooter(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
        }

        /// <summary>
        /// Updates the sprite's position based on user input
        /// </summary>
        /// <param name="gameTime">The GameTime</param>
        public void Update(GameTime gameTime, Vector2 characterPosition)
        {
            var idle = true;
            var multiplier = 150 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 adjCharacterPosition = new Vector2(characterPosition.X, characterPosition.Y - 30);
            Vector2 chaseAngle = adjCharacterPosition - position;
            if (Math.Abs(adjCharacterPosition.X - position.X) > 1)
            {
                if (adjCharacterPosition.X < position.X)
                {
                    flipped = true;
                }
                else
                {
                    flipped = false;
                }
            }
            chaseAngle.Normalize();

            position += chaseAngle * Vector2.One / 2f;
            Bounds = new BoundingCircle(position + new Vector2(0, 10), 15);
        }

        /// <summary>
        /// Draws the sprite using the supplied SpriteBatch
        /// </summary>
        /// <param name="gameTime">The game time</param>
        /// <param name="spriteBatch">The spritebatch to render with</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer > 0.25)
            {
                if (Hit)
                {
                    Health--;
                    Hit = false;
                    animationTimer -= 0.25;
                }

                if (Health <= 0)
                {
                    position = new Vector2(-600, -600);
                    Shown = false;
                }
                animationFrame++;
                if (animationFrame > 2) animationFrame = 0;
                animationTimer -= 0.25;
            }

            var source = new Rectangle(0 + 72 * animationFrame, 148, 72, 72);
            SpriteEffects effect = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (Shown)
            {
                spriteBatch.Draw(texture, position, source, Hit ? Color.Red : Color.White, 0.0f, new Vector2(36, 36), 1f, effect, 0);
            }
        }
    }
}
