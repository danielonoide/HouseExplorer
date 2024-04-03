using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class ModelSelection : Control
{
    private ItemList _itemList;
    private FileDialog _fileDialog;
    public const string ModelsFolderName = "SavedModels";
    public const string ModelImagesFolderName = "SavedModelImages";
    public const string ModelsFolderPath = $"user://{ModelsFolderName}/";
    public const string ModelImagesFolderPath = $"user://{ModelImagesFolderName}/";
    public const string DefaultModelPath = "res://Assets/3DModels/House_001_GLB.glb";
    private Texture2D defaultModelImage = GD.Load<Texture2D>("res://Assets/Images/ModelImage.png");

    private Dictionary<int, string> _modelFileNames = new();

    private int? _selectedItem = null;
    private GameManager _gameManager;

	private string[] _supportedFiles = {"gltf", "glb"};

    public override void _Ready()
    {
		_gameManager = GetNode<GameManager>("/root/GameManager");
        _gameManager.GltfImageSaved += LoadModelImage;

        _itemList = GetNode<ItemList>("CanvasLayer/Selector/ItemList");
        _fileDialog = GetNode<FileDialog>("CanvasLayer/FileDialog");

        _itemList.AddItem("Default Model", defaultModelImage);

        LoadModelImages();
    }

/*     private async Task LoadModelImages()
    {
        DirAccess dirAccess = DirAccess.Open(ModelsFolderPath);

        if(dirAccess == null)
        {
            return;
        }

        string[] fileNames = dirAccess.GetFiles();
        GltfToImage gltfToImage = GltfToImage.GetGltfToImage(new Vector3(0, 1000, 0)); 
        AddChild(gltfToImage);       

        foreach(var fileName in fileNames)
        {
            //GD.Print("Cargar: ", fileName);
            string extension = fileName.GetExtension();
            if(!extension.Equals("gltf") && !extension.Equals("glb"))
            {
                continue;
            }

            string filePath =$"{ModelsFolderPath}{fileName}";

            await LoadModelImage(filePath, fileName, gltfToImage);
        }

        gltfToImage.QueueFree();
    } */


    /*     private async Task LoadModelImage(string path, string fileName, GltfToImage gltfToImage) //gltfToImage has to be a child
    {
        //GltfToImage gltfToImage = GltfToImage.GetGltfToImage(new Vector3(0, 1000, 0)); 
        //AddChild(gltfToImage); 
          
        Image image = await gltfToImage.ConvertGltfToImage(path);
        var texture2D = ImageTexture.CreateFromImage(image);

        int index = _itemList.AddItem(fileName, texture2D);
        _modelFileNames[index] = fileName;
    } */

    private void LoadModelImages()
    {
        DirAccess dirAccess = DirAccess.Open(ModelImagesFolderPath);

        if(dirAccess == null)
        {
            return;
        }

        string[] fileNames = dirAccess.GetFiles();
        
        foreach(var fileName in fileNames)
        {
            string filePath =$"{ModelImagesFolderPath}{fileName}";
            LoadModelImage(filePath);
        }
    }

    private void LoadModelImage(string imagePath)
    {
        var image = Image.LoadFromFile(imagePath);
        Texture2D texture2D = ImageTexture.CreateFromImage(image);     
        string fileName = imagePath.GetFile().Replace(".png", ""); 

        int index = _itemList.AddItem(fileName, texture2D);
        _modelFileNames[index] = $"{fileName}.glb";
        _itemList.Select(index);
        _selectedItem = index;
    }

    private void _on_file_dialog_file_selected(string path)
    {
        if(_supportedFiles.Contains(path))
        {
            OS.Alert("Tipo de archivo incorrecto, intenta con gltf o glb", "ERROR");
            return;
        }

        _gameManager.EmitSignal(GameManager.SignalName.ModelSelected, path, true);
    }

    private void _on_add_model_button_pressed()
    {
        _fileDialog.Visible = true;
    }

    private void _on_remove_model_button_pressed()
    {
        if(_selectedItem == null)
        {
            OS.Alert("No hay ningún modelo seleccionado", "Selecciona un modelo");
            return;
        }

        if(_selectedItem == 0)
        {
            OS.Alert("No se puede eliminar el modelo predeterminado", "Error");
            return;
        }

        string fileName =  _modelFileNames[_selectedItem.Value];

        DirAccess dir = DirAccess.Open(ModelsFolderPath);
        Error error = dir.Remove(fileName);


        if(error == Error.Ok)
        {
            _itemList.RemoveItem(_selectedItem.Value);

            for(int i = _selectedItem.Value; i < _itemList.ItemCount; i++) //recorrer los índices
            {
                _modelFileNames[i] = _modelFileNames[i+1];
            }

            _modelFileNames.Remove(_itemList.ItemCount);
            _selectedItem = null;
        }
        else
        {
            OS.Alert($"Error al intentar eliminar el modelo: {error}", "Alerta!");
        }

        dir = DirAccess.Open(ModelImagesFolderPath);
        error = dir.Remove(fileName.Replace(fileName.GetExtension(), "png"));

        if(error != Error.Ok)
        {
            OS.Alert($"Error al intentar eliminar la imagen del modelo: {error}", "Alerta!");
        }

    }


    private void _on_item_list_item_selected(int index)
    {
        _selectedItem = index;

        string path; 

        if(index == 0)
            path = DefaultModelPath;
        else
            path = $"{ModelsFolderPath}{_modelFileNames[index]}";

        _gameManager.EmitSignal(GameManager.SignalName.ModelSelected, path, false);
    }

    private void _on_close_button_pressed()
    {
        QueueFree();
    }

    public static ModelSelection GetModelSelection()
    {
        var scene = GD.Load<PackedScene>("res://Scenes/ModelSelection.tscn");
        return scene.Instantiate<ModelSelection>();
    }
}
