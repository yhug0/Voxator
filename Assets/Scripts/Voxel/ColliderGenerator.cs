using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voxel.Tools;

namespace Voxel
{
    public class ColliderGenerator
    {
        Array3UshortOpt Data = new Array3UshortOpt(2);

        public List<GameObject> RectObject{get; private set;} = new List<GameObject>();
        List<KeyValuePair<Vector3UInt, Vector3UInt>> Rectangulars = new List<KeyValuePair<Vector3UInt, Vector3UInt>>();
        private Settings Settings;

        private Transform transform;

        private int tesselationFactor;

        public ColliderGenerator(Transform parent, Array3UshortOpt data, Settings settings)
        {
            Data = data;
            this.Settings = settings;
            tesselationFactor = 1;
            transform = parent;
        }

        public void GenerateCollider()
        {
                        FindRectangulars();
            SetCollisionRactangular();
        }

        void FindRectangulars()
        {
            bool[] testedIndex = new bool[Data.Size];
            for (uint index = 0; index < Data.Size; index++)
            {
                if (testedIndex[index] || Data.RowData[index] < 1)
                    continue;
                var startRectPos = Data.GetVoxelDataPosition(index);
                var RectSize = SpreadRectangular(startRectPos, ref testedIndex);
                Rectangulars.Add(new KeyValuePair<Vector3UInt, Vector3UInt>(startRectPos, RectSize));
            }
        }

        Vector3UInt SpreadRectangular(Vector3UInt StartPos, ref bool[] testedIndex)
        {
            Vector3UInt Size = new Vector3UInt(StartPos.X, StartPos.Y, StartPos.Z);
            SpreadMaxByVec(ref testedIndex, StartPos, ref Size, new Vector3UInt(1, 0, 0));
            SpreadMaxByVec(ref testedIndex, StartPos, ref Size, new Vector3UInt(0, 1, 0));
            SpreadMaxByVec(ref testedIndex, StartPos, ref Size, new Vector3UInt(0, 0, 1));
            CheckBool(ref testedIndex, StartPos, Size);
            Size += new Vector3UInt(1, 1, 1);
            return Size - StartPos;
        }

        bool TestIndexSpread(uint x, uint y, uint z, bool[] testedIndex)
        {
            return x < 16 && y < 16 && z < 16 && Data[x, y, z] > 0 && !testedIndex[x | y << Data.YOffset | z << Data.ZOffset];
        }

        void SpreadMaxByVec(ref bool[] testedIndex, Vector3UInt StartPos, ref Vector3UInt Size, Vector3UInt direction)
        {
            bool CanSpread = true;
            Size += direction;

            while (CanSpread)
            {
                for (uint x = StartPos.X; x <= Size.X && CanSpread; x++)
                    for (uint y = StartPos.Y; y <= Size.Y && CanSpread; y++)
                        for (uint z = StartPos.Z; z <= Size.Z && CanSpread; z++)
                        {
                            CanSpread = TestIndexSpread(x, y, z, testedIndex);
                        }
                if (CanSpread)
                {
                    StartPos += direction;
                    Size += direction;
                }
            }

            Size -= direction;
        }

        void CheckBool(ref bool[] testedIndex, Vector3UInt StartPos, Vector3UInt EndPOs)
        {
            for (uint x = StartPos.X; x <= EndPOs.X; x++)
                for (uint y = StartPos.Y; y <= EndPOs.Y; y++)
                    for (uint z = StartPos.Z; z <= EndPOs.Z; z++)
                    {
                        testedIndex[x | y << Data.YOffset | z << Data.ZOffset] = true;
                    }
        }

        void SetCollisionRactangular()
        {
            int colliderIndex = 0;
            var enumerator = Rectangulars.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Vector3 position = (Vector3)enumerator.Current.Key + ((Vector3)enumerator.Current.Value / 2.0f);
                GameObject boxObject = new GameObject(string.Format("Collider {0}", colliderIndex));
                BoxCollider boxCollider = boxObject.AddComponent<BoxCollider>();
                Transform boxTransform = boxObject.transform;
                boxTransform.parent = transform;
                boxTransform.localPosition = new Vector3();
                boxTransform.localRotation = new Quaternion();
                boxCollider.center = position * Settings.SizeShared * tesselationFactor;
                boxCollider.size = (Vector3)enumerator.Current.Value * Settings.SizeShared * tesselationFactor;
                ++colliderIndex;

            }
        }
    }
}
