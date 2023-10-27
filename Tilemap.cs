using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RetroHeroes
{
    public class Tilemap
    {
        int tileWidth, tileHeight, mapWidth, mapHeight;

        Texture2D tileSetTexture;

        Rectangle[] tiles;

        int[] map;

        string filename;

        public Tilemap(string fileName)
        {
            filename = fileName;
        }

        public void LoadContent(ContentManager contentManager)
        {
            string data = File.ReadAllText(Path.Join(contentManager.RootDirectory, filename));

            var lines = data.Split("\n");

            var tileSetFileName = lines[0].Trim();
            tileSetTexture = contentManager.Load<Texture2D>(tileSetFileName);

            var secondLine = lines[1].Split(",");
            tileWidth = int.Parse(secondLine[0]);
            tileHeight = int.Parse(secondLine[1]);

            int tilesetRows = tileSetTexture.Width / tileWidth;
            int tilesetColumns = tileSetTexture.Height / tileHeight;
            tiles = new Rectangle[tilesetColumns * tilesetRows];

            for (int y = 0; y < tilesetColumns; y++)
            {
                for (int x = 0; x < tilesetRows; x++)
                {
                    int index = y * tilesetColumns + x;

                    tiles[index] = new Rectangle(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
                }
            }

            var thirdLine = lines[2].Split(",");
            mapWidth = int.Parse(thirdLine[0]);
            mapHeight = int.Parse(thirdLine[1]);

            var fourthLine = lines[3].Split(',');
            map = new int[mapWidth * mapHeight];

            for (int i = 0; i < mapWidth * mapHeight; i++)
            {
                map[i] = int.Parse(fourthLine[i]);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            float scale = 1.68f;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int index = map[y * mapWidth + x] - 1;
                    if (index == -1)
                    {
                        continue;
                    }
                    spriteBatch.Draw(tileSetTexture, new Vector2(x * scale * tileWidth, y * scale * tileHeight), tiles[index], Color.White, 0f, new Vector2(), scale, SpriteEffects.None, 0);
                }
            }
        }
    }
}