using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;

namespace JumpyKitty.Core.Screens;

internal class TitleScreen : GameScreen
{
    public TitleScreen(Game game) : base(game)
    {
        DrawWhenInactive = false;
        UpdateWhenInactive = false;
    }

    public override void Draw(GameTime gameTime)
    {
        // TODO: draw title screen stuff
    }

    public override void Update(GameTime gameTime)
    {
    }
}
