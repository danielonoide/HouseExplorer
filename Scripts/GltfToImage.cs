using Godot;
using System;
using System.Threading.Tasks;

public partial class GltfToImage : Node3D
{
    [Export]
    private bool Debug = false;
    private SubViewport _subViewport;

    private FileDialog _fileDialog;

    public override void _Ready()
    {
        _subViewport = GetNode<SubViewport>("SubViewport");
        _fileDialog = GetNode<FileDialog>("FileDialog");

        if(Debug)
        {
            _fileDialog.Visible = true;
        }
    }

    public async Task<Image> ConvertGltfToImage(string gltfPath)
    {
        GltfDocument gltfDocument = new();
		GltfState gltfState = new();
		var error = gltfDocument.AppendFromFile(gltfPath, gltfState);

        Node3D gltfScene = gltfDocument.GenerateScene(gltfState) as Node3D;
        AddChild(gltfScene);
/*         GetTree().CreateTimer(0.5f).Timeout +=  () => 
        {
            var image = _subViewport.GetViewport().GetTexture().GetImage();
            image.SavePng(savePath);
        }; */

        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

        return _subViewport.GetViewport().GetTexture().GetImage();
    }

    public static GltfToImage GetGltfToImage()
    {
        var scene = GD.Load<PackedScene>("res://Scenes/GltfToImage.tscn");
        GltfToImage result = scene.Instantiate<GltfToImage>();

        return result;
    }

    private async void _on_file_dialog_file_selected(string path)
    {
        Image image = await ConvertGltfToImage(path);
        FileManager.SaveImage(image, "user://modelImage.png", "png");
    }

}
