using Godot;
using Microsoft.VisualBasic;
using System;
using System.Runtime.CompilerServices;

public partial class Ui : Control
{
	private Player3d _player;
	private string _hovering = "falchionHover";
	private string _secItemSend = null;
	private bool _loaded = false;
	private Control _loadingUI;
	private ShaderMaterial _loadingMaterial;
	private Control _slotSelect;
	public override void _Ready()
	{
		if (GetParent() is Player3d player)
		{
			_player = player;
		}
		_loadingUI = GetNode<Control>("Loading");
		_slotSelect = GetNode<Control>("Inv/SubPort/Sub/SlotSelector");
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

	//--- Dialouge ---
	private void _on_accept_button_button_up() { _player.QuestAccepted(); }
	private void _on_ignore_button_button_up() { _player.QuestIgnored(); }
	private void _on_continue_button_up() { _player.ContinueDialouge(); }


	// --- Falchion ---
	private void _on_falchion_mouse_entered() { PlayAnim("Falchion", true); }
	private void _on_falchion_mouse_exited() { PlayAnim("Falchion", false); }
	private void _on_falchion_button_up() { _player.SwitchPrimaryWeapon("Falchion"); }
	
	// --- ShortSword ---
	private void _on_shortsword_mouse_entered() { PlayAnim("ShortSword", true); }
	private void _on_shortsword_mouse_exited() { PlayAnim("ShortSword", false); }
	private void _on_shortsword_button_up() { _player.SwitchPrimaryWeapon("ShortSword"); }

	// --- StakeGun ---
	private void _on_stake_gun_mouse_entered() { PlayAnim("StakeGun", true); }
	private void _on_stake_gun_mouse_exited() { PlayAnim("StakeGun", false); }
	private void _on_stake_gun_button_up()
	{
		_secItemSend = "StakeGun";
		_slotSelect.Visible = true;
	}

	// --- Flintlock ---
	private void _on_gun_mouse_entered() { PlayAnim("flintlock", true); }
	private void _on_gun_mouse_exited() { PlayAnim("flintlock", false); }
	private void _on_gun_button_up()
	{
		_secItemSend = "FlintGun";
		_slotSelect.Visible = true;
    }

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

	private void _on_slot_4_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot4");
		_secItemSend = null;
	}

	private void _on_slot_3_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot3");
		_secItemSend = null;
	}

	private void _on_slot_2_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot2");
		_secItemSend = null;
	}
	
	private void _on_slot_1_button_up()
    {
		_slotSelect.Visible = false;
		GD.Print("sendslot1");
		_secItemSend = null;
    }
	
}
