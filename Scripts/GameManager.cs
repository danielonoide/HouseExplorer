using Godot;
using System;

public partial class GameManager : Node
{
    [Signal]
    public delegate void PauseMenuClosedEventHandler();

    [Signal]
    public delegate void FileButtonPressedEventHandler();

    [Signal]
    public delegate void ModelSelectedEventHandler(string path, bool save);

    [Signal]
    public delegate void GltfImageSavedEventHandler(string imagePath);

    [Signal]
    public delegate void CloseLoadingScreenEventHandler();

    [Signal]
    public delegate void FlyToggleButtonToggledEventHandler(bool toggledOn);

    [Signal]
    public delegate void HUDVisibilityEventHandler(bool visible);

	public static bool IsMobile = true;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
		//DisplayServer.WindowSetSize(DisplayServer.WindowGetSize());
        if(!OS.GetName().Equals("Android"))
		{
			IsMobile = false;
		}

        GD.Print("ES móbil: ", IsMobile);
    }

	public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            // Verifica si se presiona la tecla F11
            if (eventKey.Pressed && (int)eventKey.Keycode == (int)Key.F11)
            {
                //OS.WindowFullscreen = !OS.WindowFullscreen;  //ya no funciona en godot 4

                // cambia el estado de pantalla completa
				//DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? DisplayServer.WindowMode.Windowed : DisplayServer.WindowMode.Fullscreen);

				var currentMode = DisplayServer.WindowGetMode();
				var newMode = (currentMode == DisplayServer.WindowMode.Fullscreen) 
					? DisplayServer.WindowMode.Windowed 
					: DisplayServer.WindowMode.Fullscreen;

				DisplayServer.WindowSetMode(newMode);
				
            }
        }
    }
}
