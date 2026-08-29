using JumpyKitty.Core.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using System;

namespace JumpyKitty.Core.Platforms;

/// <summary>
/// Represents a system that handles the rendering and updating of platforms in the game. Each platform 
/// consists of a grid of blocks, and this system is responsible for drawing each block in the correct 
/// position based on the platform's width and height in blocks. The update method handles disabling them 
/// when they move off-screen. Then they are re-enabled when they are back on screen at the right hand side 
/// of the screen to give the illusion of continuous platforms coming from the right of the screen.
/// </summary>
internal class PlatformSystem : EntityUpdateSystem
{
    private const int _blockSpriteWidth = 64, _blockSpriteHeight = 64;
    private const int _maxNumberOfPlatforms = 10;
    private const int _minimumPlatformWidth = 2, _maximumPlatformWidth = 6;
    private const int _minimumPlatformHeight = 10, _maximumPlatformHeight = 15;
    private const int _randomNextPlatformSpawnTimerMinimumValue = 850, _randomNextPlatformSpawnTimerMaximumValue = 950;
    private const float _scrollSpeed = -450f;

    private readonly Sprite[] _blockSprites = new Sprite[Enum.GetValues<BlockType>().Length];
    private readonly ContentManager _contentManager = default!;
    private float _countdownToNextPlatformSpawn = 0;
    private ComponentMapper<EntityStateComponent> _entityStateMapper = default!;
    private ComponentMapper<MultiplexedSpriteComponent> _multiplexedSpriteMapper = default!;
    private ComponentMapper<PlatformComponent> _platformMapper = default!;
    private readonly Random _randomNumberGenerator = new();
    private Texture2DAtlas _spriteAtlas = default!;
    private readonly SpriteBatch _spriteBatch = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public PlatformSystem(SpriteBatch spriteBatch, ContentManager contentManager) : base(Aspect.All(
        typeof(EntityStateComponent),
        typeof(PlatformComponent),
        typeof(Transform2)))
    {
        _spriteBatch = spriteBatch;
        _contentManager = contentManager;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _entityStateMapper = mapperService.GetMapper<EntityStateComponent>();
        _multiplexedSpriteMapper = mapperService.GetMapper<MultiplexedSpriteComponent>();
        _platformMapper = mapperService.GetMapper<PlatformComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Load the sprite atlas for the platform blocks        
        var texture = _contentManager.Load<Texture2D>("Platforms/Blocks");
        _spriteAtlas = Texture2DAtlas.Create("Atlas/Blocks", texture, _blockSpriteWidth, _blockSpriteHeight);

        // Get the sprites for the top and bottom of the platform from the sprite atlas        
        _blockSprites[(int)BlockType.TopLeft] = _spriteAtlas.CreateSprite(0);
        _blockSprites[(int)BlockType.TopMiddle] = _spriteAtlas.CreateSprite(1);
        _blockSprites[(int)BlockType.TopRight] = _spriteAtlas.CreateSprite(2);
        _blockSprites[(int)BlockType.LeftTrunk] = _spriteAtlas.CreateSprite(3);
        _blockSprites[(int)BlockType.MiddleTrunk] = _spriteAtlas.CreateSprite(4);
        _blockSprites[(int)BlockType.RightTrunk] = _spriteAtlas.CreateSprite(5);

        // Create several Platform entities to use and re-use
        for (int i = 0; i < _maxNumberOfPlatforms; i++)
        {
            var entity = CreateEntity();

            // Add the necessary components to the entity
            entity.Attach(new EntityStateComponent { State = EntityState.Disabled });
            entity.Attach(new MultiplexedSpriteComponent());
            entity.Attach(new PlatformComponent());
            entity.Attach(new Transform2());
        }
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var timeToRespawnAnotherPlatform = false;

        // Framerate independent timer
        _countdownToNextPlatformSpawn -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        // If we've reached 0 then spawn the next Platform
        if (_countdownToNextPlatformSpawn < 0)
        {
            // Reset the timer to some random value
            _countdownToNextPlatformSpawn = _randomNumberGenerator.Next(
                _randomNextPlatformSpawnTimerMinimumValue,
                _randomNextPlatformSpawnTimerMaximumValue);

            // Now spawn a Platform ;-)
            timeToRespawnAnotherPlatform = true;
        }

        foreach (var entityId in ActiveEntities)
        {
            var entityStateComponent = _entityStateMapper.Get(entityId);
            var multiplexedSpriteComponent = _multiplexedSpriteMapper.Get(entityId);
            var platformComponent = _platformMapper.Get(entityId);
            var transformComponent = _transformMapper.Get(entityId);

            // Is this Platform disabled and ready to be respawned?
            if (timeToRespawnAnotherPlatform && entityStateComponent.State == EntityState.Disabled)
            {
                // Yes, but we only want to respawn one Platform at a
                // time, so set the flag to false now
                timeToRespawnAnotherPlatform = false;

                // Reset the Platform's state back to 'alive', set its dimensions
                // and position to the right hand side of the screen ready for
                // it to move left across the screen
                ResetPlatform(entityStateComponent, transformComponent, platformComponent, multiplexedSpriteComponent);
            }

            // Is this Platform disabled? If so, we don't want to draw it or move it...
            if (entityStateComponent.State == EntityState.Disabled)
                continue;

            // Otherwise this Platform is still on screen so update it
            UpdatePlatform(_scrollSpeed * deltaTime, transformComponent, platformComponent, multiplexedSpriteComponent);

            // Has this Platform moved off the left hand side of the screen? If so, disable it
            if (transformComponent.Position.X + platformComponent.WidthInBlocks * _blockSpriteWidth < 0)
            {
                entityStateComponent.State = EntityState.Disabled;
                continue;
            }
        }
    }

