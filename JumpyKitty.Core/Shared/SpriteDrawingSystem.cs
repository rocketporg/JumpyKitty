using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;

namespace JumpyKitty.Core.Shared;

internal class SpriteDrawingSystem : EntityDrawSystem, IUpdateSystem
{
    private readonly OrthographicCamera _camera;
    private ComponentMapper<EntityStateComponent> _entityStateMapper = default!;
    private ComponentMapper<MultiplexedSpriteComponent> _multiplexedSpriteMapper = default!;
    private readonly SpriteBatch _spriteBatch;
    private ComponentMapper<Sprite> _spriteMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public SpriteDrawingSystem(SpriteBatch spriteBatch, OrthographicCamera camera) : base(Aspect
        .All(typeof(EntityStateComponent))
        .One(typeof(Sprite), typeof(MultiplexedSpriteComponent)))
    {
        _spriteBatch = spriteBatch;
        _camera = camera;
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(
            sortMode: SpriteSortMode.Immediate,
            blendState: null,
            samplerState: SamplerState.PointClamp,
            depthStencilState: null,
            rasterizerState: null,
            effect: null,
            transformMatrix: _camera.GetViewMatrix());

        foreach (var entityId in ActiveEntities)
        {
            // Get our component reference for this entity            
            var entityStateComponent = _entityStateMapper.Get(entityId);

            // Skip if this entity is not 'alive'...
            if (!entityStateComponent.IsAlive)
                continue;

            // Otherwise, check if this is a multiplexed sprite or a single sprite and draw accordingly
            if (_multiplexedSpriteMapper.Has(entityId))
            {
                // This is a multiplexed sprite, so draw all of its 'child' sprites
                var multiplexedSpriteComponent = _multiplexedSpriteMapper.Get(entityId);

                for (int i = 0; i < multiplexedSpriteComponent.Sprites.Length; i++)
                {
                    _spriteBatch.Draw(multiplexedSpriteComponent.Sprites[i], multiplexedSpriteComponent.Transforms[i]);
                }
            }
            else
            {
                // This is just a normal sprite, so draw it normally
                var spriteComponent = _spriteMapper.Get(entityId);
                var transformComponent = _transformMapper.Get(entityId);

                _spriteBatch.Draw(spriteComponent, transformComponent);
            }
        }

        _spriteBatch.End();
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _entityStateMapper = mapperService.GetMapper<EntityStateComponent>();
        _multiplexedSpriteMapper = mapperService.GetMapper<MultiplexedSpriteComponent>();
        _spriteMapper = mapperService.GetMapper<Sprite>();
        _transformMapper = mapperService.GetMapper<Transform2>();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var entityId in ActiveEntities)
        {
            // Get references for our components            
            var spriteComponent = _spriteMapper.Get(entityId);

            // Update animation if its an animated sprite
            if (spriteComponent is AnimatedSprite sprite)
                sprite.Update(gameTime);
        }
    }
}
