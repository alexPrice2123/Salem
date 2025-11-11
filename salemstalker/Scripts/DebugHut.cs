using Godot;
using System;
using System.Collections.Generic;

public partial class DebugHut : Node3D
{
	// --- CONSTANTS ---


	// --- VARIABLES ---
	private CsgBox3D _spawn;                   // Spawn point node where monsters will appear
	private Timer _countdown;                  // Timer node that triggers monster spawn events
	private int _number;                     // Tracks the current number of spawned monsters
	private Player3d _player;                  // Reference to the player node
	private Node3D _holder;                    // Node that holds all spawned monsters as children
	public bool _shouldSpawn = true;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	[Export]
	public float _spawnTime = 2f;
	public int _maxMonsterCount = 70;
	public double SpawnDistance = 100;        // Maximum distance from player before monsters despawn or spawning stops    
	[Export]
	public PackedScene _monster;

	// --- READY ---
	public override void _Ready()
	{
		_spawn = GetNode<CsgBox3D>("Spawn");             // Get the spawn point node
		_countdown = GetNode<Timer>("SpawnTime");        // Get the timer node
		_countdown.Start();                              // Start the spawn timer
        GetNode<Label3D>("SpawnedName").Text = Name;
		_player = this.GetParent().GetParent().GetNode<Player3d>("Player_3d"); // Get the player node (two parents up in the scene tree)
		_holder = GetNode<Node3D>("MonsterHolder");      // Get the monster holder node
		_rng.Randomize();
	}

	// --- SPAWN HANDLER ---
	private void _on_spawn_time_timeout()
	{
		SpawnMonster();
	}
	
	private async void SpawnMonster()
    {
        // Prevent spawning if player is in inventory
		if (_player._inv.Visible == true)
		{
			return;
		}

		// --- Spawn new monster ---
		if (_shouldSpawn == true)
		{
			_shouldSpawn = false;
			await ToSignal(GetTree().CreateTimer(_spawnTime), "timeout");
			CharacterBody3D monsterInstance = _monster.Instantiate<CharacterBody3D>(); // Create monster instance
			_holder.AddChild(monsterInstance);                                             // Add monster to holder node
			monsterInstance.Position = _spawn.Position;                                    // Set monster spawn position
			if (monsterInstance is Monster3d monster)
            {
				monster.RandomRangedPosition();
				monster.Debug = true;
            }
			_number += 1; // Increase monster count
			double fps = Engine.GetFramesPerSecond();
			
			GD.Print("There are " + _number + " monsters and its running at " + fps + " FPS");
		}
    }

	// --- PROCESS LOOP ---
	public override void _Process(double delta)
	{
		//
	}
}
