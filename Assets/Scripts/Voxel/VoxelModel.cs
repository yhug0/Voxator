using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    }
}