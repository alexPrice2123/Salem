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
			GetNode<Area3D>("Area3D").SetDeferred("monitoring", false);
			await ToSignal(GetTree().CreateTimer(2), "timeout");
			plr.GetNode<Ui>("UI")._fadeProg = 0;
			GetNode<Camera3D>("Fight/Camera").Current = true;
			GetNode<AnimationPlayer>("Fight/CamAnim").Play("CameraAction");
			GetNode<AnimationPlayer>("Fight/Middle").Play("metarigAction");
			GetNode<AnimationPlayer>("Fight/Right1").Play("metarig_001Action");
			GetNode<AnimationPlayer>("Fight/Right2").Play("metarig_001Action_001");
			GetNode<AnimationPlayer>("Fight/Left").Play("metarig_002Action_001");
			await ToSignal(GetTree().CreateTimer(1f), "timeout");
			GetNode<Node3D>("Fight/metarig_001").Visible = true;
			GetNode<Node3D>("Fight/metarig_002").Visible = true;
			await ToSignal(GetTree().CreateTimer(2f), "timeout");
			plr.GetNode<Ui>("UI")._fadeProg = 1;
			await ToSignal(GetTree().CreateTimer(2), "timeout");
			plr.GetNode<Label>("UI/TutTextComb").Visible = true;

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
			GetNode<Node3D>("Fight/metarig_001").Visible = false;
			GetNode<Node3D>("Fight/metarig_002").Visible = false;
			GetNode<Node3D>("Fight/metarig").Visible = false;
			plr.CutsceneToggle(false);
			plr.GetNode<Ui>("UI")._fadeProg = 0;
			GetNode<Camera3D>("Fight/Camera").Current = false;

        }
    }
}
