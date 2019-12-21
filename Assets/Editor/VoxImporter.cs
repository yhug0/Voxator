using System.IO;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using Voxel.VoxFile;
using Voxel;
using UnityEngine.Rendering;
using Voxel.Tools;
using System;
using static Voxel.MeshGenerator;

[ScriptedImporter(1, "vox")]

public class VoxImporter : ScriptedImporter
{
    public GameObject gameObject;
    public string fileName;

    public string path;
    public override void OnImportAsset(AssetImportContext ctx)
    {
        VoxFileReader file = new VoxFileReader(ctx.assetPath);

        fileName = getName(ctx);
        path = ctx.assetPath;
        gameObject = new GameObject(fileName + "-prefab");

        gameObject.AddComponent<MeshRenderer>();
        var filter = gameObject.AddComponent<MeshFilter>();
        ctx.AddObjectToAsset(fileName + "VoxelPrefab", gameObject);
        var model = VoxelModelInit(file);
        ctx.AddObjectToAsset(fileName + "-VoxelModel", model);
        ctx.AddObjectToAsset(fileName + "texture", file.texturePalette, file.texturePalette);

        Material material = new Material(GetDefaulTexture());
        material.SetTexture("_BaseMap", file.texturePalette);
        material.SetColor("_BaseColor", Color.white);
        ctx.AddObjectToAsset(fileName + "-VoxelSetting", material);

        var setting = VoxelSettingInit(file);
        setting.RenderRules[0].material = material;
        ctx.AddObjectToAsset(fileName + "-VoxelSetting", setting);

        saveMesh(gameObject, ctx, setting, model);
    }


    Material GetDefaulTexture()
    {
        return GraphicsSettings.renderPipelineAsset.defaultMaterial;
    }
    void saveMesh(GameObject gameObject, AssetImportContext ctx, Settings settings, VoxelModel model)
    {
        var ChunkModels = model.SplitModelInChunkLModel(factor: 2);
        int i = 0;
        float size = 16 * settings.SizeShared;
        for (int x = 0; x < ChunkModels.Item2.x; x++)
            for (int y = 0; y < ChunkModels.Item2.y; y++)
                for (int z = 0; z < ChunkModels.Item2.z; z++)
                {
                    if (ChunkModels.Item1[x, y, z].Only0)
                        continue;
                    var meshBulder = new MeshGenerator(settings, ChunkModels.Item1[x, y, z]);
                    meshBulder.NeighbourChunck = new Tuple<SurfaceAction, Array3UshortOpt>[6]
                    {
                        z + 1 <  ChunkModels.Item2.z ? new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.RenderBasedOnNeighbourChunk, ChunkModels.Item1[x, y, z + 1]) :
                            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // front
                        z - 1 >= 0 ? new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.RenderBasedOnNeighbourChunk, ChunkModels.Item1[x, y, z - 1]) :
                            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // back 
                        y + 1 <  ChunkModels.Item2.y ? new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.RenderBasedOnNeighbourChunk, ChunkModels.Item1[x, y + 1, z]) :
                            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // top
                        y - 1 >= 0 ? new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.RenderBasedOnNeighbourChunk, ChunkModels.Item1[x, y - 1, z]) :
                            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // buttom
                        x - 1 >= 0 ? new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.RenderBasedOnNeighbourChunk, ChunkModels.Item1[x - 1, y, z]) :
                            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // right
                        x + 1 <  ChunkModels.Item2.x ? new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.RenderBasedOnNeighbourChunk, ChunkModels.Item1[x + 1, y, z]) :
                            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // left
                    };
                    
                    meshBulder.GenerateMesh(fileName + "_childMesh_" + i);
                    if (meshBulder.mesh.vertexCount == 0)
                        continue;
                    var child = new GameObject(fileName + "_child_" + i);
                    child.transform.parent = gameObject.transform;
                    child.transform.position = new Vector3(x * size, y * size, z * size);
                    child.AddComponent<MeshRenderer>().material = settings.RenderRules[0].material;
                    child.AddComponent<MeshFilter>().mesh = meshBulder.mesh;
                    ctx.AddObjectToAsset(fileName + "_childMesh_" + i, meshBulder.mesh);
                    i++;

                }
    }
    string getName(AssetImportContext ctx)
    {
        var name = ctx.assetPath.Split('/');

        return name[name.Length - 1].Split('.')[0];
    }

    VoxelModel VoxelModelInit(VoxFileReader file)
    {
        var model = ScriptableObject.CreateInstance<VoxelModel>();
        model.data = new ushort[file.ModelList[0].GetLength(0) * file.ModelList[0].GetLength(1) * file.ModelList[0].GetLength(2)];
        for (int x = 0; x < file.ModelList[0].GetLength(0); x++)
            for (int y = 0; y < file.ModelList[0].GetLength(1); y++)
                for (int z = 0; z < file.ModelList[0].GetLength(2); z++)
                    model.data[x + y * file.ModelList[0].GetLength(0) + z * file.ModelList[0].GetLength(0) * file.ModelList[0].GetLength(1)] = file.ModelList[0][x, y, z];
        model.SetSize(new Vector3Int(file.ModelList[0].GetLength(0), file.ModelList[0].GetLength(1), file.ModelList[0].GetLength(2)));

        model.hideFlags = HideFlags.NotEditable;
        EditorUtility.SetDirty(model);
        return model;
    }

    Settings VoxelSettingInit(VoxFileReader file)
    {
        var setting = ScriptableObject.CreateInstance<Settings>();
        setting.AddRule();
        return setting;
    }
}
