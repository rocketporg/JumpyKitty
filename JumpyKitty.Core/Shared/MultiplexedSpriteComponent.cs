using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace JumpyKitty.Core.Shared;

internal class MultiplexedSpriteComponent
{
    public Sprite[] Sprites = default!;
    public Transform2[] Transforms = default!;
}
