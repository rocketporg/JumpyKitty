using JumpyKitty.Core.Shared;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace JumpyKitty.Core.Player;

internal class JumpSystem : EntityProcessingSystem
{
    private ComponentMapper<JumpComponent> _jumpMapper = default!;
    private ComponentMapper<VelocityComponent> _physicsMapper = default!;
    private ComponentMapper<PlayerComponent> _playerMapper = default!;

    public JumpSystem() : base(Aspect.All(
        typeof(JumpComponent),
        typeof(VelocityComponent),
        typeof(PlayerComponent))) { }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _jumpMapper = mapperService.GetMapper<JumpComponent>();
        _physicsMapper = mapperService.GetMapper<VelocityComponent>();
        _playerMapper = mapperService.GetMapper<PlayerComponent>();    
    }

    public override void Process(GameTime gameTime, int entityId)
    {                
        var playerComponent = _playerMapper.Get(entityId);

        // Player jumping?
        if (playerComponent.JumpPressed && playerComponent.IsOnGround)
        {
            var jumpComponent = _jumpMapper.Get(entityId);
            var physicsComponent = _physicsMapper.Get(entityId);
            physicsComponent.Velocity += new Vector2(0, jumpComponent.JumpStrength);

            //_jumpSound.Play();
        }
    }
}
