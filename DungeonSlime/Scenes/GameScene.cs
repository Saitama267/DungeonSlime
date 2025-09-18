using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using System;

namespace DungeonSlime.Scenes
{
    public class GameScene :Scene
    {

        private AnimatedSprite _slime;

        private AnimatedSprite _bat;

        private Vector2 _slimePosition;

        private const float MOVEMENT_SPEED = 5.0f;

        private Vector2 _batPosition;

        private Vector2 _batVelocity;

        private Tilemap _tileMap;

        private Rectangle _roomBounds;

        private SoundEffect _bounceSoundEffect;

        private SoundEffect _collectSoundEffect;

        private SpriteFont _font;

        private int _score;

        private Vector2 _scoreTextPosition;

        private Vector2 _scoteTextOrigin;

        private Panel _pausePanel;

        private Button _resumeButton;

        private SoundEffect _uiSoundEffect;

        public override void Initialize()
        {
            base.Initialize();

            Core.ExitOnEscape = false;

            Rectangle screenBounds = Core.GraphicsDevice.PresentationParameters.Bounds;

            _roomBounds = new Rectangle(
                (int)_tileMap.TileWidth,
                (int)_tileMap.TileHeight,
                screenBounds.Width - (int)_tileMap.TileWidth * 2,
                screenBounds.Height - (int)_tileMap.TileHeight * 2
            );

            int centerRow = _tileMap.Rows / 2;
            int centerColumn = _tileMap.Columns / 2;
            _slimePosition = new Vector2(centerColumn * _tileMap.TileWidth, centerRow * _tileMap.TileHeight);

            _batPosition = new Vector2(_roomBounds.Left, _roomBounds.Top);

            _scoreTextPosition = new Vector2(_roomBounds.Left, _tileMap.TileWidth * 0.5f);

            float scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
            _scoteTextOrigin = new Vector2(0, scoreTextYOrigin);

            AssignRandomBatVelocity();

            InitializeUI();
        }

        public override void LoadContent()
        {
            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

            _slime = atlas.CreateAnimatedSprite("slime-animation");
            _slime.Scale = new Vector2(4.0f, 4.0f);

            _bat = atlas.CreateAnimatedSprite("bat-animation");
            _bat.Scale = new Vector2(4.0f, 4.0f);

            _tileMap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
            _tileMap.Scale = new Vector2(4.0f, 4.0f);

            _bounceSoundEffect = Content.Load<SoundEffect>("audio/bounce");

            _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");

            _font = Content.Load<SpriteFont>("fonts/04B_30");

            _uiSoundEffect = Core.Content.Load<SoundEffect>("audio/ui");
        }

        public override void Update(GameTime gameTime)
        {
            GumService.Default.Update(gameTime);

            if (_pausePanel.IsVisible)
            {
                return;
            }

            _slime.Update(gameTime);

            _bat.Update(gameTime);

            CheckKeyboardInput();

            CheckGamePadInput();

            Circle slimeBounds = new Circle(
                (int)(_slimePosition.X + (_slime.Width * 0.5f)),
                (int)(_slimePosition.Y + (_slime.Height * 0.5f)),
                (int)(_slime.Width * 0.5f)
            );

            if (slimeBounds.Left < _roomBounds.Left)
            {
                _slimePosition.X = _roomBounds.Left;
            }
            else if (slimeBounds.Right > _roomBounds.Right)
            {
                _slimePosition.X = _roomBounds.Right - _slime.Width;
            }

            if (slimeBounds.Top < _roomBounds.Top)
            {
                _slimePosition.Y = _roomBounds.Top;
            }
            else if (slimeBounds.Bottom > _roomBounds.Bottom)
            {
                _slimePosition.Y = _roomBounds.Bottom - _slime.Height;
            }

            Vector2 newBatPosition = _batPosition + _batVelocity;

            Circle batBounds = new Circle(
                (int)(newBatPosition.X + (_bat.Width * 0.5f)),
                (int)(newBatPosition.Y + (_bat.Height * 0.5f)),
                (int)(_bat.Width * 0.5f)
            );

            Vector2 normal = Vector2.Zero;

            if (batBounds.Left < _roomBounds.Left)
            {
                normal.X = Vector2.UnitX.X;
                newBatPosition.X = _roomBounds.Left;
            }
            else if (batBounds.Right > _roomBounds.Right)
            {
                normal.X = -Vector2.UnitX.X;
                newBatPosition.X = _roomBounds.Right - _slime.Width;
            }

            if (batBounds.Top < _roomBounds.Top)
            {
                normal.Y = Vector2.UnitY.Y;
                newBatPosition.Y = _roomBounds.Top;
            }
            else if (batBounds.Bottom > _roomBounds.Bottom)
            {
                normal.Y = -Vector2.UnitY.Y;
                newBatPosition.Y = _roomBounds.Bottom - _slime.Height;
            }

            if (normal != Vector2.Zero)
            {
                normal.Normalize();
                _batVelocity = Vector2.Reflect(_batVelocity, normal);

                Core.Audio.PlaySoundEffect(_bounceSoundEffect);
            }

            _batPosition = newBatPosition;

            if (slimeBounds.Intersects(batBounds))
            {

                int column = Random.Shared.Next(1, _tileMap.Columns - 1);
                int row = Random.Shared.Next(1, _tileMap.Rows - 1);

                _batPosition = new Vector2(column * _bat.Width, row * _bat.Height);

                AssignRandomBatVelocity();

                Core.Audio.PlaySoundEffect(_collectSoundEffect);

                _score += 100;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _tileMap.Draw(Core.SpriteBatch);

            _slime.Draw(Core.SpriteBatch, _slimePosition);

            _bat.Draw(Core.SpriteBatch, _batPosition);

            Core.SpriteBatch.DrawString(
                _font,
                $"Score: {_score}",
                _scoreTextPosition,
                Color.White,
                0.0f,
                _scoteTextOrigin,
                1.0f,
                SpriteEffects.None,
                0.0f
            );

            Core.SpriteBatch.End();

            GumService.Default.Draw();
        }

        private void AssignRandomBatVelocity()
        {
            float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);
            Vector2 direction = new Vector2(x, y);

            _batVelocity = direction * MOVEMENT_SPEED;
        }

