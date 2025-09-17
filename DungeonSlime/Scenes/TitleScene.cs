using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace DungeonSlime.Scenes
{
    public class TitleScene : Scene
    {
        private const string DUNGEON_TEXT = "Dungeon";
        private const string SLIME_TEXT = "Slime";
        private const string PRESS_ENTER_TEXT = "Press Enter To Start";

        private SpriteFont _font;

        private SpriteFont _font5X;

        private Vector2 _dungeonTextPos;

        private Vector2 _dungeonTextOrigin;

        private Vector2 _slimeTextPos;

        private Vector2 _slimeTextOrigin;

        private Vector2 _pressEnterPos;

        private Vector2 _pressEnterOrigin;

        private Texture2D _backgroundPattern;

        private Rectangle _backgroundDestination;

        // The offset to apply when drawing the background pattern so it appears to
        // be scrolling.
        private Vector2 _backgroundOffset;

        private float _scrollSpeed = 50.0f;

        public override void Initialize()
        {
            base.Initialize();

            Core.ExitOnEscape = true;

            Vector2 size = _font5X.MeasureString(DUNGEON_TEXT);
            _dungeonTextPos = new Vector2(640, 100);
            _dungeonTextOrigin = size * 0.5f;

            size = _font5X.MeasureString(SLIME_TEXT);
            _slimeTextPos = new Vector2(757, 207);
            _slimeTextOrigin = size * 0.5f;

            size = _font.MeasureString(PRESS_ENTER_TEXT);
            _pressEnterPos = new Vector2(640, 620);
            _pressEnterOrigin = size * 0.5f;

            _backgroundOffset = Vector2.Zero;

            _backgroundDestination = Core.GraphicsDevice.PresentationParameters.Bounds;
        }

        public override void LoadContent()
        {
            _font = Core.Content.Load<SpriteFont>("fonts/04B_30");

            _font5X = Core.Content.Load<SpriteFont>("fonts/04B_30_5x");

            _backgroundPattern = Core.Content.Load<Texture2D>("images/background-pattern");
        }

        public override void Update(GameTime gameTime)
        {
            if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
            {
                Core.ChangeScene(new GameScene());
            }

            // Update the offsets for the background pattern wrapping so that it
            // scrolls down and to the right.
            float offset = _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _backgroundOffset.X -= offset;
            _backgroundOffset.Y -= offset;

            // Ensure that the offsets do not go beyond the texture bounds so it is
            // a seamless wrap.
            _backgroundOffset.X %= _backgroundPattern.Width;
            _backgroundOffset.Y %= _backgroundPattern.Height;
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
            Core.SpriteBatch.Draw(_backgroundPattern, _backgroundDestination, new Rectangle(_backgroundOffset.ToPoint(), _backgroundDestination.Size), Color.White * 0.5f);
            Core.SpriteBatch.End();

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Color dropShadowColor = Color.Black * 0.5f;

            Core.SpriteBatch.DrawString(_font5X, DUNGEON_TEXT, _dungeonTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            Core.SpriteBatch.DrawString(_font5X, DUNGEON_TEXT, _dungeonTextPos, Color.White, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            Core.SpriteBatch.DrawString(_font5X, SLIME_TEXT, _slimeTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            Core.SpriteBatch.DrawString(_font5X, SLIME_TEXT, _slimeTextPos, Color.White, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            Core.SpriteBatch.DrawString(_font, PRESS_ENTER_TEXT, _pressEnterPos, Color.White, 0.0f, _pressEnterOrigin, 1.0f, SpriteEffects.None, 0.0f);

            Core.SpriteBatch.End();
        }
    }
}
