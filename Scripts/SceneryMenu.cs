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
    private const string BackgroundTexturesFolderName = "BackgroundTextures";

    private const string FloorTexturesPath = $"user://{FloorTexturesFolderName}/";
    private const string BackgroundTexturesPath = $"user://{BackgroundTexturesFolderName}/";


    private Control _textureSelector;

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

    private Texture2D _landTexture = GD.Load<Texture2D>("res://Assets/Images/floor_texture.png");
    private Texture2D _backgroundTexture = GD.Load<Texture2D>("res://Assets/Images/panorama_image.png");

    private ItemList _itemList;

    private int? _selectedItem = null;

    private FileDialog _fileDialog;

    private string[] _supportedImageFormats = {"png", "jpg", "webp"};  

    private System.Collections.Generic.Dictionary<int, string> _textureFileNames = new();

    //Background
    private HSlider _bgBrightSlider;
    private Label _bgBrightLabel;

    //Lighting
    private HSlider _lightEnergySlider;
    private Label _lightEnergyLabel;

    private ColorPicker _colorPicker;

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


        _itemList = GetNode<ItemList>("%ItemList");

        _textureSelector = GetNode<Control>("TextureSelector");
        _fileDialog = GetNode<FileDialog>("FileDialog");

        //LoadTextures(FloorTexturesPath);

        ChangeTab(_currentTab);

        //Background
        _bgBrightSlider = GetNode<HSlider>("%BgBrightSlider");
        _bgBrightLabel = _bgBrightSlider.GetChild<Label>(0);

        _bgBrightSlider.Value = World.EnvironmentEnergy;
        
        //Lighting
        _lightEnergySlider = GetNode<HSlider>("%LightEnergySlider");
        _lightEnergyLabel = _lightEnergySlider.GetChild<Label>(0);

        _lightEnergySlider.Value = World.LightEnergy;

        _colorPicker = GetNode<ColorPicker>("%ColorPicker");
        _colorPicker.Color = World.LightColor;
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

    private void LoadTextures(string path)
    {
        DirAccess dirAccess = DirAccess.Open(path);

        if(dirAccess == null)
        {
            return;
        }

        string[] fileNames = dirAccess.GetFiles();

        foreach(var fileName in fileNames)
        {
            string filePath = _currentTab == Tab.Land ? $"{FloorTexturesPath}{fileName}" : $"{BackgroundTexturesPath}{fileName}" ;
            //GD.Print("Current Tab: ", _currentTab, " file Path: ", filePath);
            //filePath = ProjectSettings.GlobalizePath(filePath);
            
            var image = Image.LoadFromFile(filePath);
            Texture2D texture2D = ImageTexture.CreateFromImage(image);
            int index = _itemList.AddIconItem(texture2D);

            _textureFileNames[index] = fileName;
        }
    }

    private void ChangeTab(Tab tab)
    {
        _currentTab = tab;
        _itemList.Clear();

        if(tab == Tab.Land)
        {
            _itemList.AddIconItem(_landTexture);
            LoadTextures(FloorTexturesPath);
        }
        else if(tab == Tab.Background)
        {
            _itemList.AddIconItem(_backgroundTexture);
            LoadTextures(BackgroundTexturesPath);
        }

        foreach (var menu in _tabMenus.Values)
        {
            menu.Visible = false;
        }

        _tabMenus[tab].Visible = true;
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

        Texture2D texture2D = _itemList.GetItemIcon(index); 

        if(_currentTab == Tab.Land)
        {
            World.FloorTexture = texture2D;
        }  
        else
        {
            World.BackgroundTexture = texture2D;
        }
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
        string folderName = _currentTab == Tab.Land ? FloorTexturesFolderName : BackgroundTexturesFolderName;
        CreateFolder("user://", folderName);
        _fileDialog.Visible = true;
    }

    private void _on_remove_texture_button_pressed()
    {
        if(_selectedItem == null)
        {
            OS.Alert("No hay ninguna textura seleccionada", "Selecciona una textura");
            return;
        }

        if(_selectedItem == 0)
        {
            OS.Alert("No se puede eliminar la textura predeterminada", "Error");
            return;
        }

        string path = _currentTab == Tab.Land ? FloorTexturesPath : BackgroundTexturesPath;

        DirAccess dir = DirAccess.Open(path);
        Error error = dir.Remove(_textureFileNames[_selectedItem.Value]);

        if(error == Error.Ok)
        {
            _itemList.RemoveItem(_selectedItem.Value);

            for(int i = _selectedItem.Value; i < _itemList.ItemCount; i++) //recorrer los índices
            {
                _textureFileNames[i] = _textureFileNames[i+1];
            }

            _textureFileNames.Remove(_itemList.ItemCount);
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

        int index = _itemList.AddIconItem(imageTexture);
        _itemList.Select(index);
        _selectedItem = index;

        if(_currentTab == Tab.Land)
        {       
            World.FloorTexture = imageTexture;
        }
        else
        {
            World.BackgroundTexture = imageTexture;
        }
        
        string fileName = $"textura{_itemList.ItemCount}.{extension}";
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
        string filePath = _currentTab == Tab.Land ? $"{FloorTexturesPath}{fileName}" : $"{BackgroundTexturesPath}{fileName}";

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

    //background
    private void _on_bg_bright_slider_value_changed(float value)
    {
        World.EnvironmentEnergy = value;
        _bgBrightLabel.Text = value.ToString();
    }



    //lighting
    private void _on_light_energy_slider_value_changed(float value)
    {
        World.LightEnergy = value;
        _lightEnergyLabel.Text = value.ToString();
    }

    private void _on_color_picker_color_changed(Color color)
    {
        World.LightColor = color;
    }
    
}
