using Godot;
using System;

public partial class theHushedBark : Node3D
{
	// Called when the node enters the scene tree for the first time.
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Node3D _targetTree;
	private float _enemySpawnRadius = 10f;
	private Player3d _player;
	private bool _canBeHit = true;
	private Node3D _hitFX;
	private Node3D _body;
	private int _maxTrees = 3;
	private const float MaxHealth = 175f;
	private float _health = 175f;
	private float _distance;
	private bool _teleporting = false;
	private float _spikeDamage = 0 * 35f;
	private Node3D _holder;
	private float SpawnRange;
	private PackedScene _vineTangler = GD.Load<PackedScene>("res://Scenes/Monsters/vineTangler.tscn");
	private PackedScene _underBrush = GD.Load<PackedScene>("res://Scenes/Monsters/underBrush.tscn");
	private PackedScene _weepingSpine = GD.Load<PackedScene>("res://Scenes/Monsters/weepingSpine.tscn");
	public int _weepingCount = 0;
	public override void _Ready()
    {
		_rng.Randomize();
		_player = this.GetParent().GetNode<Player3d>("Player_3d");
		string rngNumber = _rng.RandiRange(1, _maxTrees).ToString();
		_targetTree = GetNode<Node3D>("Tree" + rngNumber);
		_targetTree.GetNode<Decal>("Face").Visible = true;
		_holder = GetParent().GetNode<Node3D>("MonsterHolder/Hold2/Hold");
		SpawnRange = GetNode<OmniLight3D>("Range").OmniRange;

		GetNode<Timer>("TanglerTimer").Start();
		GetNode<Timer>("UnderbrushTimer").Start();
		GetNode<Timer>("WeepingTimer").Start();

		_hitFX = _targetTree.GetNode<Node3D>("HitFX");
		_body = _targetTree.GetNode<Node3D>("BossTree");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        _distance = (_player.GlobalPosition - _targetTree.GlobalPosition).Length();
    }

	public async void Damaged(Area3D body, Node3D tree)
	{
		if (body.IsInGroup("Weapon") && _canBeHit)
		{
			// Quick visual hit reaction
			//_hitFX.GetNode<AnimationPlayer>("AnimationPlayer").Play("idle");
			if (tree == _targetTree)
            {
				_hitFX.Visible = true;
				_body.Visible = false; 
            }
			_canBeHit = false;
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

			// Reduce health
			if (tree == _targetTree)
			{
				_health -= _player._damage;
				_hitFX.Visible = false;
				_body.Visible = true;
			}

			if (_teleporting == false)
			{
				_teleporting = true;
				DefensiveAttack(tree);
			}
			else
			{
				DamageCooldown();
			}
		}
	}

	private async void DamageCooldown()
    {
       await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
		_canBeHit = true; 
    }
	private async void DefensiveAttack(Node3D tree)
	{
		if (tree == _targetTree)
		{
			if (_health > MaxHealth / 2)
			{
				_targetTree.GetNode<GpuParticles3D>("Charge").Emitting = true;
				await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
				_canBeHit = true;
				await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
				_targetTree.GetNode<GpuParticles3D>("Charge").Emitting = false;
				SelectNewTree();
				await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
				_teleporting = false;
			}
			else
			{
				_targetTree.GetNode<GpuParticles3D>("Charge").Emitting = true;
				await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
				_canBeHit = true;
				await ToSignal(GetTree().CreateTimer(0.75f), "timeout");
				_targetTree.GetNode<GpuParticles3D>("Charge").Emitting = false;
				_targetTree.GetNode<AnimationPlayer>("AnimationPlayer").Play("Spikes");
				_targetTree.GetNode<GpuParticles3D>("Attack").Emitting = true;
				SelectNewTree();
				if (_distance <= 9f)
				{
					_player.Damaged(_spikeDamage, null, "BarkSpikes");
				}
				await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
				_canBeHit = true;
				_teleporting = false;
			}
		}
        else
        {
			tree.GetNode<AnimationPlayer>("AnimationPlayer").Play("Spikes");
			tree.GetNode<GpuParticles3D>("Attack").Emitting = true;
			_player.Damaged(_spikeDamage, null, "BarkSpikes");
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			_canBeHit = true;
			_teleporting = false;
        }
	}

	private void SelectNewTree()
	{
		string rngNumber = _rng.RandiRange(1, _maxTrees).ToString();
		if ("Tree" + rngNumber == _targetTree.Name)
		{
			if (rngNumber.ToInt() + 1 > _maxTrees)
			{
				rngNumber = "1";
			}
			else
			{
				rngNumber = (rngNumber.ToInt() + 1).ToString();
			}
		}
		_targetTree.GetNode<Decal>("Face").Visible = false;
		_targetTree = GetNode<Node3D>("Tree" + rngNumber);
		_targetTree.GetNode<Decal>("Face").Visible = true;
		_hitFX.Visible = false;
		_body.Visible = true;
		_hitFX = _targetTree.GetNode<Node3D>("HitFX");
		_body = _targetTree.GetNode<Node3D>("BossTree");
	}

	private void _on_tangler_timer_timeout()
	{
		SpawnMonster(_vineTangler);
	}

	private void _on_underbrush_timer_timeout()
	{
		SpawnMonster(_underBrush);
	}
	
	private void _on_weeping_timer_timeout()
    {
		if (_weepingCount < 5)
        {
           SpawnMonster(_weepingSpine); 
		   _weepingCount += 1;
        }
    }
	private void SpawnMonster(PackedScene spawnedMonster)
	{
		CharacterBody3D monsterInstance = spawnedMonster.Instantiate<CharacterBody3D>(); // Create monster instance                                           // Add monster to holder node
		float _spawnX = _rng.RandfRange(-SpawnRange, SpawnRange);
		float _spawnZ = _rng.RandfRange(-SpawnRange, SpawnRange);
		monsterInstance.GlobalPosition = GlobalPosition + new Vector3(_spawnX, FindGroundY(_spawnX, _spawnZ), _spawnZ);                                    // Set monster spawn position
		_holder.AddChild(monsterInstance);  
		if (monsterInstance is Monster3d monster)
		{
			monster.RandomRangedPosition();
			monster.Biome = "Swamp";
		}
		if (monsterInstance is weepingSpine ws)
        {
            ws._hushSpawned = true;
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
}
