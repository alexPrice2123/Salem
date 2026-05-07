using Godot;
using System;

public partial class kyronArena : Node3D
{
	private PackedScene _kyron = GD.Load<PackedScene>("res://Scenes/Monsters/Bosses/theKyron.tscn");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_area_3d_area_entered(Area3D body)
	{
		if (body.IsInGroup("PlayerHurtbox"))
		{
			GetNode<Area3D>("Area3D").SetDeferred("monitoring", false);
			theKyron kyronInstance = _kyron.Instantiate<theKyron>(); 
			GetParent().GetNode<Node3D>("MonsterHolder/Hold2/Hold").AddChild(kyronInstance);                                            
			kyronInstance.GlobalPosition = GetNode<Marker3D>("SpawnPos").GlobalPosition;
			GD.Print(GlobalPosition+ "POS");
		}
	}
}
