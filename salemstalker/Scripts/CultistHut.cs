using Godot;
using System;
using System.Collections.Generic;

public partial class CultistHut : Node3D
{
	// --- CONSTANTS ---


	// --- VARIABLES ---
	private CsgBox3D _spawn;                   // Spawn point node where monsters will appear
	private Timer _countdown;                  // Timer node that triggers monster spawn events
	private int _number;                     // Tracks the current number of spawned monsters
	private Player3d _player;                  // Reference to the player node
	private Node3D _holder;                    // Node that holds all spawned monsters as children
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	[Export(PropertyHint.Enum, "Plains,Swamp,Forest")]
	public string _biome = "Plains";
	[Export]
	public float _spawnTime = 6f;
	[Export]
	public int _maxMonsterCount = 70;
	[Export]
	public bool _canShadow = false;
	[Export]
	public double SpawnDistance = 100;        // Maximum distance from player before monsters despawn or spawning stops    
	[Export]
	public Godot.Collections.Array<PackedScene> _monsterList { get; set; } = [];
	[Export]
	public Godot.Collections.Array<int> _monsterCount { get; set; } = [];
	public Godot.Collections.Array<int> _currenctMonsterCount { get; set; } = [];
	private bool _destroyed = false;

	// --- READY ---
	public override void _Ready()
	{
		_spawn = GetNode<CsgBox3D>("Spawn");             // Get the spawn point node
		_countdown = GetNode<Timer>("SpawnTime");        // Get the timer node
		_countdown.WaitTime = _spawnTime + _rng.RandfRange(-1,1);
		_countdown.Start();                              // Start the spawn timer
		_currenctMonsterCount = _monsterCount;

		_player = this.GetParent().GetParent().GetNode<Player3d>("Player_3d"); // Get the player node (two parents up in the scene tree)
		_holder = GetNode<Node3D>("MonsterHolder");      // Get the monster holder node
		_rng.Randomize();
	}

	// --- SPAWN HANDLER ---
	private void _on_spawn_time_timeout()
	{
		SpawnMonster();
		_countdown.WaitTime = _spawnTime + _rng.RandfRange(-1,1);
	}
	
	private async void SpawnMonster()
    {
        // Prevent spawning if player is in inventory
		if (_player._inv.Visible == true)
		{
			return;
		}
		// Distance between player and hut
		float distance = (_player.GlobalPosition - GlobalPosition).Length();

		// --- Prevent spawning if at max count or player too far ---
		if (_number >= _maxMonsterCount || distance >= SpawnDistance)
		{
			return;
		}

		// --- Spawn new monster ---
		int monsterIndex = _rng.RandiRange(0, _monsterCount.Count-1);
		if (_currenctMonsterCount[monsterIndex] > 0)
		{
			_currenctMonsterCount[monsterIndex] -= 1;
			PackedScene monsterSelection = _monsterList[monsterIndex];
			CharacterBody3D monsterInstance = monsterSelection.Instantiate<CharacterBody3D>(); // Create monster instance
			_holder.AddChild(monsterInstance);                                             // Add monster to holder node
			monsterInstance.Position = _spawn.Position;                                    // Set monster spawn position
			if (monsterInstance is Monster3d monster)
            {
                monster.RandomRangedPosition();
				monster.Biome = _biome;
            }
			_number += 1; // Increase monster count
			double fps = Engine.GetFramesPerSecond();
			
			GD.Print("There are " + _number + " monsters and its running at " + fps + " FPS");
		}
        else
		{
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			SpawnMonster();
			//GD.Print("Tried to spawn" + _monsterList[monsterIndex] + " but was at max");
        }
    }

	// --- PROCESS LOOP ---
	public override void _Process(double delta)
	{
		if (_holder.GetChildCount() <= 0 && _number >= _maxMonsterCount && _destroyed == false)
        {
            _player._shrinesDestroyed += 1;
			_destroyed = true;
			GetNode<MeshInstance3D>("Orb").Visible = false;
			GetNode<OmniLight3D>("Light").Visible = false;
			GetNode<GpuParticles3D>("Magic").Emitting = false;
			GetNode<GpuParticles3D>("Boom").Emitting = true;
        }
		if (_destroyed == true)
        {
            GetNode<Node3D>("Shrine").Position -= new Vector3(0f, 0.01f, 0f);
			if (GetNode<Node3D>("Shrine").Position.Y <= -3)
            {
                QueueFree();
            }
        }
	}
}
