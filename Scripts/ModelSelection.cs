using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class ModelSelection : Control
{
    private ItemList _itemList;
    private FileDialog _fileDialog;
    public const string ModelsFolderName = "SavedModels";

    public const string ModelsFolderPath = $"user://{ModelsFolderName}/";
    public const string DefaultModelPath = "res://Assets/3DModels/House_001_GLB.glb";
    private Texture2D defaultModelImage = GD.Load<Texture2D>("res://Assets/Images/ModelImage.png");

    private Dictionary<int, string> _modelFileNames = new();

    private int? _selectedItem = null;

    private GameManager _gameManager;

	private string[] _supportedFiles = {"gltf", "glb"};

    public async override void _Ready()
    {
		_gameManager = GetNode<GameManager>("/root/GameManager");

        _itemList = GetNode<ItemList>("Selector/ItemList");
        _fileDialog = GetNode<FileDialog>("FileDialog");

        _itemList.AddItem("Default Model", defaultModelImage);

        await LoadModelImages();
    }

    private async Task LoadModelImages()
    {
        DirAccess dirAccess = DirAccess.Open(ModelsFolderPath);

        if(dirAccess == null)
        {
            return;
        }

        string[] fileNames = dirAccess.GetFiles();
        GltfToImage gltfToImage = GltfToImage.GetGltfToImage(); 
        AddChild(gltfToImage);       

        foreach(var fileName in fileNames)
        {
            string extension = fileName.GetExtension();
            if(!extension.Equals("gltf") && !extension.Equals("glb"))
            {
                continue;
            }

            string filePath =$"{ModelsFolderPath}{fileName}";

            Image image = await gltfToImage.ConvertGltfToImage(filePath);
            var texture2D = ImageTexture.CreateFromImage(image);

            int index = _itemList.AddItem(fileName, texture2D);
            _modelFileNames[index] = fileName;
        }

        gltfToImage.QueueFree();
    }

    private void _on_file_dialog_file_selected(string path)
    {
        if(_supportedFiles.Contains(path))
        {
            OS.Alert("Tipo de archivo incorrecto, intenta con gltf o glb", "ERROR");
            return;
        }

        _gameManager.EmitSignal(GameManager.SignalName.ModelSelected, path, true);
        GetParent().GetParent().QueueFree();        
        Input.MouseMode = Input.MouseModeEnum.Captured;
        GetTree().Paused = false;
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

        GD.Print("file to remove: ", fileName);
        string extension = fileName.GetExtension();

        Error error;

/*         if(extension.Equals("gltf"))
        {
            string fileNoExtension = fileName.Remove(fileName.Length - extension.Length);
            string dirPath = $"{ModelsFolderPath}{fileNoExtension}";

            dirPath = ProjectSettings.GlobalizePath(dirPath);
            GD.Print("DIr path: ", dirPath);
            
            error = OS.MoveToTrash(dirPath);
        <}
        else
        { */
            DirAccess dir = DirAccess.Open(ModelsFolderPath);
            error = dir.Remove(fileName);
        //}

        if(error == Error.Ok)
        {
            _itemList.RemoveItem(_selectedItem.Value);

            for(int i = _selectedItem.Value; i < _itemList.ItemCount; i++) //recorrer los índices
            {
                _modelFileNames[i] = _modelFileNames[i+1];
            }

            _modelFileNames.Remove(_itemList.ItemCount);
            _selectedItem = null;
            return;
        }
        
        OS.Alert($"Error al intentar eliminar el modelo: {error}", "Alerta!");

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
}
