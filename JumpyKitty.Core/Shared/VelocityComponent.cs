using Microsoft.Xna.Framework;

namespace JumpyKitty.Core.Shared;

internal class VelocityComponent
{
    public bool IsFalling => Velocity.Y > 0;
    public bool IsJumping => Velocity.Y < 0;
    public Vector2 Velocity;
}
