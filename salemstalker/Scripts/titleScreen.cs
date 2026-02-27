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
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (_play)
        {
            _loadingMaterial.SetShaderParameter("progress", (float)_loadingMaterial.GetShaderParameter("progress")-0.05f);
			if ((float)_loadingMaterial.GetShaderParameter("progress") <= -1f)
            {
                GetTree().ChangeSceneToFile("res://Scenes/cutscene_1.tscn");
            }
        }
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
