using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Game1 : Game
{
    Texture2D tilemap;

    GraphicsDeviceManager _graphics;
    SpriteBatch _spriteBatch;

    Map map = new Map(20, 15);

    ButtonState previousLeftMouseState = ButtonState.Released;

    int TileScreenSize => Math.Min(
        _graphics.GraphicsDevice.Viewport.Width / map.Width,

        _graphics.GraphicsDevice.Viewport.Height / map.Height
    );

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1000 * 3 / 2;
        _graphics.PreferredBackBufferHeight = 750 * 3 / 2;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        tilemap = Content.Load<Texture2D>("tilemap");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.R)) map.Reset();

        var mouseJustPressed = Mouse.GetState().LeftButton == ButtonState.Pressed && previousLeftMouseState == ButtonState.Released;
        if (mouseJustPressed || Keyboard.GetState().IsKeyDown(Keys.Space))
            map.GenerateTile();

        previousLeftMouseState = Mouse.GetState().LeftButton;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.MediumSeaGreen);

        // TODO: Add your drawing code here
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        DrawMap(_spriteBatch, map);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    void DrawTile(SpriteBatch spriteBatch, Tile tile, int x, int y, float alpha = 1) {
        Debug.Assert(alpha is >= 0 and <= 1);

        var source = Tiles.GetTilemapSource(tile);
        var dest = new Rectangle(x * TileScreenSize, y * TileScreenSize, TileScreenSize, TileScreenSize);
        spriteBatch.Draw(tilemap, dest, source, Color.White * alpha);
    }

    void DrawMap(SpriteBatch _spriteBatch, Map map) {
        for (int x = 0; x < map.Width; x++) {
            for (int y = 0; y < map.Height; y++) {
                var tiles = map.GetPossibleTiles(x, y).ToArray();

                foreach (var t in tiles) {
                    DrawTile(_spriteBatch, t, x, y, 1f / tiles.Length);
                }
            }
        }
    }
}
