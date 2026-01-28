using Godot;
using System;

public partial class StakeGun : Node3D
{
    private PackedScene _gunBullet = GD.Load<PackedScene>("res://Scenes/OffHandWeapons/weaponExtra/stake_bullet.tscn");
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
        RigidBody3D temPrj = _gunBullet.Instantiate<RigidBody3D>();
        AddSibling(temPrj);
        temPrj.Position = _barrel.Position;  
        temPrj.Rotation = _barrel.Rotation;
        temPrj.Reparent(temPrj.GetParent().GetParent().GetParent().GetParent().GetParent());
        shooting = false;
    }
}
