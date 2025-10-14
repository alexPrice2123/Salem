using Godot;
using System;

public partial class KillMonstersQuest : NpcVillager
{
	public override void _Ready()
	{
		Villager = this;

		InitializeVillager();
	}

	public override void _PhysicsProcess(double delta)
	{
		EveryFrame(delta);
		if (_player._monstersKilled >= 5)
		{
			_questComplete = true;
		}
	}
}
