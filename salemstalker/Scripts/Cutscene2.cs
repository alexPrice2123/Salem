using Godot;
using System;

public partial class Cutscene2 : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private async void _on_area_3d_area_entered(Area3D area)
    {  
        if (area.GetParent() is Player3d plr)
        {
			plr.GetNode<Ui>("UI")._fadeProg = 1;
			plr.CutsceneToggle(true);
			GetNode<Area3D>("Area3D").Monitoring = false;
			await ToSignal(GetTree().CreateTimer(2), "timeout");
			plr.GetNode<Ui>("UI")._fadeProg = 0;
			GetNode<Camera3D>("Camera").Current = true;
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			GetNode<AnimationPlayer>("CamAnim").Play("CameraAction");
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			GetNode<AnimationPlayer>("Hollow1").Play("metarigAction_001");
			GetNode<AnimationPlayer>("Hollow2").Play("metarigAction_003");
			GetNode<AnimationPlayer>("Hollow3").Play("metarigAction_005");
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			GetNode<Node3D>("metarig").Visible = true;
			await ToSignal(GetTree().CreateTimer(2.2f), "timeout");
			GetNode<Node3D>("metarig_001").Visible = true;
			await ToSignal(GetTree().CreateTimer(0.7f), "timeout");
			GetNode<Node3D>("metarig_002").Visible = true;
			await ToSignal(GetTree().CreateTimer(0.7f), "timeout");
			plr.GetNode<Ui>("UI")._fadeProg = 1;
			await ToSignal(GetTree().CreateTimer(1), "timeout");

			for (int i = 1; i <= 3; i++)
            {
				Vector3 spawnPos = GetNode<Node3D>($"H{i}").GlobalPosition;
              	PackedScene monsterSelection = GD.Load<PackedScene>("res://Scenes/Monsters/hollowNormal.tscn");
				CharacterBody3D monsterInstance = monsterSelection.Instantiate<CharacterBody3D>(); // Create monster instance
				if (monsterInstance is Monster3d monster)
				{
					//monster.RandomRangedPosition();
					monster.Biome = "Forest";
					monster.SpawnRange = 1000f;
					monster._currentSpawnRange = 1000f;
					monster._startPos = GlobalPosition;
					if (i < 3){monster.Disabled = true;}
					monster.Cutscene = true;
					monster.MaxHealth += i*10;
				}
				GetParent().GetNode<Node3D>("MonsterHolder/Hold2/Hold").AddChild(monsterInstance);    
				monsterInstance.GlobalPosition = spawnPos;
            }
			GetNode<Node3D>("metarig").Visible = false;
			GetNode<Node3D>("metarig_001").Visible = false;
			GetNode<Node3D>("metarig_002").Visible = false;
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			plr.CutsceneToggle(false);
			plr.GetNode<Ui>("UI")._fadeProg = 0;
			GetNode<Camera3D>("Camera").Current = false;

			
        }
    }
}
