using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace WorldSim
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PlanetMapGenerator generator;
        private Texture2D planetTexture;

        private SpriteFont font;

        public int WindowWidth = 800;
        public int WindowHeight = 800;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferredBackBufferWidth = WindowWidth;
            _graphics.PreferredBackBufferHeight = WindowHeight;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Debug");

            generator = new PlanetMapGenerator();

            planetTexture = generator.GeneratePlanet(GraphicsDevice, WindowWidth, WindowHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            MouseState mouseState = Mouse.GetState();

            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            var mx= Math.Clamp( (int)mousePosition.X,0,WindowWidth-1);
            var my= Math.Clamp( (int)mousePosition.Y, 0, WindowHeight-1);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(planetTexture, new Vector2(0, 0), Color.White);

            _spriteBatch.DrawString(font, "Temperature: " + generator.temperatureData[my * WindowWidth + mx], new Vector2(10, 0), Color.Black);
            _spriteBatch.DrawString(font, "Humidity: " + generator.humidityData[my * WindowWidth + mx], new Vector2(10, 20), Color.Black);
            _spriteBatch.DrawString(font, "Height: " + generator.heightData[my * WindowWidth + mx], new Vector2(10, 40), Color.Black);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
