using Godot;
using System;
using System.Linq;

public partial class World : Node3D
{
	bool pauseMenuActive = false;
	CanvasLayer ui;
	FileDialog fileDialog;
	GameManager gameManager;

	//string currentModelName = "CurrentModel";
	Node3D currentModel;
	string[] supportedFiles = {"gltf"};//, "fbx", "dae", "obj};

	public override void _Ready()
	{
        //Input.MouseMode = Input.MouseModeEnum.Captured;

		ui = GetNode<CanvasLayer>("UI");
		currentModel = GetNode<Node3D>("CurrentModel");
        gameManager = GetNode<GameManager>("/root/GameManager");
		gameManager.PauseMenuClosed+=PauseMenuClosed;
		gameManager.FileButtonPressed+=FileButtonPressed;

		fileDialog = GetNode<FileDialog>("%FileDialog");
		fileDialog.Invalidate();

		if(!GameManager.IsMobile)
		{
			ui.Visible = false;
			//fileDialog.RootSubfolder = OS.GetUserDataDir();
		}
		else
		{
			OS.RequestPermissions();
			fileDialog.RootSubfolder = OS.GetSystemDir(OS.SystemDir.Downloads, true);
		}

	}

	public override void _Process(double delta)
	{
	}

	private void ShowPauseMenu()
	{
		if(GameManager.IsMobile)
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
		if(GameManager.IsMobile)
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

		//GD.Print("Ruta del archivo: ", path);

		if (!supportedFiles.Contains(extension))
		{
			OS.Alert($"Archivo no soportado: {extension}");
			Input.MouseMode = Input.MouseModeEnum.Captured;
			return;
		}

		try
		{
			//OS.ShellOpen(path);
/* 			if (extension == "obj")
			{
				HandleObjFile(path);
				return;
			} */
			
			HandleGltfFile(path);

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

	private void HandleObjFile(string path)  //No funciona correctamente
	{
/* 		MeshInstance3D modelInstance = new()
		{
			Mesh = ResourceLoader.Load<Mesh>(path)
		};

		modelInstance.CreateTrimeshCollision();

		ReplaceCurrentModel(modelInstance); */

		GDScript gdScript = GD.Load<GDScript>("res://Scripts/ObjFileLoader.gd");
		GodotObject objLoader = (GodotObject) gdScript.New();
		try
		{
			MeshInstance3D modelInstance = new()
			{
				Mesh = (Mesh)objLoader.Call("load_obj", path, path.Replace(".obj", ".mtl"))
			};
			

			modelInstance.CreateTrimeshCollision();

			ReplaceCurrentModel(modelInstance);
		}catch(Exception e)
		{
			OS.Alert("No se pudo cargar el modelo ", e.Message);
		}
	}

	private void HandleGltfFile(string path)
	{
		GltfDocument gltfDocument = new();
		GltfState gltfState = new();
		var error = gltfDocument.AppendFromFile(path, gltfState);
		if(error == Error.Ok)
		{
			var gltfScene = gltfDocument.GenerateScene(gltfState);
			//AddCollisionsGltf(gltfScene);
			ReplaceCurrentModel(gltfScene as Node3D);
		}
		else
		{
			OS.Alert("No se puedo procesar el archivo GLTF");
		}

	}

	private void AddCollisionsGltf(Node gltfScene)
	{
		GD.Print("LLamado");
		foreach(Node node in gltfScene.GetChildren())
		{
			if(node is MeshInstance3D meshInstance3D)
			{
				meshInstance3D.CreateConvexCollision(true, true);
			}
			else
			{
				AddCollisionsGltf(gltfScene);
			}
		}

	}

	private void HandleOtherFile(string path)
	{
		PackedScene packedScene = ResourceLoader.Load<PackedScene>(path);
		Node3D node3D = packedScene.Instantiate<Node3D>();

		ReplaceCurrentModel(node3D);

/* 		PackedScene packedScene = GD.Load<PackedScene>(path);
		Node3D node3D = packedScene.Instantiate<Node3D>();

		foreach(Node node in node3D.GetChildren())
		{
			if(node is MeshInstance3D meshInstance3D)
			{
				meshInstance3D.CreateTrimeshCollision();
				currentModel?.QueueFree();
				AddChild(meshInstance3D);
				currentModel = meshInstance3D;
				return;
			}
		}

		OS.Alert("No se pueden agregar colisiones a este tipo de archivo");
		ReplaceCurrentModel(node3D); */
	}

	private void CreateCollision3D(MeshInstance3D baseModel)
	{
		Aabb aabb = baseModel.Mesh.GetAabb();

		
        BoxShape3D boxShape = new()
        {
            Size = aabb.Size
        };

		CollisionShape3D collisionShape = new()
		{
			Shape = boxShape
		};

		//area3D.AddChild(collisionShape);	

    }

	private void ReplaceCurrentModel(Node3D newModel)
	{
		//Node3D existingModel = GetNodeOrNull<Node3D>(currentModelName);
		currentModel?.QueueFree();

		AddChild(newModel);
		currentModel = newModel;
	}


}
