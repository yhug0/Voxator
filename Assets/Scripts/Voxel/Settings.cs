using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System;

namespace Voxel
{
    [Serializable]
    [CreateAssetMenu(order = 0, fileName = "VoxelSettings", menuName = "Voxel Settings")]
    public class Settings : ScriptableObject
    {
        private float Size = 0.1f;

        public uint MaxTypeOfVoxel = 255;

        public RenderRule[] RenderRules;
        public float SizeShared { get { return Size; } set { Size = value; } }
        public Vector3[] PrecalculatedVertices { get; private set; } = new Vector3[8];
        public readonly Vector3Int[] NeighboursCheckFaces =
        {
        new Vector3Int(0,0,1),
        new Vector3Int(0,0,-1),
        new Vector3Int(0,1,0),
        new Vector3Int(0,-1,0),
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0),
    };
        public readonly int[,] VerticesFacesIndex = {
        {1,6,5,2}, // front
        {0,7,3,4}, // back 
        {4,6,7,5}, // top
        {0,2,1,3}, // buttom
        {3,6,2,7}, // right
        {0,5,4,1}, // left
    };

        public readonly Vector2[] UvsRef = {
        new Vector2(0f, 0f),
        new Vector2(1f, 1f),
        new Vector2(1f, 0f),
        new Vector2(0f, 1f)
    };


        public void Awake()
        {
            UpdateVertices();
        }
        public void UpdateVertices()
        {
            PrecalculatedVertices[0] = new Vector3(0f, 0f, 0f);
            PrecalculatedVertices[1] = new Vector3(0f, 0f, Size);
            PrecalculatedVertices[2] = new Vector3(Size, 0f, Size);
            PrecalculatedVertices[3] = new Vector3(Size, 0f, 0f);
            PrecalculatedVertices[4] = new Vector3(0f, Size, 0f);
            PrecalculatedVertices[5] = new Vector3(0f, Size, Size);
            PrecalculatedVertices[6] = new Vector3(Size, Size, Size);
            PrecalculatedVertices[7] = new Vector3(Size, Size, 0f);
        }
        public void AddRule()
        {
            var list = this.RenderRules != null ? new List<RenderRule>(this.RenderRules) : new List<RenderRule>();
            var renderRule = new RenderRule
            {
                Min = this.RenderRules != null && this.RenderRules.Length != 0 ? list[list.Count - 1].Max : 0,
                Max = this.MaxTypeOfVoxel,
                NeighbourTransparentFaced = true,
                RenctangleSize = 1,
            };
            list.Insert(list.Count, renderRule);
            this.RenderRules = list.ToArray();
        }

        public Material[] GetMaterials()
        {
            Material[] materials = new Material[RenderRules.Length];
            for (int i = 0; i < RenderRules.Length; i++)
                materials[i] = RenderRules[i].material;
            return materials;
        }

    }
    [Serializable]
    public struct RenderRule
    {
        public Material material;
        public float Min;
        public float Max;
        public bool Transparent;
        public bool NeighbourTransparentFaced;
        public bool NeighbourOpaqueFaced;
        public uint RenctangleSize;

        public uint NbOfRectByLine;
    }
}