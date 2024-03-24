using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ObjParse : Node
{
    private const bool Debug = false;

    // ... (Otras funciones y constantes)

    // Devuelve un diccionario de materiales desde un archivo MTL
    // public

    // Crea una malla a partir de las rutas de obj y mtl
    public static Mesh LoadObj(string objPath, string mtlPath = "")
    {
        if (mtlPath == "")
        {
            mtlPath = SearchMtlPath(objPath);
        }

        var obj = GetData(objPath);
        var mats = new Dictionary<string, StandardMaterial3D>();

        if (mtlPath != "")
        {
            mats = CreateMtl(GetData(mtlPath), GetMtlTex(mtlPath));
        }

        return CreateObj(obj, mats);
    }

    // Crea una malla a partir de obj y materiales. Los materiales deben ser {"matname": data}
    public static Mesh LoadObjFromBuffer(string objData, Dictionary<string, StandardMaterial3D> materials)
    {
        return CreateObj(objData, materials);
    }

    // Crea materiales
    public static Dictionary<string, StandardMaterial3D> LoadMtlFromBuffer(string mtlData, Dictionary<string, byte[]> textures)
    {
        return CreateMtl(mtlData, textures);
    }

    // Obtiene datos desde la ruta del archivo
    public static string GetData(string path)
    {
        if (path != "")
        {
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

            if (file.IsOpen())
            {
                var res = file.GetAsText();
                file.Close();
                return res;
            }
        }

        return "";
    }

    // Obtiene texturas desde la ruta del archivo MTL (devuelve {"tex_path": data})
    public static Dictionary<string, byte[]> GetMtlTex(string mtlPath)
    {
        var filePaths = GetMtlTexPaths(mtlPath);
        var textures = new Dictionary<string, byte[]>();

        foreach (var k in filePaths)
        {
            textures[k] = GetImage(mtlPath, k).SavePngToBuffer();
        }

        return textures;
    }

    // Obtiene rutas de texturas desde la ruta del archivo MTL
    public static List<string> GetMtlTexPaths(string mtlPath)
    {
        var file = FileAccess.Open(mtlPath, FileAccess.ModeFlags.Read);
        var paths = new List<string>();

        if (file.IsOpen())
        {
            var lines = file.GetAsText().Split("\n");
            file.Close();

            foreach (var line in lines)
            {
                var parts = line.Split(" ");
                if (parts[0] == "map_Kd" || parts[0] == "map_Ks" || parts[0] == "map_Ka")
                {
                    var path = line.Split(" ")[1];
                    if (!paths.Contains(path))
                    {
                        paths.Add(path);
                    }
                }
            }
        }

        return paths;
    }

    // Intenta encontrar la ruta del archivo MTL desde la ruta del archivo OBJ
    public static string SearchMtlPath(string objPath)
    {
/*         var mtlPath = objPath.GetBaseDir().PlusFile(objPath.GetFile().RSplit(".", false, 1)[0] + ".mtl");

        if (!FileAccess.FileExists(mtlPath))
        {
            mtlPath = objPath.GetBaseDir().PlusFile(objPath.GetFile() + ".mtl");
        }

        if (!FileAccess.FileExists(mtlPath))
        {
            return "";
        }

        return mtlPath; */

/*         var lines: PackedStringArray = obj.split("\n")
        for line in lines:
            var split: PackedStringArray = line.split(" ", false)
            if (split.size() < 2): continue
            if (split[0] != "mtllib"): continue
            return split[1].strip_edges()
        return "" */

        var lines = objPath.Split("\n");

        foreach(var line in lines)
        {
            var split = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if(split.Length<2) continue;
            if(!split[0].Equals("mtlib")) continue;
            return split[1].StripEdges();
        }

        return string.Empty;
    }

    // Funciones privadas (start with underscore _)

    private static Dictionary<string, StandardMaterial3D> CreateMtl(string obj, Dictionary<string, byte[]> textures)
    {
        var mats = new Dictionary<string, StandardMaterial3D>();
        StandardMaterial3D currentMat = null;

        var lines = obj.Split("\n");

        foreach (var line in lines)
        {
            var parts = line.Split(" ");
            switch (parts[0])
            {
                case "#":
                    // Comentario
                    break;

                case "newmtl":
                    // Crear un nuevo material
                    if (Debug)
                    {
                        GD.Print("Adding new material " + parts[1]);
                    }

                    currentMat = new StandardMaterial3D();
                    mats[parts[1]] = currentMat;
                    break;

                case "Ka":
                    // Color ambiente
                    break;

                case "Kd":
                    // Color difuso
                    currentMat.AlbedoColor = new Color(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                    if (Debug)
                    {
                        GD.Print("Setting material color " + currentMat.AlbedoColor);
                    }
                    break;

                default:
                    if (parts[0] == "map_Kd" || parts[0] == "map_Ks" || parts[0] == "map_Ka")
                    {
                        var path = line.Split(" ")[1];
                        if (textures.ContainsKey(path))
                        {
                            currentMat.AlbedoTexture = CreateTexture(textures[path]);
                        }
                    }
                    break;
            }
        }

        return mats;
    }

    private static Image GetImage(string mtlFilePath, string texFilename)
    {
        if (Debug)
        {
            GD.Print("    Debug: Mapping texture file " + texFilename);
        }

        var texFilePath = texFilename;

        if (texFilename.IsRelativePath())
        {
            texFilePath = $"{mtlFilePath.GetBaseDir()}{texFilename}";
        }
        

        var img = new Image();
        img.Load(texFilePath);
        return img;
    }

    private static ImageTexture CreateTexture(byte[] data)
    {
        var img = new Image();
        var tex = new ImageTexture();
        img.LoadPngFromBuffer(data);
        ImageTexture.CreateFromImage(img);
        return tex;
    }

    private static Mesh CreateObj(string obj, Dictionary<string, StandardMaterial3D> mats)
    {
        var mesh = new ArrayMesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var faces = new Dictionary<string, List<Dictionary<string, List<int>>>>();
        var fans = new List<List<Dictionary<string, List<int>>>>();
        var firstSurface = true;
        var matName = "default";
        var countMtl = 0;

        var lines = obj.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0])
            {
                case "#":
                    break;

                case "v":
                    // Vértice
                    //GD.Print("float parse 1: ", parts[1], " parts 2: ", parts[2], " parts 3: ", parts[3]);
                    var nV = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                    vertices.Add(nV);
                    break;

                case "vn":
                    // Normal
                    var nVn = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                    normals.Add(nVn);
                    break;

                case "vt":
                    // UV
                    var nUv = new Vector2(float.Parse(parts[1]), 1 - float.Parse(parts[2]));
                    uvs.Add(nUv);
                    break;

                case "usemtl":
                    // Grupo de material
                    countMtl++;
                    matName = parts[1].StripEdges();
                    if (!faces.ContainsKey(matName))
                    {
                        var matsKeys = mats.Keys.ToArray();

                        if (!mats.ContainsKey(matName))
                        {
                            if (matsKeys.Length > countMtl)
                            {
                                matName = matsKeys[countMtl];
                            }
                        }

                        faces[matName] = new List<Dictionary<string, List<int>>>();
                    }
                    break;

                case "f":
                    if (!faces.ContainsKey(matName))
                    {
                        var matsKeys = mats.Keys.ToArray();

                        if (matsKeys.Length > countMtl)
                        {
                            matName = matsKeys[countMtl];
                        }

                        faces[matName] = new List<Dictionary<string, List<int>>>();
                    }

                    // Cara
                    if (parts.Length == 4)
                    {
                        // Triángulo
                        var face = new Dictionary<string, List<int>>
                        {
                            { "v", new List<int>() },
                            { "vt", new List<int>() },
                            { "vn", new List<int>() }
                        };

                        for (var i = 0; i < parts.Length; i++)
                        {
                            var verticesIndex = parts[i].Split("/");
                            if (verticesIndex[0] != "f")
                            {
                                face["v"].Add(int.Parse(verticesIndex[0]) - 1);
                                face["vt"].Add(int.Parse(verticesIndex[1]) - 1);
                                if (verticesIndex.Length > 2)
                                {
                                    face["vn"].Add(int.Parse(verticesIndex[2]) - 1);
                                }
                            }
                        }

                        if (faces.ContainsKey(matName))
                        {
                            faces[matName].Add(face);
                        }
                    }
                    else if (parts.Length > 4)
                    {
                        // Triangularizar
                        var points = new List<List<int>>();

                        for (var i = 0; i < parts.Length; i++)
                        {
                            var verticesIndex = parts[i].Split("/");

                            if (verticesIndex[0] != "f" && verticesIndex.Length>2)
                            {
                                //GD.Print("vertices index 1: ", verticesIndex[0], " vertices 2: ", verticesIndex[1], " vertices 3: ", verticesIndex[2]);

                                var point = new List<int>
                                {
                                    int.Parse(verticesIndex[0]) - 1,
                                    int.Parse(verticesIndex[1]) - 1
                                };

                                if (verticesIndex.Length > 2)
                                {
                                    point.Add(int.Parse(verticesIndex[2]) - 1);
                                }

                                points.Add(point);
                            }
                        }

                        for (var i = 0; i < points.Count; i++)
                        {
                            if (i != 0)
                            {
                                var face = new Dictionary<string, List<int>>
                                {
                                    { "v", new List<int>() },
                                    { "vt", new List<int>() },
                                    { "vn", new List<int>() }
                                };

                                var point0 = points[0];
                                var point1 = points[i];
                                var point2 = points[i - 1];

                                face["v"].Add(point0[0]);
                                face["v"].Add(point2[0]);
                                face["v"].Add(point1[0]);

                                face["vt"].Add(point0[1]);
                                face["vt"].Add(point2[1]);
                                face["vt"].Add(point1[1]);

                                if (point0.Count > 2)
                                {
                                    face["vn"].Add(point0[2]);
                                }

                                if (point2.Count > 2)
                                {
                                    face["vn"].Add(point2[2]);
                                }

                                if (point1.Count > 2)
                                {
                                    face["vn"].Add(point1[2]);
                                }

                                if (faces.ContainsKey(matName))
                                {
                                    faces[matName].Add(face);
                                }
                            }
                        }
                    }

                    break;
            }
        }

        // Crear triángulos
        foreach (var matGroup in faces.Keys)
        {
            if (Debug)
            {
                GD.Print("Creating surface for matgroup " + matGroup + " with " + faces[matGroup].Count + " faces");
            }

            // Ensamblador de mallas
            var st = new SurfaceTool();
            st.Begin(Mesh.PrimitiveType.Triangles);

            if (!mats.ContainsKey(matGroup))
            {
                mats[matGroup] = new StandardMaterial3D();
            }

            st.SetMaterial(mats[matGroup]);

            foreach (var face in faces[matGroup])
            {
                if (face["v"].Count == 3)
                {
                    // Vértices
                    var fanV = new List<Vector3>();
                    fanV.Add(vertices[face["v"][0]]);
                    fanV.Add(vertices[face["v"][2]]);
                    fanV.Add(vertices[face["v"][1]]);

                    // Normales
                    var fanVn = new List<Vector3>();
                    if (face["vn"].Count > 0)
                    {
                        fanVn.Add(normals[face["vn"][0]]);
                        fanVn.Add(normals[face["vn"][2]]);
                        fanVn.Add(normals[face["vn"][1]]);
                    }

                    // Texturas
                    var fanVt = new List<Vector2>();
                    if (face["vt"].Count > 0)
                    {
                        for (var k = 0; k < 3; k++)
                        {
                            var f = face["vt"][k];
                            if (f > -1)
                            {
                                var uv = uvs[f];
                                fanVt.Add(uv);
                            }
                        }
                    }

                    st.AddTriangleFan(fanV.ToArray(), fanVt.ToArray(), Array.Empty<Color>(), Array.Empty<Vector2>(), fanVn.ToArray(), new Godot.Collections.Array());
                }
            }

            mesh = st.Commit(mesh);
        }

        for (var k = 0; k < mesh.GetSurfaceCount(); k++)
        {
            var mat = mesh.SurfaceGetMaterial(k);
            matName = "";

            foreach (var m in mats)
            {
                if (m.Value == mat)
                {
                    matName = m.Key;
                }
            }

            mesh.SurfaceSetName(k, matName);
        }

        return mesh;
    }
}
