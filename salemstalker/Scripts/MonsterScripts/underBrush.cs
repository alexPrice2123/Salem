using Godot;
using System;

public partial class underBrush : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private float _attackOffset = 0.437f;
	public float _currentAttackOffset = 0f;
	private float _countDown = 5f;
	private int _attackAnimSwitch = 1;
	public override void _Ready()
	{
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = true;     // Can this monster move while attacking
		Flying = false;              // Should gravity be applied to this monster
		Stationery = false;          // If the monster shouldnt move at all
		BaseDamage = 10.0f;         // Base damage of the monster
		AttackSpeed = 0.5f;         // The time between its attacks
		AttackRange = 1f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 100.0f;         // Maximum monster health
		WanderRange = 10;           // The range the monster can wander from its spawn point
		AgroFOV = 5.0f;          	// The vision FOV of the monster
		AgroLength = 5.0f;          // The detection length of the monsters vision
		WalkRange = 15.0f;          // Walk hearing detection (sprint hearing is 3x this)
		WalkSpeed = 2f;             // Movement speed when they are wandering
		RunSpeed = 5f;              // Movement speed when they are chasing the player
		
		Initialization();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		if (_health <= 0)
		{
			_player.MonsterKilled("underBrush", Biome);
			if (Debug == true)
			{
				if (GetParent().GetParent() is DebugHut dh) { dh._shouldSpawn = true; }
			}
			QueueFree(); // Destroy monster when health hits zero
		}
		_countDown -= (float)delta;
		if (_countDown <= 0f)
        {
			_currentAttackOffset = 0f;
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
			_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "StaminaDrain");
			_currentAttackOffset += _attackOffset;
			if (_player._blocking == false)
            {
				_countDown = 5f;
				if (_currentAttackOffset >= 1.311f) { _currentAttackOffset = 1.311f; }   
            }
			_attackBox.Disabled = true;
			_hasHit = true;
		}
	}

	public async void Attack()
	{
		_hasHit = false;
		if (_attackAnimSwitch == 1)
        {
            _attackAnimSwitch = 2;
        }
        else
        {
            _attackAnimSwitch = 1;
        }
		_attackAnim = true;
		await ToSignal(GetTree().CreateTimer(.34), "timeout");
		_speedOffset = 2.5f;
		_attackBox.GetParent<Area3D>().Monitoring = true;
        await ToSignal(GetTree().CreateTimer(0.25), "timeout");
		_attackBox.GetParent<Area3D>().Monitoring = false;
		_canAttack = false;
		//await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		_attackAnim = false;
		await ToSignal(GetTree().CreateTimer(AttackSpeed - _currentAttackOffset), "timeout");
        _canAttack = true;
	}
}
