using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public partial class FileManager : Node
{

    public static HashSet<string> GetDirectoryFiles(string directoryPath)
    {
        return DirAccess.Open(directoryPath)?.GetFiles()?.ToHashSet();
    }

    public static Node GenGltfScene(string path)
    {
        GltfDocument gltfDocument = new();
        GltfState gltfState = new();
        var error = gltfDocument.AppendFromFile(path, gltfState);
        
        if (error == Error.Ok)
        {
            return gltfDocument.GenerateScene(gltfState);
        }
        else
        {
            OS.Alert("No se puedo procesar el archivo GLTF", "ERROR");
            return null;
        }
    }

    public static void CreateFolder(string path, string folderName)
    {
        DirAccess dirAccess = DirAccess.Open(path);

        if(dirAccess.DirExists(folderName))
        {
            return;
        }

        dirAccess.MakeDir(folderName);
    }

    public static string GetUniqueFileName(HashSet<string> files, string fileName, string extension)  //fileName must include the extension
    {
        if(files.Contains(fileName)) //si ya existe el nombre de archivo
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

            return GetUniqueFileName(files, fileName, extension);
        }

        return fileName;
    }

    public static void SaveImage(Image image, string filePath, string extension)
    {
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
