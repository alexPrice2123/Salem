using Godot;
using System;

public partial class fakeTree : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_hurtbox_area_entered(Area3D body)
    {
        if (GetParent<Node3D>() is theHushedBark thb)
        {
			thb.Damaged(body);
        }
    }
}
