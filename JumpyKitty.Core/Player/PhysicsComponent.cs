using Microsoft.Xna.Framework;

namespace JumpyKitty.Core.Player;

internal class PhysicsComponent
{
    public bool IsFalling => Velocity.Y > 0;
    public bool IsJumping => Velocity.Y < 0;
    public Vector2 Velocity;
}
