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
        LookAt(new Vector3(_playerOrb.GlobalPosition.X, _playerOrb.GlobalPosition.Y, _playerOrb.GlobalPosition.Z), Vector3.Up);
		ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * speed);
		GetNode<AnimationPlayer>("axe/AnimationPlayer").Play("axeAction");
	}

	public async void ReturnToUser(float speed)
	{
		_returning = true;
		GetNode<AnimationPlayer>("axe/AnimationPlayer").Play("axeAction");
		GetNode<Area3D>("Attackbox").SetDeferred("monitoring", true);
        LookAt(_lumberJack._spawn.GlobalPosition, Vector3.Up);
		ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * speed/2);
        await ToSignal(GetTree().CreateTimer(2), "timeout");
        _lumberJack._hasAxe = true;
		QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float dist = (GlobalPosition - _lumberJack.GlobalPosition).Length();
		GD.Print(dist);
        if ((dist <= 2 || dist >= 15) && _returning)
		{
			_lumberJack._hasAxe = true;
			_lumberJack._handAxe.Visible = true;
			_lumberJack._grabAnim = false;
			QueueFree();
		}
	}

	public async void _on_attackbox_body_entered(Node3D body)
	{
		if (body.Name == "TerrainCollider" && !_returning)
		{
			GetNode<AnimationPlayer>("axe/AnimationPlayer").Stop();
			LinearVelocity = Vector3.Zero;
			AngularVelocity = Vector3.Zero;
			GravityScale = 0f;
			GetNode<Area3D>("Attackbox").SetDeferred("monitoring", false);
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
			_lumberJack._attackAnim = false;
		}
	}

    private void _on_tree_exiting()
    {
        _lumberJack._currentAxe = null;
		_lumberJack._attackAnim = false;
        GD.Print("Axe Parried");
    }
}
