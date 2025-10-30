using Godot;
using Microsoft.VisualBasic;
using System;
using System.Runtime.CompilerServices;

public partial class Ui : Control
{
	private Player3d _player;
	private string _hovering = "falchionHover";
	private string _prevSelection;
	private string _secItemSend = null;
	private bool _loaded = false;
	private Control _loadingUI;
	private ShaderMaterial _loadingMaterial;
	private Control _slotSelect;
	private OptionButton _shopTypeSelection;
	private int _typeSelection;
	private Button _shopOption1;
	private Button _shopOption2;
	private Button _shopOption3;
	private Button _shopOption4;
	public override void _Ready()
	{
		PlayShopAnim("Falchion");
		PlayShopAnim("Shortsword");
		
		if (GetParent() is Player3d player)
		{
			_player = player;
		}
		_loadingUI = GetNode<Control>("Loading");
		_shopTypeSelection = GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions");
		_shopOption1 = GetNode<Button>("BlacksmithShop/ShopOption1");
		_shopOption2 = GetNode<Button>("BlacksmithShop/ShopOption2");
		_shopOption3 = GetNode<Button>("BlacksmithShop/ShopOption3");
		_shopOption4 = GetNode<Button>("BlacksmithShop/ShopOption4");
		_slotSelect = GetNode<Control>("Inv/SubPort/Sub/SlotSelector");
		_loadingUI.Visible = true;
		_loadingMaterial = _loadingUI.Material as ShaderMaterial;
		Load();
	}

	public override void _Process(double delta)
	{
		// Switch the titles of the buttons based on what shopOption you have selected
		if (_shopTypeSelection.Selected == 0) 
		{
			_shopOption1.Text = "Shortsword";
			_shopOption2.Text = "Falchion";
			_shopOption3.Text = "Rapier";
			_shopOption4.Text = "Dagger";
			_shopOption4.Visible = true;
		}
		else if (_shopTypeSelection.Selected == 1)
		{
			_shopOption1.Text = "Longsword";
			_shopOption2.Text = "Greatsword";
			_shopOption3.Text = "Battle Axe";
			_shopOption4.Text = "Halberd";
			_shopOption4.Visible = true;
		}
		else if (_shopTypeSelection.Selected == 2)
		{
			_shopOption1.Text = "Flintlock";
			_shopOption2.Text = "Stake Launcher";
			_shopOption3.Text = "Throwables";
			_shopOption4.Visible = false;
		}
		else
		{
			_shopOption1.Text = "Shield";
			_shopOption2.Text = "Chain Hook";
			_shopOption3.Text = "Holy Relic";
			_shopOption4.Visible = false;
		}
		
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

	// --- Blacksmith Shop ---
	private void _on_shop_option1_button_up() 
	{
		if (_shopTypeSelection.Selected == 0) { PlayShopAnim("Shortsword"); }
		/*if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Longsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Flintlock"); }
		if (_shopTypeSelection.Selected == 3) { PlayShopAnim("Shield"); }*/
	}
	private void _on_shop_option2_button_up()
	{
		if (_shopTypeSelection.Selected == 0) { PlayShopAnim("Falchion"); }
		/*if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Greatsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Stake Launcher"); }
		if (_shopTypeSelection.Selected == 3) { PlayShopAnim("Chain Hook"); }*/ // We only have the first 2 one-handed weapons so they're the only ones that aren't commented out
		
	}
	/*private void _on_shop_option3_button_up() // We might not have the ability to upgrade secondary weapons so we might not have any _shopOption2 or 3
	{
		if (_shopTypeSelection.Selected == 0) { PlayShopAnim("Shortsword"); }
		if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Longsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Flintlock"); }
	}
	private void _on_shop_option4_button_up()
	{
		if (_shopTypeSelection.Selected == 0) { PlayShopAnim("Shortsword"); }
		if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Longsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Flintlock"); }
	}*/
	
	// --- Falchion ---
	private void _on_falchion_mouse_entered(){ PlayInvAnim("Falchion", true); }
	private void _on_falchion_mouse_exited(){ PlayInvAnim("Falchion", false); }
	private void _on_falchion_button_up() { _player.SwitchPrimaryWeapon("Falchion"); }
	
	// --- ShortSword ---
	private void _on_shortsword_mouse_entered(){ PlayInvAnim("ShortSword", true); }
	private void _on_shortsword_mouse_exited(){ PlayInvAnim("ShortSword", false); }
	private void _on_shortsword_button_up() { _player.SwitchPrimaryWeapon("ShortSword"); }

	// --- StakeGun ---
	private void _on_stake_gun_mouse_entered() { PlayInvAnim("StakeGun", true); }
	private void _on_stake_gun_mouse_exited() { PlayInvAnim("StakeGun", false); }
	private void _on_stake_gun_button_up()
	{
		_secItemSend = "StakeGun";
		_slotSelect.Visible = true;
	}

	// --- Flintlock ---
	private void _on_gun_mouse_entered() { PlayInvAnim("flintlock", true); }
	private void _on_gun_mouse_exited() { PlayInvAnim("flintlock", false); }
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
		PlayInvAnim("ShortSword", false);
		PlayInvAnim("Falchion", false);
		PlayInvAnim("StakeGun", false);
	}

	private void PlayInvAnim(string sword, bool forwards)
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
	
	private void PlayShopAnim(string item)
	{
		if (_prevSelection != item) // switches from the shown weapon on the preview to the selected weapon
		{
			if (_prevSelection != null) 
			{ 
				GetNode<Node3D>("BlacksmithShop/PortContainer/Port/ShopPreviewWorld").GetNode<Node3D>(_prevSelection).GetNode<AnimationPlayer>("PreviewAnim").PlayBackwards("PreviewAnim");
			}
			_prevSelection = item;
			GetNode<Node3D>("BlacksmithShop/PortContainer/Port/ShopPreviewWorld").GetNode<Node3D>(item).GetNode<AnimationPlayer>("PreviewAnim").Play("PreviewAnim");
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
