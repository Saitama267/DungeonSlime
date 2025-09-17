using DungeonSlime.Scenes;
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
    private Song _themeSong;

    public Game1() : base("Dungen Slime",1280,720,false )
    {

    }

    protected override void Initialize()
    {
        base.Initialize();

        Audio.PlaySong(_themeSong);

        ChangeScene(new TitleScene());
    }

    protected override void LoadContent()
    {
        _themeSong = Content.Load<Song>("audio/theme");
    }
}
