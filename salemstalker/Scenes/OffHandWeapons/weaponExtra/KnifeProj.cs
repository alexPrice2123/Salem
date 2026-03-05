using Godot;
using System;

public partial class KnifeProj : RigidBody3D
{
    private float _speed = 1.5f;
    public override void _Ready()
    {
        ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * _speed);
        ApplyTorqueImpulse(-GlobalTransform.Basis.X.Normalized() * 3);
    }
    public override void _PhysicsProcess(double delta)
    {
        
    }
}
