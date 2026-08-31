using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace JumpyKitty.Core.Player;

internal class PhysicsSystem : EntityProcessingSystem
{
    private const float _fallingGravityMultiplier = 2.25f;
    private const float _gravity = 3100f;
    private const float _lowJumpGravityMultiplier = 2.75f;

    private ComponentMapper<PhysicsComponent> _physicsMapper = default!;
    private ComponentMapper<PlayerComponent> _playerMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public PhysicsSystem() : base(Aspect.All(typeof(PlayerComponent))) { }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _physicsMapper = mapperService.GetMapper<PhysicsComponent>();
        _playerMapper = mapperService.GetMapper<PlayerComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        // Get the delta time
        var deltaTime = gameTime.GetElapsedSeconds();

        // Get our components for the entity we're processing
        var physicsComponent = _physicsMapper.Get(entityId);
        var playerComponent = _playerMapper.Get(entityId);
        var transformComponent = _transformMapper.Get(entityId);

        // Start with normal gravity...
        float modifiedGravity = _gravity;

        // Modify gravity slightly depending on what the player is doing...
        if (physicsComponent.IsJumping && !playerComponent.JumpPressed)
        {
            // If the player is jumping, but NOT holding the jump button, reduce
            // the jump a bit quicker so they just do a small/low jump. Then
            // if they are holding the jump button they'll do a normal 'full' jump
            modifiedGravity = _gravity * _lowJumpGravityMultiplier;
        }
        else if (physicsComponent.IsFalling)
        {
            // When falling, we can tweak the fall speed a little to make
            // the player drop faster than normal gravity would make them...
            modifiedGravity = _gravity * _fallingGravityMultiplier;
        }

        // Sort out frame rate independent movement and apply the modified
        // gravity to the player. This is a bit of a hack, but it works well
        // enough for our purposes.
        if (physicsComponent.Velocity.Y < 0 && physicsComponent.Velocity.Y + (modifiedGravity * deltaTime) >= 0)
        {
            float t = physicsComponent.Velocity.Y / modifiedGravity;
            float y = (-0.5f * modifiedGravity * (t * t)) + (physicsComponent.Velocity.Y * t);

            physicsComponent.Velocity = new Vector2(physicsComponent.Velocity.X, -y);
            transformComponent.Position += new Vector2(physicsComponent.Velocity.X * deltaTime, physicsComponent.Velocity.Y * deltaTime);
            physicsComponent.Velocity += new Vector2(0, modifiedGravity * deltaTime);
        }
        else
        {
            physicsComponent.Velocity += new Vector2(0, modifiedGravity * deltaTime * 0.5f);
            transformComponent.Position += physicsComponent.Velocity * deltaTime;
            physicsComponent.Velocity += new Vector2(0, modifiedGravity * deltaTime * 0.5f);
        }
    }
}
