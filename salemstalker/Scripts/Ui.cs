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
	private string _shopSelection = "Shortsword";
	public override void _Ready()
	{

		if (GetParent() is Player3d player)
		{
			_player = player;
		}
		_slotSelect = GetNode<Control>("Inv/SubPort/Sub/SlotSelector");
		_loadingUI = GetNode<Control>("Loading");
		_shopTypeSelection = GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions");
		_shopOption1 = GetNode<Button>("BlacksmithShop/ShopOption1");
		_shopOption2 = GetNode<Button>("BlacksmithShop/ShopOption2");
		_shopOption3 = GetNode<Button>("BlacksmithShop/ShopOption3");
		_shopOption4 = GetNode<Button>("BlacksmithShop/ShopOption4");
		_loadingUI.Visible = true;
		_loadingMaterial = _loadingUI.Material as ShaderMaterial;
		Load();
	}

	public override void _Process(double delta)
	{
		// Switch the titles of the buttons based on what shopOption you have selected
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) 
		{
			GetNode<Button>("BlacksmithShop/ShopOption1").Text = "Shortsword";
			GetNode<Button>("BlacksmithShop/ShopOption2").Text = "Falchion";
			GetNode<Button>("BlacksmithShop/ShopOption3").Text = "Rapier";
			GetNode<Button>("BlacksmithShop/ShopOption4").Text = "Dagger";
			GetNode<Button>("BlacksmithShop/ShopOption4").Visible = true;
		}
		else if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 1)
		{
			GetNode<Button>("BlacksmithShop/ShopOption1").Text = "Longsword";
			GetNode<Button>("BlacksmithShop/ShopOption2").Text = "Greatsword";
			GetNode<Button>("BlacksmithShop/ShopOption3").Text = "Battle Axe";
			GetNode<Button>("BlacksmithShop/ShopOption4").Text = "Halberd";
			GetNode<Button>("BlacksmithShop/ShopOption4").Visible = true;
		}
		else if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 2)
		{
			GetNode<Button>("BlacksmithShop/ShopOption1").Text = "Flintlock";
			GetNode<Button>("BlacksmithShop/ShopOption2").Text = "Stake Launcher";
			GetNode<Button>("BlacksmithShop/ShopOption3").Text = "Throwables";
			GetNode<Button>("BlacksmithShop/ShopOption4").Visible = false;
		}
		else
		{
			GetNode<Button>("BlacksmithShop/ShopOption1").Text = "Shield";
			GetNode<Button>("BlacksmithShop/ShopOption2").Text = "Chain Hook";
			GetNode<Button>("BlacksmithShop/ShopOption3").Text = "Holy Relic";
			GetNode<Button>("BlacksmithShop/ShopOption4").Visible = false;
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
	}

	//--- Dialouge ---
	private void _on_accept_button_button_up() { _player.QuestAccepted(); }
	private void _on_ignore_button_button_up() { _player.QuestIgnored(); }
	private void _on_continue_button_up() { _player.ContinueDialouge(); }

	// --- Blacksmith Shop ---
	private void _on_shop_option1_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) { PlayShopAnim("Shortsword"); _shopSelection = "Shortsword";}
		/*if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Longsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Flintlock"); }
		if (_shopTypeSelection.Selected == 3) { PlayShopAnim("Shield"); }*/
	}
	private void _on_shop_option2_button_up()
	{
		if (GetNode<OptionButton>("BlacksmithShop/ShopTypeOptions").Selected == 0) { PlayShopAnim("Falchion"); _shopSelection = "Falchion";}
		/*if (_shopTypeSelection.Selected == 1) { PlayShopAnim("Greatsword"); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim("Stake Launcher"); }
		if (_shopTypeSelection.Selected == 3) { PlayShopAnim("Chain Hook"); }*/ // We only have the first 2 one-handed weapons so they're the only ones that aren't commented out
		
	}
	/*private void _on_shop_option3_button_up() // We might not have the ability to upgrade secondary weapons so we might not have any _shopOption2 or 3
	{
		if (_shopTypeSelection.Selected == 0) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 1) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim(""); }
	}
	private void _on_shop_option4_button_up()
	{
		if (_shopTypeSelection.Selected == 0) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 1) { PlayShopAnim(""); }
		if (_shopTypeSelection.Selected == 2) { PlayShopAnim(""); }
	}*/
	private void _on_upgrade_button_up() 
	{ 
		Control _upg = GetNode<Control>("BlacksmithShop/View/UpgradeMenu");
		if(!_upg.Visible)
		{
			_upg.Visible = true;
		}
		else
		{
			_upg.Visible = false;
		}
	}
	private void _on_upgrade_conf_button_up()
	{
		GD.Print("it works!!!!!!");
	}
	private void _on_upgrade_deny_button_up() 
	{ 
		GetNode<Control>("BlacksmithShop/View/UpgradeMenu").Visible = false; 
	}

	private void _on_view_button_up() 
	{ 
		ColorRect _desc = GetNode<ColorRect>("BlacksmithShop/View/WeaponDesc");
		if(!_desc.Visible)
		{
			_desc.Visible = true;
		}
		else
		{
			_desc.Visible = false;
		}
	}
	
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
		PlayInvAnim("flintlock", false);
		PlayInvAnim("dagger", false);
		PlayInvAnim("longsword", false);
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
				GetNode<Node3D>("BlacksmithShop/View/PortContainer/Port/ShopPreviewWorld").GetNode<Node3D>(_prevSelection).GetNode<AnimationPlayer>("PreviewAnim").PlayBackwards("PreviewAnim");
			}
			_prevSelection = item;
			GetNode<Node3D>("BlacksmithShop/View/PortContainer/Port/ShopPreviewWorld").GetNode<Node3D>(item).GetNode<AnimationPlayer>("PreviewAnim").Play("PreviewAnim");
		}
	}

	private async void _on_slot_4_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot4", _secItemSend);
		_player.SwitchSecondaryWeapon(_secItemSend,3);
		await ToSignal(GetTree().CreateTimer(0.01), "timeout");
		_secItemSend = null;
	}

	private async void _on_slot_3_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot3", _secItemSend);
		_player.SwitchSecondaryWeapon(_secItemSend,2);
		await ToSignal(GetTree().CreateTimer(0.01), "timeout");
		_secItemSend = null;
	}

	private async void _on_slot_2_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot2", _secItemSend);
		_player.SwitchSecondaryWeapon(_secItemSend,1);
		await ToSignal(GetTree().CreateTimer(0.01), "timeout");
		_secItemSend = null;
	}
	
	private async void _on_slot_1_button_up()
	{
		_slotSelect.Visible = false;
		GD.Print("sendslot1", _secItemSend);
		_player.SwitchSecondaryWeapon(_secItemSend,0);
		await ToSignal(GetTree().CreateTimer(0.01), "timeout");
		_secItemSend = null;
	}
	private async void _on_button_button_down() { GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest); GetTree().Quit(); }
}
