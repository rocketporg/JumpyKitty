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
/// position based on the platform's width and height in blocks. It uses a PillarFactory to create pillar 
/// entities and manages their lifecycle, including disabling them when they move off-screen. Then they
/// are re-enabled when they are back on screen at the right hand side of the screen to give the illusion 
/// of continuous platforms.
/// </summary>
internal class PlatformSystem : EntityUpdateSystem //, IDrawSystem
{
    private const int _maxNumberOfPillars = 10;
    private const int _minimumPillarWidth = 2, _maximumPillarWidth = 6;
    private const int _minimumPillarHeight = 10, _maximumPillarHeight = 15;
    private const int _randomNextPillarSpawnTimerMinimumValue = 500, _randomNextPillarSpawnTimerMaximumValue = 725;

    private Sprite[] _blockSprites = new Sprite[Enum.GetValues<BlockType>().Length];
    private int _blockSpriteWidth = 0, _blockSpriteHeight = 0;
    private readonly ContentManager _contentManager = default!;
    private float _countdownToNextPillarSpawn = 0;
    private ComponentMapper<EntityStateComponent> _entityStateMapper = default!;
    private ComponentMapper<MultiplexedSpriteComponent> _multiplexedSpriteMapper = default!;
    private ComponentMapper<PlatformComponent> _platformMapper = default!;    
    private readonly Random _randomNumberGenerator = new();
    private Texture2DAtlas _spriteAtlas = default!;
    private readonly SpriteBatch _spriteBatch = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public PlatformSystem(SpriteBatch spriteBatch, ContentManager contentManager) : base(Aspect.All(typeof(EntityStateComponent), typeof(PlatformComponent), typeof(Transform2)))
    {
        _spriteBatch = spriteBatch;
        _contentManager = contentManager;
    }

    //public void Draw(GameTime gameTime)
    //{
    //    foreach (var entityId in ActiveEntities)
    //    {
    //        var entityState = _entityStateMapper.Get(entityId);

    //        // Skip drawing this pillar if it is disabled, as it has moved off
    //        // the left hand side of the screen and is waiting to be re-used
    //        if (entityState.State == EntityState.Disabled)
    //            continue;

    //        // Otherwise, draw the pillar at its current position
    //        var multiplexedSpriteComponent = _multiplexedSpriteMapper.Get(entityId);
    //        var platformComponent = _platformMapper.Get(entityId);
    //        var transformComponent = _transformMapper.Get(entityId);

    //        DrawPillar(transformComponent, platformComponent, multiplexedSpriteComponent);
    //    }
    //}

    public override void Initialize(IComponentMapperService mapperService)
    {
        _entityStateMapper = mapperService.GetMapper<EntityStateComponent>();
        _multiplexedSpriteMapper = mapperService.GetMapper<MultiplexedSpriteComponent>();
        _platformMapper = mapperService.GetMapper<PlatformComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Load the sprite atlas for the platform blocks        
        var texture = _contentManager.Load<Texture2D>("Platforms/Blocks");
        _spriteAtlas = Texture2DAtlas.Create("Atlas/Blocks", texture, 64, 64);

        // Get the sprites for the top and bottom of the platform from the sprite atlas        
        _blockSprites[(int)BlockType.TopLeft] = _spriteAtlas.CreateSprite(0);
        _blockSprites[(int)BlockType.TopMiddle] = _spriteAtlas.CreateSprite(1);
        _blockSprites[(int)BlockType.TopRight] = _spriteAtlas.CreateSprite(2);
        _blockSprites[(int)BlockType.LeftTrunk] = _spriteAtlas.CreateSprite(3);
        _blockSprites[(int)BlockType.MiddleTrunk] = _spriteAtlas.CreateSprite(4);
        _blockSprites[(int)BlockType.RightTrunk] = _spriteAtlas.CreateSprite(5);

        // Create several pillar entities to use and re-use
        for (int i = 0; i < _maxNumberOfPillars; i++)
        {
            var entity = CreateEntity();

            // Add the necessary components to the entity
            entity.Attach(new EntityStateComponent { State = EntityState.Alive });
            entity.Attach(new MultiplexedSpriteComponent());            
            
            // Create a new platform and set its width, and attach component to the entity
            entity.Attach(new PlatformComponent
            {                
                WidthInBlocks = _randomNumberGenerator.Next(_minimumPillarWidth, _maximumPillarWidth),
                HeightInBlocks = _randomNumberGenerator.Next(_minimumPillarHeight, _maximumPillarHeight)
            });

            entity.Attach(new Transform2());
        }
        
        // Save the block sprite dimensions for later use in drawing the pillars
        _blockSpriteWidth = _blockSprites[(int)BlockType.TopLeft].Size.X;
        _blockSpriteHeight = _blockSprites[(int)BlockType.TopLeft].Size.Y;
    }

