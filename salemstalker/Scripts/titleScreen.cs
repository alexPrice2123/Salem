using Godot;
using System;

public partial class titleScreen : Node3D
{
	private bool _play = false;
	private ShaderMaterial _loadingMaterial;
	private ColorRect _loadingUI;
	private TextureButton _playButton;
	private TextureButton _creditButton;
	private TextureButton _quitButton;
	private TextureButton _backButton;
	private string _nextScene;
	public Godot.Collections.Dictionary<string,Variant> data = new Godot.Collections.Dictionary<string,Variant>();
	public string _savePath = "user://saveData.json";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_loadingUI = GetNode<ColorRect>("Loading");
		_loadingMaterial = _loadingUI.Material as ShaderMaterial;
		_playButton = GetNode<TextureButton>("Buttons/Play");
		_creditButton = GetNode<TextureButton>("Buttons/Credits");
		_quitButton = GetNode<TextureButton>("Buttons/Quit");
		_backButton = GetNode<TextureButton>("Credits/Back");
		if ( !FileAccess.FileExists(_savePath) )
		{
			GD.Print("Save file missing");
			SaveHandler.createSaveFile(_savePath);
			GD.Print("Save file created");
		}
		else{ GD.Print("Save file exists"); }
        if ( !SaveHandler.checkCompatibility(_savePath) ){GetNode<AnimatedSprite2D>("Buttons/FunnyReset").Visible = true ;}
		data = SaveHandler.LoadFromFile(_savePath);
		GD.Print("Save file loaded");
		if ((bool)data["tutorialComplete"] == true)
		{
			_nextScene = "res://Scenes/newWorld.tscn";
		}else{_nextScene = "res://Scenes/cutscene_1.tscn";}
	}


	private async void _on_play_button_up()
	{
		if ( !FileAccess.FileExists(_savePath))
		{
			GD.Print("Save file missing");
			SaveHandler.createSaveFile(_savePath);
			GD.Print("Save file created");
		}
		else{ GD.Print("Save file exists"); }
		GD.Print("Entering save file: ", _savePath);
		while ((float)_loadingMaterial.GetShaderParameter("progress") >= -1f)
		{
			_loadingMaterial.SetShaderParameter("progress", (float)_loadingMaterial.GetShaderParameter("progress")-0.05f);
			await ToSignal(GetTree().CreateTimer(0.01), "timeout"); 
		}
		GetTree().ChangeSceneToFile(_nextScene);
		
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
	private void _on_reset_save_button_pressed()
	{
		SaveHandler.createSaveFile("user://saveData.json");
        GetNode<AnimatedSprite2D>("Buttons/FunnyReset").Visible = false ;
	}
	private void _on_dev_menu_button_button_up()
	{
		GetNode<ColorRect>("DevMenu").Visible = true ;
	}
	private void _on_exit_dev_button_up()
	{
		GetNode<ColorRect>("DevMenu").Visible = false ;
	}
	private void _on_save_dev_button_up()
	{
		
	}
}

// code code coding code code 1+1=2 code woahhh code woahhhh coding code im adding yaoi to the code yaoi code + yuri code
