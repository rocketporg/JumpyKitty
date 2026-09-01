using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace JumpyKitty.Core.Player;

internal class DeathSystem : EntityProcessingSystem
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly PlayerService _playerService;    
    private ComponentMapper<Transform2> _transformMapper = default!;

    public DeathSystem(PlayerService playerService, GraphicsDevice graphicsDevice) : base(Aspect.All(typeof(PlayerComponent)))
    {
        _playerService = playerService;
        _graphicsDevice = graphicsDevice;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _transformMapper = mapperService.GetMapper<Transform2>();
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        var transformComponent = _transformMapper.Get(entityId);

        if (transformComponent.Position.X < 0 || transformComponent.Position.Y > _graphicsDevice.Viewport.Height)
            _playerService.Die();
    }    
}
