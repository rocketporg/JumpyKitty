using JumpyKitty.Core.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;

namespace JumpyKitty.Core.Background;

internal class WaterSystem : EntityUpdateSystem, IDrawSystem
{
    private readonly ContentManager _contentManager;
    private int _numberOfBlocks = default!;
    private readonly SpriteBatch _spriteBatch;
    private ComponentMapper<Transform2> _transformMapper = default!;
    private ComponentMapper<VelocityComponent> _velocityMapper = default!;
    private Color _waterColour = new(Color.White, 0.5f);
    private Texture2D _waterTexture = default!;

    public WaterSystem(ContentManager contentManager, SpriteBatch spriteBatch) : base(Aspect.All(typeof(WaterblockComponent)))
    {
        _contentManager = contentManager;
        _spriteBatch = spriteBatch;
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();

        foreach (var entityId in ActiveEntities)
        {
            var transformComponent = _transformMapper.Get(entityId);

            _spriteBatch.Draw(_waterTexture, transformComponent.Position, _waterColour);
        }

        _spriteBatch.End();
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _transformMapper = mapperService.GetMapper<Transform2>();
        _velocityMapper = mapperService.GetMapper<VelocityComponent>();

        // Load texture for the water blocks
        _waterTexture = _contentManager.Load<Texture2D>("Backgrounds/Water");

        // Create some entities for the water blocks
        _numberOfBlocks = 4 + (_spriteBatch.GraphicsDevice.Viewport.Width / _waterTexture.Width);

        for (var blockNumber = 0; blockNumber < _numberOfBlocks; blockNumber++)
        {
            var entity = CreateEntity();
            entity.Attach(new Sprite(_waterTexture));
            entity.Attach(new VelocityComponent { Velocity = new Vector2(-450, 0) });
            entity.Attach(new WaterblockComponent());
            entity.Attach(new Transform2 { Position = new Vector2(blockNumber * _waterTexture.Width, _spriteBatch.GraphicsDevice.Viewport.Height - _waterTexture.Height) });
        }
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = gameTime.GetElapsedSeconds();

        foreach (var entityId in ActiveEntities)
        {
            var transformComponent = _transformMapper.Get(entityId);
            var velocityComponent = _velocityMapper.Get(entityId);

            // Update block position
            transformComponent.Position += velocityComponent.Velocity * deltaTime;

            // Move the waterblock back to the right side of the screen when
            // its moved off the left hand side of the screen
            if (transformComponent.Position.X < -_waterTexture.Width)
                transformComponent.Position = new Vector2(transformComponent.Position.X + _numberOfBlocks * _waterTexture.Width, transformComponent.Position.Y);
        }
    }
}
