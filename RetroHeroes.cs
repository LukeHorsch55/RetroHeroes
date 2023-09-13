using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite.Sprites;
using RetroHeroes.Sprites;
using System;
using System.Diagnostics;
using System.IO;

namespace RetroHeroes
{
    public class RetroHeroes : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Fonts
        private SpriteFont Yoster;

        // Dungeon
        TextureAtlas dungeonItemAtlas;
        private Sprite sprite1;
        private Sprite sprite2;
        private Sprite sprite3;

        // Heros
        private WizardSprite wizard;

        // Projectiles
        private WizardFireballSprite[] wizardProjectiles = new WizardFireballSprite[4];
        private int fireballsShown = 0;
        private float timeSinceLastFireball = 0.75f;

        public RetroHeroes()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            wizard = new WizardSprite();
            wizardProjectiles[0] = new WizardFireballSprite();
            wizardProjectiles[1] = new WizardFireballSprite();
            wizardProjectiles[2] = new WizardFireballSprite();
            wizardProjectiles[3] = new WizardFireballSprite();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AsepriteFile aseDungeonItems = AsepriteFile.Load(Directory.GetParent(sCurrentDirectory).Parent.Parent.Parent + "\\Content\\DungeonItems.aseprite");
            dungeonItemAtlas = TextureAtlasProcessor.Process(graphics.GraphicsDevice, aseDungeonItems);
            dungeonItemAtlas.CreateRegion("Crate", new Rectangle(0,0,16,32));
            dungeonItemAtlas.CreateRegion("Barrel", new Rectangle(16,0,16,32));
            dungeonItemAtlas.CreateRegion("Pot", new Rectangle(32, 16, 16, 16));
            dungeonItemAtlas.CreateRegion("PotBroken", new Rectangle(32, 32, 16, 16));
            dungeonItemAtlas.CreateRegion("Chest", new Rectangle(96, 192, 32, 32));
            dungeonItemAtlas.CreateRegion("ChestOpen", new Rectangle(128, 192, 32, 32));
            dungeonItemAtlas.CreateRegion("Key", new Rectangle(33, 65, 16, 16));
            sprite1 = dungeonItemAtlas.CreateSprite("Chest");
            sprite2 = dungeonItemAtlas.CreateSprite("ChestOpen");
            sprite3 = dungeonItemAtlas.CreateSprite("Key");

            wizard.LoadContent(Content);
            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.LoadContent(Content);
            }
            Yoster = Content.Load<SpriteFont>("Yoster");
        }

        protected override void Update(GameTime gameTime)
        {
            // Wizard Logic
            if (wizard.Exit) Exit();
            wizard.Update(gameTime);

            // Fireball Logic
            bool newFireball = false;
            foreach ( WizardFireballSprite fireball in wizardProjectiles )
            {
                bool before = fireball.Shown;
                if (!before && timeSinceLastFireball > 0.75 && !newFireball && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    newFireball = true;
                    fireball.Update(gameTime, wizard.position, graphics.GraphicsDevice);
                    timeSinceLastFireball = 0;
                    break;
                }

                if( before )
                {
                    fireball.Update(gameTime, wizard.position, graphics.GraphicsDevice);
                }
            }
            timeSinceLastFireball += (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var fontSize = Yoster.MeasureString("Retro Heroes");
            var title = new Vector2((graphics.GraphicsDevice.PresentationParameters.Bounds.Width / 2) - fontSize.X / 2, 100f);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            wizard.Draw(gameTime, spriteBatch);
            foreach (WizardFireballSprite fireball in wizardProjectiles)
            {
                fireball.Draw(gameTime, spriteBatch);
            }
            spriteBatch.DrawString(Yoster, "Retro Heroes", title, Color.Goldenrod);
            spriteBatch.DrawString(Yoster, "ESC to Exit", new Vector2(10, 5), Color.BlanchedAlmond, 0.0f, new Vector2(0), 0.35f, SpriteEffects.None, 1);
            sprite1.Scale = new Vector2(1.5f);
            sprite2.Scale = new Vector2(1.5f);
            sprite3.Scale = new Vector2(4f);
            sprite1.Draw(spriteBatch, title - new Vector2(50,0));
            sprite2.Draw(spriteBatch, title + new Vector2(fontSize.X, 0));
            sprite3.Draw(spriteBatch, new Vector2(title.X + fontSize.X / 2 - sprite3.Width * sprite3.ScaleX / 2, 250));
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}