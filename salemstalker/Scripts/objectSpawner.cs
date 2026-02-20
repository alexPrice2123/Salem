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
			objInst.GlobalPosition = new Vector3(objInst.GlobalPosition.X, 10f, objInst.GlobalPosition.Z);
			objInst.Name = _objectName;
	}

	// --- PROCESS LOOP ---
	public override void _Process(double delta)
	{
		//
	}
}
