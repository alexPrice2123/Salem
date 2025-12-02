using Godot;
using System;

public partial class titleScreen : Node3D
{
	private bool _play = false;
	private ShaderMaterial _loadingMaterial;
	private ColorRect _loadingUI;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		_loadingUI = GetNode<ColorRect>("Loading");
        _loadingMaterial = _loadingUI.Material as ShaderMaterial;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (_play)
        {
            _loadingMaterial.SetShaderParameter("progress", (float)_loadingMaterial.GetShaderParameter("progress")-0.05f);
			if ((float)_loadingMaterial.GetShaderParameter("progress") <= -1f)
            {
                GetTree().ChangeSceneToFile("res://Scenes/newWorld.tscn");
            }
        }
    }

	private void _on_play_button_up()
    {
        _play = true;
    }
	private void _on_credits_button_up()
    {
		if (_play){return;}
        GetNode<ColorRect>("Credits").Visible = true;
    }
	private void _on_quit_button_up()
    {
		if (_play){return;}
        GetTree().Quit(); // Quit the scene 
    }
	private void _on_back_button_up()
    {
		if (_play){return;}
        GetNode<ColorRect>("Credits").Visible = false;
    }
	
}
