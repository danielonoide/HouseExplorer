using Godot;
using System;
using System.Threading.Tasks;

public partial class GltfToImage : Node3D
{
    [Export]
    private bool Debug = false;
    private SubViewport _subViewport;

    private FileDialog _fileDialog;

    [Export]
    private Vector3 _axisPosition = Vector3.Zero;
    private Node3D _axis;

    public override void _Ready()
    {
        _subViewport = GetNode<SubViewport>("SubViewport");
        _fileDialog = GetNode<FileDialog>("FileDialog");

        _axis = GetNode<Node3D>("SubViewport/Axis"); 


        if(Debug)
        {
            _fileDialog.Visible = true;
        }

        if(_axisPosition != Vector3.Zero)
        {
            _axis.Position = _axisPosition;
        }
    }

    public async Task<Image> ConvertGltfToImage(string gltfPath)
    {
        GltfDocument gltfDocument = new();
		GltfState gltfState = new();
		var error = gltfDocument.AppendFromFile(gltfPath, gltfState);

        if(error != Error.Ok)
        {
            OS.Alert("Error al convertir el modelo a imagen", "ERROR");
            return null;
        }

        Node3D gltfScene = gltfDocument.GenerateScene(gltfState) as Node3D;
        _axis.AddChild(gltfScene);
/*         GetTree().CreateTimer(0.5f).Timeout +=  () => 
        {
            var image = _subViewport.GetViewport().GetTexture().GetImage();
            image.SavePng(savePath);
        }; */

        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        Image image = _subViewport.GetViewport().GetTexture().GetImage();
        gltfScene.QueueFree();
        return image;
    }

    private async void _on_file_dialog_file_selected(string path)
    {
        Image image = await ConvertGltfToImage(path);
        FileManager.SaveImage(image, "user://modelImage.png", "png");
    }

    public static GltfToImage GetGltfToImage()
    {
        var scene = GD.Load<PackedScene>("res://Scenes/GltfToImage.tscn");
        GltfToImage result = scene.Instantiate<GltfToImage>();

        return result;
    }

    public static GltfToImage GetGltfToImage(Vector3 position)
    {
        var scene = GD.Load<PackedScene>("res://Scenes/GltfToImage.tscn");
        GltfToImage result = scene.Instantiate<GltfToImage>();

        result._axisPosition = position;

        return result;
    }

}
