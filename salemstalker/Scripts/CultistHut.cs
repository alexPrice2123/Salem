using Godot;
using System;





public partial class CultistHut : Node3D
{
	private PackedScene _monsterScene = GD.Load<PackedScene>("res://Scenes/Monster_3D.tscn");


	private CsgBox3D _spawn;
	private Timer _countdown;
	private float _number;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_spawn = GetNode<CsgBox3D>("Spawn");
		_countdown = GetNode<Timer>("SpawnTime");
		_countdown.Start();
	}

	private void _on_spawn_time_timeout()
	{
		if (_number >= 50)
		{
			return;
		}
		CharacterBody3D monsterInstance = _monsterScene.Instantiate<CharacterBody3D>();
		AddChild(monsterInstance);
		monsterInstance.Position = _spawn.Position;
		_number += 1;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GD.Print(_number);
	}
}
