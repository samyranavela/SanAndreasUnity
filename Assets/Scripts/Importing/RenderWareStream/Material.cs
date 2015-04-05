﻿using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public struct Color4
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public Color4(BinaryReader reader)
        {
            R = reader.ReadByte();
            G = reader.ReadByte();
            B = reader.ReadByte();
            A = reader.ReadByte();
        }
    }

    [SectionType(7)]
    public class Material : SectionData
    {
        public readonly Color4 Colour;
        public readonly UInt32 TextureCount;
        public readonly Texture[] Textures;

        public readonly UInt32 Flags;
        public readonly Single Ambient;
        public readonly Single Specular;
        public readonly Single Diffuse;
        public readonly Single Smoothness;

        public Material(SectionHeader header, Stream stream)
        {
            SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            Flags = reader.ReadUInt32();
            Colour = new Color4(reader);
            reader.ReadUInt32();
            TextureCount = reader.ReadUInt32();
            Textures = new Texture[TextureCount];
            Ambient = reader.ReadSingle();
            Specular = reader.ReadSingle();
            Diffuse = reader.ReadSingle();

            for (var i = 0; i < TextureCount; ++i) {
                Textures[i] = Section<Texture>.ReadData(stream);
            }

            var extHeader = SectionHeader.Read(stream);
            if (extHeader.Size == 0) return;

            while (stream.Position < stream.Length) {
                var section = Section<SectionData>.ReadData(stream);

                var reflection = section as ReflectionMaterial;
                if (reflection != null) {
                    Smoothness = reflection.Intensity;
                    continue;
                }

                var specular = section as SpecularMaterial;
                if (specular != null) {
                    Specular = specular.SpecularLevel;
                    continue;
                }
            }
        }
    }

    [SectionType(0x0253F2FC)]
    public class ReflectionMaterial : SectionData
    {
        public readonly Vector2 Scale;
        public readonly Vector2 Translation;

        public readonly float Intensity;

        public ReflectionMaterial(SectionHeader header, Stream stream)
        {
            var reader = new BinaryReader(stream);

            Scale = new Vector2(reader);
            Translation = new Vector2(reader);

            Intensity = reader.ReadSingle();
        }
    }

    [SectionType(0x0253F2F6)]
    public class SpecularMaterial : SectionData
    {
        public readonly float SpecularLevel;

        public SpecularMaterial(SectionHeader header, Stream stream)
        {
            var reader = new BinaryReader(stream);

            SpecularLevel = reader.ReadSingle();
        }
    }
}
