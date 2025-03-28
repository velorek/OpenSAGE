﻿using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <summary>
/// AABTree header. Each mesh can have an associated Axis-Aligned-Bounding-Box tree
/// which is used for collision detection and certain rendering algorithms (like
/// texture projection.
/// </summary>
public sealed record W3dMeshAabTreeHeader(uint NodeCount, uint PolyCount)
    : W3dChunk(W3dChunkType.W3D_CHUNK_AABTREE_HEADER)
{
    internal static W3dMeshAabTreeHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var nodeCount = reader.ReadUInt32();
            var polyCount = reader.ReadUInt32();

            reader.ReadBytes(6 * sizeof(uint)); // Padding

            return new W3dMeshAabTreeHeader(nodeCount, polyCount);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(NodeCount);
        writer.Write(PolyCount);

        for (var i = 0; i < 6; i++) // Padding
        {
            writer.Write(0u);
        }
    }
}
