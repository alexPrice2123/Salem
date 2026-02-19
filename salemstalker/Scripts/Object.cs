using Godot;
using System;

public partial class Object : CharacterBody3D
{
	public Player3d _player;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
    {
		_rng.Randomize();
        if (((string)Name).Contains("Log")){GetNode<Node3D>($"Log{_rng.RandiRange(1,2)}").Visible = false;}
		GD.Print(Name);
    }
	public override void _PhysicsProcess(double delta)
	{
		if (_player == null){return;}
		if (_player._lastSeen != this)
		{
			GetNode<Label3D>("Title").Visible = false;
		}
	}
}
