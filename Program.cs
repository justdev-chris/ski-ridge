using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System.Collections.Generic;
using System;

namespace SkiRidge;

class Program
{
    class Player
    {
        public Vector3 Position;
        public float Speed = 0.2f;
        public float Tilt = 0;
        public Color Color = Color.Red;
    }
    
    class Tree
    {
        public Vector3 Position;
        public Color Color = Color.Green;
    }
    
    static void Main()
    {
        InitWindow(1024, 768, "Ski Ridge");
        
        Camera3D camera = new();
        camera.Up = new Vector3(0, 1, 0);
        camera.FovY = 70;
        camera.Projection = CameraProjection.Perspective;
        
        Player player = new() { Position = new Vector3(0, 0.5f, 0) };
        
        List<Tree> trees = new();
        Random rand = new();
        
        // Generate trees down the mountain
        for (int i = 0; i < 50; i++)
        {
            trees.Add(new Tree
            {
                Position = new Vector3(
                    (float)(rand.NextDouble() * 20 - 10),
                    0,
                    (float)(rand.NextDouble() * 100 + 5)
                )
            });
        }
        
        SetTargetFPS(60);
        
        while (!WindowShouldClose())
        {
            // Input - skiing controls
            if (IsKeyDown(KeyboardKey.Right)) 
            {
                player.Position.X += 0.15f;
                player.Tilt = -0.2f;
            }
            else if (IsKeyDown(KeyboardKey.Left)) 
            {
                player.Position.X -= 0.15f;
                player.Tilt = 0.2f;
            }
            else
            {
                player.Tilt = 0;
            }
            
            // Speed boost with DOWN key
            if (IsKeyDown(KeyboardKey.Down))
                player.Speed = 0.4f;
            else
                player.Speed = 0.2f;
            
            // Move forward (down the mountain)
            player.Position.Z += player.Speed;
            
            // Camera follows from behind
            camera.Target = player.Position;
            camera.Position = player.Position + new Vector3(0, 4, -12);
            
            BeginDrawing();
            ClearBackground(Color.SkyBlue);
            
            BeginMode3D(camera);
            
            // Mountain slope (angled ground)
            for (int z = -5; z < 100; z+=2)
            {
                for (int x = -15; x < 15; x+=2)
                {
                    float height = (float)Math.Sin(x * 0.3f) * 0.2f; // Bumpy terrain
                    DrawCube(new Vector3(x, height, z), 1.8f, 0.1f, 1.8f, Color.White);
                }
            }
            
            // Draw trees
            foreach (var tree in trees)
            {
                if (tree.Position.Z > player.Position.Z - 20 && tree.Position.Z < player.Position.Z + 30)
                {
                    // Trunk
                    DrawCube(tree.Position, 0.4f, 2.0f, 0.4f, Color.Brown);
                    // Leaves (3 layers for low poly tree)
                    DrawCube(tree.Position + new Vector3(0, 1.5f, 0), 1.2f, 0.8f, 1.2f, tree.Color);
                    DrawCube(tree.Position + new Vector3(0, 2.2f, 0), 0.9f, 0.6f, 0.9f, tree.Color);
                    DrawCube(tree.Position + new Vector3(0, 2.8f, 0), 0.6f, 0.4f, 0.6f, tree.Color);
                }
            }
            
            // Draw skier - use RlPushMatrix etc
            Rlgl.PushMatrix();
            Rlgl.Translatef(player.Position.X, player.Position.Y, player.Position.Z);
            Rlgl.Rotatef(player.Tilt * 50, 0, 1, 0);
            
            // Body
            DrawCube(new Vector3(0, 0.8f, 0), 0.5f, 0.8f, 0.3f, player.Color);
            // Head
            DrawCube(new Vector3(0, 1.4f, 0), 0.3f, 0.3f, 0.3f, Color.White);
            // Skis
            DrawCube(new Vector3(-0.2f, 0.2f, 0.2f), 0.2f, 0.1f, 0.8f, Color.DarkGray);
            DrawCube(new Vector3(0.2f, 0.2f, 0.2f), 0.2f, 0.1f, 0.8f, Color.DarkGray);
            
            Rlgl.PopMatrix();
            
            EndMode3D();
            
            // UI
            DrawText("🏔️ SKI RIDGE", 10, 10, 30, Color.White);
            DrawText($"Speed: {player.Speed * 100:F0} km/h", 10, 50, 20, Color.White);
            DrawText("← → : Steer    ↓ : Speed boost", 10, 80, 20, Color.LightGray);
            
            EndDrawing();
        }
        
        CloseWindow();
    }
}
