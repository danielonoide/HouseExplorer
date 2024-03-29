using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public partial class PauseMenu : Control
{
	HSlider[] sensitivitySliders = new HSlider[2];

	HSlider speedSlider, jumpSlider;
	Label speedSliderLabel, jumpSliderLabel;
	CheckButton flyButton;
	Label[] labels = new Label[2];

	private const float HAdjustment = 10000f;
	private const float VAdjustment = 5000f;

	GameManager gameManager;
	Control mainMenuControl, characterOptionsControl, modelOptionsControl, sceneryOptionsControl;
	TextureButton backButton;
	ColorRect colorRect;

	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
		mainMenuControl = GetNode<Control>("CanvasLayer/MainMenu");
		characterOptionsControl = GetNode<Control>("CanvasLayer/CharacterOptions");
		modelOptionsControl = GetNode<Control>("CanvasLayer/ModelOptions");
		sceneryOptionsControl = GetNode<Control>("CanvasLayer/SceneryMenu");
		backButton = GetNode<TextureButton>("CanvasLayer/BackButton");
		jumpSlider = GetNode<HSlider>("%JumpSlider");
		jumpSliderLabel = jumpSlider.GetChild<Label>(0);
		speedSlider = GetNode<HSlider>("%SpeedSlider");
		speedSliderLabel = speedSlider.GetChild<Label>(0);
		flyButton = GetNode<CheckButton>("%FlyToggleButton");

		colorRect = GetNode<ColorRect>("CanvasLayer/ColorRect");


		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;

		var sliderNodes = GetTree().GetNodesInGroup("HSlider");
        for(int i = 0; i<sliderNodes.Count; i++) 
		{
            //sensitivitysensitivitySliders[i].Connect("value_changed", new Callable(this, nameof(OnSensitivitySliderValueChanged)).Bind(nameof(OnSensitivitySliderValueChanged), new Godot.Collections.Array { sensitivitysensitivitySliders[i] })  );
            //hSlider.Connect("value_changed", new Callable(this, nameof(OnSensitivitySliderValueChanged)).Bind(new Godot.Collections.Array { hSlider }));

			HSlider hSlider = sensitivitySliders[i] = (HSlider)sliderNodes[i];
			int sliderIndex = i; //"closure" de no hacer esto se almacenaría la referencia i y no funcionaría 
			//por ello se crea una variable sliderIndex, que se crea en cada iteración
			//si la señal no tiene parámetro solo se pone () en vez de value en este caso
			//estamos añadiendo un delegado
            hSlider.ValueChanged += value => { OnSensitivitySliderValueChanged(value, sliderIndex); }; 

			labels[i] = hSlider.GetChild<Label>(0);
        }

		sensitivitySliders[0].Value = Player.HSensitivity * HAdjustment;
		sensitivitySliders[1].Value = Player.VSensitivity * VAdjustment;

		labels[0].Text = sensitivitySliders[0].Value.ToString();
		labels[1].Text = sensitivitySliders[1].Value.ToString();


		jumpSlider.Value = Player.JumpVelocity / Player.InitialJumpVelocity;
		speedSlider.Value = Player.Speed / Player.InitialSpeed;
		flyButton.ButtonPressed = Player.Fly;

	}

	private void OnSensitivitySliderValueChanged(double value, int sliderIndex)
	{
		labels[sliderIndex].Text = value.ToString();

		if(sliderIndex == 0)
		{
			Player.HSensitivity = (float)(value / HAdjustment);
			return;
		}
		
		Player.VSensitivity = (float)(value / VAdjustment);
	}

	private void _on_resume_button_pressed()
	{
		Close();
		gameManager.EmitSignal(GameManager.SignalName.PauseMenuClosed);
	}

	private void _on_restart_button_pressed()
	{
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
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

	private void _on_model_button_pressed()
	{
		mainMenuControl.Visible = false;
		backButton.Visible = false;
		modelOptionsControl.Visible = true;
		colorRect.Visible = false;
	}

	private void _on_character_button_pressed()
	{
		mainMenuControl.Visible = false;
		backButton.Visible = true;
		characterOptionsControl.Visible = true;
	}

	private void _on_scenery_button_pressed()
	{
		mainMenuControl.Visible = false;
		backButton.Visible = true;
		sceneryOptionsControl.Visible = true;
	}

	private void _on_back_button_pressed()
	{
		mainMenuControl.Visible = true;
		backButton.Visible = false;
		characterOptionsControl.Visible = false;
		sceneryOptionsControl.Visible = false;
	}

	private void _on_speed_slider_value_changed(float value)
	{
		Player.Speed = Player.InitialSpeed * value;
		speedSliderLabel.Text = value.ToString();
	}

	private void _on_jump_slider_value_changed(float value)
	{
		Player.JumpVelocity = Player.InitialJumpVelocity * value;
		jumpSliderLabel.Text = value.ToString();
	}

	private void _on_fly_button_toggled(bool toggled_on)
	{
		Player.Fly = toggled_on;
	}
	
}
