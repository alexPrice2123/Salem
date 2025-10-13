using Godot;
using Microsoft.VisualBasic;
using System;
using System.Runtime.CompilerServices;

public partial class Ui : Control
{
	private Player3d _player;
	private string _hovering = "falchionHover";
	private bool _loaded = false;
	private Control _loadingUI;
	private ShaderMaterial _loadingMaterial;
	public override void _Ready()
	{
		if (GetParent() is Player3d player)
		{
			_player = player;
		}
		_loadingUI = GetNode<Control>("Loading");
		_loadingUI.Visible = true;
		_loadingMaterial = _loadingUI.Material as ShaderMaterial;
		Load();
	}

	public override void _Process(double delta)
	{

		if (_loaded == true)
		{
			_loadingMaterial.SetShaderParameter("progress", (float)_loadingMaterial.GetShaderParameter("progress") + 0.05f);
			GD.Print((float)_loadingMaterial.GetShaderParameter("progress"));
			if ((float)_loadingMaterial.GetShaderParameter("progress") >= 3f)
			{
				_loaded = false;
				_loadingUI.Visible = false;
			}
		}
		GetNode<ColorRect>("Border").Color = _player._lightColor;
	}
	// --- Falchion ---
	private void _on_falchion_mouse_entered(){ PlayAnim("Falchion", true); }
	private void _on_falchion_mouse_exited(){ PlayAnim("Falchion", false); }
	private void _on_falchion_button_up() { _player.SwitchPrimaryWeapon("Falchion"); }
	
	// --- ShortSword ---
	private void _on_shortsword_mouse_entered(){ PlayAnim("ShortSword", true); }
	private void _on_shortsword_mouse_exited(){ PlayAnim("ShortSword", false); }
	private void _on_shortsword_button_up() { _player.SwitchPrimaryWeapon("ShortSword"); }

	// --- StakeGun ---
	private void _on_stake_gun_mouse_entered(){ PlayAnim("StakeGun", true); }
	private void _on_stake_gun_mouse_exited(){ PlayAnim("StakeGun", false); }

	private async void Load()
	{
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		_loaded = true;
    }
	public void Opened()
    {
		PlayAnim("ShortSword", false);
		PlayAnim("Falchion", false);
		PlayAnim("StakeGun", false);
    }

	private void PlayAnim(string sword, bool forwards)
	{

		if (forwards == true)
		{
			GetNode<Node3D>("Inv/SubPort/Sub/InvWorld").GetNode<Node3D>(sword).GetNode<AnimationPlayer>("HoverAnim").Play("Hover");
		}
        else
        {
        	GetNode<Node3D>("Inv/SubPort/Sub/InvWorld").GetNode<Node3D>(sword).GetNode<AnimationPlayer>("HoverAnim").PlayBackwards("Hover");
        }

	}
	
}
