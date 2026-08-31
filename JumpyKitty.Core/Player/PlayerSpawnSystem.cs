using JumpyKitty.Core.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;

namespace JumpyKitty.Core.Player;

internal class PlayerSpawnSystem : EntityProcessingSystem
{
    private readonly ContentManager _contentManager;
    private Texture2D _texture = default!;

    public PlayerSpawnSystem(ContentManager contentManager) : base(Aspect.All(typeof(PlayerComponent)))
    {
        _contentManager = contentManager;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Load player sprite texture        
        _texture = _contentManager.Load<Texture2D>("Characters/Kitty");

        // Create player entity and attach components
        var entity = CreateEntity();
        entity.Attach(new BoundingBoxComponent());
        entity.Attach(new EntityStateComponent { State = EntityState.Alive });
        entity.Attach(new JumpComponent());
        entity.Attach(new PhysicsComponent());
        entity.Attach(new PlayerComponent());
        entity.Attach(new SizeComponent { Width = _texture.Width, Height = _texture.Height });
        entity.Attach(new Sprite(_texture));
        entity.Attach(new Transform2 { Position = new Vector2(200, 400) });
    }

    public override void Process(GameTime gameTime, int entityId)
    {
    }
}
