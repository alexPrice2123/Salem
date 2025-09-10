using Godot;
using System;

public partial class CultistHut : Node3D
{
    // --- CONSTANTS ---
    private double _spawnDistance = 100;        // Maximum distance from player before monsters despawn or spawning stops

    // --- VARIABLES ---
    private PackedScene _monsterScene = GD.Load<PackedScene>("res://Scenes/Monster_3D.tscn"); // PackedScene for spawning monsters
    private CsgBox3D _spawn;                   // Spawn point where monsters appear
    private Timer _countdown;                  // Timer that triggers spawn events
    private float _number;                     // Current number of spawned monsters
    private Player3d _player;           // Reference to the player
    private Node3D _holder;                    // Parent node that holds all spawned monsters

    // --- READY ---
    public override void _Ready()
    {
        _spawn = GetNode<CsgBox3D>("Spawn");
        _countdown = GetNode<Timer>("SpawnTime");
        _countdown.Start();

        _player = this.GetParent().GetParent().GetNode<Player3d>("Player_3d");
        _holder = GetNode<Node3D>("MonsterHolder");
    }

    // --- SPAWN HANDLER ---
    private void _on_spawn_time_timeout()
    {
        if (_player._inv.Visible == true)
        {
            return;
        }
        float distance = (_player.GlobalPosition - GlobalPosition).Length();
    
        // --- Despawn monsters if player is too far ---
        if (distance >= _spawnDistance)
        {
            foreach (CharacterBody3D monster in _holder.GetChildren())
            {
                monster.QueueFree();
                _number = 0;
            }
        }

        // --- Prevent spawning if at max count or player too far ---
        if (_number >= 25 || distance >= _spawnDistance)
        {
            return;
        }

        // --- Spawn new monster ---
        CharacterBody3D monsterInstance = _monsterScene.Instantiate<CharacterBody3D>();
        _holder.AddChild(monsterInstance);
        monsterInstance.Position = _spawn.Position;

        _number += 1;
    }

    // --- PROCESS LOOP ---
    public override void _Process(double delta)
    {
        // 
    }
}
