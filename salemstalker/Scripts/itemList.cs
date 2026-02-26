using Godot;
using System;
using System.Collections.Generic;

public partial class itemList : Control
{
	public Dictionary<string, float> _items = new Dictionary<string, float>();
	[Export]
	public Godot.Collections.Dictionary<string, Texture2D> _itemImages { get; set; } = [];
	private Godot.Collections.Array<string> _keysArray { get; set; } = [];
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		for(int i = 1; i < 30; i++)
        {
          _keysArray.Add("");
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
    }

	private int GetFirstValidSlot()
    {
		foreach (string i in _keysArray)
        {
          if (i == ""){return _keysArray.IndexOf(i);}  
        }
        return -1;
    }

	public void AddResource(string resource, int amount)
    {
		Panel currentSlot = null;
		foreach (Panel slot in GetNode<GridContainer>("GridContainer").GetChildren())
        {
            if (slot.IsInGroup($"{resource}item")){currentSlot = slot;}
        }
		if (currentSlot != null)
        {
            _items[resource] += amount;
			currentSlot.GetNode<Label>("Count").Text = $"{_items[resource]}x";
        }
        else
        {
            _items.Add(resource, amount);
			GD.Print(GetFirstValidSlot());
			_keysArray.Insert(GetFirstValidSlot(), resource);
			Panel itemSlot = GetNode<Panel>($"GridContainer/InvSlot{_keysArray.IndexOf(resource)+1}");
			itemSlot.GetNode<TextureRect>("Image").Texture = _itemImages[resource];
			itemSlot.GetNode<Label>("Count").Text = $"{_items[resource]}x";
			itemSlot.AddToGroup($"{resource}item");
        }
    }

	public int GetItemCount(string resource)
    {
		float count;
		if (_items.TryGetValue(resource, out count)){return (int)count;}
        return 0;
    }

	public void SubtractResource(string resource, int amount)
    {
		Panel currentSlot = null;
		foreach (Panel slot in GetNode<GridContainer>("GridContainer").GetChildren())
        {
            if (slot.IsInGroup($"{resource}item")){currentSlot = slot;}
        }
		if (currentSlot != null)
        {
            _items[resource] -= amount;
			currentSlot.GetNode<Label>("Count").Text = $"{_items[resource]}x";
			if (_items[resource] <= 0)
            {
                currentSlot.RemoveFromGroup($"{resource}item");
				currentSlot.GetNode<Label>("Count").Text = "";
				currentSlot.GetNode<TextureRect>("Image").Texture = null;
				_items.Remove(resource);
				_keysArray[_keysArray.IndexOf(resource)] = "";
            }
        }
        else
        {
            GD.Print($"Player has no [{resource}]s");
        }
    }
}
