using Godot;
using System;
using System.IO;
using System.Linq;

public partial class World : Node3D
{
	bool pauseMenuActive = false;
	CanvasLayer ui;
	FileDialog fileDialog;
	GameManager gameManager;

	//string currentModelName = "currentModel";
	private static Node3D currentModel;
	string[] supportedFiles = {"gltf", "glb", "obj"};//, "fbx", "dae", "obj};

	static PlaneMesh floorMesh;
	static BoxShape3D floorCollision;
	public static Vector2 FloorSize 
	{
		get=> floorMesh.Size; 
	
		set
		{
			floorMesh.Size = value;
			floorCollision.Size = new Vector3(value.X, 0.1f, value.Y);
		}
	}

	static StandardMaterial3D floorMaterial;
	public static bool TriplanarFloorMaterial {get=> floorMaterial.Uv1Triplanar; set=>floorMaterial.Uv1Triplanar = value;}
	public static Texture2D FloorTexture {get=>floorMaterial.AlbedoTexture; set=>floorMaterial.AlbedoTexture = value;}

	static DirectionalLight3D light3D;

	public static float LightEnergy {get => light3D.LightEnergy; set=> light3D.LightEnergy = value;}

	static Godot.Environment environment;

	public static float EnvironmentEnergy {get => environment.BackgroundEnergyMultiplier; set=> environment.BackgroundEnergyMultiplier= value;}

	static PanoramaSkyMaterial panoramaSkyMaterial;

	public static Texture2D BackgroundTexture
	{
		get
		{
			return panoramaSkyMaterial.Panorama;
		}

		set
		{
			panoramaSkyMaterial.Panorama = value;
		}
	}
	public override void _Ready()
	{
        //Input.MouseMode = Input.MouseModeEnum.Captured;

		ui = GetNode<CanvasLayer>("UI");
		currentModel = GetNode<Node3D>("CurrentModel");
        gameManager = GetNode<GameManager>("/root/GameManager");
		gameManager.PauseMenuClosed += PauseMenuClosed;
		gameManager.FileButtonPressed += FileButtonPressed;

		fileDialog = GetNode<FileDialog>("%FileDialog");
		fileDialog.Invalidate();

		floorMesh = GetNode<MeshInstance3D>("Floor").Mesh as PlaneMesh;
		floorCollision = GetNode<CollisionShape3D>("Floor/StaticBody3D/CollisionShape3D").Shape as BoxShape3D;

		floorMaterial = GetNode<MeshInstance3D>("Floor").MaterialOverlay as StandardMaterial3D;
		light3D = GetNode<DirectionalLight3D>("DirectionalLight3D");

		environment = GetNode<WorldEnvironment>("WorldEnvironment").Environment;
		panoramaSkyMaterial = environment.Sky.SkyMaterial as PanoramaSkyMaterial;

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
        Input.MouseMode = Input.MouseModeEnum.Captured;
		fileDialog.Visible = false;
	}

	private void _on_file_dialog_file_selected(string path)
	{
		string extension = path.GetExtension().ToLower();

		//GD.Print("Ruta del archivo: ", path);

		if (!supportedFiles.Contains(extension))
		{
			OS.Alert($"Archivo no soportado: {extension}", "Alerta");
			Input.MouseMode = Input.MouseModeEnum.Captured;
			return;
		}

		try
		{
			//OS.ShellOpen(path);
			if (extension == "obj")
			{
				HandleObjFile(path);
				return;
			}
			
			HandleGltfFile(path);

		}
		catch (InvalidCastException invalidCastException)
		{
			OS.Alert($"Error al cargar el archivo: {invalidCastException.Message}", "ERROR");
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

		ReplacecurrentModel(modelInstance); */

		GDScript gdScript = GD.Load<GDScript>("res://Scripts/ObjFileLoader.gd");
		GodotObject objLoader = (GodotObject) gdScript.New();
		try
		{
			string mtl_path = path.Replace(".obj", ".mtl");
			
			if(!Godot.FileAccess.FileExists(mtl_path))
			{
				mtl_path = string.Empty;
			}
			

			MeshInstance3D modelInstance = new()
			{
				Mesh = (Mesh)objLoader.Call("load_obj", path, mtl_path)
			};
			

			modelInstance.CreateTrimeshCollision();

			ReplacecurrentModel(modelInstance);
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
			ReplacecurrentModel(gltfScene as Node3D);
		}
		else
		{
			OS.Alert("No se puedo procesar el archivo GLTF", "ERROR");
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

		ReplacecurrentModel(node3D);

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
		ReplacecurrentModel(node3D); */
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

	private void ReplacecurrentModel(Node3D newModel)
	{
		//Node3D existingModel = GetNodeOrNull<Node3D>(currentModelName);
		currentModel?.QueueFree();

		AddChild(newModel);
		currentModel = newModel;
	}

	public static void SetModelPosition(Vector3 position)
	{
		currentModel.Position = position;
	}

	public static void SetModelScale(Vector3 scale)
	{
		currentModel.Scale = scale;
	}

	public static void SetModelRotation(Vector3 rotation)
	{
		currentModel.RotationDegrees = rotation;
	}

	public static Vector3 GetModelPosition()
	{
		return currentModel.Position;
	}

	public static Vector3 GetModelRotation()
	{
		return currentModel.RotationDegrees;
	}
	public static Vector3 GetModelScale()
	{
		return currentModel.Scale;
	}

	private void _on_accept_dialog_confirmed()
	{
		fileDialog.Visible = true;
	}

	private void _on_accept_dialog_canceled()
	{
		fileDialog.Visible = true;
	}

}
