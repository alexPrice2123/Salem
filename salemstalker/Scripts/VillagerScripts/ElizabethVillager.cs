using Godot;
using System;

public partial class ElizabethVillager : NpcVillager
{
	public override void _Ready()
	{
		Villager = this;
		InitializeVillager();
	}

	public override void _PhysicsProcess(double delta)
	{
		EveryFrame(delta);
		if (_player._itemInv.GetItemCount("log") >= 15) //Replace with the correct quest completion condition
		{
			_questComplete = true;
		}
	}
}
