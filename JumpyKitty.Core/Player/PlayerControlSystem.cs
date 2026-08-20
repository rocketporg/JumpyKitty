using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace JumpyKitty.Core.Player;

internal class PlayerControlSystem : EntityProcessingSystem
{
    private ComponentMapper<PlayerComponent> _playerMapper = default!;

    public PlayerControlSystem() : base(Aspect.All(typeof(PlayerComponent))) { }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _playerMapper = mapperService.GetMapper<PlayerComponent>();
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        // Get the states of the various input devices
        var gamePadState = GamePad.GetState(PlayerIndex.One);
        var keyboardState = Keyboard.GetState();
        var touchPanelState = TouchPanel.GetState();

        // Deal with touch input..
        var touching = touchPanelState.Count > 0;

        // Get references for our components
        var playerComponent = _playerMapper.Get(entityId);

        // If the player has pressed the jump button, set the JumpPressed flag to true
        playerComponent.JumpPressed = keyboardState.IsKeyDown(Keys.Space)
            || gamePadState.Buttons.A == ButtonState.Pressed
            || touching;
    }
}