using Godot;
using System;

public partial class Cutscene3 : Node3D
{
	private bool _doubleVision = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void StartCut(Player3d plr)
    {
		plr.GetNode<Ui>("UI")._fadeProg = 1;
        plr.CutsceneToggle(true);
		GetNode<Node3D>("Waking").Visible = true;
		GetNode<Node3D>("Cursed").Visible = true;
		GetNode<Node3D>("Drag").Visible = true;
		plr.GetNode<Label>("UI/TutTextComb").Visible = false;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		plr.GetNode<Ui>("UI")._fadeProg = 0;
		GetNode<Node3D>("metarig").Visible = true;
		GetNode<AnimationPlayer>("CamAnim").Play("CameraAction");
		GetNode<AnimationPlayer>("CultAnim").Play("attack_1");
		GetNode<Camera3D>("Camera").Current = true;
		await ToSignal(GetTree().CreateTimer(5.82), "timeout");
		plr.GetNode<Ui>("UI")._fadeProg = 1;
		plr.GetNode<ColorRect>("Fade").Color = new Color(0,0,0,1);
		GetNode<Camera3D>("Camera").Current = false;
		GetNode<Camera3D>("Drag/Camera").Current = true;
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<AnimationPlayer>("Drag/Drag").Play("CameraAction");
		plr.GetNode<Ui>("UI")._fadeProg = 0;
		ShaderMaterial mat = plr.GetNode<ColorRect>("Dither").Material as ShaderMaterial;
		Vision(mat, true);
		await ToSignal(GetTree().CreateTimer(4.5f), "timeout");
		plr.GetNode<Ui>("UI")._fadeProg = 1; 
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<Camera3D>("Drag/Camera").Current = false; 
		GetNode<Camera3D>("Cursed/Camera").Current = true;
		plr.GetNode<Ui>("UI")._fadeProg = 0;
		GetNode<AnimationPlayer>("Cursed/CamAnim").Play("CameraAction");
		GetNode<AnimationPlayer>("Cursed/CultAnim1").Play("chargeup");
		GetNode<AnimationPlayer>("Cursed/CultAnim2").Play("chargeup_001");
		GetNode<AnimationPlayer>("Cursed/CultAnim3").Play("chargeup_002");
		await ToSignal(GetTree().CreateTimer(5.3), "timeout");
		plr.GetNode<Ui>("UI")._fadeProg = 1; 
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		GetNode<Camera3D>("Cursed/Camera").Current = false; 
		GetNode<Camera3D>("Waking/metarig_001/Skeleton3D/spine_006/Camera").Current = true;
		plr.GetNode<Ui>("UI")._fadeProg = 0;
		GetNode<AnimationPlayer>("Waking/Wake").Play("metarig_001Action");
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		Vision(mat, false);
		await ToSignal(GetTree().CreateTimer(4), "timeout");
		plr.GetNode<Ui>("UI")._fadeProg = 1; 
		plr.StartCut(2f);
		await ToSignal(GetTree().CreateTimer(1.5), "timeout");

		plr.GetParent<NewWorld>().data["tutorialComplete"] = true;
		SaveHandler.SaveToFile(plr.GetParent<NewWorld>().data,plr.GetParent<NewWorld>()._savePath);
		GetNode<Node3D>("Waking").Visible = false;
		GetNode<Node3D>("Cursed").Visible = false;
		GetNode<Node3D>("Drag").Visible = false;
		plr.GlobalTransform = plr.GetParent().GetNode<Node3D>("PlayerSpawnAfterCut").GlobalTransform;
		GetNode<Camera3D>("Waking/metarig_001/Skeleton3D/spine_006/Camera").Current = false;
		plr.GetNode<Ui>("UI")._fadeProg = 0;
		plr.CutsceneToggle(false);
		plr.CamLookAtPos(plr.GetParent().GetNode<Node3D>("Matthew/LookAt").GlobalPosition);
		plr._inCombat = false;
    }
	private async void Vision(ShaderMaterial mat, bool toggle)
	{
		_doubleVision = toggle;
		if (toggle)
        {
           	float t = 0f;
			while (_doubleVision)
			{
				float dizzy = 0.7f + Mathf.Sin(t) * 0.3f;
				mat.SetShaderParameter("dizziness", dizzy);

				t += 2.0f * (float)GetProcessDeltaTime()*2; // speed of wobble

				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			} 
        }
        else
        {
            for (int i = 0; i < 125; i++)
            {
                mat.SetShaderParameter("dizziness", (float)mat.GetShaderParameter("dizziness") - 0.0064f);
				GD.Print(i);
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
			mat.SetShaderParameter("dizziness", 0);
        }
		
	}
}
