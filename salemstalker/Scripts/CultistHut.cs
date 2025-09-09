using Godot;
using System;

public partial class CultistHut : Node3D
{
    private PackedScene _monsterScene = GD.Load<PackedScene>("res://Scenes/Monster_3D.tscn"); // A PackedScene for the monster that will be instantiated when spawning
	private CsgBox3D _spawn; // Node for the spawn point where monsters will appear
    private Timer _countdown; // Timer for controlling how often monsters spawn
    private float _number; // Keeps track of how many monsters have been spawned
    private CharacterBody3D _player; // Player reference
    private double _spawnDistance = 100; // Disntacne from player to start spawining monsters
    private Node3D _holder; // The node that the monsters are spawned into
    public override void _Ready()
    {
        // Get references to the spawn point and countdown timer nodes
        _spawn = GetNode<CsgBox3D>("Spawn");
        _countdown = GetNode<Timer>("SpawnTime");

        // Start the timer for the monster spawn
        _countdown.Start();

        _player = this.GetParent().GetParent().GetNode<Player3d>("Player_3d");

        _holder = GetNode<Node3D>("MonsterHolder");
    }

    // Called when the countdown timer reaches zero (timeout)
    private void _on_spawn_time_timeout()
    {
        //Disntace from player to this
        float distance = (_player.GlobalPosition - GlobalPosition).Length();

        //Handles monster despawn
        if (distance >= _spawnDistance)
        {
            foreach (CharacterBody3D monster in _holder.GetChildren())
            {
                monster.QueueFree();
           }
        }

        // Prevent spawning if the number of monsters has reached or exceeded 25
        if (_number >= 25 || distance >= _spawnDistance)
        {
            return;
        }

        // Handles spawning the monster
        CharacterBody3D monsterInstance = _monsterScene.Instantiate<CharacterBody3D>();
        _holder.AddChild(monsterInstance);
        monsterInstance.Position = _spawn.Position;

        // Increment the spawn counter
        _number += 1;
    }
    public override void _Process(double delta)
    {
        //
    }
}
