using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace JumpyKitty.Core.Shared;

internal class CameraSystem : EntityProcessingSystem
{
    private readonly OrthographicCamera _camera;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public CameraSystem(OrthographicCamera camera) : base(Aspect.All(typeof(Transform2)))
    {
        _camera = camera;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Get component mapper services        
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Set zoom limits just in case
        _camera.MinimumZoom = 0.5f; // Restrict zoom out to 50%
        _camera.MaximumZoom = 2f;   // Restrict zoom in to 200%
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        var transformComponent = _transformMapper.Get(entityId);
    }
}
