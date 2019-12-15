using UnityEngine;

namespace Voxel.Tools
{
        public struct Array3UshortOpt
    {
        public ushort[] RowData { get; private set; }
        public ushort YOffset { get; private set; }
        public ushort ZOffset { get; private set; }

        public bool Only0;

        private ushort BMask;

        public ushort Width { get; private set; }
        public uint Size { get; private set; }

        public ushort this[uint x, uint y, uint z]
        {
            get { return this.RowData[x | y << YOffset | z << ZOffset]; }
            set
            {
                if (value != 0) Only0 = false;
                this.RowData[x | y << YOffset | z << ZOffset] = value;
            }
        }

        public Array3UshortOpt(ushort factor)
        {
            Only0 = true;
            if (factor > 4)
                throw new System.Exception("Array3UshortOpt\'s factor too high can\'t exeed 4");
            // 4 << factor * 4 << factor * 4 << factor
            Size = (uint)(64 << factor << factor << factor);
            RowData = new ushort[Size];
            YOffset = (ushort)(1 << factor);
            ZOffset = (ushort)(2 << factor);
            Width = (ushort)(4 << factor);
            BMask = (ushort)(Width - 1);
        }

        public Vector3UInt GetVoxelDataPosition(uint index)
        {
            uint blockX = index & BMask;
            uint blockY = (index >> YOffset) & BMask;
            uint blockZ = (index >> ZOffset) & BMask;
            return new Vector3UInt(blockX, blockY, blockZ);
        }
    }
}
