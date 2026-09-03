using JumpyKitty.Core.Shared;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace JumpyKitty.Core.Player;

internal class PlayerControlSystem : EntityProcessingSystem
{
    private readonly InputService _inputService;
    private ComponentMapper<PlayerComponent> _playerMapper = default!;

    public PlayerControlSystem(InputService inputService) : base(Aspect.All(typeof(PlayerComponent)))
    {
        _inputService = inputService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _playerMapper = mapperService.GetMapper<PlayerComponent>();
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        // Get references for our components
        var playerComponent = _playerMapper.Get(entityId);

        // If the player has pressed the jump button, set the JumpPressed flag to true
        _inputService.Poll();
        playerComponent.JumpPressed = _inputService.IsJumpPressed;
    }
}