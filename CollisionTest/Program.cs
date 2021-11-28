using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Linq;
using static CollisionTest.Utils;

namespace CollisionTest
{
    class Program
    {
        private static RectangleShape oldPlayer;
        private static RectangleShape player = new RectangleShape()
        {
            Size = new Vector2f(49, 49),
            FillColor = Color.Blue
        };
        private static Vector2f playerVelocity;

        private static readonly RectangleShape[,] tiles = new RectangleShape[20, 20];

        private static RenderWindow window;
        private static Font arial;

        private static byte debug;
        private static RectangleShape[] debug2Var;

        static void Main(string[] args)
        {
            // create window
            window = new RenderWindow(new VideoMode(800, 600), "CollisionTest");
            window.KeyPressed += UpdateInput;
            window.SetKeyRepeatEnabled(false);
            window.Closed += (_, __) => window.Close(); // close button

            arial = new Font("Fonts/arial.ttf");

            var debugText = new Text("", arial)
            {
                Position = new(550, 10),
                FillColor = Color.Black,
                CharacterSize = 12
            };

            // draw grid to renderTexture
            RenderTexture renderTexture = new(800, 600);
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    renderTexture.Draw(new RectangleShape()
                    {
                        Position = new Vector2f(x * 50, y * 50),
                        Size = new Vector2f(50, 50),
                        OutlineColor = Color.Green,
                        OutlineThickness = 1,
                        FillColor = Color.Transparent
                    });
                }
            }
            var grid = new RectangleShape(new Vector2f(800, 600))
            {
                Texture = renderTexture.Texture
            };

            // generate debug tile map
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    bool singleBlock = (x == 2 && y == 2); // singe tile
                    bool singleBlock2 = (x == 2 && y == 4); // singe tile
                    bool block3x3 = x > 5 && x <= 8 && y > 2 && y <= 5; // 3x3 block
                    bool L = (x == 3 && y >= 4 && y <= 6) || x == 4 && y == 6; // L shape

                    if (singleBlock || singleBlock2 || block3x3 || L)
                    {
                        tiles[x, y] = new RectangleShape()
                        {
                            Size = new Vector2f(50, 50),
                            FillColor = Color.Black,
                            Position = new(x * 50, y * 50)
                        };
                    }
                }
            }

            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear(Color.White);

                Update();

                // debug1: Draw Grid
                if ((debug & 0b00000001) != 0)
                {
                    window.Draw(grid);
                }

                // debug2: Draw SurroundingTiles
                if ((debug & 0b00000010) != 0)
                {
                    foreach (var tile in tiles)
                    {
                        if (tile != null)
                        {
                            tile.FillColor = Color.Black;
                        }
                    }

                    for (byte i = 0; i < debug2Var.Length; i++)
                    {
                        if (debug2Var[i] != null)
                        {
                            byte color = (byte)(255 / (i + 1) / 2);
                            debug2Var[i].FillColor = new Color(color, color, color);
                        }
                    }
                }

                // draw tiles
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    for (int x = 0; x < tiles.GetLength(0); x++)
                    {
                        var tile = tiles[x, y];
                        if (tile != null)
                        {
                            window.Draw(tile);
                        }
                    }
                }

                window.Draw(player);

                // draw debug text
                if ((debug & 0b00000100) != 0)
                {
                    debugText.DisplayedString = $"Pos:{player.Position}\nTilePos:{new Vector2i((int)(player.Center().X / player.Size.X), (int)(player.Center().Y / player.Size.X))}\nVel:{playerVelocity}";
                    window.Draw(debugText);
                }

                window.Display();
            }
        }

        private static void UpdateInput(object sender, KeyEventArgs e)
        {
            // debug settings
            if (e.Code == Keyboard.Key.Num1)
            {
                debug ^= 0b00000001;
            }

            if (e.Code == Keyboard.Key.Num2)
            {
                debug ^= 0b00000010;
                foreach (var tile in tiles)
                {
                    if (tile != null)
                    {
                        tile.FillColor = Color.Black;
                    }
                }
            }

            if (e.Code == Keyboard.Key.Num3)
            {
                debug ^= 0b00000100;
            }
        }

        public static void Update()
        {
            oldPlayer = new RectangleShape(player);

            // decrease player velocity (drag)
            playerVelocity *= 0.01f;

            // update player velocity based on inputs
            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                playerVelocity.Y -= 0.1f;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                playerVelocity.Y += 0.1f;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                playerVelocity.X -= 0.1f;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                playerVelocity.X += 0.1f;
            }

            // debug teleport
            if (Mouse.IsButtonPressed(Mouse.Button.Right))
            {
                player.Position = (Vector2f)Mouse.GetPosition(window) - player.Size / 2;
            }

            // update player position
            player.Position += playerVelocity;

            HandleCollision();
        }

        private static void HandleCollision()
        {
            // only get surrounding tiles to improve performance
            RectangleShape[] surroundingTiles = GetSurroundingTiles();

            // sort array by distance from player. VERY IMPORTANT
            surroundingTiles = surroundingTiles.OrderBy(x => x != null ? Vector.Distance(player.Center(), x.Center()) : float.MaxValue).ToArray();
            debug2Var = surroundingTiles; // debug variable

            for (int j = 0; j < surroundingTiles.Length; j++)
            {
                // if the surrounding tile is not empty and if the player is colliding with it
                if (surroundingTiles[j] != null && Collision.RectangleRectangle(player, surroundingTiles[j]))
                {
                    ResolveCollision(surroundingTiles[j]);
                }
            }
        }

        // returns the surrounding 9 tiles around the player (and the player tile itself)
        private static RectangleShape[] GetSurroundingTiles()
        {
            Vector2f pos = player.Center();
            int tileposX = (int)pos.X / (int)player.Size.X; // player position in tile coords
            int tileposY = (int)pos.Y / (int)player.Size.Y; // player position in tile coords

            var surroundingTiles = new RectangleShape[9];
            int i = 0;

            /*
             [(-1, -1), (0, -1), (1, -1)]
             [(-1,  0), (0,  0), (1,  0)]
             [(-1, +1), (0, +1), (1, +1)]      
             */
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    // prevent IndexOutOfBounds
                    if (tileposX + offsetX >= 0 && tileposY + offsetY >= 0 && tileposX + offsetX < tiles.GetLength(0) && tileposY + offsetY < tiles.GetLength(1))
                    {
                        surroundingTiles[i] = tiles[tileposX + offsetX, tileposY + offsetY];
                    }
                    i++;
                }
            }

            return surroundingTiles;
        }

        private static void ResolveCollision(RectangleShape block)
        {
            Vector2f nextPosition = player.Position;
            const float offsetAmount = 0.0001f;

            // top collision 
            if (player.Bottom() >= block.Position.Y && oldPlayer.Bottom() < block.Position.Y)
            {
                nextPosition.Y = block.Position.Y - offsetAmount - player.Size.Y;
            }
            // bottom collision
            else if (player.Position.Y <= block.Bottom() && oldPlayer.Position.Y > block.Bottom())
            {
                nextPosition.Y = block.Bottom() + offsetAmount;
            }
            // right collision
            else if (player.Right() >= block.Position.X && oldPlayer.Right() < block.Position.X)
            {
                nextPosition.X = block.Position.X - offsetAmount - player.Size.X;
            }
            // left collision
            else if (player.Position.X <= block.Right() && oldPlayer.Position.X > block.Right())
            {
                nextPosition.X = block.Right() + offsetAmount;
            }

            player.Position = nextPosition;
            playerVelocity = new Vector2f(0, 0);
        }
    }
}
