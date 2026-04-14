using Godot;
using System;

public partial class SpawningRoot : Node3D
{
	// Called when the node enters the scene tree for the first time.
	private ShaderMaterial _disShad;
	private bool _dissolving = false;
	public override void _Ready()
    {
        _disShad = GetNode<MeshInstance3D>("Sphere").MaterialOverride as ShaderMaterial;
		_disShad.SetShaderParameter("dissolveSlider", 1.5f);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		if (_dissolving)
        {
           _disShad.SetShaderParameter("dissolveSlider", Mathf.Lerp((float)_disShad.GetShaderParameter("dissolveSlider"), -2f, (float)delta)); 
        }
        else
        {
            _disShad.SetShaderParameter("dissolveSlider", Mathf.Lerp((float)_disShad.GetShaderParameter("dissolveSlider"), 1.5f, (float)delta));
        }
        
    }

	public async void SpawnMonster(PackedScene monst, theCoiledOne snake)
    {
		GetNode<AnimationPlayer>("A1").Play("CubeAction");
		GetNode<AnimationPlayer>("A2").Play("Cube_001Action");
		GetNode<AnimationPlayer>("A3").Play("Cube_002Action");
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");
		_dissolving = true;
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");
		CharacterBody3D monsterInstance = monst.Instantiate<CharacterBody3D>();
		GD.Print(monsterInstance.Name);
		int amount = 1;
		if(monsterInstance.Name == "revenanT"){amount = snake._revenantLeft; snake._revenantLeft = 0;}
		for (int i = 0; i < amount; i++)
        {
			monsterInstance = monst.Instantiate<CharacterBody3D>();
          	GetParent().AddChild(monsterInstance);
			monsterInstance.GlobalPosition = GlobalPosition;
			if (monsterInstance is Monster3d monster)
			{
				monster.RandomRangedPosition();
				monster.Biome = "Swamp";
				monster.SpawnRange = 250;
				monster._currentSpawnRange = 250;
				monster._startPos = GlobalPosition;
				monster._snake = snake;
			}  
        }
		_dissolving = false;
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		QueueFree();
    }
}
