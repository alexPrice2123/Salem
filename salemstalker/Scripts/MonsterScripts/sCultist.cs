using Godot;
using System;

public partial class sCultist : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private PackedScene _darkOrb = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/orb.tscn"); // Scene reference to the dark orb
	private float _distance;
	public Node3D _spawn;
	public float _projectileSpeed = 20f;
	private float _meleeRange = 3f;
	private float _meleeDamage = 0f;
	private int _attackAnimSwitch = 1;	
	private GpuParticles3D _leftArmMagic;
	private GpuParticles3D _rightArmMagic;
	private GpuParticles3D _magicOrbParticle;
	private MeshInstance3D _orb;
	private Vector3 _orbGoal = new Vector3(0f, 0f, 0f);
	private float _orbTweenTime = 1f;
	private bool _meleeAnim = false;
	private float _meleeCooldown = 0f;
	private bool _shouldPush = false;
	public override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = true;     // Can this monster move while attacking
		Flying = false;              // Should gravity be applied to this monster
		Stationery = false;          // If the monster shouldnt move at all
		BaseDamage = 20.0f;         // Base damage of the monster
		AttackSpeed = 3f;         // The time between its attacks
		AttackRange = 10f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 50.0f;         // Maximum monster health
		WanderRange = 40;           // The range the monster can wander from its spawn point
		AgroFOV = 10.0f;          	// The vision FOV of the monster
		AgroLength = 20.0f;          // The detection length of the monsters vision
		WalkRange = 8f;	         	// The noise range monsters hear the player walking
		WalkSpeed = 2.5f;             // Movement speed when they are wandering
		RunSpeed = 3.5f;              // Movement speed when they are chasing the player

		// -- Other -- //
		Monster = this;
		_spawn = GetNode<Node3D>("Spawn");
		Initialization();

		_leftArmMagic = GetNode<GpuParticles3D>("Body/metarig/Skeleton3D/arur_l/arur_l/Magic");
		_rightArmMagic = GetNode<GpuParticles3D>("Body/metarig/Skeleton3D/arua_r/arua_r/Magic");
		_magicOrbParticle = GetNode<GpuParticles3D>("Magic");
		_orb = GetNode<MeshInstance3D>("Orb");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		_distance = (GlobalPosition - _player.GlobalPosition).Length();
		if (_distance <= _meleeRange && !_canAttack && _meleeCooldown <= 0){_shouldPush = true;}
		if (_health <= 0)
		{
			_player.MonsterKilled("sCultist", Biome);
			if (Debug == true)
			{
				if (GetParent().GetParent() is DebugHut dh){ dh._shouldSpawn = true; }
			}
			QueueFree(); // Destroy monster when health hits zero
		}
		RotateFunc(delta);
		if (_meleeCooldown > 0f)
		{
			_meleeCooldown -= (float)delta;
		}
		_orb.Scale = _orb.Scale.Lerp(_orbGoal, _orbTweenTime * (float)delta);
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

	public void _on_hurtbox_area_entered(Area3D body)
	{
		Damaged(body);
	}

	public void _on_attackbox_area_entered(Node3D body)
	{
		if (body.IsInGroup("Player") && _hasHit == false && body.Name == "Hurtbox")
		{
			_player.Damaged(_meleeDamage, this as Monster3d, "Push");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		_shouldPush = false;
		if (_distance > _meleeRange || _meleeCooldown > 0f)
		{
			_hasHit = false;
			_attackAnim = true;
			_canAttack = false;
			_rightArmMagic.Emitting = false;
			_leftArmMagic.Emitting = false;
			_magicOrbParticle.Emitting = true;
			_orbTweenTime = 1f;
			_orbGoal = new Vector3(0.2f, 0.2f, 0.2f);
			_canAttack = false;
			_attackAnim = true;
			await ToSignal(GetTree().CreateTimer(AttackSpeed), "timeout");
			_canAttack = true;
			await ToSignal(GetTree().CreateTimer(0.92), "timeout");
			_canAttack = false;
			_rightArmMagic.Emitting = true;
			_leftArmMagic.Emitting = true;
			_orbTweenTime = 5f;
			_orbGoal = new Vector3(0f, 0f, 0f);
			_magicOrbParticle.Emitting = false;
			RigidBody3D projectileInstance = _darkOrb.Instantiate<RigidBody3D>(); // Create monster instance
			_player.GetParent().AddChild(projectileInstance);                                             // Add monster to holder node
			projectileInstance.GlobalPosition = _spawn.GlobalPosition;
			if (projectileInstance is Orb ball)
			{
				ball._playerOrb = _player;
				ball._damageOrb = BaseDamage + _damageOffset;
				ball.Shoot(_projectileSpeed);
			}
			for (int i = 0; i < AttackSpeed*10; i++)
			{
				await ToSignal(GetTree().CreateTimer(AttackSpeed/(AttackSpeed*10)), "timeout");
				if (AttackSpeed/(AttackSpeed*10)*i >= 0.995f){_attackAnim = false;}
				if (_shouldPush){break;}
			}
			_canAttack = true;
			_attackAnim = false;
		}
		else
		{
			_meleeCooldown = 7f;
			_hasHit = false;
			_meleeAnim = true;
			_canAttack = false;
			await ToSignal(GetTree().CreateTimer(0.85f), "timeout");
			GetNode<GpuParticles3D>("Push").Emitting = true;
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
			await ToSignal(GetTree().CreateTimer(0.2), "timeout");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
			_meleeAnim = false;
			await ToSignal(GetTree().CreateTimer(0.7), "timeout");
			_canAttack = true;
		}
	}
}
