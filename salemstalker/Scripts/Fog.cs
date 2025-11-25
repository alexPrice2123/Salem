using Godot;
using System;

public partial class Fog : Node3D
{
	private Player3d _player;
	[Export] public float _defaultThickFog = 0.5f;
	[Export] public float _defaultNormalFog = 0.035f;
	private float _thickFog;
	private float _normalFog;
	private PackedScene _hollowShadow = GD.Load<PackedScene>("res://Scenes/Monsters/hollowShadow.tscn"); // Scene reference for the hollow
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Vector3 _rangedPosition;
	private bool _spawned = false;
	private Node3D _holder;
	float _exitFadeTime = 0.5f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		_player = GetParent().GetNode<Player3d>("Player_3d");
		_holder = GetParent().GetNode<Node3D>("MonsterHolder/Hold2/Hold");
		_thickFog = _defaultThickFog;
		_normalFog = _defaultNormalFog;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_player._inGoalArea == false)
        {
			_exitFadeTime = 2f;
            float distance = (_player.GlobalPosition - GetParent().GetNode<Node3D>("GoalArea").GlobalPosition).Length();
			_normalFog = ((1-_player._lookingAtGoalPoint)*distance*0.015f)+0.055f;
        }
        else
        {
            _normalFog = _defaultNormalFog;
        }
		if (_player._hallucinationFactor > 0.1f)
		{
			_exitFadeTime = 0.5f;
			GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogDensity = Mathf.Lerp(GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogDensity, _thickFog, (float)delta);
			if (_spawned == false)
			{
				StartCountdown();
				_spawned = true;
				CharacterBody3D monsterInstance = _hollowShadow.Instantiate<CharacterBody3D>(); // Create monster instance
				_holder.AddChild(monsterInstance);
				monsterInstance.Position = _player.GetNode<Node3D>("Head/TripSpawn").GlobalPosition + new Vector3(_rng.RandfRange(-5, 5), 0f, _rng.RandfRange(-5, 5));
			}
		}
		else
		{
			GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogDensity = Mathf.Lerp(GetNode<WorldEnvironment>("WorldEnvironment").Environment.VolumetricFogDensity, _normalFog, (float)delta*_exitFadeTime);
		}
	}
	
	private async void StartCountdown()
    {
		await ToSignal(GetTree().CreateTimer(2.5f), "timeout");
		_spawned = false;
    }
}
