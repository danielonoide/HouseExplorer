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

    private const float Delay = 0.5f;

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

    public async Task SaveGltfImage(string gltfPath, string savePath)
    {
        Image image = await ConvertGltfToImage(gltfPath);
        image.SavePng(savePath);
    }

    public async Task<Image> ConvertGltfToImage(string gltfPath)
    {
        Node3D gltfScene = FileManager.GenGltfScene(gltfPath) as Node3D;
        _axis.AddChild(gltfScene);

        /*         GetTree().CreateTimer(0.5f).Timeout +=  () => 
        {
            var image = _subViewport.GetViewport().GetTexture().GetImage();
            image.SavePng(savePath);
        }; */

        await WaitForViewportUpdate();
        Image image = GetViewportImage();
        gltfScene.QueueFree();
        return image;
    }

    public async Task SaveGltfImage(Node3D gltfScene, string savePath)
    {
        Image image = await ConvertGltfToImage(gltfScene);
        image.SavePng(savePath);
    }

    public async Task<Image> ConvertGltfToImage(Node3D gltfScene)
    {
        _axis.AddChild(gltfScene);
        await WaitForViewportUpdate();
        Image image = GetViewportImage();
        //gltfScene.QueueFree();
        return image;
    }


    public async Task WaitForViewportUpdate()
    {
        await ToSignal(GetTree().CreateTimer(Delay), "timeout");
    }

    public Image GetViewportImage()
    {
        return _subViewport.GetViewport().GetTexture().GetImage();
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
