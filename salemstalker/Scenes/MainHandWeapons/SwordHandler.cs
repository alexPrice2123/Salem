using Godot;
using System;
using System.Collections.Generic;

public partial class SwordHandler : Node3D
{
	protected List<Monster3d> _monsterList = new List<Monster3d>();
	protected float _firstDelay = 0.05f;
	protected float _secondDelay = 0.2f;
	public bool _crit = false;
	protected async void _on_hitbox_body_entered(Node3D body)
	{
		if (body.IsInGroup("Monster"))
		{
			await ToSignal(GetTree().CreateTimer(_firstDelay), "timeout");
			if (_crit == true)
            {
				GetNode<GpuParticles3D>("Blood").Emitting = true;
				_crit = false;
            }
			await ToSignal(GetTree().CreateTimer(_secondDelay), "timeout");
			if (body is Monster3d monster){ _monsterList.Add(monster); }
			GetNode<GpuParticles3D>("Blood").Emitting = false;
			GD.Print("enemy hit");
		}
	}

	public void ResetMonsterDebounce()
	{
		foreach (Monster3d monsterInstence in _monsterList)
		{
			monsterInstence._canBeHit = true;
		}

	}
	public void ResetMonsterList()
	{
		if (_monsterList.Count <= 0){ return; }
		foreach (Monster3d monsterInstence in _monsterList)
        {
			_monsterList.Remove(monsterInstence);
        }
        
    }
}
