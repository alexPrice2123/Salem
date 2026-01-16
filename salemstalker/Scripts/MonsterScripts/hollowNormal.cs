using Godot;
using System;

public partial class hollowNormal : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private int _attackAnimSwitch = 1;
	public override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = true;     // Can this monster move while attacking
		Flying = false;              // Should gravity be applied to this monster
		Stationery = false;          // If the monster shouldnt move at all
		BaseDamage = 15.0f;         // Base damage of the monster
		AttackSpeed = 1.2f;         // The time between its attacks
		AttackRange = 1.5f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 65.0f;         // Maximum monster health
		WanderRange = 35;           // The range the monster can wander from its spawn point
		AgroFOV = 7f;          	// The vision FOV of the monster
		AgroLength = 6.5f;          // The detection length of the monsters vision
		WalkRange = 4.5f;	         	// The noise range monsters hear the player walking
		WalkSpeed = 1.5f;             // Movement speed when they are wandering
		RunSpeed = 4.5f;              // Movement speed when they are chasing the player

		// -- Other -- //
		Monster = this;
		Initialization();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		if (_health <= 0)
		{
			_player.MonsterKilled("hollowNormal", Biome);
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
			_attackBox.Disabled = true;
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
		_speedOffset = 2.5f;
		_attackBox.GetParent<Area3D>().Monitoring = true;
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		_attackBox.GetParent<Area3D>().Monitoring = false;
		_canAttack = false;
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");
		_attackAnim = false;
		await ToSignal(GetTree().CreateTimer(AttackSpeed-0.1f), "timeout");
		_canAttack = true;
	}
}
