using Godot;
using System;
using System.Linq;

public partial class World : Node3D
{
	bool pauseMenuActive = false;
	CanvasLayer ui;
	FileDialog fileDialog;
	GameManager gameManager;

	string currentModelName = "CurrentModel";
	string[] supportedFiles = {"obj", "dae", "fbx", "gltf"};

	public override void _Ready()
	{
        //Input.MouseMode = Input.MouseModeEnum.Captured;

		ui = GetNode<CanvasLayer>("UI");
        gameManager = GetNode<GameManager>("/root/GameManager");
		gameManager.PauseMenuClosed+=PauseMenuClosed;
		gameManager.FileButtonPressed+=FileButtonPressed;

		fileDialog = GetNode<FileDialog>("%FileDialog");

	}

	public override void _Process(double delta)
	{
	}

	private void ShowPauseMenu()
	{
		ui.Visible = false;
		AddChild(PauseMenu.GetPauseMenu());		
		pauseMenuActive = true;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if(@event.IsActionReleased("pause"))
		{
			if(!pauseMenuActive)
			{
				ShowPauseMenu();
			}	
			else
			{
				PauseMenuClosed();
			}
		}
	}

	private void _on_touch_screen_button_pressed()
	{
		ShowPauseMenu();
	}

	private void PauseMenuClosed()
	{
		pauseMenuActive = false;
		ui.Visible = true;
	}

	private void FileButtonPressed()
	{
		PauseMenuClosed();
		fileDialog.Visible = true;
	}

	private void _on_file_dialog_canceled()
	{
		GD.Print("Se cancela");
        Input.MouseMode = Input.MouseModeEnum.Captured;
		fileDialog.Visible = false;
	}

	private void _on_file_dialog_file_selected(string path)
	{
		string extension = path.GetExtension().ToLower();

		if (!supportedFiles.Contains(extension))
		{
			OS.Alert($"Archivo no soportado: {extension}");
			Input.MouseMode = Input.MouseModeEnum.Captured;
			return;
		}

		try
		{
			if (extension == "obj")
			{
				HandleObjFile(path);
				return;
			}
			
			HandleOtherFile(path);
		}
		catch (InvalidCastException invalidCastException)
		{
			OS.Alert($"Error al cargar el archivo: {invalidCastException.Message}");
		}
		finally
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

	private void HandleObjFile(string path)
	{
		MeshInstance3D modelInstance = new()
		{
			Mesh = GD.Load<Mesh>(path),
		};

		ReplaceCurrentModel(modelInstance);
	}

	private void HandleOtherFile(string path)
	{
		PackedScene packedScene = GD.Load<PackedScene>(path);
		Node3D node3D = packedScene.Instantiate<Node3D>();

		ReplaceCurrentModel(node3D);
	}

	private void ReplaceCurrentModel(Node newModel)
	{
		Node existingModel = GetNodeOrNull(currentModelName);
		existingModel?.QueueFree();

		AddChild(newModel);
		currentModelName = newModel.Name;
	}


}
