using Godot;
using System;
using System.Threading.Tasks;
public partial class theKyron : Monster3d
{
	private const int MaxResin = 6;
	private float _distance;
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
	private Node3D _currentBody;

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
		MaxHealth = 700.0f;         // Maximum monster health
		
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
		_currentBody = GetNode<Node3D>("Body");

		foreach (var node in GetNode<Skeleton3D>("Body/metarig/Skeleton3D").GetChildren())
        {
			if (node is MeshInstance3D mesh)
            {
                for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
				{
					if (!_matArray.Contains(mesh.GetSurfaceOverrideMaterial(i) as ShaderMaterial))
                    {
                        _matArray.Add(mesh.GetSurfaceOverrideMaterial(i) as ShaderMaterial);
						GD.Print(mesh.GetSurfaceOverrideMaterial(i)+" MAT ARRAY");
                    }
				}
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
        GetNode<Node3D>("Body").Visible = false;
		GetNode<Node3D>("Body2").Visible = true;
		_currentBody = GetNode<Node3D>("Body2");

    }
	private async void ChangeAnimState(string anim, float times)
    {
        _animState = anim;
		await ToSignal(GetTree().CreateTimer(times), "timeout");
		_animState = "Idle";
		await ToSignal(GetTree().CreateTimer(0.6f), "timeout");
		_attacking = false;
    }
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		//if (_health <= MaxHealth / 2 && _phase == 1) { TransitionPhase(); }
		_distance = (GlobalPosition - _player.GlobalPosition).Length();
		_wanderCount++;
		_stompCountDown -= (float)delta;
		if (_active) { _player._inCombat = true; }
		if (_health <= 300 && _phase == 1){_phase = 2; TransitionPhase();}
		if (_legHealth <= 0 && !_attacking && _phase == 1){_animState = "Downed";}	//drink a bannanannana (yuri and yaoi)
		if (_legHealth <= 0 && !_attacking && _phase == 1) { _downCount += (float)delta; }
		if (_downCount >= 4)
		{
			Teleport();
		}
		if (_wanderCount == 50 || (_distance <= 5 && !_attacking))
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
		_legHealth -= 0.2f;
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

	public void _on_attackbox_area_entered(Area3D body) { TryHitPlayer(body, "Push"); }

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
			if (_distance <= 5 && _stompCountDown <= 0)
			{
				Stomp();
			}
			else
			{
				float randNum = _rng.RandiRange(1,2);
				if (randNum == 1)
				{SpitBall();}
				else
				{
					float warpInt = _rng.RandiRange(1,3);
					if (warpInt == 1){BigWarp();}
					else{WarpHole();}
				} 
			} 
        }
        else
        {
            
        }
		
	}

	private async void SpitBall()
    {
		ChangeAnimState("Spit", 1.6f);
		_attacking = true;
		await ToSignal(GetTree().CreateTimer(1.26f), "timeout");
        RigidBody3D projectileInstance = _darkOrb.Instantiate<RigidBody3D>(); 
		_player.GetParent().AddChild(projectileInstance);                                            
		projectileInstance.GlobalPosition = _currentBody.GetNode<MeshInstance3D>("metarig/Skeleton3D/spine_005/projectile").GlobalPosition;
		if (projectileInstance is bigOrb ball)
		{
			ball._playerOrb = _player;
			ball._damageOrb = BaseDamage + _damageOffset;
			ball.Shoot(20);
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

	private async void Teleport()
    {
		_attacking = true;
		GD.Print(_matArray.Count+" MAT COUNT");
		foreach (ShaderMaterial mat in _matArray){TweenMat(mat, true);}
		_downCount = 0;
		await ToSignal(GetTree().CreateTimer(1.3), "timeout");
		_animState = "Idle";
		GD.Print(_animState);
		float randZ = _startPos.Z + _rng.RandiRange(-WanderRange, WanderRange);
		float randX = _startPos.X + _rng.RandiRange(-WanderRange, WanderRange);
		GlobalPosition = new Vector3(randX, 0f, randZ);

		foreach (ShaderMaterial mat in _matArray){TweenMat(mat, false);}
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");
		_legHealth = 100;
		_health -= 400;
		_attacking = false;
	}

	private async void TweenMat(ShaderMaterial mat, bool toggle)// true = dissolve, false = undissolve
    {
		if (toggle)
        {
        	for (float i = -1; i < 1.5; i += 0.1f)
			{
				mat.SetShaderParameter("dissolveSlider", i);
				GD.Print(mat.GetShaderParameter("dissolveSlider")+" MAT VAL");
				await ToSignal(GetTree().CreateTimer(0.05), "timeout");
			}
        }
        else
        {
            for (float i = 1.5f; i > -1; i -= 0.1f)
			{
				mat.SetShaderParameter("dissolveSlider", i);
				await ToSignal(GetTree().CreateTimer(0.05), "timeout");
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
		ChangeAnimState("Stomp", 1.5f);
		_attacking = true;
		await ToSignal(GetTree().CreateTimer(1.23), "timeout");
		GetNode<GpuParticles3D>("Push").Emitting = true;
		GetNode<Area3D>("Attackbox").SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		GetNode<Area3D>("Attackbox").SetDeferred("monitoring", false);
	}

	public async void PlayerParried()
	{
		if (_animState == "Smash") { return; }
		else { _parryCounters++; _hasHit = true; _animState = "Hit"; _attacking = false; }
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
