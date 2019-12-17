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
        private List<Vector3> Vertices = new List<Vector3>();
        private List<Vector2> UVs = new List<Vector2>();
        private int VertextCounter;

        public enum SurfaceAction
        {
            NotRender,
            Render,
            RenderBasedOnNeighbourChunk
        }

        public Array3UshortOpt[] NeighbourChunck { private get; set; } = new Array3UshortOpt[6]
        {
            default, // front
            default, // back 
            default, // top
            default, // buttom
            default, // right
            default, // left
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
            ApplyDataToMesh();
            UpdateMesh(name);
        }

        void Innit()
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        bool ChekVoxel(uint x, uint y, uint z)
        {
            if (x < 0 || x >= Data.Width || y < 0 || y >= Data.Width || z < 0 || z >= Data.Width || Data[x, y, z] < 1)
                return false;
            return true;
        }
        void ApplyDataToMeshForOneVoxel(Vector3 position)
        {

            if (!ChekVoxel((uint)position.x, (uint)position.y, (uint)position.z))
                return;

            var SizedPosition = position * setting.SizeShared;
            for (int i = 0; i < 6; i++)
            {
                var checkPos = setting.NeighboursCheckFaces[i];
                if (ChekVoxel((uint)position.x + (uint)checkPos.x, (uint)position.y + (uint)checkPos.y, (uint)position.z + (uint)checkPos.z))
                    continue;

                Vertices.Add(setting.PrecalculatedVertices[setting.VerticesFacesIndex[i, 0]] + SizedPosition);
                Vertices.Add(setting.PrecalculatedVertices[setting.VerticesFacesIndex[i, 1]] + SizedPosition);
                Vertices.Add(setting.PrecalculatedVertices[setting.VerticesFacesIndex[i, 2]] + SizedPosition);
                Vertices.Add(setting.PrecalculatedVertices[setting.VerticesFacesIndex[i, 3]] + SizedPosition);
                UVs.Add(new Vector2((setting.UvsRef[0].x + Data[(uint)position.x, (uint)position.y, (uint)position.z] - 1) / 256 + 0.5f / 256, setting.UvsRef[0].y + 0.5f));
                UVs.Add(new Vector2((setting.UvsRef[1].x + Data[(uint)position.x, (uint)position.y, (uint)position.z] - 1) / 256 + 0.5f / 256, setting.UvsRef[1].y + 0.5f));
                UVs.Add(new Vector2((setting.UvsRef[2].x + Data[(uint)position.x, (uint)position.y, (uint)position.z] - 1) / 256 + 0.5f / 256, setting.UvsRef[2].y + 0.5f));
                UVs.Add(new Vector2((setting.UvsRef[3].x + Data[(uint)position.x, (uint)position.y, (uint)position.z] - 1) / 256 + 0.5f / 256, setting.UvsRef[3].y + 0.5f));
                Triangles.Add(VertextCounter);
                Triangles.Add(VertextCounter + 1);
                Triangles.Add(VertextCounter + 2);
                Triangles.Add(VertextCounter + 3);
                Triangles.Add(VertextCounter + 1);
                Triangles.Add(VertextCounter);

                VertextCounter += 4;
            }
        }
        public void UpdateMesh(string name)
        {
            mesh.vertices = Vertices.ToArray();
            mesh.triangles = Triangles.ToArray();
            mesh.uv = UVs.ToArray();
            mesh.RecalculateNormals();
            mesh.name = "VoxelMesh-" + name;
            Triangles.Clear();
            Vertices.Clear();
            UVs.Clear();
            VertextCounter = 0;
        }
        public void ApplyDataToMesh()
        {
            for (float x = 0; x < Data.Width; x += 1)
                for (float y = 0; y < Data.Width; y += 1)
                    for (float z = 0; z < Data.Width; z += 1)
                        ApplyDataToMeshForOneVoxel(new Vector3(x, y, z));
        }

    }

}