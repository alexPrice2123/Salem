using Godot;
using System;
public partial class theCoiledOne : Monster3d
{
	private const int MaxResin = 1;
	private float _distance;
	private int _attackAnimSwitch = 1;
	public Godot.Collections.Array<Node3D> _resinArray { get; set; } = []; 
	private int _resinCount = 0;
	private MeshInstance3D _roots;
	public float _currentDamage = 0;
	public int _underbrushLeft = 2;
	public int _vinetanglerLeft = 1;
	public int _revenantLeft = 4; 
	
	[Export] public PackedScene _spawnRootScene { get; set; }
	[Export] public PackedScene _vineTangler { get; set; }
	[Export] public PackedScene _underBrush { get; set; }
	[Export] public PackedScene _revanant { get; set; }
	private CsgSphere3D _rangeObj;
	private int _spawnCount = 0;
	public string _animState = "Idle";
	public int _phase = 1;

	public override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = false;     // Can this monster move while attacking
		IsObject = true;              // Should gravity be applied to this monster
		Stationery = true;          // If the monster shouldnt move at all
		BaseDamage = 12.5f;         // Base damage of the monster
		AttackSpeed = 2.5f;         // The time between its attacks
		AttackRange = 1.5f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 200.0f;         // Maximum monster health
		WanderRange = 35;           // The range the monster can wander from its spawn point
		AgroFOV = 7.0f;          	// The vision FOV of the monster
		AgroLength = 5.5f;          // The detection length of the monsters vision
		WalkRange = 3.5f;	         	// The noise range monsters hear the player walking
		WalkSpeed = 1f;             // Movement speed when they are wandering
		RunSpeed = 3.5f;              // Movement speed when they are chasing the player 

		// -- Other -- //
		Monster = this;
		Initialization();

