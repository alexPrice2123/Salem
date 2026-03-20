using Godot;
using System;

public partial class Object : CharacterBody3D
{
	public Player3d _player;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	[Export]
	bool _applyGravity = false;
	MeshInstance3D _currentObject;

	public async override void _Ready()
	{
		await ToSignal(GetTree().CreateTimer(.1), "timeout");
		GD.Print(Name);	
		_rng.Randomize();
	   	if (((string)Name).Contains("Log")){GetNode<Node3D>($"Log{_rng.RandiRange(1,2)}").Visible = true;}
		else {_currentObject = GetNodeOrNull<MeshInstance3D>((string)Name); _currentObject.Visible = true;}
		
	}
	public override void _PhysicsProcess(double delta)
	{
		if (_applyGravity)
		{
			if (!IsOnFloor()) { Velocity += new Vector3(0f,-9.8f,0f) * (float)delta; } // Apply gravity if not on the floor
			MoveAndSlide();
		}
		if (_player == null){return;}
		if (Name == "Taz" || Name == "Bridger" || Name == "Rogue" || Name == "Gnocchi")
		{
			GetNode<Node3D>("GoalLookAt").LookAt(new Vector3(_player.GlobalPosition.X, GlobalPosition.Y, _player.GlobalPosition.Z), Vector3.Up);
			RotateFunc(delta, GetNode<Node3D>("GoalLookAt"));
		}
		

		if (_player._lastSeen != this)
		{
			GetNode<Label3D>("Title").Visible = false;
		}
		
	}

	private void RotateFunc(double delta, Node3D lookDirection)
	{
		if (Mathf.RadToDeg(lookDirection.GlobalRotation.Y) >= 175 || Mathf.RadToDeg(lookDirection.GlobalRotation.Y) <= -175)
		{
			GlobalRotation = new Vector3(GlobalRotation.X, lookDirection.GlobalRotation.Y, GlobalRotation.Z);
		}
		else
		{
			float newRotation = Mathf.Lerp(GlobalRotation.Y, lookDirection.GlobalRotation.Y, (float)delta * 10f);
			GlobalRotation = new Vector3(GlobalRotation.X, newRotation, GlobalRotation.Z);
		}
	}
}
