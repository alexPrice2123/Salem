using Godot;
using System;
using System.Threading.Tasks;
public partial class theKyron : Monster3d
{
	private const int MaxResin = 6;
	public float _distance;
	private int _attackAnimSwitch = 1;
	public Godot.Collections.Array<Node3D> _resinArray { get; set; } = [];
	private PackedScene _darkOrb = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/bigOrb.tscn");
	private PackedScene _pullOrb = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/pullOrb.tscn");
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
	public float _downCount = 0;
	public string _animState = "Idle";
	public string _currentCutscene = "0";
	public int _phase = 1;
	private Godot.Collections.Array<ShaderMaterial> _matArray { get; set; } = [];
	public int _parryCounters = 1;
	private bool _active = false;
	public bool _transitioning = false;
	private bool _summoning = false;
	public float _legHealth = 100;
	private float _wanderCount = 199;
	private float _stompCountDown = 0;
	public Vector3 _goalLookPos;
	public bool _teleporting = false;

	public override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = false;     // Can this monster move while attacking
		Fleeing = true;              // Should gravity be applied to this monster
		Stationery = false;          // If the monster shouldnt move at all
		BaseDamage = 12.5f;         // Base damage of the monster
		AttackSpeed = 2.5f;         // The time between its attacks
		AttackRange = 1.5f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 800.0f;         // Maximum monster health
		
		AgroFOV = 7.0f;             // The vision FOV of the monster
		AgroLength = 5.5f;          // The detection length of the monsters vision
		WalkRange = 3.5f;               // The noise range monsters hear the player walking
		WalkSpeed = 1f;             // Movement speed when they are wandering
		RunSpeed = 3.5f;              // Movement speed when they are chasing the player 

		// -- Other -- //
		Monster = this;
		Initialization();

		_rangeObj = GetNode<CsgSphere3D>("Range");
		_rangeObj.Visible = false;
		WanderRange = (int)_rangeObj.Radius;           // The range the monster can wander from its spawn point
		_body = GetNode<Node3D>("Body");
		_startPos = GlobalPosition;

		foreach (var node in GetNode<Skeleton3D>("Body/metarig/Skeleton3D").GetChildren())
        {
			if (node is MeshInstance3D mesh)
            {
                for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
				{
					if (!_matArray.Contains(mesh.GetSurfaceOverrideMaterial(i) as ShaderMaterial))
                    {
                        _matArray.Add(mesh.GetSurfaceOverrideMaterial(i) as ShaderMaterial);
                    }
				}
            }
        }
		MeshInstance3D swordMesh = GetNode<MeshInstance3D>("Body2/metarig/Skeleton3D/hand_R/Cube");
		for (int i = 0; i < swordMesh.GetSurfaceOverrideMaterialCount(); i++)
		{
			if (!_matArray.Contains(swordMesh.GetSurfaceOverrideMaterial(i) as ShaderMaterial))
			{
				_matArray.Add(swordMesh.GetSurfaceOverrideMaterial(i) as ShaderMaterial);
			}
		}
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
/*
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
		GetNode<Camera3D>("Cutscene/Camera_001").Curxrent = false;
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
*/

	private async void TransitionPhase()
    {
		_phase = 2;
		_attacking = true;
		TweenMat(GetNode<MeshInstance3D>("Body2/metarig/Skeleton3D/hand_R/Cube").GetSurfaceOverrideMaterial(1) as ShaderMaterial, true);
		Node3D trans = GetNode<Node3D>("Transition");
        GetNode<Node3D>("Body").Visible = false;
		trans.Visible = true;
		trans.GetNode<AnimationPlayer>("AnimationPlayer").Play("ArmatureAction");
		trans.GetNode<AnimationPlayer>("AnimationPlayer2").Play("Armature_001Action");
		trans.GetNode<AnimationPlayer>("AnimationPlayer3").Play("metarigAction");
		await ToSignal(GetTree().CreateTimer(8.91), "timeout");
		TweenMat(GetNode<MeshInstance3D>("Body2/metarig/Skeleton3D/hand_R/Cube").GetSurfaceOverrideMaterial(1) as ShaderMaterial, false);
		await ToSignal(GetTree().CreateTimer(1.2), "timeout");
		trans.Visible = false;
		GetNode<Node3D>("Body2").Visible = true;
		ChangeHitbox(false);
		_body = GetNode<Node3D>("Body2");
		_legHealth = 100;
		_animState = "Idle";
		Teleport(Vector3.Zero, -1);
    }

