using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public partial class PauseMenu : Control
{

	private HSlider[] sensitivitySliders = new HSlider[2];
	private HSlider speedSlider, jumpSlider;
	private Label speedSliderLabel, jumpSliderLabel;
	private CheckButton flyButton;
	private Label[] labels = new Label[2];
	private const float HAdjustment = 10000f;
	 private const float VAdjustment = 5000f;
	protected GameManager gameManager;
	private Control mainMenuControl, characterOptionsControl, modelOptionsControl, sceneryOptionsControl;
	private TextureButton backButton;
	private ColorRect colorRect;

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
			//GD.Print("Slider name: ", hSlider.Name);
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
/* 		GetTree().Paused = false;
		GD.Print("Current scene: ", GetTree().CurrentScene.Name);
		GD.Print(GetTree().ReloadCurrentScene()); */

/* 		GetTree().Paused = false;
		var rootChild = GetTree().Root.GetChild(3);

		GD.Print("Root child: ", rootChild.Name);
		rootChild.QueueFree();

		var scene = GD.Load<PackedScene>("res://Scenes/World.tscn");
		World world = scene.Instantiate<World>();

		GetTree().Root.AddChild(world); */

		//var executablePath = OS.GetExecutablePath();
		//OS.Execute(executablePath, Array.Empty<string>());

		if(GameManager.IsMobile)
		{
			GetTree().Paused = false;
			gameManager.EmitSignal(GameManager.SignalName.PauseMenuClosed);
			GetTree().ReloadCurrentScene();
			return;
		}

		OS.CreateInstance(Array.Empty<string>());
		GetTree().Quit();
	}

	private void _on_model_selection_button_pressed()
	{
/* 		GetTree().Paused = false;
		QueueFree();
		gameManager.EmitSignal(GameManager.SignalName.FileButtonPressed); */

/* 		backButton.Visible = true;
		mainMenuControl.Visible = false;
		modelSelectionControl.Visible = true; */

		AddChild(ModelSelection.GetModelSelection());
	}

	private void _on_quit_button_pressed()
	{
		GetTree().Quit();
	}

	protected void Close()
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
		colorRect.Visible = false;
	}

	private void _on_back_button_pressed()
	{
		mainMenuControl.Visible = true;
		backButton.Visible = false;
		characterOptionsControl.Visible = false;
		sceneryOptionsControl.Visible = false;
		colorRect.Visible =  true;
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
		gameManager.EmitSignal(GameManager.SignalName.FlyToggleButtonToggled, toggled_on);
	}
	
}
