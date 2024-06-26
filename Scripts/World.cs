using Godot;
using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

public partial class World : Node3D
{
	private bool pauseMenuActive = false;
	//private bool modelSelectorActive = false;

	private CanvasLayer ui;
	private GameManager gameManager;

	private ConfirmationDialog saveModelDialog;

	private GltfDocument gltfDocument = new();
	private string modelPath = string.Empty;

	private TouchScreenButton jumpButton;


	//string currentModelName = "currentModel";
	private static Node3D currentModel;
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


	//background

	static Godot.Environment environment;

	public static float EnvironmentEnergy {get => environment.BackgroundEnergyMultiplier; set=> environment.BackgroundEnergyMultiplier= value;}

	static PanoramaSkyMaterial panoramaSkyMaterial;

	private static Texture2D  backgroundTexture;

	public static Texture2D BackgroundTexture
	{
		get
		{
			return backgroundTexture;
			//return panoramaSkyMaterial.Panorama;
		}

		set
		{
			backgroundTexture = value;
			panoramaSkyMaterial.Panorama = value;
		}
	}

	public static bool ImageBgEnabled 
	{
		get
		{
			return environment.Sky.SkyMaterial is PanoramaSkyMaterial;
		}

		set
		{
			if(value)
			{
                panoramaSkyMaterial = new()
                {
                    Panorama = backgroundTexture
                };

                environment.Sky.SkyMaterial = panoramaSkyMaterial;
			}
			else
			{
				environment.Sky.SkyMaterial = new ProceduralSkyMaterial();
			}
		}
	}

	//lighting

	static DirectionalLight3D light3D;

	public static float LightEnergy {get => light3D.LightEnergy; set=> light3D.LightEnergy = value;}

	public static Color LightColor {get=> light3D.LightColor; set=> light3D.LightColor = value;}

	public static Vector2 LightPosition 
	{
		get
		{
			return new Vector2(light3D.Position.X, light3D.Position.Y);
		}
		
		set
		{
			light3D.Position = new(value.X, value.Y, 0);
		}
	}

	public static float LightRotationX
	{
		get
		{
			return light3D.RotationDegrees.X;
		}

		set
		{
			//light3D.RotateX(value);
			light3D.RotationDegrees = new Vector3(value, light3D.RotationDegrees.Y, light3D.RotationDegrees.Z);
		}
	}

	public static bool LightShadow {get => light3D.ShadowEnabled; set=> light3D.ShadowEnabled = value;}


	public override void _Ready()
	{
        //Input.MouseMode = Input.MouseModeEnum.Captured;

		ui = GetNode<CanvasLayer>("UI");
		currentModel = GetNode<Node3D>("CurrentModel");
		jumpButton = GetNode<TouchScreenButton>("%JumpTouchButton");

        gameManager = GetNode<GameManager>("/root/GameManager");
		// gameManager.PauseMenuClosed += PauseMenuClosed;
		// gameManager.ModelSelected += (path, saveModel) => HandleGltfFile(path, saveModel);
		// gameManager.FlyToggleButtonToggled += (toggleOn) => jumpButton.Visible = !toggleOn;
		gameManager.Connect(GameManager.SignalName.PauseMenuClosed, Callable.From(PauseMenuClosed));
		gameManager.Connect(GameManager.SignalName.ModelSelected, Callable.From<string, bool>((path, saveModel) => HandleGltfFile(path, saveModel)));
		gameManager.Connect(GameManager.SignalName.FlyToggleButtonToggled, Callable.From<bool>((toggleOn) => jumpButton.Visible = !toggleOn));
		gameManager.Connect(GameManager.SignalName.HUDVisibility, Callable.From<bool>((visible) => ui.Visible = visible));

		saveModelDialog = GetNode<ConfirmationDialog>("SaveModelConfirmDialog");
/* 		saveModelDialog.Canceled += () => 
		{
			saveModelDialog.Visible = false; 
			Input.ParseInputEvent(new InputEventAction(){Action = "pause", Pressed = true}); 
			Input.ParseInputEvent(new InputEventAction(){Action = "pause", Pressed = false});
		}; */

		saveModelDialog.Connect(ConfirmationDialog.SignalName.Canceled, Callable.From(
			() => 
			{
				saveModelDialog.Visible = false; 
				Input.ParseInputEvent(new InputEventAction(){Action = "pause", Pressed = true}); 
				Input.ParseInputEvent(new InputEventAction(){Action = "pause", Pressed = false});
			}
		));


		floorMesh = GetNode<MeshInstance3D>("Floor").Mesh as PlaneMesh;
		floorCollision = GetNode<CollisionShape3D>("Floor/StaticBody3D/CollisionShape3D").Shape as BoxShape3D;

		floorMaterial = GetNode<MeshInstance3D>("Floor").MaterialOverlay as StandardMaterial3D;
		light3D = GetNode<DirectionalLight3D>("DirectionalLight3D");

		environment = GetNode<WorldEnvironment>("WorldEnvironment").Environment;
		//panoramaSkyMaterial = environment.Sky.SkyMaterial as PanoramaSkyMaterial;

		ui.Visible = false;
		gameManager.EmitSignal(GameManager.SignalName.HUDVisibility, false);
		OS.RequestPermissions();

        FileManager.CreateFolder("user://", ModelSelection.ModelsFolderName);
        FileManager.CreateFolder("user://", ModelSelection.ModelImagesFolderName);
	}

