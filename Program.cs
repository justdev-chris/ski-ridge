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
        public Vector3 Velocity;
        public float Speed = 0.2f;
        public float Tilt = 0;
        public Color Color = Color.Red;
        public bool OnGround = true;
        public float Height = 1.0f;
    }
    
    class Tree
    {
        public Vector3 Position;
        public Color Color = Color.Green;
        public float Radius = 1.0f;
        public float Height = 3.0f;
    }
    
    class TerrainChunk
    {
        public Vector3 Position;
        public float[,] Heights = new float[20, 20];
    }
    
    static void Main()
    {
        InitWindow(1024, 768, "Ski Ridge");
        
        Camera3D camera = new();
        camera.Up = new Vector3(0, 1, 0);
        camera.FovY = 70;
        camera.Projection = CameraProjection.Perspective;
        
        Player player = new() { Position = new Vector3(0, 2.0f, 0) };
        
        List<Tree> trees = new();
        List<TerrainChunk> terrainChunks = new();
        Random rand = new();
        
        // Generate initial trees and terrain
        for (int i = 0; i < 30; i++)
        {
            trees.Add(new Tree
            {
                Position = new Vector3(
                    (float)(rand.NextDouble() * 30 - 15),
                    0,
                    (float)(rand.NextDouble() * 50)
                ),
                Color = Color.Green
            });
        }
        
        SetTargetFPS(60);
        
        while (!WindowShouldClose())
        {
            // Apply gravity
            player.Velocity.Y -= 0.01f; // Gravity
            
            // Input
            if (IsKeyDown(KeyboardKey.Right)) 
            {
                player.Position.X += 0.15f;
                player.Tilt = 0.2f;
            }
            else if (IsKeyDown(KeyboardKey.Left)) 
            {
                player.Position.X -= 0.15f;
                player.Tilt = -0.2f;
            }
            else
            {
                player.Tilt = 0;
            }
            
            // Speed boost
            if (IsKeyDown(KeyboardKey.Down))
                player.Speed = 0.4f;
            else
                player.Speed = 0.2f;
            
            // Move forward
            player.Velocity.Z = player.Speed;
            
            // Update position with velocity
            Vector3 newPosition = player.Position + player.Velocity;
            
            // Get ground height at new position
            float groundHeight = GetGroundHeight(newPosition.X, newPosition.Z);
            
            // Tree collision
            foreach (var tree in trees)
            {
                float distance = Vector3.Distance(
                    new Vector3(newPosition.X, 0, newPosition.Z),
                    new Vector3(tree.Position.X, 0, tree.Position.Z)
                );
                
                if (distance < tree.Radius + 0.5f)
                {
                    // Crash! Reset position
                    newPosition = new Vector3(0, groundHeight + player.Height, 0);
                    player.Velocity = Vector3.Zero;
                    break;
                }
            }
            
            // Apply ground collision
            if (newPosition.Y <= groundHeight + player.Height)
            {
                newPosition.Y = groundHeight + player.Height;
                player.Velocity.Y = 0;
                player.OnGround = true;
                
                // Add some bounce when landing
                if (player.Velocity.Y < -0.5f)
                {
                    player.Velocity.Y = 0.2f;
                }
            }
            else
            {
                player.OnGround = false;
            }
            
            player.Position = newPosition;
            
            // Generate new terrain
            if (player.Position.Z > 30)
            {
                // Move trees backward
                for (int i = trees.Count - 1; i >= 0; i--)
                {
                    trees[i].Position.Z -= 50;
                    if (trees[i].Position.Z < -20)
                    {
                        trees.RemoveAt(i);
                    }
                }
                
                // Add new trees ahead
                for (int i = 0; i < 5; i++)
                {
                    trees.Add(new Tree
                    {
                        Position = new Vector3(
                            (float)(rand.NextDouble() * 30 - 15),
                            0,
                            (float)(rand.NextDouble() * 50 + 40)
                        ),
                        Color = Color.Green
                    });
                }
                
                player.Position = new Vector3(player.Position.X, player.Position.Y, 0);
            }
            
            // Camera follow
            camera.Target = player.Position;
            camera.Position = player.Position + new Vector3(0, 5, -15);
            
            BeginDrawing();
            ClearBackground(Color.SkyBlue);
            
            BeginMode3D(camera);
            
            // Draw terrain
            for (int z = -10; z < 40; z+=2)
            {
                for (int x = -20; x < 20; x+=2)
                {
                    float height = GetGroundHeight(x, z);
                    Color terrainColor = height < 0.2f ? Color.White : new Color(200, 200, 255, 255);
                    DrawCube(new Vector3(x, height - 0.5f, z), 1.8f, 0.3f, 1.8f, terrainColor);
                }
            }
            
            // Draw trees with collision visualization
            foreach (var tree in trees)
            {
                if (tree.Position.Z > player.Position.Z - 30 && tree.Position.Z < player.Position.Z + 40)
                {
                    float groundY = GetGroundHeight(tree.Position.X, tree.Position.Z);
                    Vector3 treeBase = new Vector3(tree.Position.X, groundY, tree.Position.Z);
                    
                    // Trunk
                    DrawCube(treeBase, 0.4f, 2.0f, 0.4f, Color.Brown);
                    // Leaves
                    DrawCube(treeBase + new Vector3(0, 1.5f, 0), 1.2f, 0.8f, 1.2f, tree.Color);
                    DrawCube(treeBase + new Vector3(0, 2.2f, 0), 0.9f, 0.6f, 0.9f, tree.Color);
                    DrawCube(treeBase + new Vector3(0, 2.8f, 0), 0.6f, 0.4f, 0.6f, tree.Color);
                    
                    // Draw collision radius (debug)
                    DrawCircle3D(treeBase + new Vector3(0, 0.5f, 0), tree.Radius, new Vector3(1, 0, 0), 90, Color.Red);
                }
            }
            
            // Draw skier
            Rlgl.PushMatrix();
            Rlgl.Translatef(player.Position.X, player.Position.Y - player.Height, player.Position.Z);
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
            DrawText($"Height: {player.Position.Y:F1}m", 10, 70, 20, Color.White);
            DrawText("← → : Steer    ↓ : Speed boost", 10, 100, 20, Color.LightGray);
            
            if (!player.OnGround)
            {
                DrawText("!!! AIRBORNE !!!", 400, 360, 30, Color.Yellow);
            }
            
            EndDrawing();
        }
        
        CloseWindow();
    }
    
    static float GetGroundHeight(float x, float z)
    {
        // Generate some hills and bumps
        float height = (float)(
            Math.Sin(x * 0.2f) * Math.Cos(z * 0.1f) * 0.5f +
            Math.Sin(x * 0.5f) * 0.3f +
            Math.Sin(z * 0.3f) * 0.3f
        );
        
        // Base ground level
        return height + 0.5f;
    }
}
