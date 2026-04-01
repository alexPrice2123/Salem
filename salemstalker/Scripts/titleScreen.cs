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
    private Vector2 _baseButtonScale;
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
        _baseButtonScale = _playButton.Scale;
        if ( !FileAccess.FileExists(_savePath) )
        {
            GD.Print("Save file missing");
            SaveHandler.createSaveFile(_savePath);
            GD.Print("Save file created");
        }
        else{ GD.Print("Save file exists"); }
        data = SaveHandler.LoadFromFile(_savePath);
        GD.Print("Save file loaded");
        if ((bool)data["tutorialComplete"] == true)
        {
            _nextScene = "res://Scenes/newWorld.tscn";
        }else{_nextScene = "res://Scenes/cutscene_1.tscn";}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (_playButton.IsHovered() && _playButton.Scale < _baseButtonScale * new Vector2(1.1f, 1.1f)){ScaleButton(_playButton, true);}
        else if (!_playButton.IsHovered() && _playButton.Scale > _baseButtonScale){ScaleButton(_playButton, false);}

        if (_creditButton.IsHovered() && _creditButton.Scale < _baseButtonScale * new Vector2(1.1f, 1.1f)){ScaleButton(_creditButton, true);}
        else if (!_creditButton.IsHovered() && _creditButton.Scale > _baseButtonScale){ScaleButton(_creditButton, false);}

        if (_quitButton.IsHovered() && _quitButton.Scale < _baseButtonScale * new Vector2(1.1f, 1.1f)){ScaleButton(_quitButton, true);}
        else if (!_quitButton.IsHovered() && _quitButton.Scale > _baseButtonScale){ScaleButton(_quitButton, false);}

        if (_backButton.IsHovered() && _backButton.Scale < _baseButtonScale * new Vector2(1.1f, 1.1f)){ScaleButton(_backButton, true);}
        else if (!_backButton.IsHovered() && _backButton.Scale > _baseButtonScale){ScaleButton(_backButton, false);}
    }

    private void ScaleButton(TextureButton button, bool sign)
    {
        if (sign){button.Scale += new Vector2(0.01f,0.01f);}
        else{button.Scale -= new Vector2(0.01f,0.01f);}
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
    }
	
}

// code code coding code code 1+1=2 code woahhh code woahhhh coding code im adding yaoi to the code yaoi code + yuri code