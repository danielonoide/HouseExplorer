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

    private static int? _selectedItem = null;
    private GameManager _gameManager;

	private string[] _supportedFiles = {"gltf", "glb"};

    private Button _loadModelButton;

    public override void _Ready()
    {
		_gameManager = GetNode<GameManager>("/root/GameManager");
        _gameManager.GltfImageSaved += OnGltfImageSaved;

        _itemList = GetNode<ItemList>("CanvasLayer/Selector/ModelItemList");
        _fileDialog = GetNode<FileDialog>("CanvasLayer/FileDialog");
        _loadModelButton = GetNode<Button>("CanvasLayer/Selector/HBoxContainer/LoadModelButton");

        _itemList.AddItem("Default Model", defaultModelImage);
        LoadModelImages();

        if(_selectedItem != null)
        {
            _itemList.Select(_selectedItem.Value);
            _loadModelButton.Visible = true;
        }
    }
    
    private void LoadModelImages()
    {
        DirAccess dirAccess = DirAccess.Open(ModelImagesFolderPath);

        if(dirAccess == null)
        {
            OS.Alert("Error al cargar las imagenes", "La carpeta no existe");
            return;
        }

        string[] fileNames = dirAccess.GetFiles();

        foreach(var fileName in fileNames)
        {
            //string filePath = $"{ModelImagesFolderPath}{fileName}";
            string filePath = Path.Combine(ModelImagesFolderPath, fileName);
            LoadModelImage(filePath);
        }
    }

    private int LoadModelImage(string imagePath)
    {
        var image = Image.LoadFromFile(imagePath);
        Texture2D texture2D = ImageTexture.CreateFromImage(image);     
        string fileName = imagePath.GetFile().Replace(".png", ""); 

        if(!IsInstanceValid(_itemList))
        {
            GD.Print("Item list eliminao");
            _itemList = GetNode<ItemList>("CanvasLayer/Selector/ModelItemList");
        }

        int index = _itemList.AddItem(fileName, texture2D);
        _modelFileNames[index] = $"{fileName}.glb";

        return index;
    }

    private void OnGltfImageSaved(string imagePath)
    {
        int index = LoadModelImage(imagePath);
        _itemList.Select(index);
        _selectedItem = index;
    }

    private void DeselectItem()
    {
        _selectedItem = null;
        _loadModelButton.Visible = false;
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
            DeselectItem();
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

    private void _on_model_item_list_item_selected(int index)
    {
        _selectedItem = index;
        _loadModelButton.Visible = true;
    }

    private void _on_load_model_button_pressed()
    {
        string path = _selectedItem.Value == 0 ? DefaultModelPath : $"{ModelsFolderPath}{_modelFileNames[_selectedItem.Value]}"; 
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
