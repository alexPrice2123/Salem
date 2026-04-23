using Godot;
using System;
using System.Threading.Tasks;
public partial class theCoiledOne : Monster3d
{
	private const int MaxResin = 6;
	private float _distance;
	private int _attackAnimSwitch = 1;
	public Godot.Collections.Array<Node3D> _resinArray { get; set; } = []; 
	private PackedScene _poisonBall = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/poisonBall.tscn");
	private int _resinCount = 0;
	public MeshInstance3D _roots;
	public float _currentDamage = 0;
	public int _underbrushLeft = 2;
	public int _vinetanglerLeft = 1;
	public int _revenantLeft = 4; 
	
	[Export] public PackedScene _spawnRootScene { get; set; }
	[Export] public PackedScene _vineTangler { get; set; }
	[Export] public PackedScene _underBrush { get; set; }
	[Export] public PackedScene _revanant { get; set; }
	private CsgSphere3D _rangeObj;
	public int _spawnCount = 0;
	public int _moveCount = 0;
	public string _animState = "Idle";
	public string _currentCutscene = "0";
	public int _phase = 1;
	private Godot.Collections.Array<string> _playerFieldPos { get; set; } = [];
	public int _parryCounters = 0;
	private bool _active = false;
	public bool _transitioning = false;
	private bool _summoning = false;
	

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
            if (((string)roots.Name).Contains("RootWall") && !((string)roots.Name).Contains("Stay"))
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
		if (_resinCount <= 0)
        {
			_roots.Visible = false;
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			_animState = "Stunned";
        }
		else{await ToSignal(GetTree().CreateTimer(1), "timeout"); _animState = "Idle";}
    }

	private void SpawnResin()
    {
		_currentDamage = 0;
		if (_roots != null){_roots.Visible = true;}
		_resinCount = 0;
		if (_health <= MaxHealth / 2){return;}
		_animState = "Idle";
		_player.Damaged(0, this, "Push");
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
		_summoning = true;
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

		if (rootInstance is not SpawningRoot spawnRoot)
		{
			rootInstance.QueueFree();
			return;
		}
		if (!_roots.Visible){_spawnCount = 0; return;}
		_animState = "Summon";
		await ToSignal(GetTree().CreateTimer(1.65/_phase), "timeout");
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
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");	
		_summoning = false;
	}

	private async void DeathPhase()
    {
		_active = false;
        _player.MonsterKilled("theCoiledOne", Biome);
		_player.CutsceneToggle(true);
		_player.GetNode<Ui>("UI")._fadeProg = 1;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<Camera3D>("Cutscene/Camera").Current = true;
		_player.GetNode<Ui>("UI")._fadeProg = 0;
		_currentCutscene = "3";
		await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
		_animState = "Dead";
		foreach (Node3D roots in GetParent().GetParent().GetChildren())
        {
            if (((string)roots.Name).Contains("UnderWallDown"))
            {
				roots.Position = new Vector3(roots.Position.X, -3, roots.Position.Z);
			}
			if (((string)roots.Name).Contains("UnderWallGone"))
            {
                roots.QueueFree();
            }
		}
		await ToSignal(GetTree().CreateTimer(5f), "timeout");
		_player.GetNode<Ui>("UI")._fadeProg = 1;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<Camera3D>("Cutscene/Camera").Current = true;
		_player.GetNode<Ui>("UI")._fadeProg = 0;
		_player.CutsceneToggle(false);
		QueueFree();
    }

	private async void TransitionPhase()
    {
		_attacking = true;
		_phase = 2;
		_currentDamage = 100;
		_transitioning = true;
        foreach (Node3D roots in GetParent().GetParent().GetChildren())
        {
            if (((string)roots.Name).Contains("RootWall") && !((string)roots.Name).Contains("Stay"))
            {
				roots.QueueFree();
			}
			if (((string)roots.Name).Contains("UnderWall"))
            {
                roots.Position = new Vector3(roots.Position.X, -1, roots.Position.Z);
            }
		}
		_rangeObj = GetNode<CsgSphere3D>("Range2");
		_player.GlobalPosition = _rangeObj.GlobalPosition + new Vector3(0, 1, 0);
		_roots.GlobalPosition = new Vector3(0, -10, 0);

		_player.CutsceneToggle(true);
		_player.GetNode<Ui>("UI")._fadeProg = 1;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<Camera3D>("Cutscene/Camera").Current = true;
		_player.GetNode<Ui>("UI")._fadeProg = 0;
		await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
		_currentCutscene = "1.5";
		_animState = "Cutscene2";
		await ToSignal(GetTree().CreateTimer(0.99), "timeout");
		GetNode<Camera3D>("Cutscene/Camera_001").Current = true;
		GetNode<Camera3D>("Cutscene/Camera").Current = false;
		await ToSignal(GetTree().CreateTimer(0.97), "timeout");
		GetNode<Camera3D>("Cutscene/Camera_002").Current = true;
		GetNode<Camera3D>("Cutscene/Camera_001").Current = false;
		await ToSignal(GetTree().CreateTimer(0.87), "timeout");
		GetNode<Camera3D>("Cutscene/Camera_002").Current = false;
		GetNode<Camera3D>("Cutscene/Camera").Current = true;
		await ToSignal(GetTree().CreateTimer(1.97), "timeout");
		_player.GetNode<Ui>("UI")._fadeProg = 1;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<Camera3D>("Cutscene/Camera").Current = false;
		_player.GetNode<Ui>("UI")._fadeProg = 0;
		_animState = "Idle";
		_player.GetNode<Camera3D>("Head/Camera3D").Current = true;
		_player.CutsceneToggle(false);
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		_transitioning = false;
		_attacking = false;
    }

	public override void _Process(double delta)
	{
		EveryFrame(delta);
		if (_health <= MaxHealth / 2 && _phase == 1){TransitionPhase();}
		if (_active){_player._inCombat = true;}
		if (_roots.Visible && _active){_spawnCount++;}
		if (_phase == 2 && !_attacking && _roots.Visible){_moveCount++;}
		if (_moveCount >= 75)
        {
			if (!_summoning && _animState != "Stunned")
            {
                _moveCount = _rng.RandiRange(-50, 25);
				ChooseAttack();
            }
            
        }
		if (_spawnCount > (300*(_phase*1.5)))
        {
			if (!_attacking && _animState != "Stunned")
            {
               _spawnCount = _rng.RandiRange(-200, 25);
				SpawnEnemy(); 
            }
        }
		if (_roots != null)
        {
            if (!_roots.Visible && _currentDamage >= 50f)
			{
				SpawnResin();
			}
        }
		
		if (_health <= 0 && _active)
		{
			DeathPhase();
		}
	}

	public void _on_hurtbox_area_entered(Area3D body){if(_animState == "Stunned"){Damaged(body);}}

	public void _on_attackbox_area_entered(Area3D body){CoiledHitPlayer(body, "Push", 30f);}

	private void _on_right_box_area_entered(Area3D area)
    {
		if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Add("Right");}
    }
	private void _on_right_box_area_exited(Area3D area)
    {
		if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Remove("Right"); }   
    }

	private void _on_left_box_area_entered(Area3D area)
    {
		if (area.IsInGroup("PlayerHurtbox")){ _playerFieldPos.Add("Left");}
    }
	private void _on_left_box_area_exited(Area3D area)
    {
		if (area.IsInGroup("PlayerHurtbox")){ _playerFieldPos.Remove("Left");} 
    }

	private void _on_middle_box_area_entered(Area3D area)
    {
		if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Add("Middle");} 
    }
	private void _on_middle_box_area_exited(Area3D area)
    {
		if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Remove("Middle");} 
    }
	private void _on_spit_box_area_entered(Area3D area)
    {
        if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Add("Spit");} 
    }

	private void _on_spit_box_area_exited(Area3D area)
    {
        if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Remove("Spit");} 
    }

	private void _on_left_spike_area_entered(Area3D area)
    {
        if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Add("LeftSpit");} 
    }

	private void _on_left_spike_area_exited(Area3D area)
    {
        if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Remove("LeftSpit");} 
    }

	private void _on_right_spike_area_entered(Area3D area)
    {
        if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Add("RightSpit");} 
    }

	private void _on_right_spike_area_exited(Area3D area)
    {
        if (area.IsInGroup("PlayerHurtbox")){_playerFieldPos.Remove("RightSpit");} 
    }

	private void _on_enter_area_entered(Area3D area)
    {
        if (area.IsInGroup("PlayerHurtbox")){StartBattle();} 
    }

	private async void StartBattle()
    {
		GetNode<Area3D>("Enter").SetDeferred("monitoring", false);
		_player.GlobalPosition = _rangeObj.GlobalPosition + new Vector3(0, 1, 0);
        foreach (Node3D roots in GetParent().GetParent().GetChildren())
        {
            if (((string)roots.Name).Contains("RootWall"))
            {
                roots.Position = new Vector3(roots.Position.X, -1, roots.Position.Z);
            }
		}
		_player.CutsceneToggle(true);
		_player.GetNode<Ui>("UI")._fadeProg = 1;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<Camera3D>("Cutscene/Camera").Current = true;
		_player.GetNode<Ui>("UI")._fadeProg = 0;
		await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
		_currentCutscene = "1";
		_animState = "Cutscene1";
		await ToSignal(GetTree().CreateTimer(4.3), "timeout");
		_player.GetNode<Ui>("UI")._fadeProg = 1;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<Camera3D>("Cutscene/Camera").Current = false;
		_player.GetNode<Ui>("UI")._fadeProg = 0;
		_animState = "Idle";
		_player.GetNode<Camera3D>("Head/Camera3D").Current = true;
		_player.CutsceneToggle(false);
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		_active = true;
	}

	private async void ChooseAttack()
    {
		if (!_active){return;}
		_attacking = true;
		_hasHit = false;
        if (_playerFieldPos.Count > 0)
        {
            string plrSpot = _playerFieldPos.PickRandom();
			if (plrSpot == "Right")
            {
                int attackChoice = _rng.RandiRange(1,2);
				if (attackChoice == 1)
                {
                    await RightSwipe();
                }
                else
                {
                    await Poke();
                }
            }
			else if (plrSpot == "Left")
            {
                int attackChoice = _rng.RandiRange(1,2);
				if (attackChoice == 1)
                {
                    await LeftSwipe();
                }
                else
                {
                    await Poke();
                }
            }
            else if (plrSpot == "Middle")
            {
                 int attackChoice = _rng.RandiRange(1,2);
				if (attackChoice == 1)
                {
                    await Smash();
                }
                else
                {
                    await Poke();
                }
            }
            else if (plrSpot == "Spit")
            {
                await Spit();
            }
			else if (plrSpot == "LeftSpit")
            {
                await SpitBall("SpitL");
            }
			else if (plrSpot == "RightSpit")
            {
                await SpitBall("SpitR");
            }
        }
		if (_animState != "Hit" && _animState != "Stunned")
        {
            _animState = "Idle";
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			_attacking = false;
        }
    }

	private async Task<bool> RightSwipe()
    {
        _animState = "RightSwipe";
		GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_r/RightAttackBox").SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(1.6), "timeout");
		GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_r/RightAttackBox").SetDeferred("monitoring", false);
		return true;
    }
	private async Task<bool> LeftSwipe()
    {
       _animState = "LeftSwipe";
	   GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_l/LeftAttackBox").SetDeferred("monitoring", true);
	   await ToSignal(GetTree().CreateTimer(1.2), "timeout");
	   GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_l/LeftAttackBox").SetDeferred("monitoring", false);
		return true;
    }
	private async Task<bool> Smash()
    {
        _animState = "Smash";
		await ToSignal(GetTree().CreateTimer(1.33), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
		GetNode<GpuParticles3D>("Push").Emitting = true;
		await ToSignal(GetTree().CreateTimer(0.07), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
		return true;
    }
	private async Task<bool> Poke()
    {
        _animState = "Poke";
		GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_r/RightAttackBox").SetDeferred("monitoring", true);
		GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_l/LeftAttackBox").SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(2.1), "timeout");
		GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_r/RightAttackBox").SetDeferred("monitoring", false);
		GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_l/LeftAttackBox").SetDeferred("monitoring", false);
		return true;
    }

	private async Task<bool> SpitBall(string anim)
    {
        _animState = anim;
		await ToSignal(GetTree().CreateTimer(0.85), "timeout");
		RigidBody3D projectileInstance = _poisonBall.Instantiate<RigidBody3D>();
		_player.GetParent().AddChild(projectileInstance);
		projectileInstance.GlobalPosition = GetNode<GpuParticles3D>("Body/Armature/Skeleton3D/Bone_012/Spit").GlobalPosition;
		if (projectileInstance is poisonBall ball)
		{
			ball._playerOrb = _player;
			ball._damageOrb = 10;
			ball.Shoot(10);
		}
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		return true;
    }
 
	private async Task<bool> Spit()
    {
        _animState = "Spit";
		_moveCount = _rng.RandiRange(-100, -50);
		await ToSignal(GetTree().CreateTimer(0.67), "timeout");
		GetNode<GpuParticles3D>("Body/Armature/Skeleton3D/Bone_012/Spit").Emitting = true;
		for (int i = 0; i < 3; i++)
        {
            await ToSignal(GetTree().CreateTimer(0.2), "timeout");
			if (_playerFieldPos.Contains("Spit") && !_hasHit){_player.Damaged(10, this, "None"); _hasHit = true;}
        }
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");
		GetNode<GpuParticles3D>("Body/Armature/Skeleton3D/Bone_012/Spit").Emitting = false;
		await ToSignal(GetTree().CreateTimer(0.4), "timeout");
		return true;
    }

	private void _on_left_attack_box_area_entered(Area3D area)
    {
        CoiledHitPlayer(area, "None", 20);
    }
	private void _on_right_attack_box_area_entered(Area3D area)
    {
        CoiledHitPlayer(area, "None", 20);
    }

	private void CoiledHitPlayer(Area3D body, string extraArgs, float damage)
    {
        if (body is Area3D area && area.IsInGroup("PlayerHurtbox") && !_hasHit)
		{
			_hasHit = true;
			_player.Damaged(damage, this, extraArgs);
		}
    }

	public async void PlayerParried()
    {
		if (_animState == "Smash"){return;}
		else{_parryCounters ++; _hasHit = true; _animState = "Hit"; _attacking = false; _moveCount = 0;}
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		if (_parryCounters >= 2)
        {
            _parryCounters = 0;
			_hasHit = true;
			_animState = "Stunned";
			_roots.Visible = false;
			GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_r/RightAttackBox").SetDeferred("monitoring", false);
			GetNode<Area3D>("Body/Armature/Skeleton3D/Bone_007_l/LeftAttackBox").SetDeferred("monitoring", false);
        }
    }
}
