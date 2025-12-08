using Godot;
using System;

public partial class hollowNormal : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private int _attackAnimSwitch = 1;
	public override void _Ready()
	{
		Speed = 2.5f;             // Movement speed
		RunSpeed = 2.5f;			//How much gets added to the speed (ex. Speed = 2.5, RunSpeed = 2.5; The monsters speed is 5 because Speed+RunSpeed)
		MaxHealth = 60.0f;         // Maximum monster health
		Range = 15.0f;            // Detection range for chasing
		agroRange = 5.0f;            // Detection range for chasing
		SpawnDistance = 100;    // Distance from player before despawning
		BaseDamage = 15.0f;
		WanderRange = 50;
		AttackSpeed = 0.6f;
		AttackRange = 1.5f;
		Monster = this;
		Chaser = true;
		MoveWhileAttack = true;
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
