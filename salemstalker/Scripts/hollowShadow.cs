using Godot;
using System;

public partial class hollowShadow : Monster3d
{
	// Called when the node enters the scene tree for the first time.

	private float _distance;
	private float _transparencyGoal = 0f;
	private float _fadeFactor = 1f;
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
		if (_player._hallucinationFactor <= 0.1)
        {
            _fadeFactor = 3f;
			_transparencyGoal = 1f;
			GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
        }

		RotateFunc(delta);
		GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/Cube").Transparency = Mathf.Lerp(GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/Cube").Transparency, _transparencyGoal, _fadeFactor * (float)delta);
		GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/Cube_002").Transparency = Mathf.Lerp(GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/Cube_002").Transparency, _transparencyGoal, _fadeFactor * (float)delta);
		GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/Cube_003").Transparency = Mathf.Lerp(GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/Cube_003").Transparency, _transparencyGoal, _fadeFactor * (float)delta);

		GD.Print(GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/Cube_003").Transparency);
		if (GetNode<MeshInstance3D>("Body/metarig/Skeleton3D/Cube_003").Transparency > 0.99f)
        {
			QueueFree();
        }
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

	public async void Attack()
	{
		_hasHit = false;
		_attackAnim = true;
		await ToSignal(GetTree().CreateTimer(.4), "timeout");
		_fadeFactor = 10f;
		_transparencyGoal = 1f;
		GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
	}
}
