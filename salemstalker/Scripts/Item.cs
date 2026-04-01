using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

public partial class Item : CharacterBody3D
{
	public CharacterBody3D InstantiatedItem;
	public Vector3 _dropDirection = new Vector3();
	public Node3D _world;
	public bool _worldSet = false;

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		if (!IsOnFloor()) { velocity.Y -= 7f * (float)delta; } else { velocity = new Vector3(0, 0, 0); }
		Velocity = velocity;
		
		MoveAndSlide();
	}
	private void _on_hitbox_body_entered(Node3D body) // This method essentially works as a PickUpItem method
	{
		if (body is Player3d player)
		{
			GD.Print("picked up " + InstantiatedItem.Name + " at " + player.GlobalPosition);
			player._itemInv.AddResource(Regex.Replace(InstantiatedItem.Name, @"\d", string.Empty), 1);
			InstantiatedItem.QueueFree();
		}
	}

	public async void PickUpPause(Area3D detbox)
	{
		detbox.Monitoring = false;
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		detbox.Monitoring = true;
	}
}

	