    private static void UpdatePlatform(float velocity, Transform2 transformComponent, PlatformComponent platformComponent, MultiplexedSpriteComponent multiplexedSpriteComponent)
    {
        // Move the Platform left across the screen based on the speed
        var translation = new Vector2(velocity, 0);
        transformComponent.Position += translation;

        // Each Platform consists of a grid of blocks, so we need to 
        // update each blocks individual position based on the speed too
        for (var i = 0; i < multiplexedSpriteComponent.Transforms.Length; i++)
            multiplexedSpriteComponent.Transforms[i].Position += translation;
    }

    private void ResetPlatform(EntityStateComponent entityStateComponent, Transform2 transformComponent, PlatformComponent platformComponent, MultiplexedSpriteComponent multiplexedSpriteComponent)
    {
        // Re-enable the Platform and set its position to the right hand side of the screen
        entityStateComponent.State = EntityState.Alive;

        // Randomly set the Platform's width and height in blocks
        platformComponent.WidthInBlocks = _randomNumberGenerator.Next(_minimumPlatformWidth, _maximumPlatformWidth + 1);
        platformComponent.HeightInBlocks = _randomNumberGenerator.Next(_minimumPlatformHeight, _maximumPlatformHeight + 1);

        // Set the position of this platform to the right hand side of the screen, and
        // at the bottom of the screen minus the height of the platform in blocks
        transformComponent.Position = new Vector2(
            _spriteBatch.GraphicsDevice.Viewport.Bounds.Right, 
            _spriteBatch.GraphicsDevice.Viewport.Bounds.Bottom - platformComponent.HeightInBlocks * _blockSpriteHeight);
        
        // Each Platform consists of a grid of blocks, so we need to draw each block in
        // the correct position based on the Platform's width and height in blocks                
        multiplexedSpriteComponent.Sprites = new Sprite[platformComponent.WidthInBlocks * platformComponent.HeightInBlocks];
        multiplexedSpriteComponent.Transforms = new Transform2[platformComponent.WidthInBlocks * platformComponent.HeightInBlocks];

        // Each Platform consists of a grid of blocks, so we need to draw each block in
        // the correct position based on the Platform's width and height in blocks                
        for (int x = 0; x < platformComponent.WidthInBlocks; x++)
        {
            for (int y = 0; y < platformComponent.HeightInBlocks; y++)
            {
                BlockType currentBlockType;

                // Are we drawing the top of the tree?
                if (y == 0)
                {
                    // Yes, so we draw a left side, middle, and right side
                    // block depending on the x position
                    if (x == 0) currentBlockType = BlockType.TopLeft;
                    else if (x == platformComponent.WidthInBlocks - 1) currentBlockType = BlockType.TopRight;
                    else currentBlockType = BlockType.TopMiddle;
                }
                else
                {
                    // No, we're drawing the trunk... so we draw a left side, middle, and
                    // right side block depending on the x position
                    if (x == 0) currentBlockType = BlockType.LeftTrunk;
                    else if (x == platformComponent.WidthInBlocks - 1) currentBlockType = BlockType.RightTrunk;
                    else currentBlockType = BlockType.MiddleTrunk;
                }

                // Get the correct sprite for the current block type and draw it at the correct position
                var sprite = _blockSprites[(int)currentBlockType];
                var transform = new Transform2(transformComponent.Position + new Vector2(_blockSpriteWidth * x, _blockSpriteHeight * y));

                // Draw the sprite at the correct position based on the Platform's position and the block's position within the Platform
                multiplexedSpriteComponent.Sprites[y * platformComponent.WidthInBlocks + x] = sprite;
                multiplexedSpriteComponent.Transforms[y * platformComponent.WidthInBlocks + x] = transform;
            }
        }
    }
}
