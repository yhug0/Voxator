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

        public Settings setting { private get; set; }

        public Array3UshortOpt Data { private get; set; }

        public uint Precision { get; set; } = 1;

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



        /// <summary>
        /// Generate mesh based on the parametter of this object
        /// </summary>
        public void GenerateMesh()
        {

        }

    }

}