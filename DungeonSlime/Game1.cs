using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using System;

namespace DungeonSlime;

public class Game1 : Core
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

    public Game1() : base("Dungen Slime",1280,720,false )
    {

    }

    protected override void Initialize()
    {
        base.Initialize();

        Rectangle screenBounds = GraphicsDevice.PresentationParameters.Bounds;

        _roomBounds = new Rectangle(
            (int)_tileMap.TileWidth,
            (int)_tileMap.TileHeight,
            screenBounds.Width - (int)_tileMap.TileWidth *2,
            screenBounds.Height - (int)_tileMap.TileHeight * 2
        );

        int centerRow = _tileMap.Rows / 2;
        int centerColumn = _tileMap.Columns / 2;
        _slimePosition = new Vector2(centerColumn * _tileMap.TileWidth, centerRow * _tileMap.TileHeight);

        _batPosition = new Vector2(_roomBounds.Left, _roomBounds.Top);

        AssignRandomBatVelocity();
    }

    protected override void LoadContent()
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

        Song theme = Content.Load<Song>("audio/theme");

        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
        }

        MediaPlayer.Play(theme);

        MediaPlayer.IsRepeating = true;

    }

    protected override void Update(GameTime gameTime)
    {
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

            _bounceSoundEffect.Play();
        }

        _batPosition = newBatPosition;

        if (slimeBounds.Intersects(batBounds))
        {

            int column = Random.Shared.Next(1, _tileMap.Columns - 1);
            int row = Random.Shared.Next(1, _tileMap.Rows - 1);

            _batPosition = new Vector2(column * _bat.Width, row * _bat.Height);

            AssignRandomBatVelocity();

            _collectSoundEffect.Play();
        }

        base.Update(gameTime);
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
        float speed = MOVEMENT_SPEED;
        if (Input.Keyboard.IsKeyDown(Keys.Space))
        {
            speed *= 1.5f;
        }

        if (Input.Keyboard.IsKeyDown(Keys.W) || Input.Keyboard.IsKeyDown(Keys.Up))
        {
            _slimePosition.Y -= speed;
        }

        if (Input.Keyboard.IsKeyDown(Keys.S) || Input.Keyboard.IsKeyDown(Keys.Down))
        {
            _slimePosition.Y += speed;
        }

        if (Input.Keyboard.IsKeyDown(Keys.A) || Input.Keyboard.IsKeyDown(Keys.Left))
        {
            _slimePosition.X -= speed;
        }

        if (Input.Keyboard.IsKeyDown(Keys.D) || Input.Keyboard.IsKeyDown(Keys.Right))
        {
            _slimePosition.X += speed;
        }
    }

    private void CheckGamePadInput()
    {
        GamePadInfo gamePadOne = Input.GamePads[(int)PlayerIndex.One];

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

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _tileMap.Draw(SpriteBatch);

        _slime.Draw(SpriteBatch, _slimePosition);

        _bat.Draw(SpriteBatch, _batPosition);

        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
