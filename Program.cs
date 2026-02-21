using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System.Collections.Generic;

namespace SkiRidge;

class Program
{
    class Player
    {
        public Vector3 Position;
        public float Speed = 0.1f;
        public float Rotation = 0;
    }
    
    class Tree
    {
        public Vector3 Position;
    }
    
    static void Main()
    {
        InitWindow(1024, 768, "Ski Ridge");
        
        Camera3D camera = new();
        camera.Up = new Vector3(0, 1, 0);
        camera.FovY = 60;
        camera.Projection = CameraProjection.Perspective;
        
        Player player = new() { Position = new Vector3(0, 0, 0) };
        
        List<Tree> trees = new();
        Random rand = new();
        
        // Generate trees
        for (int i = 0; i < 20; i++)
        {
            trees.Add(new Tree
            {
                Position = new Vector3(
                    (float)(rand.NextDouble() * 16 - 8),
                    0,
                    (float)(rand.NextDouble() * 20 + 5)
                )
            });
        }
        
        SetTargetFPS(60);
        
        while (!WindowShouldClose())
        {
            // Input
            if (IsKeyDown(KeyboardKey.Right)) player.Position.X += 0.1f;
            if (IsKeyDown(KeyboardKey.Left)) player.Position.X -= 0.1f;
            
            // Auto move forward
            player.Position.Z += player.Speed;
            
            // Camera follows player
            camera.Target = player.Position;
            camera.Position = player.Position + new Vector3(0, 5, -10);
            
            BeginDrawing();
            ClearBackground(Color.SkyBlue);
            
            BeginMode3D(camera);
            
            // Ground (sloped mountain)
            DrawGrid(40, 1.0f);
            
            // Draw trees
            foreach (var tree in trees)
            {
                if (tree.Position.Z > player.Position.Z - 10 && tree.Position.Z < player.Position.Z + 20)
                {
                    // Trunk
                    DrawCube(tree.Position, 0.3f, 1.5f, 0.3f, Color.Brown);
                    // Leaves
                    DrawCube(tree.Position + new Vector3(0, 1.0f, 0), 1.0f, 1.0f, 1.0f, Color.Green);
                }
            }
            
            // Draw player (skiier)
            DrawCube(player.Position, 0.5f, 0.8f, 0.3f, Color.Red);
            DrawCube(player.Position + new Vector3(0, 0.5f, 0), 0.3f, 0.3f, 0.3f, Color.Black);
            
            EndMode3D();
            
            // UI
            DrawText($"Ski Ridge - Speed: {player.Speed:F1}", 10, 10, 20, Color.Black);
            DrawText("LEFT/RIGHT arrows to steer", 10, 40, 20, Color.DarkGray);
            
            EndDrawing();
        }
        
        CloseWindow();
    }
}
