using System.IO;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using Voxel.VoxFile;
using Voxel;
using UnityEngine.Rendering;

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

        //var manager = gameObject.AddComponent<VoxelChunkManager>();
        ctx.AddObjectToAsset(fileName + "VoxelPrefab", gameObject);
        var model = VoxelModelInit(file);
        ctx.AddObjectToAsset(fileName+ "-VoxelModel", model);
        ctx.AddObjectToAsset(fileName + "texture", file.texturePalette, file.texturePalette);

        Material material = new Material(GetDefaulTexture());
        material.SetTexture("_BaseMap", file.texturePalette);
        material.SetColor("_BaseColor", Color.white);
        ctx.AddObjectToAsset(fileName + "-VoxelSetting", material);

        var setting = VoxelSettingInit(file);
        setting.RenderRules[0].material = material;
        ctx.AddObjectToAsset(fileName + "-VoxelSetting", setting);

        /*manager.model = model;
        manager.setting = setting;
        manager.Childname = fileName+"_child";
        manager.CreateChunck();
        manager.AddCollision();*/
        //saveMesh(manager, ctx);
    }


    Material GetDefaulTexture()
    {
        return GraphicsSettings.renderPipelineAsset.defaultMaterial;
    }
    /*void saveMesh(VoxelChunkManager manager, AssetImportContext ctx)
    {
        for (int i = 0; i < manager.chunkList.gamobjectNb; i++)
        {   
            if ( manager.chunkList.RawGOList[i] == null)
                continue;
            var mesh = manager.chunkList.RawGOList[i].GetComponent<MeshFilter>().sharedMesh;
            ctx.AddObjectToAsset(mesh.name, mesh);
        }
    }*/
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
