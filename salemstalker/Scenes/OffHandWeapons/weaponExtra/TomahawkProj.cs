using Godot;
using System;

public partial class TomahawkProj : RigidBody3D
{
    private float _speed = 20.0f;
    public override void _Ready()
    {
        ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * _speed);
        ApplyTorqueImpulse(-GlobalTransform.Basis.X.Normalized() * 30);
    }
    public override void _PhysicsProcess(double delta)
    {
        
    }
}
