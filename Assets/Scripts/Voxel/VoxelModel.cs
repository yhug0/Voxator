using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Voxel.Tools;

namespace Voxel
{
    [Serializable]
    [CreateAssetMenu(order = 0, fileName = "VoxelMesh", menuName = "Voxel Mesh")]
    public class VoxelModel : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private int XSizeData;
        [SerializeField]
        [HideInInspector]
        private int YSizeData;
        [SerializeField]
        [HideInInspector]
        private int ZSizeData;


        public int XSize { get { return XSizeData; } private set { XSizeData = value; } }

        public int YSize { get { return YSizeData; } private set { YSizeData = value; } }

        public int ZSize { get { return ZSizeData; } private set { ZSizeData = value; } }
        [HideInInspector]
        public ushort[] data;

        public ushort this[int x, int y, int z]
        {
            get { return data[x + y * XSize + z * XSize * YSize]; }
            set { data[x + y * XSize + z * XSize * YSize] = value; }
        }

        public void SetSize(Vector3Int size)
        {
            XSize = size.x;
            YSize = size.y;
            ZSize = size.z;
        }

        public Tuple<Array3UshortOpt[,,], Vector3Int> SplitModelInChunkLModel(ushort factor)
        {
            int sizeChunk = 4 << factor;
            var nbChunkModel = chunkModelNb((uint)sizeChunk);

            var chunkModels = new Array3UshortOpt[nbChunkModel.x, nbChunkModel.y, nbChunkModel.z];
            var checkCreated = new bool[nbChunkModel.x, nbChunkModel.y, nbChunkModel.z];
            for (int x = 0; x < XSize; x++)
                for (int y = 0; y < YSize; y++)
                    for (int z = 0; z < ZSize; z++)
                    {
                        if (!checkCreated[x / sizeChunk, y / sizeChunk, z / sizeChunk])
                        {
                            chunkModels[x / sizeChunk, y / sizeChunk, z / sizeChunk] = new Array3UshortOpt(factor);
                            checkCreated[x / sizeChunk, y / sizeChunk, z / sizeChunk] = true;
                        }
                        chunkModels[x / sizeChunk, y / sizeChunk, z / sizeChunk][(uint)(x % sizeChunk), (uint)(y % sizeChunk), (uint)(z % sizeChunk)] = this[x, y, z];
                    }
            return new Tuple<Array3UshortOpt[,,], Vector3Int>(chunkModels, nbChunkModel);
        }

        private int Round(float f)
        {
            return Mathf.FloorToInt(f) + (f % 0.1 > 0 ? 1 : 0);
        }

        private Vector3Int chunkModelNb(uint SizeOfChunk)
        {
            Vector3 dest = data != null ? new Vector3(XSize / SizeOfChunk, YSize / SizeOfChunk, ZSize / SizeOfChunk) : new Vector3(-1, -1, -1);
            return new Vector3Int(Round(dest.x), Round(dest.y), Round(dest.z));
        }
    }
}