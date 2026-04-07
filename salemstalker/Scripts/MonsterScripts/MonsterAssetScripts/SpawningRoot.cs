using Godot;
using System;

public partial class SpawningRoot : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void SpawnMonster(PackedScene monst)
    {
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		CharacterBody3D monsterInstance = monst.Instantiate<CharacterBody3D>();
		GD.Print(monsterInstance.Name);
		int amount = 1;
		if(monsterInstance.Name == "revenanT"){amount = 4;}
		for (int i = 0; i < amount; i++)
        {
          	GetParent().AddChild(monsterInstance);
			monsterInstance.GlobalPosition = GlobalPosition;
			if (monsterInstance is Monster3d monster)
			{
				monster.RandomRangedPosition();
				monster.Biome = "Swamp";
				monster.SpawnRange = 250;
				monster._currentSpawnRange = 250;
				monster._startPos = GlobalPosition;
			}  
        }
		QueueFree();
    }
}