    public override void Update(GameTime gameTime)
    {
        var respawnPillar = false;

        // Framerate independent timer
        _countdownToNextPillarSpawn -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        // If we've reached 0 then spawn the next pillar
        if (_countdownToNextPillarSpawn < 0)
        {
            // Reset the timer to some random value
            _countdownToNextPillarSpawn = _randomNumberGenerator.Next(
                _randomNextPillarSpawnTimerMinimumValue,
                _randomNextPillarSpawnTimerMaximumValue);

            // Now spawn a pillar ;-)
            respawnPillar = true;
        }

        foreach (var entityId in ActiveEntities)
        {
            var entityStateComponent = _entityStateMapper.Get(entityId);
            var multiplexedSpriteComponent = _multiplexedSpriteMapper.Get(entityId);
            var platformComponent = _platformMapper.Get(entityId);
            var transformComponent = _transformMapper.Get(entityId);

            // Is this pillar disabled and ready to be respawned?
            if (respawnPillar && entityStateComponent.State == EntityState.Disabled)
            {
                // Yes, but we only respawn one pillar per update
                respawnPillar = false;

                // Reset the pillar's state, dimensions and position to
                // the right hand side of the screen
                ResetPillar(entityStateComponent, transformComponent, platformComponent);

                // Skip to the next pillar as we don't want to check if
                // this one has moved off the left hand side of the screen
                continue;
            }

            if (entityStateComponent.State == EntityState.Disabled)
                continue;

            transformComponent.Position += new Vector2(-10, 0); // Move the pillar to the left

            // This pillar has moved off the left hand side of the screen so disable it
            if (transformComponent.Position.X + platformComponent.WidthInBlocks + _blockSpriteWidth < 0)
            {
                entityStateComponent.State = EntityState.Disabled;
                continue;
            }

            // Otherwise its still on screen so draw it at its current position
            DrawPillar(transformComponent, platformComponent, multiplexedSpriteComponent);
        }
    }

    private void DrawPillar(Transform2 transformComponent, PlatformComponent platformComponent, MultiplexedSpriteComponent multiplexedSpriteComponent)
    {
        // Each pillar consists of a grid of blocks, so we need to draw each block in
        // the correct position based on the pillar's width and height in blocks                
        multiplexedSpriteComponent.Sprites = new Sprite[platformComponent.WidthInBlocks * platformComponent.HeightInBlocks];
        multiplexedSpriteComponent.Transforms = new Transform2[platformComponent.WidthInBlocks * platformComponent.HeightInBlocks];

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

                // Draw the sprite at the correct position based on the pillar's position and the block's position within the pillar
                multiplexedSpriteComponent.Sprites[y * platformComponent.WidthInBlocks + x] = sprite;
                multiplexedSpriteComponent.Transforms[y * platformComponent.WidthInBlocks + x] = transform;
            }
        }
    }

    private void ResetPillar(EntityStateComponent entityStateComponent, Transform2 transformComponent, PlatformComponent platformComponent)
    {
        // Re-enable the pillar and set its position to the right hand side of the screen
        entityStateComponent.State = EntityState.Alive;
        transformComponent.Position = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width, _spriteBatch.GraphicsDevice.Viewport.Height - platformComponent.HeightInBlocks * _blockSpriteHeight);

        // Randomly set the pillar's width and height in blocks
        platformComponent.WidthInBlocks = _randomNumberGenerator.Next(_minimumPillarWidth, _maximumPillarWidth + 1);
        platformComponent.HeightInBlocks = _randomNumberGenerator.Next(_minimumPillarHeight, _maximumPillarHeight + 1);
    }
}