	private void ShowPauseMenu()
	{
		if(GameManager.IsMobile)
		{
			ui.Visible = false;
			gameManager.EmitSignal(GameManager.SignalName.HUDVisibility, false);
		}

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
		{
			gameManager.EmitSignal(GameManager.SignalName.HUDVisibility, true);
		}
	}

/* 	private void _on_file_dialog_canceled()
	{
        Input.MouseMode = Input.MouseModeEnum.Captured;
		//fileDialog.Visible = false;
	} */

/* 	private void _on_file_dialog_file_selected(string path)
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
			
			HandleGltfFile(path, true);
		}
		catch (InvalidCastException invalidCastException)
		{
			OS.Alert($"Error al cargar el archivo: {invalidCastException.Message}", "ERROR");
		}
		finally
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	} */

	private void HandleObjFile(string path)  //No funciona correctamente
	{
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

	private async void SaveGltfImage(string savePath)
    {
		Vector3 viewportPos = new (0, 1000, 0);
        GltfToImage gltfToImage = GltfToImage.GetGltfToImage(viewportPos);
        AddChild(gltfToImage);

        currentModel.Position = viewportPos;
		await gltfToImage.WaitForViewportUpdate();
		Image image = gltfToImage.GetViewportImage();
		image.SavePng(savePath);
        gltfToImage.QueueFree();
		currentModel.Position = Vector3.Zero;

		gameManager.EmitSignal(GameManager.SignalName.GltfImageSaved, savePath);
		gameManager.EmitSignal(GameManager.SignalName.CloseLoadingScreen); 
    }

	private async void SaveGltfFile(string savePath)
    {
		GltfState gltfStateSave = new();
		Error error = await Task.Run(() => gltfDocument.AppendFromScene(currentModel, gltfStateSave));

		if (error != Error.Ok)
		{
			OS.Alert("Error al convertir el modelo", $"ERROR: {error}");
			return; // No emitir la señal de cierre de pantalla de carga si hay un error
		}
		//error = await Task.Run(() => gltfDocument.WriteToFilesystem(gltfStateSave, savePath));
		
		error = gltfDocument.WriteToFilesystem(gltfStateSave, savePath);

		if (error != Error.Ok)
		{
			OS.Alert("Error al guardar el modelo", $"ERROR: {error}");
			return; // No emitir la señal de cierre de pantalla de carga si hay un error
		}
		else
		{
			gameManager.EmitSignal(GameManager.SignalName.CloseLoadingScreen);
		}
    }

	private void SaveGlbFile(string savePath)
	{
		string pathWithoutFile = modelPath.Replace(modelPath.GetFile(), "");

		DirAccess dir = DirAccess.Open(pathWithoutFile);
		Error error = dir.Copy(modelPath, savePath);

		if (error != Error.Ok)
        {
            OS.Alert("Error al guardar el modelo", $"ERROR: {error}");
        } 
	}

    private async void HandleGltfFile(string path, bool save)
    {

		if(path.Equals(ModelSelection.DefaultModelPath))
		{
			var scene = GD.Load<PackedScene>(path);
			Node3D node = scene.Instantiate<Node3D>();
			ReplacecurrentModel(node);
			gameManager.EmitSignal(GameManager.SignalName.CloseLoadingScreen); 
			return;
		}

		GltfState gltfState = new();

		await Task.Run(
			() =>
			{
				var error = gltfDocument.AppendFromFile(path, gltfState);
				
				if (error != Error.Ok)
				{
					OS.Alert("No se puedo procesar el archivo GLTF", "ERROR");
					return;
				}
			}
		);

        Node3D gltfScene = gltfDocument.GenerateScene(gltfState) as Node3D;
        ReplacecurrentModel(gltfScene);
		gameManager.EmitSignal(GameManager.SignalName.CloseLoadingScreen); 

		if(save)
		{
			modelPath = path;
			saveModelDialog.Visible = true;
		}

    }

	private void _on_save_model_confirm_dialog_confirmed()
	{
		AddChild(LoadingScreen.GetLoadingScreen());
		string fileName = Path.GetFileNameWithoutExtension(modelPath);
		var files = FileManager.GetDirectoryFilesExtensionless(ModelSelection.ModelsFolderPath);
		fileName = FileManager.GetUniqueFileNameExtensionless(files, fileName);
		
		string gltfImageSavePath = Path.Combine(ModelSelection.ModelImagesFolderPath, fileName);
		gltfImageSavePath = Path.ChangeExtension(gltfImageSavePath, "png");
		SaveGltfImage(gltfImageSavePath);

		string gltfSavePath = Path.Combine(ModelSelection.ModelsFolderPath, fileName);		
		gltfSavePath = Path.ChangeExtension(gltfSavePath, "glb"); 

		if(modelPath.GetExtension().Equals("gltf"))
		{
			SaveGltfFile(gltfSavePath);
		}
		else
			SaveGlbFile(gltfSavePath);
		
	}

	private void HandleOtherFile(string path)
	{
		PackedScene packedScene = ResourceLoader.Load<PackedScene>(path);
		Node3D node3D = packedScene.Instantiate<Node3D>();

		ReplacecurrentModel(node3D);
	}

	private void ReplacecurrentModel(Node3D newModel)
	{
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
		if(GameManager.IsMobile)
		{
			ui.Visible = false;
			gameManager.EmitSignal(GameManager.SignalName.HUDVisibility, false);
		}

		var modelFiles = FileManager.GetDirectoryFiles(ModelSelection.ModelImagesFolderPath);

		bool fileDialogVisible = modelFiles == null || !modelFiles.Any();

		pauseMenuActive = true;
		AddChild(ModelSelection.GetModelSelection(fileDialogVisible));
		GetTree().Paused = true;


	}

	private void _on_accept_dialog_canceled()
	{
		_on_accept_dialog_confirmed();
	}

}
