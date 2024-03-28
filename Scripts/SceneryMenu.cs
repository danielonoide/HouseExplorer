using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

public partial class SceneryMenu : Control
{
    private const string FloorTexturesFolderName = "FloorTextures";
    private const string TexturesPath = $"user://{FloorTexturesFolderName}/";

    private enum Tab
    {
        Land,
        Background,
        Lighting
    }

    private Godot.Collections.Dictionary<Tab, Control> _tabMenus = new();
    private Tab _currentTab = Tab.Land;

    private LineEdit _widthEdit, _heightEdit;

    private CheckButton _triplanarButton;


    private Vector2 NewFloorSize => new((float)Convert.ToDouble(_widthEdit.Text), (float)Convert.ToDouble(_heightEdit.Text));

    private ItemList _landItemList;

    private int? _selectedItem = null;

    private FileDialog _fileDialog;

    private string[] _supportedImageFormats = {"png", "jpg", "webp"};  

    private System.Collections.Generic.Dictionary<int, string> _textureFileNames = new();

    public override void _Ready()
    {
        _tabMenus[Tab.Land] = GetNode<Control>("LandMenu");
        _tabMenus[Tab.Background] = GetNode<Control>("BgMenu");
        _tabMenus[Tab.Lighting] = GetNode<Control>("LightingMenu");

        _widthEdit = GetNode<LineEdit>("%WidthEdit");
        _heightEdit = GetNode<LineEdit>("%HeightEdit");

        _widthEdit.Text = World.FloorSize.X.ToString();
        _heightEdit.Text = World.FloorSize.Y.ToString();

        _triplanarButton = GetNode<CheckButton>("%TriplanarButton");
        _triplanarButton.ButtonPressed = World.TriplanarFloorMaterial;


        _landItemList = GetNode<ItemList>("%LandItemList");

        Texture2D _landTexture = GD.Load<Texture2D>("res://Assets/Images/floor_texture.png");
        _landItemList.AddIconItem(_landTexture);

        _fileDialog = GetNode<FileDialog>("FileDialog");

        LoadTextures();

        ChangeTab(_currentTab);
    }

    private void CreateFolder(string path, string folderName)
    {
        DirAccess dirAccess = DirAccess.Open(path);

        if(dirAccess.DirExists(folderName))
        {
            return;
        }

        dirAccess.MakeDir(folderName);
    }

    private void LoadTextures()
    {
        DirAccess dirAccess = DirAccess.Open(TexturesPath);

        if(dirAccess == null)
        {
            return;
        }

        string[] fileNames = dirAccess.GetFiles();

        foreach(var fileName in fileNames)
        {
            string filePath = $"{TexturesPath}{fileName}";
            //filePath = ProjectSettings.GlobalizePath(filePath);
            
            var image = Image.LoadFromFile(filePath);
            Texture2D texture2D = ImageTexture.CreateFromImage(image);
            int index = _landItemList.AddIconItem(texture2D);

            _textureFileNames[index] = fileName;
        }
    }

    private void ChangeTab(Tab tab)
    {
        foreach (var menu in _tabMenus.Values)
        {
            menu.Visible = false;
        }

        _tabMenus[tab].Visible = true;
        _currentTab = tab;
    }

    private void _on_tab_bar_tab_changed(int tab)
    {
        ChangeTab((Tab)tab);
    }

    private void _on_width_edit_text_changed(string newText)
    {
        World.FloorSize = NewFloorSize;
    }

    private void _on_height_edit_text_changed(string newText)
    {
        World.FloorSize = NewFloorSize;
    }

    private void _on_triplanar_button_toggled(bool toggled_on)
    {
        World.TriplanarFloorMaterial = toggled_on;
    }

    private void _on_land_item_list_item_selected(int index)
    {
        _selectedItem = index;

        Texture2D texture2D = _landItemList.GetItemIcon(index);   
        World.FloorTexture = texture2D;
    }

    private void PrintDictionary()
    {
        GD.Print("Diccionario: ");
        foreach(var KeyValuePair in _textureFileNames)
        {
            GD.Print("LLave: ", KeyValuePair.Key, " Valor: ", KeyValuePair.Value);
        }
    }

    private void _on_add_texture_button_pressed()
    {
        CreateFolder("user://", FloorTexturesFolderName);
        _fileDialog.Visible = true;

    }

    private void _on_remove_texture_button_pressed()
    {
        if(_selectedItem == null)
        {
            OS.Alert("No hay ninguna textura seleccionada", "Seleccionad una textura");
            return;
        }

        if(_selectedItem == 0)
        {
            OS.Alert("No se puede eliminar la textura predeterminada", "Error");
            return;
        }
        
        DirAccess dir = DirAccess.Open(TexturesPath);
        Error error = dir.Remove(_textureFileNames[_selectedItem.Value]);

        if(error == Error.Ok)
        {
            _landItemList.RemoveItem(_selectedItem.Value);

            for(int i = _selectedItem.Value; i < _landItemList.ItemCount; i++) //recorrer los índices
            {
s                _textureFileNames[i] = _textureFileNames[i+1];
            }

            _textureFileNames.Remove(_landItemList.ItemCount);
            _selectedItem = null;
            return;
        }
        
        OS.Alert($"Error al intentar eliminar la textura: {error}", "Alerta!");
    }

    private void _on_file_dialog_file_selected(string path)
    {
		string extension = path.GetExtension().ToLower();
        
		if (!_supportedImageFormats.Contains(extension))
		{
            string formats = _supportedImageFormats.Join(", ");
			OS.Alert($"Archivo no soportado: {extension}\nIntenta con los siguientes formatos: {formats}");
			return;
		}

        var image = Image.LoadFromFile(path);
        var imageTexture = ImageTexture.CreateFromImage(image);

        int index = _landItemList.AddIconItem(imageTexture);
        _landItemList.Select(index);
        _selectedItem = index;
        World.FloorTexture = imageTexture;
        
        string fileName = $"textura{_landItemList.ItemCount}.{extension}";
        SaveImage(image, extension, index, fileName);

    }

    private void SaveImage(Image image, string extension, int index, string fileName)
    {
        //if(_textureFileNames.Contains(new KeyValuePair<int, string>(index, fileName))) //si ya existe el nombre de archivo
        if(_textureFileNames.ContainsValue(fileName)) //si ya existe el nombre de archivo
        {
            string pattern = @"\((\d+)\)";
            Match match = Regex.Match(fileName, pattern);

            if(match.Success) //si ya hay un número dentro de paréntesis (1)
            {
                int num = match.Value[1] - '0';
                num++;
                string newString = $"({num})";
                fileName = fileName.Replace(match.Value, newString);
            }
            else
            {
                StringBuilder sb = new();
                sb.Append(fileName.Remove(fileName.Length-extension.Length-1));
                sb.Append("(1)");
                sb.Append('.');
                sb.Append(extension);

                fileName = sb.ToString();
            }

            SaveImage(image, extension, index, fileName);
            return;
        }

        _textureFileNames[index] = fileName;
        string filePath = $"{TexturesPath}{fileName}";


/*         extension = extension.Capitalize();
        image.Call($"Save{extension}", filePath); */

        switch(extension)
        {
            case "png":
                image.SavePng(filePath);
                break;
            
            case "jpg":
                image.SaveJpg(filePath);
                break;

            case "webp":
                image.SaveWebp(filePath);
                break;
        }
    }
}
