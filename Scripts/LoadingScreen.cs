using Godot;
using System;
using System.Threading.Tasks;
using static Godot.ResourceLoader;

public partial class LoadingScreen : Control
{
    private GameManager _gameManager;
    private ProgressBar _progressBar;
    private string _sceneName = string.Empty;

    Godot.Collections.Array progress = new();

    ThreadLoadStatus status;

    public async override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        //_gameManager.CloseLoadingScreen += Close;

        // _gameManager.Connect(GameManager.SignalName.CloseLoadingScreen, Callable.From(async() => 
        // {
        //     await ToSignal(GetTree().CreateTimer(0.4f), "timeout");
        //     _progressBar.Value = 100;
        //     QueueFree();
        // }));

        _gameManager.Connect(GameManager.SignalName.CloseLoadingScreen, Callable.From(() => 
        {
            var timer = GetTree().CreateTimer(0.5f);
            timer.Timeout += OnCloseLoadingScreenTimerTimeout;
        }));

        _progressBar = GetNode<ProgressBar>("CanvasLayer/ProgressBar");

        if(_sceneName == string.Empty)
        {
            for (int i = 0; i < 99; i++)
            {
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                _progressBar.Value = i + 1;
            }
            QueueFree();
        }
    }

    private void OnCloseLoadingScreenTimerTimeout()
    {
        _progressBar.Value = 100;
        QueueFree();
    }

/* 
    private void Close()
    {
        if(IsInstanceValid(this)) 
        {
            _progressBar.Value = 100; 
            QueueFree();
        }
        else
        {
            _gameManager.CloseLoadingScreen -= Close;
        }
    }
 */
    public override void _Process(double delta)
    {
        if(_sceneName != string.Empty)
        {
            LoadThreadedGetStatus(_sceneName, progress);
            _progressBar.Value = Convert.ToInt32(progress[0])*100;

            if(status ==  ThreadLoadStatus.Loaded)
            {
                var scene = LoadThreadedGet(_sceneName) as PackedScene; 
                GetTree().ChangeSceneToPacked(scene);
            }
        }
    }

    public static LoadingScreen GetLoadingScreen()
    {
        var scene = Load<PackedScene>("res://Scenes/LoadingScreen.tscn");
        return scene.Instantiate<LoadingScreen>();
    }

    public static LoadingScreen GetLoadingScreen(string sceneName)
    {
        var scene = Load<PackedScene>("res://Scenes/LoadingScreen.tscn");
        var result = scene.Instantiate<LoadingScreen>();
        result._sceneName = sceneName;

        return result;
    }

}
