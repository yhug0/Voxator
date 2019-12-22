using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voxel.Tools;

namespace Voxel
{

    /// <summary>
    /// Generate Meshes from a Voxel Matrix
    /// </summary>
    public class MeshGenerator
    {
        public uint Precision { get; set; } = 1;

        private Settings setting;

        public Mesh mesh = new Mesh();

        private Array3UshortOpt Data;
        private List<int> Triangles = new List<int>();

        private List<List<int>> Submesh = new List<List<int>>();
        private List<Vector3> Vertices = new List<Vector3>();
        private List<Vector2> UVs = new List<Vector2>();
        private int VertextCounter;

        public enum SurfaceAction
        {
            Render,
            NotRender,
            RenderBasedOnNeighbourChunk
        }

        public Tuple<SurfaceAction, Array3UshortOpt>[] NeighbourChunck { get; set; } = new Tuple<SurfaceAction, Array3UshortOpt>[6]
        {
            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // front
            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // back 
            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // top
            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // buttom
            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // right
            new Tuple<SurfaceAction, Array3UshortOpt>(SurfaceAction.Render, default), // left
        };

        public MeshGenerator(Settings setting, Array3UshortOpt data)
        {
            this.setting = setting;
            this.Data = data;
            Innit();
        }

        /// <summary>
        /// Generate mesh based on the parametter of this object
        /// </summary>
        public void GenerateMesh(string name)
        {
            for (int i = 1; i < setting.RenderRules.Length; i++)
                Submesh.Add(new List<int>());
            ApplyDataToMesh();
            UpdateMesh(name);
        }

        void Innit()
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        bool ChekVoxel(int x, int y, int z, RenderRule originRule)
        {
            bool OtherChunk = true;
            if (
            (x < 0 && (OtherChunk = ChekOtherChunk((Data.Width - 1), y, z, VoxelFaces.Right, originRule))) ||
            (x >= Data.Width && (OtherChunk = ChekOtherChunk(0, y, z, VoxelFaces.Left, originRule))) ||
            (y < 0 && (OtherChunk = ChekOtherChunk(x, (Data.Width - 1), z, VoxelFaces.Buttom, originRule))) ||
            (y >= Data.Width && (OtherChunk = ChekOtherChunk(x, 0, z, VoxelFaces.Top, originRule))) ||
            (z < 0 && (OtherChunk = ChekOtherChunk(x, y, (Data.Width - 1), VoxelFaces.Back, originRule))) ||
            (z >= Data.Width && (OtherChunk = ChekOtherChunk(x, y, 0, VoxelFaces.Front, originRule))) ||
            (OtherChunk && (Data[(uint)x, (uint)y, (uint)z] < 1 || CreateWallByRule(originRule, Data[(uint)x, (uint)y, (uint)z]))))
                return false;
            return true;
        }

        bool ChekOtherChunk(int x, int y, int z, VoxelFaces face, RenderRule rule)
        {
            if (NeighbourChunck[(int)face].Item1 == SurfaceAction.RenderBasedOnNeighbourChunk &&
                NeighbourChunck[(int)face].Item2[(uint)x, (uint)y, (uint)z] > 0 && !CreateWallByRule(rule, NeighbourChunck[(int)face].Item2[(uint)x, (uint)y, (uint)z]))
            {
                return false;
            }

            return true;
        }
        void ApplyDataToMeshForOneVoxel(Vector3 position)
        {

            if (Data[(uint)position.x, (uint)position.y, (uint)position.z] < 1)
                return;

            var SizedPosition = position * setting.SizeShared;
            for (int i = 0; i < 6; i++)
            {
                var checkPos = setting.NeighboursCheckFaces[i];
                var ruleId = getRuleId(Data[(uint)position.x, (uint)position.y, (uint)position.z]);
                var rule = setting.RenderRules[ruleId];
                if (ChekVoxel((int)position.x + (int)checkPos.x, (int)position.y + (int)checkPos.y, (int)position.z + (int)checkPos.z, rule))
                    continue;
                
                for (int j = 0; j < 4; j++)
                {
                    Vertices.Add(setting.PrecalculatedVertices[setting.VerticesFacesIndex[i, j]] + SizedPosition);
                    UVs.Add(new Vector2((setting.UvsRef[j].x + Data[(uint)position.x, (uint)position.y, (uint)position.z] - 1) / 256 - 0.25f * 16, setting.UvsRef[j].y + 0.25f * 16));
                    if (ruleId < 1)
                        Triangles.Add(VertextCounter + j);
                    else
                        Submesh[(int)ruleId - 1].Add(VertextCounter + j);

                }
                if (ruleId < 1)
                {
                    Triangles.Add(VertextCounter + 1);
                    Triangles.Add(VertextCounter);
                } else {
                    Submesh[(int)ruleId - 1].Add(VertextCounter + 1);
                    Submesh[(int)ruleId - 1].Add(VertextCounter);
                }

                VertextCounter += 4;
            }
        }
        public void UpdateMesh(string name)
        {
            mesh.subMeshCount = Submesh.Count + 1;
            mesh.vertices = Vertices.ToArray();
            mesh.SetTriangles(Triangles.ToArray(), 0);
            mesh.uv = UVs.ToArray();
            for (int i = 0; i < Submesh.Count; i++)
                mesh.SetTriangles(Submesh[i], i + 1);
            mesh.RecalculateNormals();
            mesh.name = "VoxelMesh-" + name;
            Triangles.Clear();
            Vertices.Clear();
            UVs.Clear();
            Submesh.Clear();
            VertextCounter = 0;
        }
        public void ApplyDataToMesh()
        {
            for (float x = 0; x < Data.Width; x += 1)
                for (float y = 0; y < Data.Width; y += 1)
                    for (float z = 0; z < Data.Width; z += 1)
                        ApplyDataToMeshForOneVoxel(new Vector3(x, y, z));
        }

        public uint getRuleId(ushort value)
        {
            if (setting.RenderRules.Length <= 1)
                return (0);
            for (uint i = 0; i < setting.RenderRules.Length; i++)
                if (value <= (ushort)setting.RenderRules[i].Max)
                    return i;
            return 0;
        }

        public bool CreateWallByRule(RenderRule originRule, ushort value)
        {
            if (setting.RenderRules.Length <= 1)
                return value < 1;
            var rule = setting.RenderRules[getRuleId(value)];
            return ((rule.Transparent && originRule.NeighbourTransparentFaced) || (!rule.Transparent && originRule.NeighbourOpaqueFaced));
        }

    }

}