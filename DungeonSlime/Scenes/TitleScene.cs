using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameGum.GueDeriving;
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

        private Texture2D _backgroundPattern;

        private Rectangle _backgroundDestination;

        private SoundEffect _uiSoundEffect;

        private Panel _titleScreenButtonsPanel;

        private Panel _optionsPanel;

        private Button _optionsButton;

        private Button _optionsBackButton;

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

            _backgroundOffset = Vector2.Zero;

            _backgroundDestination = Core.GraphicsDevice.PresentationParameters.Bounds;

            InitializeUI();
        }

        public override void LoadContent()
        {
            _font = Core.Content.Load<SpriteFont>("fonts/04B_30");

            _font5X = Core.Content.Load<SpriteFont>("fonts/04B_30_5x");

            _backgroundPattern = Core.Content.Load<Texture2D>("images/background-pattern");

            _uiSoundEffect = Core.Content.Load<SoundEffect>("audio/ui");
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

            GumService.Default.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
            Core.SpriteBatch.Draw(_backgroundPattern, _backgroundDestination, new Rectangle(_backgroundOffset.ToPoint(), _backgroundDestination.Size), Color.White * 0.5f);
            Core.SpriteBatch.End();

            if (_titleScreenButtonsPanel.IsVisible)
            {
                Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

                Color dropShadowColor = Color.Black * 0.5f;

                Core.SpriteBatch.DrawString(_font5X, DUNGEON_TEXT, _dungeonTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

                Core.SpriteBatch.DrawString(_font5X, DUNGEON_TEXT, _dungeonTextPos, Color.White, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

                Core.SpriteBatch.DrawString(_font5X, SLIME_TEXT, _slimeTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

                Core.SpriteBatch.DrawString(_font5X, SLIME_TEXT, _slimeTextPos, Color.White, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

                Core.SpriteBatch.End(); 
            }

            GumService.Default.Draw();
        }

        private void CreateTitlePanel()
        {
            _titleScreenButtonsPanel = new Panel();
            _titleScreenButtonsPanel.Dock(Gum.Wireframe.Dock.Fill);
            _titleScreenButtonsPanel.AddToRoot();

            var startButton = new Button();
            startButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
            startButton.Visual.X = 50;
            startButton.Visual.Y = -12;
            startButton.Visual.Width = 70;
            startButton.Text = "Start";
            startButton.Click += HandleStartClicked;
            _titleScreenButtonsPanel.AddChild(startButton);

            _optionsButton = new Button();
            _optionsButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
            _optionsButton.Visual.X = -50;
            _optionsButton.Visual.Y = -12;
            _optionsButton.Visual.Width = 70;
            _optionsButton.Text = "Options";
            _optionsButton.Click += HandleOptionsClicked;
            _titleScreenButtonsPanel.AddChild(_optionsButton);

            startButton.IsFocused = true;
        }

        private void HandleOptionsClicked(object sender, System.EventArgs e)
        {
            Core.Audio.PlaySoundEffect(_uiSoundEffect);

            _titleScreenButtonsPanel.IsVisible = false;

            _optionsPanel.IsVisible = true;

            _optionsBackButton.IsFocused = true;
        }

        private void HandleStartClicked(object sender, System.EventArgs e)
        {
            Core.Audio.PlaySoundEffect(_uiSoundEffect);

            Core.ChangeScene(new GameScene());
        }

        private void CreateOptionsPanel()
        {
            _optionsPanel = new Panel();
            _optionsPanel.Dock(Gum.Wireframe.Dock.Fill);
            _optionsPanel.IsVisible = false;
            _optionsPanel.AddToRoot();

            var optionsText = new TextRuntime();
            optionsText.X = 10;
            optionsText.Y = 10;
            optionsText.Text = "OPTIONS";
            _optionsPanel.AddChild(optionsText);

            var musicSlider = new Slider();
            musicSlider.Anchor(Gum.Wireframe.Anchor.Top);
            musicSlider.Visual.Y = 30f;
            musicSlider.Minimum = 0;
            musicSlider.Maximum = 1;
            musicSlider.Value = Core.Audio.SongVolume;
            musicSlider.SmallChange = .1;
            musicSlider.LargeChange = .2;
            musicSlider.ValueChanged += HandleMusicSliderValueChanged;
            musicSlider.ValueChangeCompleted += HandleMusicSliderValueChangeCompleted;
            _optionsPanel.AddChild(musicSlider);

            var sfxSlider = new Slider();
            sfxSlider.Anchor(Gum.Wireframe.Anchor.Top);
            sfxSlider.Visual.Y = 93;
            sfxSlider.Minimum = 0;
            sfxSlider.Maximum = 1;
            sfxSlider.Value = Core.Audio.SoundEffectVolume;
            sfxSlider.SmallChange = .1;
            sfxSlider.LargeChange = .2;
            sfxSlider.ValueChanged += HandleSfxSliderValueChanged; ;
            sfxSlider.ValueChangeCompleted += HandleSfxSliderValueChangeCompleted; ;
            _optionsPanel.AddChild(sfxSlider);

            _optionsBackButton = new Button();
            _optionsBackButton.Text = "Back";
            _optionsBackButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
            _optionsBackButton.X = -28f;
            _optionsBackButton.Y = -10f;
            _optionsBackButton.Click += HandleOptionsButtonBack;
            _optionsPanel.AddChild(_optionsBackButton);
        }

        private void HandleOptionsButtonBack(object sender, System.EventArgs e)
        {
            Core.Audio.PlaySoundEffect(_uiSoundEffect);

            _titleScreenButtonsPanel.IsVisible = true;

            _optionsPanel.IsVisible = false;

            _optionsButton.IsFocused = true;
        }

        private void HandleSfxSliderValueChangeCompleted(object sender, System.EventArgs e)
        {
            Core.Audio.PlaySoundEffect(_uiSoundEffect);
        }

        private void HandleSfxSliderValueChanged(object sender, System.EventArgs e)
        {
            var slider = (Slider)sender;

            Core.Audio.SoundEffectVolume = (float)slider.Value;
        }

        private void HandleMusicSliderValueChangeCompleted(object sender, System.EventArgs e)
        {
            var slider = (Slider)sender;

            Core.Audio.SongVolume = (float)slider.Value;
        }

        private void HandleMusicSliderValueChanged(object sender, System.EventArgs e)
        {
            Core.Audio.PlaySoundEffect(_uiSoundEffect);
        }

        private void InitializeUI()
        {
            GumService.Default.Root.Children.Clear();

            CreateTitlePanel();
            CreateOptionsPanel();
        }
    }
}
