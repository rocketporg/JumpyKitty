using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace JumpyKitty.Core.Shared;

internal class BoundingBoxSystem : EntityProcessingSystem
{
    private ComponentMapper<BoundingBoxComponent> _boundingBoxMapper = default!;
    private ComponentMapper<SizeComponent> _sizeMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public BoundingBoxSystem() : base(Aspect.All(
        typeof(BoundingBoxComponent),
        typeof(SizeComponent),
        typeof(Transform2)))
    { }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _boundingBoxMapper = mapperService.GetMapper<BoundingBoxComponent>();
        _sizeMapper = mapperService.GetMapper<SizeComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        var boundingBoxComponent = _boundingBoxMapper.Get(entityId);
        var sizeComponent = _sizeMapper.Get(entityId);
        var transformComponent = _transformMapper.Get(entityId);

        boundingBoxComponent.BoundingBox = new RectangleF(
            transformComponent.Position.X,
            transformComponent.Position.Y,
            sizeComponent.Width,
            sizeComponent.Height
        );
    }
}
