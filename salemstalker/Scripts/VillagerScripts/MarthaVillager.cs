using Godot;
using System;

public partial class MarthaVillager /*Replace with script name*/ : NpcVillager
{
	public override void _Ready()
	{
		Villager = this;
		InitializeVillager();
	}

	public override void _PhysicsProcess(double delta)
	{
		EveryFrame(delta);
		if (true == false) //Replace with the correct quest completion condition
		{
			_questComplete = true;
		}
	}
}
