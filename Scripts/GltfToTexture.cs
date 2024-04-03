using Godot;
using System;

public partial class GltfToTexture : Node3D
{

    private SubViewport _subViewport;
    private string _filePath;

    public override void _Ready()
    {
        _subViewport = GetNode<SubViewport>("SubViewport");
    }
    private async void _on_file_dialog_file_selected(string path)
    {
        GltfDocument gltfDocument = new();
		GltfState gltfState = new();
		var error = gltfDocument.AppendFromFile(path, gltfState);

        Node3D gltfScene = gltfDocument.GenerateScene(gltfState) as Node3D;
        AddChild(gltfScene);
        GetTree().CreateTimer(0.5f).Timeout +=  () => 
        {
            var image = _subViewport.GetViewport().GetTexture().GetImage();
            image.SavePng("user://image.png");
        };

        
        //ConvertGLTFToImage(gltfScene, "user://image.png", 1080, 720);
    }





}
