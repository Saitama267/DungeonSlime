using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace MonoGameLibrary.Scenes
{
    public abstract class Scene : IDisposable
    {
        /// <summary>
        /// Gets the ContentManager used for loading scene-specified assets.
        /// </summary>
        /// <remarks>
        /// Assets loaded through this ContentManager will be automatically unloaded when this scene ends.
        /// </remarks>
        protected ContentManager Content { get; }

        /// <summary>
        /// Gets a value that indicates if the scene has been disposed of.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a new scene instance.
        /// </summary>
        protected Scene()
        {
            Content = new ContentManager(Core.Content.ServiceProvider);

            Content.RootDirectory = Core.Content.RootDirectory;
        }

        ~Scene() => Dispose();

        /// <summary>
        /// Initializes the scene.
        /// </summary>
        /// <remarks>
        /// When overriding this in a derived class, ensure that base.Initialize()
        /// still called as this is when LoadContent is called.
        /// </remarks>
        public virtual void Initialize()
        {
            LoadContent();
        }

        /// <summary>
        /// Override to provide logic to load content for the scene.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Unload scene-specified content.
        /// </summary>
        public virtual void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Update this scene.
        /// </summary>
        /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
        public virtual void Draw(GameTime gameTime) { }

        /// <summary>
        /// Disposes of this scene.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes if this scene.
        /// </summary>
        /// <param name="disposing">Indicates wheter managed resources should be disposed. This value is only true when called from the main
        /// Dispose method. Whwn called from the finalizer, this will be false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                UnloadContent();
                Content.Dispose();
            }
        }
    }
}
