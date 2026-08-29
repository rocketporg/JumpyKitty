using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using System;

namespace JumpyKitty.Core.Background;

internal class BackgroundSystem : EntityDrawSystem, IUpdateSystem
{    
    private const int _backgroundParallaxStrength = 6;
    private const int _backgroundObjectScale = 1;
    private const int _cloudHeightOffsetMinimum = -256, _cloudHeightOffsetMaximum = 256;
    private const int _maxClouds = 24;
    private const int _randomNextCloudSpawnTimerMinimumValue = 400, _randomNextCloudSpawnTimerMaximumValue = 800;

    private readonly ContentManager _contentManager;    
    private Texture2D _cloudTextureSmall = default!;
    private Texture2D _cloudTextureMedium = default!;
    private Texture2D _cloudTextureLarge = default!;
    private float _countdownToNextCloudSpawn = _randomNextCloudSpawnTimerMinimumValue;
    private Texture2D _forestCloseTexture = default!;
    private float _forestCloseX = 0;
    private Texture2D _forestDistantTexture = default!;
    private float _forestDistantX = 0;
    private readonly Random _randomNumberGenerator = new();
    private Texture2D _skyTexture = default!;
    private readonly SpriteBatch _spriteBatch;
    private ComponentMapper<Sprite> _spriteMapper = default!;
    private Vector2 _spriteScaleVector;
    private Texture2D _sunTexture = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public BackgroundSystem(ContentManager contentManager, SpriteBatch spriteBatch) : base(Aspect.All(        
        typeof(BackgroundObjectComponent),
        typeof(Sprite),
        typeof(Transform2)))
    {
        _contentManager = contentManager;
        _spriteBatch = spriteBatch;
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(
            sortMode: SpriteSortMode.Immediate,
            blendState: null,
            samplerState: SamplerState.PointClamp,
            depthStencilState: null,
            rasterizerState: null,
            effect: null,
            transformMatrix: null);

        DrawSky();
        DrawSun();
        //DrawBackgroundObjects(_clouds);
        DrawForest();

        _spriteBatch.End();
    }

    private void DrawForest()
    {
        // Draw distant forest
        _spriteBatch.Draw(
            texture: _forestDistantTexture,
            destinationRectangle: new Rectangle((int)_forestDistantX, _spriteBatch.GraphicsDevice.Viewport.Height - _forestDistantTexture.Height, _spriteBatch.GraphicsDevice.Viewport.Width, _forestDistantTexture.Height),
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );

        _spriteBatch.Draw(
            texture: _forestDistantTexture,
            destinationRectangle: new Rectangle((int)_forestDistantX + _spriteBatch.GraphicsDevice.Viewport.Width, _spriteBatch.GraphicsDevice.Viewport.Height - _forestDistantTexture.Height, _spriteBatch.GraphicsDevice.Viewport.Width, _forestDistantTexture.Height),
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );

        // Draw close forest
        _spriteBatch.Draw(
            texture: _forestCloseTexture,
            destinationRectangle: new Rectangle((int)_forestCloseX, _spriteBatch.GraphicsDevice.Viewport.Height - _forestCloseTexture.Height, _spriteBatch.GraphicsDevice.Viewport.Width, _forestCloseTexture.Height),
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );

        _spriteBatch.Draw(
            texture: _forestCloseTexture,
            destinationRectangle: new Rectangle((int)_forestCloseX + _spriteBatch.GraphicsDevice.Viewport.Width, _spriteBatch.GraphicsDevice.Viewport.Height - _forestCloseTexture.Height, _spriteBatch.GraphicsDevice.Viewport.Width, _forestCloseTexture.Height),
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }

    private void DrawSky()
    {
        _spriteBatch.Draw(
            texture: _skyTexture,
            destinationRectangle: new Rectangle(0, 0, _spriteBatch.GraphicsDevice.Viewport.Width, _spriteBatch.GraphicsDevice.Viewport.Height),
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }

    private void DrawSun()
    {
        var x = _spriteBatch.GraphicsDevice.Viewport.Width - _sunTexture.Width;
        var y = _spriteBatch.GraphicsDevice.Viewport.Height / 5;

        _spriteBatch.Draw(
            texture: _sunTexture,
            position: new Vector2(x, y),
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: 1f,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _spriteMapper = mapperService.GetMapper<Sprite>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Load sky sprite
        _skyTexture ??= _contentManager.Load<Texture2D>("Backgrounds/Sky");

        // Load the sun texture
        _sunTexture ??= _contentManager.Load<Texture2D>("Backgrounds/Sun");

        // Load forest texture
        _forestCloseTexture ??= _contentManager.Load<Texture2D>("Backgrounds/Background Forest 2");
        _forestDistantTexture ??= _contentManager.Load<Texture2D>("Backgrounds/Background Forest 1");

        // Load cloud sprites
        _cloudTextureSmall ??= _contentManager.Load<Texture2D>("Backgrounds/Cloud 1-1");
        _cloudTextureMedium ??= _contentManager.Load<Texture2D>("Backgrounds/Cloud 1-2");
        _cloudTextureLarge ??= _contentManager.Load<Texture2D>("Backgrounds/Cloud 1-3");

        for (var cloudNumber = 0; cloudNumber < _maxClouds; cloudNumber++)
        {
            var entity = CreateEntity();
            var layer = _randomNumberGenerator.Next(0, 3);

            entity.Attach(new BackgroundObjectComponent
            {
                IsEnabled = false,
                Layer = layer,
                ObjectType = BackgroundObjectType.Cloud,
                Speed = 50f
            });

            entity.Attach(new Sprite(layer switch
            {
                0 => _cloudTextureSmall,
                1 => _cloudTextureMedium,
                _ => _cloudTextureLarge
            }));

            entity.Attach(new Transform2());
        }

        //_cloudHeight = _displayManager.VirtualScreenHeight / 4;

        _spriteScaleVector = new Vector2(_backgroundObjectScale, _backgroundObjectScale);
    }

    public void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        //_countdownToNextCloudSpawn = UpdateBackgroundObjects(gameTime, _clouds, _cloudHeight + _randomNumberGenerator.Next(_cloudHeightOffsetMinimum, _cloudHeightOffsetMaximum), _countdownToNextCloudSpawn, _randomNextCloudSpawnTimerMinimumValue, _randomNextCloudSpawnTimerMaximumValue, 4);

        // Move the distant and close forest layers to the left, creating a parallax effect
        _forestDistantX -= 100f * deltaTime;
        if (_forestDistantX < - _spriteBatch.GraphicsDevice.Viewport.Width - 1) _forestDistantX = 0;

        // Move the close forest layer to the left slightly faster, creating a parallax effect
        _forestCloseX -= 150f * deltaTime;
        if (_forestCloseX < -_spriteBatch.GraphicsDevice.Viewport.Width - 1) _forestCloseX = 0;
    }
}