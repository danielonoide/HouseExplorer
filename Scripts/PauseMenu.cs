using Godot;
using System;
using System.Linq;

public partial class PauseMenu : Control
{
	HSlider[] sliders = new HSlider[2];
	Label[] labels = new Label[2];

	private const float hAdjustment = 10000f;
	private const float vAdjustment = 5000f;

	GameManager gameManager;

	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
		
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;

		var sliderNodes = GetTree().GetNodesInGroup("HSlider");
        for(int i = 0; i<sliderNodes.Count; i++) 
		{
            //sensitivitySliders[i].Connect("value_changed", new Callable(this, nameof(OnSensitivitySliderValueChanged)).Bind(nameof(OnSensitivitySliderValueChanged), new Godot.Collections.Array { sensitivitySliders[i] })  );
            //hSlider.Connect("value_changed", new Callable(this, nameof(OnSensitivitySliderValueChanged)).Bind(new Godot.Collections.Array { hSlider }));

			HSlider hSlider = sliders[i] = (HSlider)sliderNodes[i];
			int sliderIndex = i; //"closure" de no hacer esto se almacenaría la referencia i y no funcionaría 
			//por ello se crea una variable sliderIndex, que se crea en cada iteración
			//si la señal no tiene parámetro solo se pone () en vez de value en este caso
			//estamos añadiendo un delegado
            hSlider.ValueChanged += value => { OnSensitivitySliderValueChanged(value, sliderIndex); }; 

			labels[i] = hSlider.GetChild<Label>(0);
        }

		sliders[0].Value = Player.HSensitivity * hAdjustment;
		sliders[1].Value = Player.VSensitivity * vAdjustment;

		labels[0].Text = sliders[0].Value.ToString();
		labels[1].Text = sliders[1].Value.ToString();

	}

	private void OnSensitivitySliderValueChanged(double value, int sliderIndex)
	{
		labels[sliderIndex].Text = value.ToString();

		if(sliderIndex == 0)
		{
			Player.HSensitivity = (float)(value / hAdjustment);
			return;
		}
		
		Player.VSensitivity = (float)(value / vAdjustment);
	}

	private void _on_resume_button_pressed()
	{
		Close();
		gameManager.EmitSignal(GameManager.SignalName.PauseMenuClosed);
	}

	private void _on_file_button_pressed()
	{
		GetTree().Paused = false;
		QueueFree();
		gameManager.EmitSignal(GameManager.SignalName.FileButtonPressed);
	}

	private void _on_quit_button_pressed()
	{
		GetTree().Quit();
	}

	private void Close()
	{
		Input.MouseMode=Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
		QueueFree();
	}

	public static PauseMenu GetPauseMenu()
	{
		PackedScene packedScene=(PackedScene)ResourceLoader.Load("res://Scenes/PauseMenu.tscn");
		return (PauseMenu)packedScene.Instantiate();
	}

	public override void _Input(InputEvent @event)
	{
		if(@event.IsActionPressed("pause"))
		{
			Close();
		}
	}

	
}
