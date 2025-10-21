using Godot;
using System;

public partial class BobVillager : NpcVillager
{
	public override void _Ready()
	{
		Villager = this;
		InitializeVillager();
	}

	public override void _PhysicsProcess(double delta)
	{
		EveryFrame(delta);
		if (_player._hasApple == true)
		{
			_questComplete = true;
		}
	}
}
