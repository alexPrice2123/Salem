using Godot;
using System;

public partial class Object : CharacterBody3D
{
	public Player3d _player;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	[Export]
	bool _applyGravity = false;

	public override void _Ready()
    {
		_rng.Randomize();
        if (((string)Name).Contains("Log")){GetNode<Node3D>($"Log{_rng.RandiRange(1,2)}").Visible = false;}
    }
	public override void _PhysicsProcess(double delta)
	{
		if (_applyGravity)
        {
			if (!IsOnFloor()) { Velocity += new Vector3(0f,-9.8f,0f) * (float)delta; } // Apply gravity if not on the floor
            MoveAndSlide();
        }
		if (_player == null){return;}
		if (_player._lastSeen != this)
		{
			GetNode<Label3D>("Title").Visible = false;
		}
	}
}