		foreach (Node3D roots in GetParent().GetParent().GetChildren())
        {
            if (((string)roots.Name).Contains("RootWall"))
            {
                foreach (Node3D resin in roots.GetChildren())
                {
					if (((string)resin.Name).Contains("Resin"))
                    {
                        _resinArray.Add(resin);
                    }
                }
            }
        }
		SpawnResin();
		_roots = GetNode<MeshInstance3D>("Roots");
		_roots.Visible = true;
		_rangeObj = GetNode<CsgSphere3D>("Range");
		_rangeObj.Visible = false;
	}

	public async void ResinBroken()
    {
		_resinCount -= 1;
		GD.Print(_resinCount+" Resin Left");
		if (_resinCount <= 0)
        {
			_roots.Visible = false;
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			_animState = "Stunned";
            //SpawnResin();
        }
		else{await ToSignal(GetTree().CreateTimer(1), "timeout"); _animState = "Idle";}
    }

	private void SpawnResin()
    {
		_currentDamage = 0;
		if (_roots != null){_roots.Visible = true;}
		_resinCount = 0;
		if (_phase == 2){return;}
        for (int i = 0; i < MaxResin; i++)
        {
            if (_resinArray.PickRandom() is Resin resInst)
            {
                resInst.Grow(this);
				_resinCount += 1;
            }
        }
    }

	private async void SpawnEnemy()
	{
		var scenes = new PackedScene[3];
		var decrements = new Action[3];
		int count = 0;

		if (_underbrushLeft > 0)  { scenes[count] = _underBrush;  decrements[count++] = () => _underbrushLeft--; }
		if (_vinetanglerLeft > 0) { scenes[count] = _vineTangler; decrements[count++] = () => _vinetanglerLeft--; }
		if (_revenantLeft > 0)    { scenes[count] = _revanant;    decrements[count++] = () => _revenantLeft--; }

		if (count == 0) return;

		int choice = _rng.RandiRange(0, count - 1);
		PackedScene monsterToSpawn = scenes[choice];
		decrements[choice]();

		Node3D rootInstance = _spawnRootScene.Instantiate<Node3D>();
		GetParent().AddChild(rootInstance);

		float maxRange = _rangeObj.Radius;
		Vector3 centerPos = _rangeObj.GlobalPosition;
		rootInstance.GlobalPosition = centerPos + new Vector3(
			_rng.RandfRange(-maxRange, maxRange), 0,
			_rng.RandfRange(-maxRange, maxRange));

		GD.Print(monsterToSpawn+" Trying");
		if (rootInstance is not SpawningRoot spawnRoot)
		{
			rootInstance.QueueFree();
			return;
		}
		GD.Print(monsterToSpawn+" Spawning");
		if (!_roots.Visible){_spawnCount = 0; return;}
		_animState = "Summon";
		await ToSignal(GetTree().CreateTimer(1.65), "timeout");
		GetNode<GpuParticles3D>("Body/Armature/Skeleton3D/Bone_012/Summon").Emitting = true;
		spawnRoot.SpawnMonster(monsterToSpawn, this);
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");	
		GetNode<GpuParticles3D>("Body/Armature/Skeleton3D/Bone_012/Summon").Emitting = false;
		_animState = "Idle";
		if (_resinCount <= 0 && _phase == 1)
        {
			_roots.Visible = false;
			_animState = "Stunned";
        }
	}

	private void TransitionPhase()
    {
		_phase = 2;
        foreach (Node3D roots in GetParent().GetParent().GetChildren())
        {
            if (((string)roots.Name).Contains("RootWall") && !((string)roots.Name).Contains("Stay"))
            {
				roots.QueueFree();
			}
			if (((string)roots.Name).Contains("UnderWall"))
            {
                roots.GlobalPosition = new Vector3(roots.GlobalPosition.X, -1, roots.GlobalPosition.Z);
            }
		}
		_rangeObj = GetNode<CsgSphere3D>("Range2");
		_player.GlobalPosition = _rangeObj.GlobalPosition + new Vector3(0, 1, 0);
		_roots.GlobalPosition = new Vector3(0, -10, 0);
    }

	public override void _Process(double delta)
	{
		EveryFrame(delta);
		if (_health <= MaxHealth / 2 && _phase == 1){TransitionPhase();}

		if (_roots.Visible){_spawnCount++;}
		if (_spawnCount > (300 +(Convert.ToInt32(_health < MaxHealth/2)*400)))
        {
            _spawnCount = _rng.RandiRange(-200, 25);
			SpawnEnemy();
        }
		if (_roots != null)
        {
            if (!_roots.Visible && _currentDamage >= 50f)
			{
				SpawnResin();
				_animState = "Idle";
			}
        }
		
		if (_health <= 0)
		{
			_player.MonsterKilled("theCoiledOne", Biome);
			if (Debug == true)
			{
				if (GetParent().GetParent() is DebugHut dh) { dh._shouldSpawn = true; }
			}
			int i = 0;
			if(Cutscene)
			{
				foreach (Monster3d monst in GetParent().GetChildren())
				{
					if (monst != this)
					{
						monst.ForceSeePlayer();
						i++;
					}
				}
				if (i == 0)
				{
					_player.GetParent().GetNode<Cutscene3>("Cutscene3").StartCut(_player);
				}
			}
			if(!Cutscene)
			{
				//_itemDropper.Drop("deadooze", 0.5f, 3, GlobalPosition);
				//_itemDropper.Drop("woundedooze", 0.25f, 2, GlobalPosition);
			}
			QueueFree();
		}
	}

	public void _on_hurtbox_area_entered(Area3D body){if(!_roots.Visible){Damaged(body);}}

	public void _on_attackbox_area_entered(Node3D body){TryHitPlayer(body, "None");}

	public async void Attack()
	{
		if (_attackAnimSwitch == 1) { _attackAnimSwitch = 2; }
		else { _attackAnimSwitch = 1; }

		_hasHit = false;
		_attackAnim = true;
		await ToSignal(GetTree().CreateTimer(0.48), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
		_canAttack = false;
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");
		_attackAnim = false;
		await ToSignal(GetTree().CreateTimer(AttackSpeed - 0.1f), "timeout");
		_canAttack = true;
	}
}
