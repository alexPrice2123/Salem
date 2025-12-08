using Godot;
using System;
using System.Collections.Generic;

public partial class enemySpawner : Node3D
{
	// --- CONSTANTS ---


	// --- VARIABLES ---
	private CsgBox3D _spawn;                   // Spawn point node where monsters will appear
	private Timer _countdown;                  // Timer node that triggers monster spawn events
	private int _number;                     // Tracks the current number of spawned monsters
	private Player3d _player;                  // Reference to the player node
	private Node3D _holder;                    // Node that holds all spawned monsters as children
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	[Export]
	public int _maxMonsterCount = 70;
	[Export]
	public bool _canShadow = false;
	[Export(PropertyHint.Enum, "Plains,Swamp,Forest,Misc")]
	public string _biome = "Plains";
	public float SpawnRange;        
	[Export]
	public Godot.Collections.Array<PackedScene> _monsterList { get; set; } = [];
	[Export]
	public Godot.Collections.Array<int> _monsterCount { get; set; } = [];
	public Godot.Collections.Array<int> _currenctMonsterCount { get; set; } = [];

	// --- READY ---
	public override void _Ready()
	{
		_spawn = GetNode<CsgBox3D>("Spawn");             // Get the spawn point node
		_countdown = GetNode<Timer>("SpawnTime");        // Get the timer node
		if (Name != "RatSpawner")
        {
            _countdown.WaitTime = 0.1f;
        }
		_countdown.Start();                              // Start the spawn timer
		_currenctMonsterCount = _monsterCount;

		_player = this.GetParent().GetParent().GetNode<Player3d>("Player_3d"); // Get the player node (two parents up in the scene tree)
		_holder = GetNode<Node3D>("MonsterHolder");      // Get the monster holder node
		_rng.Randomize();

		SpawnRange = GetNode<CsgSphere3D>("Range").Radius;
		GetNode<CsgSphere3D>("Range").QueueFree();
	}

	// --- SPAWN HANDLER ---
	private void _on_spawn_time_timeout()
	{
		if (Name == "RatSpawner" && _player._questBox.FindChild("Find and kill rats around the Village") != null)
        {
            SpawnMonster();
        }
		else if (Name != "RatSpawner")
        {
            SpawnMonster();
        }
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

		// --- Recount monsters if player is too far away (despawn management) ---
		if (distance >= SpawnRange + 25f)
		{
			_number = 0;
			foreach (CharacterBody3D monster in _holder.GetChildren())
			{
				_number += 1;
			}
		}
		
		if (Name == "RatSpawner" && _player._questBox.FindChild("Find and kill rats around the Village") == null && _player._ratsKilled > 0)
        {
			foreach (CharacterBody3D rat in _holder.GetChildren())
			{
				rat.QueueFree();
			}
			_number = 0;
        }

		// --- Prevent spawning if at max count or player too far ---
		if (_number >= _maxMonsterCount || distance >= SpawnRange+25f)
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
			float _spawnX = _rng.RandfRange(-SpawnRange, SpawnRange);
			float _spawnZ = _rng.RandfRange(-SpawnRange, SpawnRange);
			_holder.AddChild(monsterInstance);     
			if (monsterInstance is Monster3d monster)
            {
				monster.RandomRangedPosition();
				monster.Biome = _biome;
				monster.SpawnRange = SpawnRange*1.5f;
				monster._currentSpawnRange = SpawnRange*1.5f;
				monster._startPos = GlobalPosition;
				if (_canShadow == true)
                {
					float shadowChange = _rng.RandfRange(1, 10);
					if (shadowChange == 1)
                    {
						monster.Shadow = true;
                    }
                }
            }                                        // Add monster to holder node
			monsterInstance.GlobalPosition = GlobalPosition + new Vector3(_spawnX, FindGroundY(_spawnX, _spawnZ), _spawnZ);                                    // Set monster spawn position
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

	private float FindGroundY(float targetX, float targetZ)
    {
        var query = new PhysicsRayQueryParameters3D();
        query.From = new Vector3(targetX, 100.0f, targetZ); 
        query.To = new Vector3(targetX, -100.0f, targetZ); 

        var spaceState = GetWorld3D().DirectSpaceState;
        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            Vector3 collisionPoint = (Vector3)result["position"];
            return collisionPoint.Y;
        }
        else
        {
            return 0f;
        }
    }

	// --- PROCESS LOOP ---
	public override void _Process(double delta)
	{
		//
	}
}
