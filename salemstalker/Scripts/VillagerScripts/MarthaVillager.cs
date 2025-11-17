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
		if (_player._swampMonstersKilled >= 5 && _player._plainsMonstersKilled >=5 && _player._forestMonstersKilled >= 5) //Replace with the correct quest completion condition
		{
			_questComplete = true;
		}
	}
}
