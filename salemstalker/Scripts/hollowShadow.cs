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
		Speed = 4.6f;             // Movement speed
		MaxHealth = 60.0f;         // Maximum monster health
		Range = 30.0f;            // Detection range for chasing
		SpawnDistance = 100;    // Distance from player before despawning
		BaseDamage = 15.0f;
		WanderRange = 50;
		AttackSpeed = 1.5f;
		AttackRange = 1f;
		Monster = this;
		Chaser = true;
		MoveWhileAttack = true;
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
