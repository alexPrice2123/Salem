using Godot;
using System;

public partial class DillonVillager /*Replace with script name*/ : NpcVillager
{
	public override void _Ready()
	{
		Villager = this;
		InitializeVillager();
	}

	public override void _PhysicsProcess(double delta)
	{
		EveryFrame(delta);
		if (_player._ratsKilled >= 10) //Replace with the correct quest completion condition
		{
			_questComplete = true;
		}
	}
}
