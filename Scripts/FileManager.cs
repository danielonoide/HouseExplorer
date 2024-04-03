using Godot;
using System;

public partial class FileManager : Node
{
/*     public static void SaveImage(Image image, string extension, int index, string fileName)
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
    } */
}
