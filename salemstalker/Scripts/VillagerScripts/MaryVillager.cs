using Godot;
using System;

public partial class MaryVillager : NpcVillager
{
	public override void _Ready()
	{
		Villager = this;
		InitializeVillager();
	}

	public override void _PhysicsProcess(double delta)
	{
		EveryFrame(delta);
		if ((_player._itemInv.GetItemCount("taz") + _player._itemInv.GetItemCount("bridger") + _player._itemInv.GetItemCount("gnocchi")) >= 3) //Replace with the correct quest completion condition
		{
			_questComplete = true;
		}
	}
}
