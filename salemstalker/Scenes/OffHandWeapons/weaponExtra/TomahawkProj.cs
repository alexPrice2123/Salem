using Godot;
using System;

public partial class TomahawkProj : RigidBody3D
{
    private float _speed = 100.0f;
    private int _pierceCount = 0;
    private int _count = 0;
    public override void _Ready()
    {
        ApplyCentralImpulse(-GlobalTransform.Basis.Z.Normalized() * _speed);
    }
    public override void _PhysicsProcess(double delta)
    {
        
    }
}
