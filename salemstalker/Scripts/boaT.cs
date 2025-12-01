using Godot;
using System;

public partial class boaT /*Replace with script name*/ : NpcVillager
{
	[Export]
	public string _teleportPoint = "None";
	public override void _Ready()
	{
		Villager = this;
		_object = "Boat";
		QuestDialogue = new Godot.Collections.Array<string>{ "Would you like to sail to "+_teleportPoint };
		InitializeVillager();
	}

	public override void _PhysicsProcess(double delta)
	{
		EveryFrame(delta);
		if (ObjectActivated == true)
        {
            ObjectActivated = false;
			_interface._loadingGoal = -1;
			_interface._loadingObjective = _object+_teleportPoint;
			_interface._loadingDone = false;
			EndDialouge();
        }
		if (_interface._loadingObjective == _object+_teleportPoint && _interface._loadingDone == true)
        {
            _interface._loadingDone = false;
			_interface._loadingObjective = "None";
			GD.Print(_teleportPoint);
			_player.GlobalPosition = GetParent().GetNode<Node3D>("TeleportPoints/"+_teleportPoint).GlobalPosition;
        }
	}
}
