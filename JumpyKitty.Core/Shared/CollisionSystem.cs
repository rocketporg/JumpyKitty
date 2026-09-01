using JumpyKitty.Core.Extensions;
using JumpyKitty.Core.Player;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using System.Linq;

namespace JumpyKitty.Core.Shared;

internal class CollisionSystem : EntityUpdateSystem
{
    private ComponentMapper<BoundingBoxComponent> _boundingBoxMapper = default!;
    private ComponentMapper<VelocityComponent> _physicsMapper = default!;
    private int _playerEntityId = default!;
    private ComponentMapper<PlayerComponent> _playerMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public CollisionSystem() : base(Aspect.One(typeof(BoundingBoxComponent))) { }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _boundingBoxMapper = mapperService.GetMapper<BoundingBoxComponent>();
        _physicsMapper = mapperService.GetMapper<VelocityComponent>();
        _playerMapper = mapperService.GetMapper<PlayerComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Find the player entity id
        _playerEntityId = ActiveEntities.Single(entityId => _playerMapper.Has(entityId));
    }

    public override void Update(GameTime gameTime)
    {
        // Reset flags at the start
        var playerComponent = _playerMapper.Get(_playerEntityId);
        playerComponent.IsOnGround = false;

        // Get the players physics and transform components so we can adjust velocity if needed
        var playerPhysicsComponent = _physicsMapper.Get(_playerEntityId);
        var playerTransformComponent = _transformMapper.Get(_playerEntityId);

        // Get players current bounding box
        var playerBoundingBox = _boundingBoxMapper.Get(_playerEntityId).BoundingBox;

        // Now check all the platform entities for collisions with the player
        foreach (var entityId in ActiveEntities)
        {
            // Skip the player entity itself, we only want to check for collisions with platforms
            if (entityId == _playerEntityId)
                continue;

            var platformBoundingBoxComponent = _boundingBoxMapper.Get(entityId);

            // Get any intersection between the player and this platform                
            var currentPositionIntersection = playerBoundingBox.GetIntersectionDepth(platformBoundingBoxComponent.BoundingBox);

            // If there's no collision with this platform then just contine the loop...
            if (currentPositionIntersection == Vector2.Zero)
                continue;

            // If we're here then there is a collision, so get intersection depths
            var intersectionDepthX = Math.Abs(currentPositionIntersection.X);
            var intersectionDepthY = Math.Abs(currentPositionIntersection.Y);

            // Shallow axis first...
            if (intersectionDepthY < intersectionDepthX)
            {
                // If our 'feet' are inside a tree, then we're on the ground otherwise not
                if (playerBoundingBox.Bottom > platformBoundingBoxComponent.BoundingBox.Top)
                {
                    // Set flag so we can know if the players on the ground or
                    // not later on (i.e. for things like jumping or not ;-)
                    playerComponent.IsOnGround = true;

                    // Score a point for landing on the tree
                    //if (_lastTreeJumpedOn != entity.ID)
                    //{
                    //    _scoreSystem.AddPointsToScore(1);
                    //    _lastTreeJumpedOn = entity.ID;
                    //}

                    // Adjust position so we're outside the tree in the Y axis
                    playerTransformComponent.Position -= new Vector2(0, (int)Math.Round(intersectionDepthY));
                    playerPhysicsComponent.Velocity = new Vector2(playerPhysicsComponent.Velocity.X, 0);
                }
            }
            else
            {
                // Are we bumping a side of this tree?
                if (playerBoundingBox.Right > platformBoundingBoxComponent.BoundingBox.Left || playerBoundingBox.Left < platformBoundingBoxComponent.BoundingBox.Right)
                {
                    // Adjust position so we're outside the tree in the X axis
                    playerTransformComponent.Position -= new Vector2((int)Math.Round(intersectionDepthX), 0);
                    playerPhysicsComponent.Velocity = new Vector2(0, playerPhysicsComponent.Velocity.Y);
                }
            }

            // If we're here then there was a collision, so we can now break out of the loop
            break;
        }
    }
}