	private void ChangeHitbox(bool toggle)// true is hurtbox1 false is hurtbox2
    {
        GetNode<Area3D>("Hurtbox").SetDeferred("monitoring", toggle);
		GetNode<Area3D>("Hurtbox2").SetDeferred("monitoring", !toggle);
		GetNode<CollisionShape3D>("Collision").SetDeferred("disabled", !toggle);
		GetNode<CollisionShape3D>("Collision2").SetDeferred("disabled", toggle);
    }
	private async void ChangeAnimState(string anim, float times)
    {
		_goalLookPos = _player.GlobalPosition;
        _animState = anim;
		await ToSignal(GetTree().CreateTimer(times), "timeout");
		if (_legHealth <= 0){return;}
		_animState = "Idle";
		GetNode<GpuParticles3D>("Stunned").Emitting = false;
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		if (_legHealth > 0){_attacking = false;}
    }
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		//if (_health <= MaxHealth / 2 && _phase == 1) { TransitionPhase(); }
		_distance = (GlobalPosition - _player.GlobalPosition).Length();
		_wanderCount++;
		_stompCountDown -= (float)delta;
		if (_active) { _player._inCombat = true; }
		if (_health <= 450 && _phase == 1){TransitionPhase();}
		if (_legHealth <= 0 && !_teleporting && _phase == 1){_animState = "Downed"; _downCount += (float)delta;}	//drink a bannanannana (yuri and yaoi)
		if (_distance <= 5 && !_attacking){_goalLookPos = _player.GlobalPosition;}
		if (_downCount >= 4)
		{
			Teleport(Vector3.Zero, -1);
		}
		if (!_attacking && (_wanderCount == 50 || (_distance <= 5 && _stompCountDown <= 0)))
        {
            ChooseAttack();
        }
		if ((_wanderCount == 25 || _wanderCount == 75 || _wanderCount == 125 || _wanderCount == 175) && _phase == 2)
        {
            WarpHole();
        }
		if (_wanderCount >= 200)
        {
            ChooseNewWander();
			_wanderCount = _rng.RandiRange(-150, 25);
        }
		if (_health <= 0 && _active)
		{
			DeathPhase();
		}
		RotateFunc(delta);
	}

	private void RotateFunc(double delta)
	{
		if (Mathf.RadToDeg(_lookDirection.GlobalRotation.Y) >= 175 || Mathf.RadToDeg(_lookDirection.GlobalRotation.Y) <= -175)
		{
			GlobalRotation = new Vector3(GlobalRotation.X, _lookDirection.GlobalRotation.Y, GlobalRotation.Z);
		}
		else
		{
			float newRotation = Mathf.Lerp(GlobalRotation.Y, _lookDirection.GlobalRotation.Y, (float)delta * 10f);
			GlobalRotation = new Vector3(GlobalRotation.X, newRotation, GlobalRotation.Z);
		}
	}

	public void _on_hurtbox_area_entered(Area3D body) { Damaged(body); }

	public void _on_attackbox_area_entered(Area3D body) { KyronHitPlayer(body, "Push"); }
	public void _on_jump_sword_area_entered(Area3D body) { KyronHitPlayer(body, "Push"); }

	private async void KyronHitPlayer(Node3D body, string extraArgs)
	{
		if (body is Area3D area && area.IsInGroup("PlayerHurtbox") && !_hasHit)
		{
            _player.Damaged(BaseDamage, this, extraArgs);
		}
	}

	/*
	private void _on_enter_area_entered(Area3D area)
	{
		if (area.IsInGroup("PlayerHurtbox")) { StartBattle(); }
	}*/

	private async void ChooseAttack()
	{
		if (_phase == 1)
        {
           if (_legHealth <= 0){return;}
			if (_distance <= 5 && _stompCountDown <= 0 && _animState == "Idle")
			{
				Stomp();
			}
			else
			{
				float randNum = _rng.RandiRange(1,2);
				if (randNum == 1)
				{SpitBall(1.6f, 1.26f);}
				else
				{
					float warpInt = _rng.RandiRange(1,4);
					if (warpInt == 1){BigWarp();}
					else{WarpHole();}
				} 
			} 
        }
        else
        {
            if (_distance <= 5 && _stompCountDown <= 0)
			{
				float randNum = _rng.RandiRange(1,2);
				if (randNum == 1)
				{MeleeAttack("JumpAttack", 1.6f, 1.25f, 0.2f);}
				else{MeleeAttack("Slice", 1.6f, 0.62f, 0.87f);}
			}
			else if (_distance > 15)
			{
				TPCombo();
			}
			else
			{
				float randNum = _rng.RandiRange(1,3);
				if (randNum > 1)
				{SpitBall(0.875f, 0.62f);}
				else
				{
					Lunge();
				} 
			} 
        }
		
	}

	private async void MeleeAttack(string anim, float totalLength, float initialWait, float hitboxToggle)
    {
		ChangeAnimState(anim, totalLength);
		_attacking = true;
		if (anim == "JumpAttack"){BaseDamage = 25;}else{BaseDamage = 10;}
		_stompCountDown = 5;
		await ToSignal(GetTree().CreateTimer(initialWait), "timeout");
        GetNode<Area3D>("Body2/metarig/Skeleton3D/hand_R/JumpSword").SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(hitboxToggle), "timeout");
		GetNode<Area3D>("Body2/metarig/Skeleton3D/hand_R/JumpSword").SetDeferred("monitoring", false);
		Teleport(Vector3.Zero, 0);
    }

	private async void Lunge()
    {
		ChangeAnimState("Lunge", 1.6f);
		_attacking = true;
		BaseDamage = 30;
		await ToSignal(GetTree().CreateTimer(1), "timeout");
        GetNode<Area3D>("Body2/metarig/Skeleton3D/hand_R/JumpSword").SetDeferred("monitoring", true);
		_goalLookPos = _player.GlobalPosition;
		ApplyKnockback(-40);
		await ToSignal(GetTree().CreateTimer(0.6), "timeout");
		GetNode<Area3D>("Body2/metarig/Skeleton3D/hand_R/JumpSword").SetDeferred("monitoring", false);
		await ToSignal(GetTree().CreateTimer(0.6), "timeout");
		MeleeAttack("Slice", 1.6f, 0.62f, 0.87f);
    }

	private async void TPCombo()
    {
		Teleport(_player.GlobalPosition, 4);
		await ToSignal(GetTree().CreateTimer(0.4), "timeout");
		ChangeAnimState("JumpThrust", 6f);
		_goalLookPos = _player.GlobalPosition;
		_attacking = true;
		BaseDamage = 15;
		await ToSignal(GetTree().CreateTimer(1.25f), "timeout");
        GetNode<Area3D>("Body2/metarig/Skeleton3D/hand_R/JumpSword").SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		GetNode<Area3D>("Body2/metarig/Skeleton3D/hand_R/JumpSword").SetDeferred("monitoring", false);
		Teleport(_player.GlobalPosition, 15);
		BaseDamage = 30;
		await ToSignal(GetTree().CreateTimer(1.3), "timeout");
		_goalLookPos = _player.GlobalPosition;
		ApplyKnockback(-50);
		GetNode<Area3D>("Body2/metarig/Skeleton3D/hand_R/JumpSword").SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(0.8), "timeout");
		GetNode<Area3D>("Body2/metarig/Skeleton3D/hand_R/JumpSword").SetDeferred("monitoring", false);
		_animState = "Done";
		GetNode<GpuParticles3D>("Stunned").Emitting = true;
    }

	private async void SpitBall(float totalLength, float firstWait)
    {
		ChangeAnimState("Spit", totalLength);
		_attacking = true;
		await ToSignal(GetTree().CreateTimer(firstWait), "timeout");
        RigidBody3D projectileInstance = _darkOrb.Instantiate<RigidBody3D>(); 
		_player.GetParent().AddChild(projectileInstance);                                            
		projectileInstance.GlobalPosition = _body.GetNode<MeshInstance3D>("metarig/Skeleton3D/spine_005/projectile").GlobalPosition;
		if (projectileInstance is bigOrb ball)
		{
			ball._playerOrb = _player;
			ball._damageOrb = BaseDamage + _damageOffset;
			ball.Shoot(20, _goalLookPos);
		}
    }
	private async void WarpHole()
    {
		if (_phase == 1)
        {
           ChangeAnimState("Warp", 2.1f);
			_attacking = true;
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			SpawnWarp(2);
        }
		else{SpawnWarp(22);}
	}

	private async void BigWarp()
    {
		ChangeAnimState("BigWarp", 2f);
		_attacking = true;
		await ToSignal(GetTree().CreateTimer(0.9), "timeout");
		for (int i = 0; i < 6; i++)
        {
            SpawnWarp(14);
			await ToSignal(GetTree().CreateTimer(0.1), "timeout");
        }
	}

	private async void Teleport(Vector3 pos, float dist)
    {
		_attacking = true;
		_teleporting = true;
		foreach (ShaderMaterial mat in _matArray){TweenMat(mat, true);}
		_downCount = 0;
		await ToSignal(GetTree().CreateTimer(1.3), "timeout");
		_animState = "Idle";
		if (pos != Vector3.Zero)
        {
           	GlobalPosition = _player.GlobalPosition + (-_player._head.GlobalTransform.Basis.Z.Normalized() * dist);
        }
        else
        {
			if (_phase == 1)
            {
                GlobalPosition = RandPosInRadius(new Vector3(_startPos.X, 0, _startPos.Z), 15);
				GD.Print(GlobalPosition);
				GD.Print(_startPos);
            }
            else
            {
                GlobalPosition = RandPosInRadius(_player.GlobalPosition, 15);
            }
            
        }
		
		foreach (ShaderMaterial mat in _matArray){TweenMat(mat, false);}
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");
		_teleporting = false;
		if (dist == -1)
        {
            _legHealth = 100;
			//_health -= 400;
			_attacking = false;
        }
	}

	private Vector3 RandPosInRadius(Vector3 pos, float radius)
	{
		float angle = _rng.Randf() * Mathf.Tau; // random angle 0-360
		return pos + new Vector3(
			Mathf.Cos(angle) * radius,
			0f,
			Mathf.Sin(angle) * radius
		);
	}

	private async void TweenMat(ShaderMaterial mat, bool toggle)// true = dissolve, false = undissolve
    {
		if (toggle)
        {
        	for (float i = -1; i < 1.5; i += 0.1f)
			{
				mat.SetShaderParameter("dissolveSlider", i);
				await ToSignal(GetTree().CreateTimer(0.01), "timeout");
			}
        }
        else
        {
            for (float i = 1.5f; i > -1; i -= 0.1f)
			{
				mat.SetShaderParameter("dissolveSlider", i);
				await ToSignal(GetTree().CreateTimer(0.01), "timeout");
			} 
        }
        
    }

	private void SpawnWarp(int _range)
    {
        RigidBody3D pullInstance = _pullOrb.Instantiate<RigidBody3D>(); 
		_player.GetParent().AddChild(pullInstance);                                            
		float randZ = _rng.RandiRange(-_range, _range);
		float randX = _rng.RandiRange(-_range, _range);
		Vector3 spawnPos = new Vector3(_player.GlobalPosition.X + randX, 0f, _player.GlobalPosition.Z + randZ);
		pullInstance.GlobalPosition = spawnPos;
		if (pullInstance is pullOrb pull)
		{
			pull._playerOrb = _player;
		}
    }

	private async void Stomp()
    {
		ChangeAnimState("Stomp", 4f);
		_stompCountDown = 5;
		BaseDamage = 20;
		_attacking = true;
		await ToSignal(GetTree().CreateTimer(1.23), "timeout");
		GetNode<GpuParticles3D>("Push").Emitting = true;
		GetNode<Area3D>("Attackbox").SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		GetNode<Area3D>("Attackbox").SetDeferred("monitoring", false);
		GetNode<GpuParticles3D>("Stunned2").Emitting = true;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<GpuParticles3D>("Stunned2").Emitting = false;
		if (_legHealth > 0){Teleport(Vector3.Zero, 0);}
	}

}
