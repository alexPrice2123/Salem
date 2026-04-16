 using Godot;
using System;

public partial class Caltrops : Node3D
{
    private PackedScene _gunBullet = GD.Load<PackedScene>("res://Scenes/OffHandWeapons/weaponExtra/CaltropProj.tscn");
    private Marker3D _barrel;
    private Marker3D _direct;
    private bool shooting = false;

    public override void _Ready()
    {
        _barrel = GetNode<Marker3D>("BarrelPos");
    }
    public async void specAction()
    {
        shooting = true;
        await ToSignal(GetTree().CreateTimer(1.45f), "timeout");
        RandomNumberGenerator _rng = new();   
        GD.Print("workplease");
        for(int i = 0; i < 5; i++)
        {
            GD.Print("workplease2");
            RigidBody3D temPrj = _gunBullet.Instantiate<RigidBody3D>();
            AddSibling(temPrj);
            temPrj.Position = _barrel.Position;  
            temPrj.Rotation = _barrel.Rotation + new Vector3(0,Mathf.DegToRad(_rng.RandfRange(-60f,60f)),0f);
            temPrj.Reparent(temPrj.GetParent().GetParent().GetParent().GetParent().GetParent());
            GD.Print("workplease3");
        }
        shooting = false;
    }
}
