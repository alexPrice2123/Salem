using Godot;
using System;

public partial class CallenVillager /*Replace with script name*/ : NpcVillager
{
	public override void _Ready()
	{
		Villager = this;
		InitializeVillager();
	}

	public override void _PhysicsProcess(double delta)
	{
		EveryFrame(delta);
		if (_player._itemInv.GetItemCount("bleedheart") >= 3 && _player._itemInv.GetItemCount("deadooze") >= 4 && _player._itemInv.GetItemCount("seed") >= 2) //Replace with the correct quest completion condition
		{
			_questComplete = true;
		}
	}
}
