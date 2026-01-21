using Godot;
using System;

public partial class lumberAxe : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public Player3d _playerOrb;
	public lumberJack _lumberJack;
	public float _damageOrb;

	private int _count = 0;
	public bool _returning = false;


	public void Shoot(float speed)
	{
        LookAt(
		new Vector3(_playerOrb.GlobalPosition.X, GlobalPosition.Y, _playerOrb.GlobalPosition.Z), Vector3.Up);
		ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * speed);
	}

	public async void ReturnToUser(float speed)
	{
		_returning = true;
		GetNode<Area3D>("Attackbox").Monitoring = true;
        LookAt(_lumberJack._spawn.GlobalPosition, Vector3.Up);
		ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * speed);
        await ToSignal(GetTree().CreateTimer(2), "timeout");
        _lumberJack._hasAxe = true;
		QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        
	}

	public async void _on_attackbox_body_entered(Node3D body)
	{
		if (body.Name == "TerrainCollider" && !_returning)
		{
			LinearVelocity = Vector3.Zero;
			AngularVelocity = Vector3.Zero;
			GravityScale = 0f;
			GetNode<Area3D>("Attackbox").Monitoring = false;
		}
	}

	public async void _on_attackbox_area_entered(Node3D body)
	{
		if (body.IsInGroup("Player") && body.Name == "Hurtbox")
		{
			_playerOrb.RangedDamaged(_damageOrb, this, "None");
			LinearVelocity = Vector3.Zero;
			AngularVelocity = Vector3.Zero;
			ReturnToUser(_lumberJack._projectileSpeed);
            _lumberJack._playerHit = true;
		}
		else if (body.GetParent() == _lumberJack && _returning)
		{
			_lumberJack._hasAxe = true;
			await ToSignal(GetTree().CreateTimer(0.25), "timeout");
			QueueFree();
		}
	}

    private void _on_tree_exiting()
    {
        _lumberJack._currentAxe = null;
        GD.Print("Axe Parried");
    }
}
