using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace MonoGameLibrary.Graphics
{
    public class Tilemap
    {
        private readonly Tileset _tileset;
        private readonly int[] _tiles;

        /// <summary>
        /// Gets the total number of rows in this tilemap.
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Gets the total number of columns in this tilemap.
        /// </summary>
        public int Columns { get; }

        /// <summary>
        /// Gets the total number of tiles in this tilemap.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets or Sets the scale factor to draw each tile at.
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// Gets the width, in pixels, each tile is drawn at.
        /// </summary>
        public float TileWidth => _tileset.TileWidth * Scale.X;

        /// <summary>
        /// Gets the height, in pixels, each tile is drawn at.
        /// </summary>
        public float TileHeight => _tileset.TileHeight * Scale.Y;

        /// <summary>
        /// Creates a new tilemap.
        /// </summary>
        /// <param name="tileset">The tileset used by this tilemap.</param>
        /// <param name="columns">The total number of columns in this tilemap.</param>
        /// <param name="rows">The total number of rows in this tilemap.</param>
        public Tilemap(Tileset tileset, int columns, int rows)
        {
            _tileset = tileset;
            Rows = rows;
            Columns = columns;
            Count = Columns * Rows;
            Scale = Vector2.One;
            _tiles = new int[Count];
        }

        /// <summary>
        /// Sets the tile at the given index in this tilemap to use the tile from
        /// the tileset at the specified tileset id.
        /// </summary>
        /// <param name="index">The index of the tile in this tilemap.</param>
        /// <param name="tilesetId">The tileset id of the tile from the tileset to use.</param>
        public void SetTile(int index, int tilesetID)
        {
            _tiles[index] = tilesetID;
        }

        /// <summary>
        /// Sets the tile at the given column and row in this tilemap to use the tile
        /// from the tileset at the specified tileset id.
        /// </summary>
        /// <param name="column">The column of the tile in this tilemap.</param>
        /// <param name="row">The row of the tile in this tilemap.</param>
        /// <param name="tilesetID">The tileset id of the tile from the tileset to use.</param>
        public void SetTile(int column, int row, int tilesetID)
        {
            int index = row * Columns + column;
            SetTile(index, tilesetID);
        }

        /// <summary>
        /// Gets the texture region of the tile from this tilemap at the specified index.
        /// </summary>
        /// <param name="index">The index of the til in this tilemap.</param>
        /// <returns>The texture region of the tile from this tilemap at the specified index.</returns>
        public TextureRegion GetTile(int index)
        {
            return _tileset.GetTile(index);
        }

        /// <summary>
        /// Gets the texture region of the tile from this tilemap at the specified 
        /// column and row.
        /// </summary>
        /// <param name="column">The column of the tile in this tilemap.</param>
        /// <param name="row">The row of the tile in this tilemap.</param>
        /// <returns>The texture region og the tile from this tilemap at the specified column and row.</returns>
        public TextureRegion GetTile(int column, int row)
        {
            int index = row * Columns + column;
            return GetTile(index);
        }

        /// <summary>
        /// Draws this tilemap using the given sprite batch.
        /// </summary>
        /// <param name="spriteBatch">THe sprite batch used to draw this tilemap.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Count; i++)
            {
                int tilesetIndex = _tiles[i];
                TextureRegion tile = _tileset.GetTile(tilesetIndex);

                int x = i % Columns;
                int y = i / Columns;

                Vector2 position = new Vector2(x * TileWidth, y * TileHeight);
                tile.Draw(spriteBatch, position, Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 1.0f);
            }
        }

        public static Tilemap FromFile(ContentManager content, string filename)
        {
            string filePath = Path.Combine(content.RootDirectory, filename);

            using (Stream stream = TitleContainer.OpenStream(filePath))
            {
                using(XmlReader reader = XmlReader.Create(stream))
                {
                    XDocument doc = XDocument.Load(reader);
                    XElement root = doc.Root;

                    // The <Tileset> element contains the information about the tileset
                    // used by the tilemap.
                    //
                    // Example
                    // <Tileset region="0 0 100 100" tileWidth="10" tileHeight="10">contentPath</Tileset>
                    //
                    // The region attribute represents the x, y, width, and height
                    // components of the boundary for the texture region within the
                    // texture at the contentPath specified.
                    //
                    // the tileWidth and tileHeight attributes specify the width and
                    // height of each tile in the tileset.
                    //
                    // the contentPath value is the contentPath to the texture to
                    // load that contains the tileset
                    XElement tileSetElement = root.Element("Tileset");

                    string regionAttribute = tileSetElement.Attribute("region").Value;
                    string[] split = regionAttribute.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    int x = int.Parse(split[0]);
                    int y = int.Parse(split[1]);
                    int width = int.Parse(split[2]);
                    int height = int.Parse(split[3]);

                    int tileWidth = int.Parse(tileSetElement.Attribute("tileWidth").Value);
                    int tileHeight = int.Parse(tileSetElement.Attribute("tileHeight").Value);
                    string contentPath = tileSetElement.Value;

                    Texture2D texture = content.Load<Texture2D>(contentPath);

                    TextureRegion textureRegion = new TextureRegion(texture, x, y, width, height);

                    Tileset tileset = new Tileset(textureRegion, tileWidth, tileHeight);
                    
                    // The <Tiles> element contains lines of strings where each line
                    // represents a row in the tilemap.  Each line is a space
                    // separated string where each element represents a column in that
                    // row.  The value of the column is the id of the tile in the
                    // tileset to draw for that location.
                    //
                    // Example:
                    // <Tiles>
                    //      00 01 01 02
                    //      03 04 04 05
                    //      03 04 04 05
                    //      06 07 07 08
                    // </Tiles>
                    XElement tilesElement = root.Element("Tiles");

                    string[] rows = tilesElement.Value.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    int columnCount = rows[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;

                    Tilemap tilemap = new Tilemap(tileset, columnCount, rows.Length);

                    for (int row = 0; row < rows.Length; row++)
                    {
                        string[] columns = rows[row].Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);

                        for (int column = 0; column < columnCount; column++)
                        {
                            int tilesetIndex = int.Parse(columns[column]);

                            tilemap.SetTile(column, row, tilesetIndex);
                        }
                    }

                    return tilemap;
                }
            }
        }
    }
}
