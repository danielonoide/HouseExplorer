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

    private CheckButton _imageBgButton;

    //Lighting
    private HSlider _lightEnergySlider, _timeSlider;
    private Label _lightEnergyLabel, _timeLabel;
    private CheckButton _shadowButton;
    private ColorPicker _colorPicker;

    private const float AngleOffset = 270;


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

        _imageBgButton = GetNode<CheckButton>("%ImageBgButton");
        _imageBgButton.ButtonPressed = World.ImageBgEnabled;
        
        //Lighting
        _lightEnergySlider = GetNode<HSlider>("%LightEnergySlider");
        _lightEnergyLabel = _lightEnergySlider.GetChild<Label>(0);

        _lightEnergySlider.Value = World.LightEnergy;

        _shadowButton = GetNode<CheckButton>("%ShadowButton");
        _shadowButton.ButtonPressed = World.LightShadow;

        _colorPicker = GetNode<ColorPicker>("%ColorPicker");
        _colorPicker.Color = World.LightColor;

        _timeSlider = GetNode<HSlider>("%TimeSlider");
        _timeLabel = _timeSlider.GetChild<Label>(0);


        float timeDegrees = World.LightRotationX;
        timeDegrees += AngleOffset;

        float currTime = timeDegrees/360 *24;
        _timeSlider.Value = currTime;

       
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
        _textureSelector.Visible = false;

        if(tab == Tab.Land)
        {
            _itemList.AddIconItem(_landTexture);
            LoadTextures(FloorTexturesPath);
            _textureSelector.Visible = true;
        }
        else if(tab == Tab.Background)
        {
            _itemList.AddIconItem(_backgroundTexture);
            LoadTextures(BackgroundTexturesPath);
            _textureSelector.Visible = _imageBgButton.ButtonPressed;
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

    private void _on_item_list_item_selected(int index)
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

    private void _on_add_texture_button_pressed()
    {
        string folderName = _currentTab == Tab.Land ? FloorTexturesFolderName : BackgroundTexturesFolderName;
        FileManager.CreateFolder("user://", folderName);
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

        bool landTab = _currentTab == Tab.Land;

        if(landTab)
        {       
            World.FloorTexture = imageTexture;
        }
        else
        {
            World.BackgroundTexture = imageTexture;
        }
        
        StringBuilder sb = new();
        sb.Append(landTab ? "Floor" : "Background");
        sb.Append($"Texture{_itemList.ItemCount}.{extension}");
        string fileName = sb.ToString();
        
        fileName = FileManager.GetUniqueFileName(new HashSet<string>(_textureFileNames.Values), fileName, extension);
        string filePath = landTab ? $"{FloorTexturesPath}{fileName}" : $"{BackgroundTexturesPath}{fileName}";

        _textureFileNames[index] = fileName;
        FileManager.SaveImage(image, filePath, extension);

    }

    //background
    private void _on_bg_bright_slider_value_changed(float value)
    {
        World.EnvironmentEnergy = value;
        _bgBrightLabel.Text = value.ToString();
    }

    private void _on_bg_brightness_restart_button_pressed()
    {
        _bgBrightSlider.Value = 1;
    }

    private void _on_image_bg_button_toggled(bool toggled_on)
    {
        _textureSelector.Visible = toggled_on;
        World.ImageBgEnabled = toggled_on;
    }

    //lighting
    private void _on_light_energy_slider_value_changed(float value)
    {
        World.LightEnergy = value;
        _lightEnergyLabel.Text = value.ToString();
    }

    private void _on_light_energy_reset_button_pressed()
    {
        _lightEnergySlider.Value = 1;
    }

    private void _on_color_picker_color_changed(Color color)
    {
        World.LightColor = color;
    }

    private void _on_restart_time_button_pressed()
    {
        _timeSlider.Value = 12;
    }

    private void _on_time_slider_value_changed(float militaryTime)
    {
        float time = militaryTime > 12 ? militaryTime % 12 : militaryTime;

        string abbrev = militaryTime >= 12 ? "pm" : "am";

        if(time == 0)
        {
            time = 12;
        }

        _timeLabel.Text = $"{time} {abbrev}";  

        float degrees = (militaryTime/24) * 360;
        degrees -= AngleOffset;
        World.LightRotationX = degrees;
    }

    private void _on_shadow_button_toggled(bool toggled_on)
    {
        World.LightShadow = toggled_on;
    }
    
}
