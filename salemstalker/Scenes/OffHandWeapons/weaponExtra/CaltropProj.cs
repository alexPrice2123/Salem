using Godot;
using System;

public partial class CaltropProj : RigidBody3D
{
    private float _speed = 1f;
    public override void _Ready()
    {
        RandomNumberGenerator _rng = new(); 
        ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * _speed * _rng.RandiRange(1,3));  
        ApplyTorqueImpulse(-GlobalTransform.Basis.X.Normalized() * _rng.RandiRange(1,5));
        ApplyTorqueImpulse(-GlobalTransform.Basis.Z.Normalized() * _rng.RandiRange(1,5));
    }
    public override void _PhysicsProcess(double delta)
    {
        
    }
    public async void _on_body_entered(Node body)
    {
        if(body.IsInGroup("Terrain")){await ToSignal(GetTree().CreateTimer(15), "timeout"); QueueFree(); }
    }
    public async void _on_hit_area_3d_body_entered(Node3D body)
    {
        if (body.IsInGroup("Monster"))
        {
            QueueFree();
        }
    }
}
