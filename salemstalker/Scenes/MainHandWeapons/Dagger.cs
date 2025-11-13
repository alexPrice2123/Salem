using Godot;
using System;

public partial class Dagger : SwordHandler
{
    public override void _Ready()
    {
        _firstDelay = 0.05f;
        _secondDelay = 0.2f;
    }
}
