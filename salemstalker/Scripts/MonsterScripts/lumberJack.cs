using Godot;
using System;

public partial class lumberJack : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private PackedScene _axe = GD.Load<PackedScene>("res://Scenes/Monsters/MonsterAssets/lumberAxe.tscn"); // Scene reference to the dark orb
	private float _distance;
	public Node3D _spawn;
	public float _projectileSpeed = 20f;
	private float _meleeRange = 4f;
	private float _meleeDamage = 15f;
	private bool _meleeAnim = false;
	public bool _grabAnim = false;
	public bool _hasAxe = true;
	public lumberAxe _currentAxe;
	public bool _playerHit = false;
	public BoneAttachment3D _handAxe;
	private MeshInstance3D _axeArm;
	private MeshInstance3D _skeleHand;
	private ShaderMaterial _skeleShader;
	private ShaderMaterial _axeShader;
	private float _skeleDissolve = -1f;
	private float _axeDissolve = 1.5f;
	public override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = true;     // Can this monster move while attacking
		Flying = false;              // Should gravity be applied to this monster
		Stationery = false;          // If the monster shouldnt move at all
		BaseDamage = 15.0f;         // Base damage of the monster
		AttackSpeed = 3f;         // The time between its attacks
		AttackRange = 10f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 60.0f;         // Maximum monster health
		WanderRange = 10;           // The range the monster can wander from its spawn point
		AgroFOV = 0.5f;          	// The vision FOV of the monster
		AgroLength = 25.0f;          // The detection length of the monsters vision
		WalkRange = 6f;	         	// The noise range monsters hear the player walking
		WalkSpeed = 2.5f;             // Movement speed when they are wandering
		RunSpeed = 4.5f;              // Movement speed when they are chasing the player

		// -- Other -- //
		Monster = this;
		_spawn = GetNode<Node3D>("Body/metarig/Skeleton3D/hand_L/Spawn");
		Initialization();
		_handAxe = GetNode<BoneAttachment3D>("Body/metarig/Skeleton3D/hand_L");
		_axeArm = GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/axe_arm");
		_skeleHand = GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/skeleton");
		_skeleShader = GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/skeleton").MaterialOverride as ShaderMaterial;
		_axeShader = GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/axe_arm").MaterialOverride as ShaderMaterial;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		_distance = (GlobalPosition - _player.GlobalPosition).Length();
		if (_distance > _meleeRange){AttackRange = 10f;}
		if (_health <= 0)
		{
			_player.MonsterKilled("lumberJack", Biome);
			if (Debug == true)
			{
				if (GetParent().GetParent() is DebugHut dh){ dh._shouldSpawn = true; }
			}
			QueueFree(); // Destroy monster when health hits zero
		}
		RotateFunc(delta);
		_skeleShader.SetShaderParameter("dissolveSlider", Mathf.Lerp((float)_skeleShader.GetShaderParameter("dissolveSlider"), _skeleDissolve, (float)delta));
		_axeShader.SetShaderParameter("dissolveSlider", Mathf.Lerp((float)_axeShader.GetShaderParameter("dissolveSlider"), _axeDissolve, (float)delta/1.5f));
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
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "None");
			_attackBox.Disabled = true;
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		
		if (_distance > _meleeRange || !_hasAxe)
		{
			if (_hasAxe)
			{
				GD.Print("Throwing Axe");
				_hasAxe = false;
				_playerHit = false;
				_hasHit = false;
				_attackAnim = true;
				_canAttack = false;
				await ToSignal(GetTree().CreateTimer(1.5), "timeout");
				_handAxe.Visible = false;
				RigidBody3D projectileInstance = _axe.Instantiate<RigidBody3D>(); // Create monster instance
				_player.GetParent().AddChild(projectileInstance);                                             // Add monster to holder node
				projectileInstance.GlobalPosition = _spawn.GlobalPosition;
				if (projectileInstance is lumberAxe ball)
				{
					_currentAxe = ball;
					ball._lumberJack = this;
					ball._playerOrb = _player;
					ball._damageOrb = BaseDamage + _damageOffset;
					ball._returning = false;
					ball.Shoot(_projectileSpeed);
				}
				await ToSignal(GetTree().CreateTimer(1), "timeout");
				_attackAnim = false;
				_canAttack = true;
			}
			else if (_currentAxe != null)
			{
				GD.Print("Retreving Axe");
				_hasHit = false;
				_canAttack = false;
				_grabAnim = true;
				if (!_playerHit)
				{
					await ToSignal(GetTree().CreateTimer(0.6), "timeout");
					_currentAxe._returning = true;
					_currentAxe.ReturnToUser(_projectileSpeed);
				}
				await ToSignal(GetTree().CreateTimer(AttackSpeed), "timeout");
				_canAttack = true;
			}
			else
			{
				_hasAxe = true;
				_stunned = true;
				_hasHit = false;
				_canAttack = false;
				await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
				//Start Charge
				_axeDissolve = -1f;
				await ToSignal(GetTree().CreateTimer(1.75f), "timeout");
				//Rip off arm
				_skeleDissolve = 1.5f;
				_skeleShader.SetShaderParameter("dissolveSlider", _skeleDissolve);
				_axeDissolve = 1.5f;
				_axeShader.SetShaderParameter("dissolveSlider", _axeDissolve);
				_handAxe.Visible = true;
				await ToSignal(GetTree().CreateTimer(0.85f), "timeout");
				//Charge arm
				_skeleDissolve = -1f;
				await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
				_stunned = false;
				await ToSignal(GetTree().CreateTimer(1f), "timeout");
				_canAttack = true;
			}
		}
		else
		{
			AttackRange = 1.5f;
			_hasHit = false;
			_meleeAnim = true;
			await ToSignal(GetTree().CreateTimer(0.74f), "timeout");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
			_canAttack = false;
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			_meleeAnim = false;
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			_canAttack = true;
		}
	}
}
