using Godot;
using System;

public partial class Object : CharacterBody3D
{
	public Player3d _player;

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null){return;}
		if (_player._lastSeen != this)
		{
			GetNode<Label3D>("Title").Visible = false;
		}
	}
}
