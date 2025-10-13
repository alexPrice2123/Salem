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
    public void NPCTalk()
    {
        if (_questComplete == true && _questInProgress == false) { return; } //if the quest is done the player can't interact

		if (_questComplete == true && _player._questBox.FindChild(QuestTitle) == null) //changes dialouge from the initial dialouge to quest dialouge 
		{
			_dialogueBox.Text = QuestDialogue;
		}
		if (_questComplete == true) //what happens when the player talks to him after completing the quest
		{
			_dialogueBox.Text = DoneDialogue;
			_questInProgress = false;
			_player.RemoveQuest(QuestTitle);
		}
		else  //what happens when the player talks to him before completing the quest
		{
			_dialogueBox.Text = WaitingDialogue;
		}
		if (_questPrompt.Visible == true && _questInProgress == false && _questComplete == false)
		{
			_questPrompt.Visible = false;
			_player._villager = this;
			_dialogue.Visible = true;
			_dialogueBox.Text = QuestDialogue;
			Input.MouseMode = Input.MouseModeEnum.Visible;
			_dialogueBox.GetNode<Label>("NameText").Text = NPCName;
		}
		//if(Input.IsActionJustPressed("continue_dialogue") && !done)
		//{

		//}
    }
}
