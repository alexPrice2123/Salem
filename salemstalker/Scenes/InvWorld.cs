using Godot;
using System;

public partial class InvWorld : Node3D
{

    private async void _on_falchion_box_mouse_entered()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("falchionHover");
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        GD.Print("Testing?");
        
    }
    private async void _on_falchion_box_mouse_exited()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("falchionDehover");
        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
    }
}
