using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Voxel.VoxFile
{
    public class VoxFileReader
    {
        private string path;

        private byte[] data;

        public uint NumberOfModel { get; private set; } = 1;
        private uint offset = 0;

        public Texture2D texturePalette = new Texture2D(256 * 16, 16);


        public List<ushort[,,]> ModelList { get; private set; } = new List<ushort[,,]>();
        private enum ChunkType
        {
            MAIN, // main chunk contain all
            PACK, //number of model
            SIZE, //model Size
            XYZI, //model
            RGBA, // palette
            MATT // Material

        }

        private uint ModeliNdex = 0;
        private struct ChunkHeader
        {
            public string chunkID;
            public uint Content;
            public uint ChildContent;
        }
        public VoxFileReader(string Path)
        {
            path = Path;
            OpenFile();
            offset += 8;
            AnnaliseFileData((uint)data.Length);
        }

        void OpenFile()
        {
            data = System.IO.File.ReadAllBytes(path);
        }

        void ReadChunkHeader(ref ChunkHeader header)
        {

            header.chunkID = Encoding.UTF8.GetString(data, (int)offset, 4);
            header.Content = System.BitConverter.ToUInt32(data, (int)offset + 4);
            header.ChildContent = System.BitConverter.ToUInt32(data, (int)offset + 8);
            offset += 12;
        }

        void AnnaliseFileData(uint Size)
        {
            ChunkHeader chunkHeader = new ChunkHeader();
            while (offset < Size)
            {
                ReadChunkHeader(ref chunkHeader);
                if (chunkHeader.Content > 0)
                    ParseChunk(chunkHeader);
                if (chunkHeader.ChildContent > 0)
                    AnnaliseFileData(chunkHeader.ChildContent + offset);
            }

        }

        void ParseChunk(ChunkHeader chunk)
        {
            ChunkType type;
            if (!System.Enum.TryParse(chunk.chunkID, out type))
                throw new System.Exception(path + ": Bad File chunk");
            switch (type)
            {
                case ChunkType.SIZE:
                    parseSize();
                    break;
                case ChunkType.XYZI:
                    parseModel();
                    break;
                case ChunkType.RGBA:
                    parsePalette();
                    break;
            }
            offset += chunk.Content;
        }

        void parseSize()
        {
            int x = System.BitConverter.ToInt32(data, (int)offset);
            int z = System.BitConverter.ToInt32(data, (int)offset + 4);
            int y = System.BitConverter.ToInt32(data, (int)offset + 8);
            ModelList.Add(new ushort[Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z)]);
        }

        void parseModel()
        {
            int voxelNum = System.BitConverter.ToInt32(data, (int)offset);
            offset += 4;
            for (int i = 0; i < voxelNum; i++)
            { 
                int x = data[(int)offset + i * 4];
                int z = data[(int)offset + 1 + i * 4];
                int y = data[(int)offset + 2 + i * 4];  
                try {
                ModelList[(int)ModeliNdex][ModelList[(int)ModeliNdex].GetLength(0) - 1 - x, y, ModelList[(int)ModeliNdex].GetLength(2) - 1 - z] = data[(int)offset + 3 + i * 4];
                } catch (System.Exception) {
                    Debug.Log(x + " " + y + " " + z);
                    Debug.Log((ModelList[(int)ModeliNdex].GetLength(0) - 1 - x) + " " + y + " " + (ModelList[(int)ModeliNdex].GetLength(2) - 1 - z));
                }
            }
            ModeliNdex++;
            offset -= 4;
        }

        void parsePalette()
        {
            for (int i = 0; i < 256; i++)
            {
                byte r = data[(int)offset + i * 4];
                byte g = data[(int)offset + 1 + i * 4];
                byte b = data[(int)offset + 2 + i * 4];
                byte a = data[(int)offset + 3 + i * 4];
                Color32 color = new Color32(r, g, b, a);
                for (int j = 0; j < 16; j++)
                    for(int k = 0; k < 16; k++)
                        texturePalette.SetPixel(i * 16 + j, k, color);


            }
            texturePalette.Compress(true);
        }
    }

}