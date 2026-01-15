using Godot;
using System;

public partial class flyingPesk : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private float _attackingRange = 1.2f;
	private float _attackingDistance = 10f;
	private bool _fleeing = false;
	private int _cooldown = 5;
	private int _currentCooldown = 0;
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
		WalkSpeed = 2f;             // Movement speed when they are wandering
		RunSpeed = 3f;              // Movement speed when they are chasing the player
		
		Initialization();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EveryFrame(delta);
		_distance = (_player.GlobalPosition - GlobalPosition).Length();
		if (_distance <= _attackingDistance && _fleeing == false)
		{
			if (_currentCooldown <= 0)
			{
				_attacking = true;
				_rangedPosition = _player.GlobalPosition;
			}
            else
            {
                _fleeing = true;
                RandomRangedPosition();
            }
		}
		GD.Print(_distance);
		if (_distance <= _attackingRange && _attacking == true && _fleeing == false)
		{
			if (_currentCooldown <= 0)
			{
				Attack();
			}
        }
		if (_health <= 0)
		{
			_player.MonsterKilled("flyingPesk", Biome);
			if (Debug == true)
            {
				if (GetParent().GetParent() is DebugHut dh){ dh._shouldSpawn = true; }
            }
			QueueFree(); // Destroy monster when health hits zero
		}
		if (_attacking == true)
        {
            Vector3 playerPos = _player.GlobalPosition;
            _lookDirection.LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);
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

	public void Fly()
	{
		if (_attacking == false)
		{
			RandomRangedPosition();
			_currentCooldown -= 1;
		}
		_fleeing = false;
	}

	public async void Attack()
	{
		GD.Print("PESK ATTACK");
		_currentCooldown = _cooldown;
		_attackAnim = true;
		_attacking = false;
		_targetVelocity = Vector3.Zero;
		Velocity = _targetVelocity;

		_player.Damaged(BaseDamage + _damageOffset, this as Monster3d, "Hallucinate");

		// await ToSignal(GetTree().CreateTimer(1f), "timeout");
		_fleeing = true;
		_attackAnim = false;
		RandomRangedPosition();
	}
}
