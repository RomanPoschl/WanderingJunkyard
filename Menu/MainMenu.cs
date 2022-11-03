using Godot;
using System;
using System.Diagnostics;
using System.Drawing.Text;

public partial class MainMenu : Control
{
    
    SceneChanger _sceneChanger;
     
    public override void _Ready()
    {
        _sceneChanger = GetNode<SceneChanger>("/root/SceneChanger");

        #if DEBUG
        _sceneChanger.ChangeSceneToFile("res://Game/Game.tscn");
        #endif
    }

    private void OnStartButtonPressed()
    {
        _sceneChanger.ChangeSceneToFile("res://Game/Game.tscn");
    }
}
