using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics
{
    public class TextureAtlas
    {
        private Dictionary<string, TextureRegion> _regions;

        ///<summary>
        /// Gets or Sets the source texture represented by this texture atlas.
        ///</summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Stores animations added to this atlas.
        /// </summary>
        private Dictionary<string, Animation> _animation;

        /// <summary>
        /// Creates a new texture atlas.
        /// </summary>
        public TextureAtlas()
        {
            _regions = new Dictionary<string, TextureRegion>();
            _animation = new Dictionary<string, Animation>();
        }

        /// <summary>
        /// Creates a new texture class atlas instance using the given texture.
        /// </summary>
        /// <param name="texture">The source texture represented by the texture atlas.</param>
        public TextureAtlas(Texture2D texture)
        {
            Texture = texture;
            _regions = new Dictionary<string, TextureRegion>();
            _animation = new Dictionary<string, Animation>();
        }

        /// <summary>
        /// Creates a new region and adds it to this texture atlas.
        /// </summary>
        /// <param name="name">The name to give the texture region.</param>
        /// <param name="x">The top-left x-coordinate position of the region boundary relative to the top-left corner if the source texture boundary.</param>
        /// <param name="y">The top-left y-coordinate position of the region boundary relative to the top-left corner if the source texture boundary.</param>
        /// <param name="width">The width, in pixels, of the region.</param>
        /// <param name="height">The height, in pixels, of the region.</param>
        public void AddRegions(string name, int x, int y, int width, int height)
        {
            TextureRegion region = new TextureRegion(Texture, x, y, width, height);
            _regions.Add(name, region);
        }

        /// <summary>
        /// Get the region from this texture atlas with the specified name.
        /// </summary>
        /// <param name="name">The name of region to retrieve.</param>
        /// <returns>The TextureRegion with the specified name.</returns>
        public TextureRegion GetRegion(string name)
        {
            return _regions[name];
        }

        /// <summary>
        /// Removes the region from this texture atlas with specified name.
        /// </summary>
        /// <param name="name">The name of the region to remove.</param>
        /// <returns>true if region is removed successfuly; otherwise, false.</returns>
        public bool RemoveRegion(string name)
        {
            return _regions.Remove(name);
        }

        /// <summary>
        /// Removes all regions from this texture atlas.
        /// </summary>
        public void Clear()
        {
            _regions.Clear();
        }

        /// <summary>
        /// Adds the given animation to this texture atlas with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to add.</param>
        /// <param name="animation">The animation to add.</param>
        public void AddAnimation(string animationName,Animation animation)
        {
            _animation.Add(animationName, animation);
        }

        /// <summary>
        /// Gets the animation from this texture atlas with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to retrieve.</param>
        /// <returns>The animation with the specified name.</returns>
        public Animation GetAnimation(string animationName)
        {
            return _animation[animationName];
        }

        /// <summary>
        /// Removes the animation with the specified name from this texture atlas.
        /// </summary>
        /// <param name="animationName">The name of the animation to remove.</param>
        /// <returns>true if animation is removed successfuly; otherwise, false.</returns>
        public bool RemoveAnimation(string animationName)
        {
            return _animation.Remove(animationName);
        }

        /// <summary>
        /// Creates a new sprite using the region from this texture atlas with the specified name.
        /// </summary>
        /// <param name="regionName">The name of the region to create the sprite with.</param>
        /// <returns>A new Sprite using the texture region with the specified name.</returns>
        public Sprite CreateSprite(string regionName)
        {
            TextureRegion region = GetRegion(regionName);
            return new Sprite(region);
        }

        /// <summary>
        /// Creates a new animated sprote using the animation from this texture atlas with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to use</param>
        /// <returns>A new AnimatedSprite using the animation with the specified name.</returns>
        public AnimatedSprite CreateAnimatedSprite(string animationName)
        {
            Animation animation = GetAnimation(animationName);
            return new AnimatedSprite(animation);
        }

        /// <summary>
        /// Creates a new texture atlas based on a texture atlas xml configuration file.
        /// </summary>
        /// <param name="content">The content manager used to load the texture for the atlas.</param>
        /// <param name="fileName">The path to xml file, relative to content root directory.</param>
        /// <returns>The texture atlas created by this method</returns>
        public static TextureAtlas FromFile(ContentManager content, string fileName)
        {
            TextureAtlas atlas = new TextureAtlas();
            
            string filePath = Path.Combine(content.RootDirectory, fileName);

            using (Stream stream = TitleContainer.OpenStream(filePath))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    XDocument doc = XDocument.Load(reader);
                    XElement root = doc.Root;

                    string texturePath = root.Element("Texture").Value;
                    atlas.Texture = content.Load<Texture2D>(texturePath);

                    var regions = root.Element("Regions")?.Elements("Region");

                    if (regions != null)
                    {
                        foreach (var region in regions)
                        {
                            string name = region.Attribute("name")?.Value;
                            int x = int.Parse(region.Attribute("x")?.Value ?? "0");
                            int y = int.Parse(region.Attribute("y")?.Value ?? "0");
                            int width = int.Parse(region.Attribute("width")?.Value ?? "0");
                            int height = int.Parse(region.Attribute("height")?.Value ?? "0");

                            if (!string.IsNullOrEmpty(name))
                            {
                                atlas.AddRegions(name, x, y, width, height);
                            }
                        }
                    }

                    var animationElements = root.Elements("Animations").Elements("Animation");

                    if (animationElements != null)
                    {
                        foreach (var animationElement in animationElements)
                        {
                            string name = animationElement.Attribute("name")?.Value;
                            float delayInMilliseconds = float.Parse(animationElement.Attribute("delay")?.Value ?? "0");
                            TimeSpan delay = TimeSpan.FromMilliseconds(delayInMilliseconds);
                            
                            List<TextureRegion> frames = new List<TextureRegion>();

                            var frameElements = animationElement.Elements("Frame");

                            if (frameElements != null)
                            {
                                foreach (var frameElement in frameElements)
                                {
                                    string regionName = frameElement.Attribute("region").Value;
                                    TextureRegion region = atlas.GetRegion(regionName);
                                    frames.Add(region);
                                }
                            }

                            Animation animation = new Animation(frames, delay);
                            atlas.AddAnimation(name, animation);
                        }
                    }

                    return atlas;
                }
            }
        }
    }
}
