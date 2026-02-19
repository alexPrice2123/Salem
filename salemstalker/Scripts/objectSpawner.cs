using Godot;
using System;
using System.Collections.Generic;
using TerraBrush;

public partial class objectSpawner : Node3D
{
	// --- CONSTANTS ---


	// --- VARIABLES ---
	private CsgBox3D _spawn;                   // Spawn point node where monsters will appear
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	[Export]
	public int _itemCount = 10;
	public float SpawnRange;  
	private float _wanderRanges; 
	[Export]
	public PackedScene _object;
	[Export]
	public string _objectName;
	private TerraBrushTool m_terraBrushNode;

	// --- READY ---
	public override void _Ready()
	{
		_spawn = GetNode<CsgBox3D>("Spawn");             // Get the spawn point node
		_rng.Randomize();

		SpawnRange = GetNode<CsgSphere3D>("Range").Radius;
		GetNode<CsgSphere3D>("Range").QueueFree();
		m_terraBrushNode = GetParent().GetNode<TerraBrushTool>("TerraBrush"); 


		for (int i = 0; i <= _itemCount; i++)
		{
			SpawnObject();
			GD.Print(i);
		}
	}
	private async void SpawnObject()
	{
			CharacterBody3D objInst = _object.Instantiate<CharacterBody3D>(); // Create monster instance
			float _spawnX = _rng.RandfRange(-SpawnRange, SpawnRange);
			float _spawnZ = _rng.RandfRange(-SpawnRange, SpawnRange);
			AddChild(objInst);     
		
			objInst.GlobalPosition = GlobalPosition + new Vector3(_spawnX, 0f, _spawnZ);                                    // Set monster spawn position
			objInst.GlobalPosition = new Vector3(objInst.GlobalPosition.X, FindGroundY(_spawnX, _spawnZ)+ 1f, objInst.GlobalPosition.Z);
			GD.Print(FindGroundY(_spawnX, _spawnZ));
			objInst.Name = _objectName;
	}

	private float FindGroundY(float x, float z)
	{
		if (m_terraBrushNode == null)
        {
            return 0.0f; // Return a default height if the terrain node isn't available
        }

        // The TerraBrush plugin provides a method to get the height at a world position.
        // Based on similar terrain plugins' C# APIs, you would typically call a method on the terrain data.
        // The TerraBrush specific method for this is 'GetHeight' which takes X and Z world coordinates.
        // It uses its internal heightmap data to return the height at that location.

        // Note: The specific function may vary slightly based on the exact version, 
        // but typically it involves the main TerraBrush node or its underlying data storage.
        // The most common implementation is a direct method call on the terrain object:
        //float height = m_terraBrushNode.GetHeight(x, z); 

        return 0;
	}

	// --- PROCESS LOOP ---
	public override void _Process(double delta)
	{
		//
	}
}
