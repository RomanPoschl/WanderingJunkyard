using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class SceneChanger : CanvasLayer
{
	public Node CurrentScene { get; set; }
	ColorRect _curtain;
	AnimationPlayer _animationPlayer;
	ProgressBar _progressBar;
	Timer _timer;
	string _path;

	int _waitFrames;
	ulong _timeMax = 20;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_progressBar = GetNode<ProgressBar>("Curtain/ProgressBar");
		_curtain = GetNode<ColorRect>("Curtain");
		_curtain.MouseFilter = Control.MouseFilterEnum.Ignore;
		_curtain.Color = new Color(0);

		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		var root = GetTree().Root;
		CurrentScene = root.GetChild(root.GetChildCount() - 1);

		_timer = new Timer();
		_timer.WaitTime = .5f;
		_timer.Timeout += OnTimerTimeout;
	}

	async void OnTimerTimeout()
	{
		var progressArray = new Godot.Collections.Array();
		var status = ResourceLoader.LoadThreadedGetStatus(_path, progressArray);
		var progress = (float)progressArray[0] * 100.0f;

		UpdateProgress(progress);

		if(status == ResourceLoader.ThreadLoadStatus.Loaded)
		{
			var resource = ResourceLoader.LoadThreadedGet(_path);
			await SetNewScene(resource);
			_timer.Stop();
		}
		else if (status != ResourceLoader.ThreadLoadStatus.InProgress)
		{
			throw new Exception("Scene load failed");
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public async void ChangeSceneToFile(string nextScene)
	{
		_animationPlayer.Play("Fade");
		await ToSignal(_animationPlayer, "animation_finished");

		_progressBar.Show();

		GoToScene(nextScene);
	}

	void GoToScene(string nextScene)
	{
		CallDeferred(nameof(DeferredGotoScene), nextScene);
	}

	void DeferredGotoScene(string path)
	{
		_path = path;
		ResourceLoader.LoadThreadedRequest(_path);

		SetProcess(true);
		CurrentScene.QueueFree();
		_waitFrames = 100;
	}

	void UpdateProgress(float progress)
	{
		_progressBar.Value = progress;
	}

	async Task SetNewScene(Resource newScene)
	{
		CurrentScene = ((PackedScene)newScene).Instantiate();
		GetTree().Root.AddChild(CurrentScene);
		GetTree().CurrentScene = CurrentScene;

		_progressBar.Hide();
		_animationPlayer.PlayBackwards("Fade");
		await ToSignal(_animationPlayer, "animation_finished");
	}
}
