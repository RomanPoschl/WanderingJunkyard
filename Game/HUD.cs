using Godot;
using System;

public class HUD : Control
{
    Sprite _joystick;
    Vector2 _joystickDefaultPosition;

    public override void _Ready()
    {
        base._Ready();
        _joystick = GetNode<Sprite>("Control/Joystick");
        _joystickDefaultPosition = _joystick.Position;
    }

    public void ShowMessage(string text)
    {
        var message = GetNode<Label>("Message");
        message.Text = text;
        message.Show();

        GetNode<Timer>("MessageTimer").Start();
    }
    
    async public void ShowGameOver()
    {
        ShowMessage("Game Over");

        var messageTimer = GetNode<Timer>("MessageTimer");
        await ToSignal(messageTimer, "timeout");

        var message = GetNode<Label>("Message");
        message.Text = "Dodge the\nCreeps!";
        message.Show();

        await ToSignal(GetTree().CreateTimer(1), "timeout");
        GetNode<Button>("StartButton").Show();
    }
    
    public void UpdateScore(int score)
    {
        GetNode<Label>("ScoreLabel").Text = score.ToString();
    }

    private void OnMessageTimerTimeout()
    {
        GetNode<Label>("Message").Hide();
    }


    void OnControlGuiInput(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touch)
        {
            if(@event.IsPressed())
            {
                _joystick.Position = touch.Position;
                _joystick.Show();
            }
            else
            {
                _joystick.Position = _joystickDefaultPosition;
            }
        }
    }

    void OnJumpButtonPressed()
    {
        var events = GetNode<Events>(Constants.Events);
        events.EmitSignal(nameof(Events.OnJumpPressedSignal));
    }

    void OnBuildButtonPressed()
    {
        var events = GetNode<Events>(Constants.Events);
        events.EmitSignal(nameof(Events.OnBuildPressedSignal));
    }
}






