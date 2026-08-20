using JumpyKitty.Core.Player;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpyKitty.Core.Physics;

internal class PhysicsSystem : EntityUpdateSystem
{    
    private ComponentMapper<PhysicsComponent> _physicsMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public PhysicsSystem() : base(Aspect.All(typeof(PhysicsComponent), typeof(Transform2))) { }

    public override void Initialize(IComponentMapperService mapperService)
    {        
        _physicsMapper = mapperService.GetMapper<PhysicsComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();
    }

    public override void Update(GameTime gameTime)
    {        
        // https://community.monogame.net/t/acceleration-and-friction-in-2d-games/9319/9
        // https://community.monogame.net/t/fps-indepedent-jump/14515/7
        // https://gamedev.stackexchange.com/questions/146331/inconsistent-jump-height

        // Get the delta time
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Apply gravity to the falling players velocity
        //float modifiedGravity = Gravity;

        //if (IsJumping && !_controlSystem.IsJumpPressed)
        //{
        //    // If the player is jumping and not holding the jump button, reduce
        //    // the jump a bit quicker so they just do a small/low jump. Then
        //    // if they are holding the jump button they'll do a normal 'full' jump
        //    modifiedGravity = Gravity * LowJumpGravityMultiplier;
        //}
        //else if (IsFalling)
        //{
        //    // When falling, we can tweak the fall speed a little to make
        //    // the player drop faster than normal gravity would make them...
        //    modifiedGravity = Gravity * FallingGravityMultiplier;
        //}

        //// Sort out frame rate independent movement
        //if (Velocity.Y < 0 && Velocity.Y + (modifiedGravity * deltaTime) >= 0)
        //{
        //    float t = Velocity.Y / modifiedGravity;
        //    float y = (-0.5f * modifiedGravity * (t * t)) + (Velocity.Y * t);

        //    Velocity = new Vector2(Velocity.X, -y);
        //    Position += new Vector2(Velocity.X * deltaTime, Velocity.Y);
        //    Velocity += new Vector2(0, modifiedGravity * deltaTime);
        //}
        //else
        //{
        //    Velocity += new Vector2(0, modifiedGravity * deltaTime * 0.5f);
        //    Position += Velocity * deltaTime;
        //    Velocity += new Vector2(0, modifiedGravity * deltaTime * 0.5f);
        //}
    }
}
