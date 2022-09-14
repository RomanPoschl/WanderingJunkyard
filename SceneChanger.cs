using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class SceneChanger : CanvasLayer
{
    public Node CurrentScene { get; set; }
    ColorRect _curtain;
    AnimationPlayer _animationPlayer;
    ProgressBar _progressBar;


    ResourceInteractiveLoader _loader;
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

        Viewport root = GetTree().Root;
        CurrentScene = root.GetChild(root.GetChildCount() - 1);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (_loader == null)
        {
            SetProcess(false);
            return;
        }

        if(_waitFrames > 0)
        {
            _waitFrames -= 1;
            return;
        }

        var t = OS.GetTicksMsec();

        while(OS.GetTicksMsec() < t + _timeMax)
        {
            var err = _loader.Poll();

            if (err == Error.FileEof)
            {
                var resource = _loader.GetResource();
                _loader = null;
                SetNewScene(resource);
                break;
            }
            else if (err == Error.Ok)
            {
                UpdateProgress();
            }
            else 
            {
                _loader = null;
                break;
            }
        }
    }

    public async void ChangeScene(string nextScene)
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
        _loader = ResourceLoader.LoadInteractive(path);

        Debug.Assert(_loader != null);

        SetProcess(true);

        CurrentScene.QueueFree();

        _waitFrames = 100;
    }

    void UpdateProgress()
    {
        var progress = (float)(_loader.GetStage() / _loader.GetStageCount());
        _progressBar.Value = progress;
    }

    async Task SetNewScene(Resource newScene)
    {
        CurrentScene = ((PackedScene)newScene).Instance();
        GetTree().Root.AddChild(CurrentScene);
        GetTree().CurrentScene = CurrentScene;

        _progressBar.Hide();
        _animationPlayer.PlayBackwards("Fade");
        await ToSignal(_animationPlayer, "animation_finished");
    }
}
