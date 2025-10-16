using Godot;
using System;
using System.Collections.Generic;

public partial class CultistHut : Node3D
{
	// --- CONSTANTS ---
	private const double SpawnDistance = 100;        // Maximum distance from player before monsters despawn or spawning stops

	// --- VARIABLES ---
	private PackedScene _monsterScene = GD.Load<PackedScene>("res://Scenes/Monsters/Monster_3D.tscn"); // Scene reference for the base monster class
	private PackedScene _theHollow = GD.Load<PackedScene>("res://Scenes/Monsters/the_hollow.tscn"); // Scene reference for the hollow
	private PackedScene _vCultist = GD.Load<PackedScene>("res://Scenes/Monsters/vCultist.tscn"); // Scene reference for the violent culstist
	private List<PackedScene> _monsterList;
	private int _monsterCount = 1;
	private CsgBox3D _spawn;                   // Spawn point node where monsters will appear
	private Timer _countdown;                  // Timer node that triggers monster spawn events
	private float _number;                     // Tracks the current number of spawned monsters
	private Player3d _player;                  // Reference to the player node
	private Node3D _holder;                    // Node that holds all spawned monsters as children
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	// --- READY ---
	public override void _Ready()
	{
		_spawn = GetNode<CsgBox3D>("Spawn");             // Get the spawn point node
		_countdown = GetNode<Timer>("SpawnTime");        // Get the timer node
		_countdown.Start();                              // Start the spawn timer

		_player = this.GetParent().GetParent().GetNode<Player3d>("Player_3d"); // Get the player node (two parents up in the scene tree)
		_holder = GetNode<Node3D>("MonsterHolder");      // Get the monster holder node
		_monsterList = new List<PackedScene> { _vCultist, _theHollow };
		_rng.Randomize();
	}

	// --- SPAWN HANDLER ---
	private void _on_spawn_time_timeout()
	{
		// Prevent spawning if player is in inventory
		if (_player._inv.Visible == true)
		{
			return;
		}

		// Distance between player and hut
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
	
		// --- Recount monsters if player is too far away (despawn management) ---
		if (distance >= SpawnDistance)
		{
			_number = 0;
			foreach (CharacterBody3D monster in _holder.GetChildren())
			{
				_number += 1;
			}
		}

		// --- Prevent spawning if at max count or player too far ---
		if (_number >= 70 || distance >= SpawnDistance)
		{
			return;
		}

		// --- Spawn new monster ---
		int monsterIndex = _rng.RandiRange(1, _monsterCount);
		PackedScene monsterSelection = _monsterList[monsterIndex-1];
		GD.Print(monsterSelection);
		CharacterBody3D monsterInstance = monsterSelection.Instantiate<CharacterBody3D>(); // Create monster instance
		_holder.AddChild(monsterInstance);                                             // Add monster to holder node
		monsterInstance.Position = _spawn.Position;                                    // Set monster spawn position

		_number += 1; // Increase monster count
		double fps = Engine.GetFramesPerSecond();
		GD.Print("There are " + _number + " monsters and its running at " + fps + " FPS");
	}

	// --- PROCESS LOOP ---
	public override void _Process(double delta)
	{
		//
	}
}