        private void CheckKeyboardInput()
        {
            KeyboardInfo keyboard = Core.Input.Keyboard;

            if (keyboard.WasKeyJustPressed(Keys.Escape))
            {
                PauseGame();
            }

            float speed = MOVEMENT_SPEED;
            if (keyboard.IsKeyDown(Keys.Space))
            {
                speed *= 1.5f;
            }

            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
            {
                _slimePosition.Y -= speed;
            }

            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
            {
                _slimePosition.Y += speed;
            }

            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
            {
                _slimePosition.X -= speed;
            }

            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
            {
                _slimePosition.X += speed;
            }

            if (keyboard.WasKeyJustPressed(Keys.M))
            {
                Core.Audio.ToggleMute();
            }

            if (keyboard.WasKeyJustPressed(Keys.OemPlus))
            {
                Core.Audio.SongVolume += 0.1f;
                Core.Audio.SoundEffectVolume += 0.1f;
            }

            if (keyboard.WasKeyJustPressed(Keys.OemMinus))
            {
                Core.Audio.SongVolume -= 0.1f;
                Core.Audio.SoundEffectVolume -= 0.1f;
            }
        }

        private void CheckGamePadInput()
        {
            GamePadInfo gamePadOne = Core.Input.GamePads[(int)PlayerIndex.One];

            if (gamePadOne.IsButtonDown(Buttons.Back))
            {
                PauseGame();
            }

            float speed = MOVEMENT_SPEED;
            if (gamePadOne.IsButtonDown(Buttons.A))
            {
                speed *= 1.5f;
                gamePadOne.SetVibration(1.0f, TimeSpan.FromSeconds(1));
            }
            else
            {
                gamePadOne.StopVibration();
            }

            if (gamePadOne.LeftThumbStick != Vector2.Zero)
            {
                _slimePosition.X += gamePadOne.LeftThumbStick.X * speed;
                _slimePosition.Y -= gamePadOne.LeftThumbStick.Y * speed;
            }
            else
            {
                if (gamePadOne.IsButtonDown(Buttons.DPadUp))
                {
                    _slimePosition.Y -= speed;
                }

                if (gamePadOne.IsButtonDown(Buttons.DPadDown))
                {
                    _slimePosition.Y += speed;
                }

                if (gamePadOne.IsButtonDown(Buttons.DPadLeft))
                {
                    _slimePosition.X -= speed;
                }

                if (gamePadOne.IsButtonDown(Buttons.DPadRight))
                {
                    _slimePosition.X += speed;
                }
            }
        }

        private void PauseGame()
        {
            _pausePanel.IsVisible = true;

            _resumeButton.IsFocused = true;
        }

        private void CreatePausePanel()
        {
            _pausePanel = new Panel();
            _pausePanel.Anchor(Anchor.Center);
            _pausePanel.Visual.WidthUnits = DimensionUnitType.Absolute;
            _pausePanel.Visual.HeightUnits = DimensionUnitType.Absolute;
            _pausePanel.Visual.Height = 70;
            _pausePanel.Visual.Width = 264;
            _pausePanel.IsVisible = false;
            _pausePanel.AddToRoot();

            var background = new ColoredRectangleRuntime();
            background.Dock(Dock.Fill);
            background.Color = Color.DarkBlue;
            _pausePanel.AddChild(background);

            var textInstance = new TextRuntime();
            textInstance.Text = "PAUSED";
            textInstance.X = 10f;
            textInstance.Y = 10f;
            _pausePanel.AddChild(textInstance);

            _resumeButton = new Button();
            _resumeButton.Text = "RESUME";
            _resumeButton.Anchor(Anchor.BottomLeft);
            _resumeButton.Visual.X = 9f;
            _resumeButton.Visual.Y = -9f;
            _resumeButton.Visual.Width = 80;
            _resumeButton.Click += HandleResumeButtonClicked;
            _pausePanel.AddChild(_resumeButton);

            var quitButton = new Button();
            quitButton.Text = "QUIT";
            quitButton.Anchor(Anchor.BottomRight);
            quitButton.Visual.X = -9f;
            quitButton.Visual.Y = -9f;
            quitButton.Width = 80;
            quitButton.Click += HandleQuitButtonClicked;
            _pausePanel.AddChild(quitButton);
        }

        private void HandleQuitButtonClicked(object sender, EventArgs e)
        {
            Core.Audio.PlaySoundEffect(_uiSoundEffect);

            Core.ChangeScene(new TitleScene());
        }

        private void HandleResumeButtonClicked(object sender, EventArgs e)
        {
            Core.Audio.PlaySoundEffect(_uiSoundEffect);

            _pausePanel.IsVisible = false;
        }

        private void InitializeUI()
        {
            GumService.Default.Root.Children.Clear();

            CreatePausePanel();
        }
    }
}
