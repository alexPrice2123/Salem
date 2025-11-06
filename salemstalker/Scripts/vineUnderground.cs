using Godot;
using System;

public partial class vineUnderground : CharacterBody3D
{
	private NavigationAgent3D _navAgent;
	public Player3d _player;
	public bool _charging = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float distance = (_player.GlobalPosition - GlobalPosition).Length();
		if (_charging == false)
        {
            _navAgent.TargetPosition = _player.GlobalPosition;
			Vector3 nextPoint = _navAgent.GetNextPathPosition();
			Velocity = (nextPoint - GlobalTransform.Origin).Normalized() * 5f;
			if (!IsOnFloor())
			{
				Velocity = new Vector3(Velocity.X, -9.8f, Velocity.Z);
			}
        }
		if (distance <= 2f || _charging == true)
		{
			_charging = true;
			Velocity = Vector3.Zero;
			GetNode<GpuParticles3D>("Dirt").Emitting = false;
			GetNode<GpuParticles3D>("Before").Emitting = true;
        }
		MoveAndSlide();
	}
}
