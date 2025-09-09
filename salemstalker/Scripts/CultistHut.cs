using Godot;
using System;

public partial class CultistHut : Node3D
{
    private PackedScene _monsterScene = GD.Load<PackedScene>("res://Scenes/Monster_3D.tscn"); // A PackedScene for the monster that will be instantiated when spawning
	private CsgBox3D _spawn; // Node for the spawn point where monsters will appear
    private Timer _countdown; // Timer for controlling how often monsters spawn
    private float _number; // Keeps track of how many monsters have been spawned
    public override void _Ready()
    {
        // Get references to the spawn point and countdown timer nodes
        _spawn = GetNode<CsgBox3D>("Spawn");
        _countdown = GetNode<Timer>("SpawnTime");

        // Start the timer for the monster spawn
        _countdown.Start();
    }

    // Called when the countdown timer reaches zero (timeout)
    private void _on_spawn_time_timeout()
    {
        // Prevent spawning if the number of monsters has reached or exceeded 50
        if (_number >= 50)
        {
            return;
        }

        // Handles spawning the monster
        CharacterBody3D monsterInstance = _monsterScene.Instantiate<CharacterBody3D>();
        AddChild(monsterInstance);
        monsterInstance.Position = _spawn.Position;

        // Increment the spawn counter
        _number += 1;
    }
    public override void _Process(double delta)
    {
        //
    }
}
