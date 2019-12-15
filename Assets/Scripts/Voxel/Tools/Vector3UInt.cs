using UnityEngine;

namespace Voxel.Tools
{
    public struct Vector3UInt
    {
        public uint X;
        public uint Y;
        public uint Z;

        public Vector3UInt(uint x, uint y, uint z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Vector3UInt operator +(Vector3UInt vector1, Vector3UInt vector2)
        {
            vector1.X += vector2.X;
            vector1.Y += vector2.Y;
            vector1.Z += vector2.Z;

            return vector1;
        }

        public static Vector3UInt operator *(Vector3UInt vector1, Vector3UInt vector2)
        {
            vector1.X *= vector2.X;
            vector1.Y *= vector2.Y;
            vector1.Z *= vector2.Z;

            return vector1;
        }

        public static Vector3UInt operator -(Vector3UInt vector1, Vector3UInt vector2)
        {
            vector1.X -= vector2.X;
            vector1.Y -= vector2.Y;
            vector1.Z -= vector2.Z;

            return vector1;
        }

        public static explicit operator Vector3(Vector3UInt vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }
    }
}
