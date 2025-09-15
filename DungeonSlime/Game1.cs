using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

    public Game1() : base("Dungen Slime",1280,720,false )
    {

    }

    protected override void Initialize()
    {
        base.Initialize();

        _batPosition = new Vector2(_slime.Width + 10, 0);

        AssignRandomBatVelocity();
    }

    protected override void LoadContent()
    {
        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
        
        _slime = atlas.CreateAnimatedSprite("slime-animation");
        _slime.Scale = new Vector2(4.0f, 4.0f);
        
        _bat = atlas.CreateAnimatedSprite("bat-animation");
        _bat.Scale = new Vector2(4.0f, 4.0f);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _slime.Update(gameTime);

        _bat.Update(gameTime);

        CheckKeyboardInput();

        CheckGamePadInput();

        Rectangle screenBounds = new Rectangle(
            0,
            0,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight
        );

        Circle slimeBounds = new Circle(
            (int)(_slimePosition.X + (_slime.Width * 0.5f)),
            (int)(_slimePosition.Y + (_slime.Height * 0.5f)),
            (int)(_slime.Width * 0.5f)
        );

        if (slimeBounds.Left < screenBounds.Left)
        {
            _slimePosition.X = screenBounds.Left;
        }
        else if (slimeBounds.Right > screenBounds.Right)
        {
            _slimePosition.X = screenBounds.Right - _slime.Width;
        }

        if (slimeBounds.Top < screenBounds.Top)
        {
            _slimePosition.Y = screenBounds.Top;
        }
        else if (slimeBounds.Bottom > screenBounds.Bottom)
        {
            _slimePosition.Y = screenBounds.Bottom - _slime.Height;
        }

        Vector2 newBatPosition = _batPosition + _batVelocity;

        Circle batBounds = new Circle(
            (int)(newBatPosition.X + (_bat.Width * 0.5f)),
            (int)(newBatPosition.Y + (_bat.Height * 0.5f)),
            (int)(_bat.Width * 0.5f)
        );

        Vector2 normal = Vector2.Zero;

        if (batBounds.Left < screenBounds.Left)
        {
            normal.X = Vector2.UnitX.X;
            newBatPosition.X = screenBounds.Left;
        }
        else if (batBounds.Right > screenBounds.Right)
        {
            normal.X = -Vector2.UnitX.X;
            newBatPosition.X = screenBounds.Right - _slime.Width;
        }

        if (batBounds.Top < screenBounds.Top)
        {
            normal.Y = Vector2.UnitY.Y;
            newBatPosition.Y = screenBounds.Top;
        }
        else if (batBounds.Bottom > screenBounds.Bottom)
        {
            normal.Y = -Vector2.UnitY.Y;
            newBatPosition.Y = screenBounds.Bottom - _slime.Height;
        }

        if (normal != Vector2.Zero)
        {
            normal.Normalize();
            _batVelocity = Vector2.Reflect(_batVelocity, normal);
        }

        _batPosition = newBatPosition;

        if (slimeBounds.Intersects(batBounds))
        {
            int totalColums = GraphicsDevice.PresentationParameters.BackBufferWidth / (int)_bat.Width;
            int totalRows = GraphicsDevice.PresentationParameters.BackBufferHeight / (int)_bat.Height;

            int column = Random.Shared.Next(0, totalColums);
            int row = Random.Shared.Next(0, totalRows);

            _batPosition = new Vector2(column * _bat.Width, row * _bat.Height);

            AssignRandomBatVelocity();
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

        _slime.Draw(SpriteBatch, _slimePosition);

        _bat.Draw(SpriteBatch, _batPosition);

        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
