using Godot;
using System;
public partial class Resin : Monster3d
{
	public bool _dead = true;

	public override void _Ready()
	{
		// -- Variables -- //
		Chaser = true;              // If this monster chasing the player or finds a point within a range of the player
		MoveWhileAttack = false;     // Can this monster move while attacking
		IsObject = true;              // Should gravity be applied to this monster
		Stationery = true;          // If the monster shouldnt move at all
		BaseDamage = 0f;         // Base damage of the monster
		AttackSpeed = 2.5f;         // The time between its attacks
		AttackRange = 1.5f;           // The distance the monster gets from the player before stopping and attacking
		MaxHealth = 25.0f;         // Maximum monster health
		WanderRange = 35;           // The range the monster can wander from its spawn point
		AgroFOV = 7.0f;          	// The vision FOV of the monster
		AgroLength = 5.5f;          // The detection length of the monsters vision
		WalkRange = 3.5f;	         	// The noise range monsters hear the player walking
		WalkSpeed = 0f;             // Movement speed when they are wandering
		RunSpeed = 0.0f;              // Movement speed when they are chasing the player

		// -- Other -- //
		Monster = this;
		Initialization();
	}

	public override void _Process(double delta)
	{
		EveryFrame(delta);
		if (_health <= 0)
		{
			_player.MonsterKilled("resin", Biome);
			if (!_dead){Die();}
			_dead = true;
		}
	}

	public void _on_hurtbox_area_entered(Area3D body){if (Visible && !_dead){Damaged(body);}}

	public void _on_attackbox_area_entered(Node3D body)
	{
		TryHitPlayer(body);
	}

	private async void Die()
    {
        GetNode<Node3D>("Body").Visible = false;
		_snake._resinArray.Add(this);
		_snake.ResinBroken();
		GetNode<GpuParticles3D>("Break").Emitting = true;
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		Visible = false;
		GetNode<Node3D>("Body").Visible = true;
    }

	public void Grow(theCoiledOne snake)
    {
        Visible = true;
		_dead = false;
		_health = 25;
		_snake = snake;
		snake._resinArray.Remove(this);
    }
}
