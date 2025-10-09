using Godot;
using System;

public partial class Blood : Node3D
{
		private async void _on_hitbox_body_entered(Node3D body)
	{
		if (body.IsInGroup("Monster"))
		{
			await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
			GetNode<MeshInstance3D>("Blade").GetNode<GpuParticles3D>("Blood").Emitting = true;
			await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
			GetNode<MeshInstance3D>("Blade").GetNode<GpuParticles3D>("Blood").Emitting = false;
		}
	}
}
