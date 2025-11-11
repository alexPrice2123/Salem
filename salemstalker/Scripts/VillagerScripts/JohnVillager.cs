using Godot;
using System;

public partial class JohnVillager : NpcVillager
{
	public override void _Ready()
	{
		Villager = this;

		InitializeVillager();
	}

	public override void _PhysicsProcess(double delta)
	{
		EveryFrame(delta);
		if (_player._monstersKilled >= 1) //Replace with the correct quest completion condition
		{
			_questComplete = true;
		}
	}
}
