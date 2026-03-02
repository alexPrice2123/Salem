using Godot;
using System;

public partial class tutorialArea : Node3D
{
	[Export]
	public string _tutorialMessage = "Tutorial Text Not Set";
	[Export]
	public float _displayTime = 2.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_area_3d_area_entered(Area3D area)
	{
		if (area.GetParent() is Player3d plr)
		{
			Tutorial(plr, _tutorialMessage, _displayTime);
		}
	}
	
	public async void Tutorial(Player3d plr, string tutMessage, float displayTime)
    {
        Label tutText = plr.GetNode<Label>("UI/Tutorial");
		tutText.Visible = true;
		tutText.Text = tutMessage;
		await ToSignal(GetTree().CreateTimer(displayTime), "timeout");
		tutText.Visible = !(tutText.Text == tutMessage);
    }
}
