using Godot;
using System;

public partial class revenanT : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private int _attackAnimSwitch = 1;
	private int _randomBody; 
	private int _randomAnim; 
	public async override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = true;     // Can this monster move while attacking
		Flying = false;              // Should gravity be applied to this monster
		Stationery = false;          // If the monster shouldnt move at all
		BaseDamage = 5.0f;         // Base damage of the monster
		AttackSpeed = 1.2f;         // The time between its attacks
		AttackRange = 1.5f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 45.0f;         // Maximum monster health
		WanderRange = 25;           // The range the monster can wander from its spawn point
		AgroFOV = 0.5f;          	// The vision FOV of the monster
		AgroLength = 5f;          // The detection length of the monsters vision
		WalkRange = 6f;	         	// The noise range monsters hear the player walking
		WalkSpeed = 1.5f;             // Movement speed when they are wandering
		RunSpeed = 3.5f;              // Movement speed when they are chasing the player

		// -- Other -- //
		Monster = this;
		//Body Randomization
		_randomBody = _rng.RandiRange(1,3);
		_randomAnim = _rng.RandiRange(1,2);
		//GD.Print(GetNode<AnimationTree>("AnimationTree" + _randomBody).TreeRoot.Get("parameters/idle")+"MALAMAR");
		GetNode<AnimationTree>("AnimationTree"+_randomBody).Set("parameters/idle/animation", "idle2");
		
		for (int i = 1; i < 4; i++)
		{
			if (i != _randomBody)
			{
				GetNode<Node3D>("Body"+i).QueueFree();
				GetNode<Node3D>("HitFX"+i).QueueFree();
				GetNode<AnimationTree>("AnimationTree"+i).QueueFree();
			}
			else
			{
			}
		}
		Initialization();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_player == null){return;}
		EveryFrame(delta);
		if (_health <= 0)
		{
			_player.MonsterKilled("revenanT", Biome);
			if (Debug == true)
			{
				if (GetParent().GetParent() is DebugHut dh){ dh._shouldSpawn = true; }
			}
			QueueFree(); // Destroy monster when health hits zero
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

	public void _on_hurtbox_area_entered(Area3D body)
	{
		Damaged(body);
	}

	public void _on_attackbox_area_entered(Node3D body)
	{
		if (body.IsInGroup("Player") && _hasHit == false && body.Name == "Hurtbox")
		{
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "None");
			_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		if (_attackAnimSwitch == 1)
		{
			_attackAnimSwitch = 2;
		}
		else
		{
			_attackAnimSwitch = 1;
		}
		_hasHit = false;
		_attackAnim = true;
		await ToSignal(GetTree().CreateTimer(0.48), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", true);
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		_attackBox.GetParent<Area3D>().SetDeferred("monitoring", false);
		_canAttack = false;
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");
		_attackAnim = false;
		await ToSignal(GetTree().CreateTimer(AttackSpeed-0.1f), "timeout");
		_canAttack = true;
	}
}
