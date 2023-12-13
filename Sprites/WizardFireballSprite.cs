using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite.Sprites;
using CollisionExample.Collisions;
using RetroHeroes;

namespace RetroHeroes.Sprites
{
    public class WizardFireballSprite
    {
        private MouseState mouseState;
        private Texture2D fullSheet;

        public bool Shown;
        public Vector2 position;
        public Vector2 velocity;
        public float angle;

        /// Animation Variables
        private double animationTimer;
        private short animationFrame = 1;

        // Collision
        public BoundingCircle Bounds;

        /// <summary>
        /// Loads the sprite texture using the provided ContentManager
        /// </summary>
        /// <param name="content">The ContentManager to load with</param>
        public void LoadContent(ContentManager content)
        {
            fullSheet = content.Load<Texture2D>("WizardProjectile");
        }

        /// <summary>
        /// Updates the sprite's position based on user input
        /// </summary>
        /// <param name="gameTime">The GameTime</param>
        public void Update(GameTime gameTime, Vector2 characterPosition, GraphicsDevice gd, float angleAdjust = 0f, bool doubleShot = false)
        {
            mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed && Shown == false)
            {
                Vector2 adjCharacterPosition = new Vector2(characterPosition.X, characterPosition.Y - 30);
                Vector2 shotAngle = new Vector2(mouseState.X, mouseState.Y) - (adjCharacterPosition);
                if (angleAdjust != 0f)
                {
                    float origAngle = (float)Math.Atan2(shotAngle.Y, shotAngle.X);
                    float degrees = origAngle * (180f / (float)Math.PI);
                    degrees += angleAdjust;
                    float adjustedAngleRadians = degrees * ((float)Math.PI / 180f);
                    shotAngle = new Vector2((float)Math.Cos(adjustedAngleRadians), (float)Math.Sin(adjustedAngleRadians));
                }
                shotAngle.Normalize();
                if (doubleShot)
                {
                    position = adjCharacterPosition + shotAngle * 50;
                }
                else
                {
                    position = adjCharacterPosition + shotAngle * 10;
                }
                velocity = shotAngle;
                Shown = true;
                angle = (float)Math.Atan2(position.Y - adjCharacterPosition.Y, position.X - adjCharacterPosition.X);
                if (angleAdjust != 0f)
                {
                    angle += angleAdjust * ((float)Math.PI / 180f);
                }
            }

            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds * 300;
            Bounds = new BoundingCircle(position, 12);

            if (position.X < 40 || position.Y < 75 || position.X > gd.Viewport.Width - 40 || position.Y > gd.Viewport.Height - 40)
            {
                Shown = false;
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

            if (animationTimer > 0.10)
            {
                animationFrame++;
                animationTimer -= 0.10;
                if (animationFrame > 3) animationFrame = 0;
            }

            var source = new Rectangle(385 + (864 * animationFrame), 193, 96, 96);
            if (Shown)
            {
                spriteBatch.Draw(fullSheet, position, source, Color.White, angle, new Vector2(48, 48), new Vector2(0.25f), SpriteEffects.None, 0);
            }
        }
    }
}
