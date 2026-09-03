using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace JumpyKitty.Core.Shared;

internal class InputService
{
    private KeyboardState _keyboardState;
    private TouchCollection _touchPanelState;
    private bool _tapped = false;
    private bool _touching = false;

    public bool IsJumpPressed => Keyboard.GetState().IsKeyDown(Keys.Space)
        || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed
        || _touching;

    public bool IsPausePressed => _keyboardState.IsKeyDown(Keys.P);

    public bool ScreenHasBeenTapped => _tapped || Keyboard.GetState().IsKeyDown(Keys.Space);

    public void Initialise() => _tapped = false;

    public void Poll()
    {
        _keyboardState = Keyboard.GetState();
        _touchPanelState = TouchPanel.GetState();

        if (_touchPanelState.Count > 0)
        {
            _touching = true;
            _tapped = true;
        }
        else _touching = false;
    }
}
