using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameLibrary
{
    public class Core: Game
    {
        
        internal static Core s_instance;
       
        /// <summary>
        /// Gets a reference to the Core instance.
        /// </summary>
        public static Core Instance => s_instance;


        /// <summary>
        /// Gets the graphics device manager to control the presentation of graphics.
        /// </summary>
        public static GraphicsDeviceManager Graphics { get; private set; }

        /// <summary>
        /// Gets the graphics device used to create graphical resources and perform primitive rendering.
        /// </summary>
        public static new GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the sprite batch used for all 2D rendering.
        /// </summary>
        public static SpriteBatch SpriteBatch { get; private set; }
        /// <summary>
        /// Gets the content manager used to load global assets.
        /// </summary>
        public static new ContentManager Content { get; private set; }

        /// <summary>
        /// Gets a reference to the input managment system.
        /// </summary>
        public static InputManager Input { get; private set; }

        /// <summary>
        /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
        /// </summary>
        public static bool ExitOnEscape { get; set; }

        /// <summary>
        /// Creates a new Core instance.
        /// </summary>
        /// <param name="title">The title to display in the title bar of the game window.</param>
        /// <param name="width">The initial width, in pixels, of the game window.</param>
        /// <param name="height">The initial height, in pixels, of the game window.</param>
        /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
        public Core(string title, int width, int height, bool fullScreen)
        {
            if (s_instance != null)
            {
                throw new InvalidOperationException($"Only a single Core instance can be created");
            }

            s_instance = this;

            Graphics = new GraphicsDeviceManager(this);

            Graphics.PreferredBackBufferWidth = width;
            Graphics.PreferredBackBufferHeight = height;
            Graphics.IsFullScreen = fullScreen;

            Graphics.ApplyChanges();

            Window.Title = title;

            Content = base.Content;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            ExitOnEscape = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            GraphicsDevice = base.GraphicsDevice;

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Input = new InputManager();
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update(gameTime);

            if (ExitOnEscape && Input.Keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            base.Update(gameTime);
        }

    }
}
