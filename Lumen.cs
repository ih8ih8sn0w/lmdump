using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace lmdump
{
    public class Lumen
    {
        public enum TagType : int
        {
            Invalid = 0x0000,

            Unk000A = 0x000A,
            Symbols = 0xF001,
            Colors = 0xF002,
            Transforms = 0xF003,
            Bounds = 0xF004,
            ActionScript = 0xF005,
            TextureAtlases = 0xF007,
            UnkF008 = 0xF008,
            UnkF009 = 0xF009,
            UnkF00A = 0xF00A,
            UnkF00B = 0xF00B,
            Properties = 0xF00C,
            UnkF00D = 0xF00D,

            Shape = 0xF022,
            Graphic = 0xF024,
            ColorMatrix = 0xF037,
            Positions = 0xF103,

            DefineEditText = 0x0025,
            DefineSprite = 0x0027,

            FrameLabel = 0x002B,
            ShowFrame = 0x0001,
            Keyframe = 0xF105,
            PlaceObject = 0x0004,
            RemoveObject = 0x0005,
            DoAction = 0x000C,

            End = 0xFF00,

            //
            Metadata = 0x8000,
        }

        public class UnhandledTag
        {
            public UnhandledTag()
            {
                type = TagType.Invalid;
            }

            public UnhandledTag(TagType type, int size, InputBuffer f)
            {
                this.type = type;
                this.size = size;
                this.data = f.read(size * 4);
            }

            public UnhandledTag(InputBuffer f)
            {
                type = (TagType)f.readInt();
                size = f.readInt();
                data = f.read(size * 4);
            }

            public void Write(OutputBuffer o)
            {
                o.writeInt((int)type);
                o.writeInt(size);
                o.write(data);
            }

            public TagType type;
            public int size;

            byte[] data;
        }

        public class Color
        {
            public float r = 0;
            public float g = 0;
            public float b = 0;
            public float a = 0;

            public Color()
            {
            }

            public Color(float r, float g, float b, float a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }

            public Color(short r, short g, short b, short a)
            {
                this.r = r / 256.0f;
                this.g = g / 256.0f;
                this.b = b / 256.0f;
                this.a = a / 256.0f;
            }

            public Color(uint rgba)
            {
                r = ((rgba >> 24) & 0xFF) / 255.0f;
                g = ((rgba >> 16) & 0xFF) / 255.0f;
                b = ((rgba >>  8) & 0xFF) / 255.0f;
                a = ((rgba      ) & 0xFF) / 255.0f;
            }

            public override string ToString()
            {
                return $"[{r}, {g}, {b}, {a}]";
            }
        }

        public class ShapeBounds
        {
            public ShapeBounds()
            {

            }

            public ShapeBounds(float l, float t, float r, float b)
            {
                left = l;
                top = t;
                right = r;
                bottom = b;
            }

            public float left;
            public float top;
            public float right;
            public float bottom;
        }

        public struct TextureAtlas
        {
            public int id;
            public int unk;

            public float width;
            public float height;
        }

        public class Vertex
        {
            public float x;
            public float y;
            public float u;
            public float v;

            public Vertex()
            {

            }

            public Vertex(float x, float y, float u, float v)
            {
                this.x = x;
                this.y = y;
                this.u = u;
                this.v = v;
            }
        }

        public class Graphic
        {
            public int nameId;
            public int atlasId;
            public short unk1;
			public short numVerts;
			public int numIndices;

            public Vertex[] verts;
            public short[] indices;

            public Graphic()
            {

            }

            public Graphic(InputBuffer f)
            {
                Read(f);
            }

            public void Read(InputBuffer f)
            {
                atlasId = f.readInt();
                unk1 = f.readShort();

                numVerts = f.readShort();
                numIndices = f.readInt();

                verts = new Vertex[numVerts];
                indices = new short[numIndices];

                for (int i = 0; i < numVerts; i++)
                {
                    verts[i] = new Vertex();
                    verts[i].x = f.readFloat();
                    verts[i].y = f.readFloat();
                    verts[i].u = f.readFloat();
                    verts[i].v = f.readFloat();
                }

                for (int i = 0; i < numIndices; i++)
                {
                    indices[i] = f.readShort();
                }

                // indices are padded to word boundaries
                if ((numIndices % 2) != 0)
                {
                    f.skip(0x02);
                }
            }

            public void Write(OutputBuffer o)
            {
                OutputBuffer chunk = new OutputBuffer();
                chunk.writeInt(atlasId);
                chunk.writeShort(unk1);
                chunk.writeShort((short)verts.Length);
                chunk.writeInt(indices.Length);

                foreach (var vert in verts)
                {
                    chunk.writeFloat(vert.x);
                    chunk.writeFloat(vert.y);
                    chunk.writeFloat(vert.u);
                    chunk.writeFloat(vert.v);
                }

                foreach (var index in indices)
                {
                    chunk.writeShort(index);
                }

                if ((indices.Length % 2) != 0)
                {
                    chunk.writeShort(0);
                }

                o.writeInt((int)TagType.Graphic);
                o.writeInt(chunk.Size / 4);
                o.write(chunk);
            }
        }

        public class Shape
        {
            public int id;
            public int unk1;
            public int boundingBoxID;
            public int unk2;
			//public int numGraphics;

            public Graphic[] graphics;

            public Shape()
            {

            }

            public Shape(InputBuffer f)
            {
                Read(f);
            }

            public void Read(InputBuffer f)
            {
                id = f.readInt();
                unk1 = f.readInt();
                boundingBoxID = f.readInt();
                unk2 = f.readInt();
				//numGraphics = f.readInt();

                int numGraphics = f.readInt();
                graphics = new Graphic[numGraphics];

                for (int i = 0; i < numGraphics; i++)
                {
                    f.skip(0x08); // graphic chunk header
                    graphics[i] = new Graphic(f);
                }
            }

            public void Write(OutputBuffer o)
            {
                o.writeInt((int)TagType.Shape);
                o.writeInt(5);

                o.writeInt(id);
                o.writeInt(unk1);
                o.writeInt(boundingBoxID);
                o.writeInt(unk2);
                o.writeInt(graphics.Length);

                foreach (var graphic in graphics)
                {
                    graphic.Write(o);
                }
            }
        }

        public struct DynamicText
        {
            public enum Alignment : short
            {
                Left = 0,
                Right = 1,
                Center = 2
            }

            public int id;
            public int unk1;
            public int placeholderTextId;
            public string placeholder; // FIXME
            public int unk2;
            public int colorId;
            public int unk3;
            public int unk4;
            public int unk5;
            public Alignment alignment;
            public short unk6;
            public int unk7;
            public int unk8;
            public float size;
            public int unk9;
            public int unk10;
            public int unk11;
            public int unk12;

            public DynamicText(InputBuffer f) : this()
            {
                Read(f);
            }

            public void Read(InputBuffer f)
            {
                id = f.readInt();
                unk1 = f.readInt();
                placeholderTextId = f.readInt();
                unk2 = f.readInt();
                colorId = f.readInt();
                unk3 = f.readInt();
                unk4 = f.readInt();
                unk5 = f.readInt();
                alignment = (Alignment)f.readShort();
                unk6 = f.readShort();
                unk7 = f.readInt();
                unk8 = f.readInt();
                size = f.readFloat();
                unk9 = f.readInt();
                unk10 = f.readInt();
                unk11 = f.readInt();
                unk12 = f.readInt();
            }

            public void Write(OutputBuffer o)
            {
                o.writeInt((int)TagType.DefineEditText);
                o.writeInt(16);

                o.writeInt(id);
                o.writeInt(unk1);
                o.writeInt(placeholderTextId);
                o.writeInt(unk2);
                o.writeInt(colorId);
                o.writeInt(unk3);
                o.writeInt(unk4);
                o.writeInt(unk5);
                o.writeShort((short)alignment);
                o.writeShort(unk6);
                o.writeInt(unk7);
                o.writeInt(unk8);
                o.writeFloat(size);
                o.writeInt(unk9);
                o.writeInt(unk10);
                o.writeInt(unk11);
                o.writeInt(unk12);
            }
        }

        public class Sprite
        {
            public class Label
            {
                public int nameId;
                public int startFrame;
                public int unk1;

                public Label(InputBuffer f)
                {
                    Read(f);
                }

                public void Read(InputBuffer f)
                {
                    nameId = f.readInt();
                    startFrame = f.readInt();
                    unk1 = f.readInt();
                }

                public void Write(OutputBuffer o)
                {
                    o.writeInt((int)TagType.FrameLabel);
                    o.writeInt(3);
                    o.writeInt(nameId);
                    o.writeInt(startFrame);
                    o.writeInt(unk1);
                }
            }

            public class Placement
            {
                public int objectId;
                public int placementId;
                public int unk1;
                public int nameId;
                public short unk2;
                public short unk3;
                public short mcObjectId;
                public short unk4;

                public short transformFlags;
                public short transformId;
                public ushort positionFlags;
                public short positionId;
                public int colorId1;
                public int colorId2;

                //ColorMatrix colorMatrix = null;
                public UnhandledTag colorMatrix = null;
                public UnhandledTag unkF014 = null;

                public Placement()
                {
                }

                public Placement(InputBuffer f) : this()
                {
                    Read(f);
                }

                public void Read(InputBuffer f)
                {
                    objectId = f.readInt();
                    placementId = f.readInt();
                    unk1 = f.readInt();
                    nameId = f.readInt();
                    unk2 = f.readShort();
                    unk3 = f.readShort();
                    mcObjectId = f.readShort();
                    unk4 = f.readShort();
                    transformFlags = f.readShort();
                    transformId = f.readShort();
                    positionFlags = (ushort)f.readShort();
                    positionId = f.readShort();
                    colorId1 = f.readInt();
                    colorId2 = f.readInt();

                    bool hasColorMatrix = (f.readInt() == 1);
                    bool hasUnkF014 = (f.readInt() == 1);

                    if (hasColorMatrix)
                    {
                        //colorMatrix = new ColorMatrix(f);
                        colorMatrix = new UnhandledTag(f);
                    }

                    if (hasUnkF014)
                    {
                        unkF014 = new UnhandledTag(f);
                    }
                }

                public void Write(OutputBuffer o)
                {
                    o.writeInt((int)TagType.PlaceObject);
                    o.writeInt(12);

                    o.writeInt(objectId);
                    o.writeInt(placementId);
                    o.writeInt(unk1);
                    o.writeInt(nameId);
                    o.writeShort(unk2);
                    o.writeShort(unk3);
                    o.writeShort(mcObjectId);
                    o.writeShort(unk4);
                    o.writeShort(transformFlags);
                    o.writeShort(transformId);
                    o.writeShort((short)positionFlags);
                    o.writeShort(positionId);
                    o.writeInt(colorId1);
                    o.writeInt(colorId2);

                    o.writeInt((colorMatrix != null) ? 1 : 0);
                    o.writeInt((unkF014 != null) ? 1 : 0);

                    if (colorMatrix != null)
                        colorMatrix.Write(o);

                    if (unkF014 != null)
                        unkF014.Write(o);
                }
            }

            public class Deletion
            {
                public int unk1;
                public short mcObjectId; // or was it placement id?
                public short unk2;

                public Deletion(InputBuffer f)
                {
                    Read(f);
                }

                public void Read(InputBuffer f)
                {
                    unk1 = f.readInt();
                    mcObjectId = (short)f.readShort();
                    unk2 = (short)f.readShort();
                }

                public void Write(OutputBuffer o)
                {
                    o.writeInt((int)TagType.RemoveObject);
                    o.writeInt(2);
                    o.writeInt(unk1);
                    o.writeShort(mcObjectId);
                    o.writeShort(unk2);
                }
            }

            public class Action
            {
                public int actionId;
                public int unk1;

                public Action(InputBuffer f)
                {
                    Read(f);
                }

                public void Read(InputBuffer f)
                {
                    actionId = f.readInt();
                    unk1 = f.readInt();
                }

                public void Write(OutputBuffer o)
                {
                    o.writeInt((int)TagType.DoAction);
                    o.writeInt(2);
                    o.writeInt(actionId);
                    o.writeInt(unk1);
                }
            }

            public class Frame
            {
                public int id;

                public List<Deletion> deletions = new List<Deletion>();
                public List<Action> actions = new List<Action>();
                public List<Placement> placements = new List<Placement>();

                public Frame()
                {
                }

                public Frame(InputBuffer f) : this()
                {
                    Read(f);
                }

                public void Read(InputBuffer f)
                {
                    id = f.readInt();
                    int numChildren = f.readInt();

                    for (int childId = 0; childId < numChildren; childId++)
                    {
                        TagType childType = (TagType)f.readInt();
                        int childSize = f.readInt();

                        if (childType == TagType.RemoveObject)
                        {
                            deletions.Add(new Deletion(f));
                        }
                        else if (childType == TagType.DoAction)
                        {
                            actions.Add(new Action(f));
                        }
                        else if (childType == TagType.PlaceObject)
                        {
                            placements.Add(new Placement(f));
                        }
                    }
                }

                // NOTE: unlike other chunk write functions, this does not include the header
                // so it can be used for both frames and keyframes.
                public void Write(OutputBuffer o)
                {
                    o.writeInt(id);
                    o.writeInt(deletions.Count + actions.Count + placements.Count);

                    foreach (var deletion in deletions)
                    {
                        deletion.Write(o);
                    }

                    foreach (var action in actions)
                    {
                        action.Write(o);
                    }

                    foreach (var placement in placements)
                    {
                        placement.Write(o);
                    }
                }
            }

            public int id;
            public int unk1;
            public int unk2;
            public int unk3;

            public Label[] labels;
            public List<Frame> frames = new List<Frame>();
            public List<Frame> keyframes = new List<Frame>();

            public Sprite()
            {
            }

            public Sprite(InputBuffer f) : this()
            {
                Read(f);
            }

            public void Read(InputBuffer f)
            {
                id = f.readInt();
                unk1 = f.readInt();
                unk2 = f.readInt();

                int numLabels = f.readInt();
                int numFrames = f.readInt();
                int numKeyframes = f.readInt();

                labels = new Label[numLabels];

                unk3 = f.readInt();

                for (int i = 0; i < numLabels; i++)
                {
                    f.skip(0x04);

                    labels[i] = new Label(f);
                }

                int totalFrames = numFrames + numKeyframes;
                for (int frameId = 0; frameId < totalFrames; frameId++)
                {
                    TagType frameType = (TagType)f.readInt();
                    f.skip(0x04);

                    Frame frame = new Frame(f);

                    if (frameType == TagType.Keyframe)
                    {
                        keyframes.Add(frame);
                    }
                    else
                    {
                        frames.Add(frame);
                    }
                }
            }

            public void Write(OutputBuffer o)
            {
                o.writeInt((int)TagType.DefineSprite);
                o.writeInt(7);
                o.writeInt(id);
                o.writeInt(unk1);
                o.writeInt(unk2);
                o.writeInt(labels.Length);
                o.writeInt(frames.Count);
                o.writeInt(keyframes.Count);
                o.writeInt(unk3);

                foreach (var label in labels)
                {
                    label.Write(o);
                }

                foreach (var frame in frames)
                {
                    o.writeInt((int)TagType.ShowFrame);
                    o.writeInt(2);
                    frame.Write(o);
                }

                foreach (var frame in keyframes)
                {
                    o.writeInt((int)TagType.Keyframe);
                    o.writeInt(2);
                    frame.Write(o);
                }
            }
        }

        public class Header
        {
            public int magic;
            public int unk0;
            public int unk1;
            public int unk2;
            public int unk3;
            public int unk4;
            public int unk5;
            public int filesize;
            public int unk6;
            public int unk7;
            public int unk8;
            public int unk9;
            public int unk10;
            public int unk11;
            public int unk12;
            public int unk13;

            public void Write(OutputBuffer o)
            {
                o.writeInt(magic);
                o.writeInt(unk0);
                o.writeInt(unk1);
                o.writeInt(unk2);
                o.writeInt(unk3);
                o.writeInt(unk4);
                o.writeInt(unk5);
                o.writeInt(filesize);
                o.writeInt(unk6);
                o.writeInt(unk7);
                o.writeInt(unk8);
                o.writeInt(unk9);
                o.writeInt(unk10);
                o.writeInt(unk11);
                o.writeInt(unk12);
                o.writeInt(unk13);
            }
        }

        public class Transform
        {
            public float M11;
            public float M12;
            public float M21;
            public float M22;
            public float M31;
            public float M32;
        }

        public class Vector2
        {
            public Vector2()
            {
            }

            public Vector2(float x, float y)
            {
                X = x;
                Y = y;
            }

            public float X;
            public float Y;
        }

        public Header header;
        public List<string> symbols { get; set; }
        public List<Color> colors { get; set; }
        public List<Transform> transforms = new List<Transform>();
        public List<Vector2> positions = new List<Vector2>();
        public List<ShapeBounds> bounds { get; set; }
        public List<TextureAtlas> textureAtlases { get; set; }
        public List<Shape> shapes { get; set; }
		public List<Graphic> graphics { get; set; }
        public List<DynamicText> texts { get; set; }
        public List<Sprite> sprites { get; set; }

        public UnhandledTag properties;
        public UnhandledTag actionscript;
        public UnhandledTag unkF008;
        public UnhandledTag unkF009;
        public UnhandledTag unkF00A;
        public UnhandledTag unk000A;
        public UnhandledTag unkF00B;
        public UnhandledTag unkF00D;

        public Metadata metadata;

        public Lumen()
        {
            header = new Header();
            symbols = new List<string>();
            colors = new List<Color>();
            bounds = new List<ShapeBounds>();
            textureAtlases = new List<TextureAtlas>();
            shapes = new List<Shape>();
			graphics = new List<Graphic>();
            texts = new List<DynamicText>();
            sprites = new List<Sprite>();

            metadata = new Metadata();
        }

        public Lumen(string filename) : this()
        {
            Read(filename);
        }

        public void Read(string filename)
        {
            InputBuffer f = new InputBuffer(filename);
            header.magic = f.readInt();
            header.unk0 = f.readInt();
            header.unk1 = f.readInt();
            header.unk2 = f.readInt();
            header.unk3 = f.readInt();
            header.unk4 = f.readInt();
            header.unk5 = f.readInt();
            header.filesize = f.readInt();
            header.unk6 = f.readInt();
            header.unk7 = f.readInt();
            header.unk8 = f.readInt();
            header.unk9 = f.readInt();
            header.unk10 = f.readInt();
            header.unk11 = f.readInt();
            header.unk12 = f.readInt();
            header.unk13 = f.readInt();

            bool done = false;
            while (!done)
            {
                uint chunkOffset = f.ptr;
                TagType chunkType = (TagType)f.readInt();
                int chunkSize = f.readInt(); // in dwords!

                switch (chunkType)
                {
                    case TagType.Invalid:
                        // uhhh. i think there's a specific exception for this
                        throw new Exception("Malformed file");

                    case TagType.Symbols:
                        int numSymbols = f.readInt();

                        while (symbols.Count < numSymbols)
                        {
                            int len = f.readInt();

                            symbols.Add(f.readString());
                            f.skip(4 - (f.ptr % 4));
                        }

                        break;

                    case TagType.Colors:
                        int numColors = f.readInt();

                        Console.WriteLine("Colors\n{");

                        for (int i = 0; i < numColors; i++)
                        {
                            var offs = f.ptr;
                            var color = new Color(f.readShort(), f.readShort(), f.readShort(), f.readShort());
                            colors.Add(color);

                            Console.WriteLine($"\t0x{i:X4}: {color} # offset = 0x{offs:X2}");
                        }

                        Console.WriteLine("}\n");
                        break;

                    case TagType.Unk000A:
                        unk000A = new UnhandledTag(chunkType, chunkSize, f);
                        break;
                    case TagType.UnkF00A:
                        unkF00A = new UnhandledTag(chunkType, chunkSize, f);
                        break;
                    case TagType.UnkF00B:
                        unkF00B = new UnhandledTag(chunkType, chunkSize, f);
                        break;
                    case TagType.UnkF008:
                        unkF008 = new UnhandledTag(chunkType, chunkSize, f);
                        break;
                    case TagType.UnkF009:
                        unkF009 = new UnhandledTag(chunkType, chunkSize, f);
                        break;
                    case TagType.UnkF00D:
                        unkF00D = new UnhandledTag(chunkType, chunkSize, f);
                        break;
                    case TagType.ActionScript:
                        actionscript = new UnhandledTag(chunkType, chunkSize, f);
                        break;

                    case TagType.End:
                        done = true;
                        break;

                    case TagType.Transforms:
                        int numTransforms = f.readInt();

                        Console.WriteLine("Transforms\n{");

                        for (int i = 0; i < numTransforms; i++)
                        {
                            var offs = f.ptr;
                            Transform xform = new Transform();
                            xform.M11 = f.readFloat();
                            xform.M12 = f.readFloat();
                            xform.M21 = f.readFloat();
                            xform.M22 = f.readFloat();
                            xform.M31 = f.readFloat();
                            xform.M32 = f.readFloat();

                            var scaleX = Math.Sign(xform.M11) * Math.Sqrt(xform.M11 * xform.M11 + xform.M21 * xform.M21);
                            var scaleY = Math.Sign(xform.M22) * Math.Sqrt(xform.M12 * xform.M12 + xform.M22 * xform.M22);

                            var angleRads = Math.Atan2(xform.M21, xform.M22);

                            Console.WriteLine($"\t0x{i:X4}: # offset = 0x{offs:X2}");
                            Console.WriteLine($"\t\tposition: [{xform.M31}, {xform.M32}]");
                            Console.WriteLine($"\t\trotation: {angleRads * (180 / Math.PI)}°");
                            Console.WriteLine($"\t\tscale: [{scaleX}, {scaleY}]\n");

                            transforms.Add(xform);
                        }

                        Console.WriteLine("}\n");

                        break;

                    case TagType.Positions:
                        int numPositions = f.readInt();
                        Console.WriteLine("Positions\n{");

                        for (int i = 0; i < numPositions; i++)
                        {
                            var offs = f.ptr;
                            var pos = new Vector2(f.readFloat(), f.readFloat());
                            Console.WriteLine($"\t0x{i:X4}: [{pos.X}, {pos.Y}] # offset = 0x{offs:X2}");

                            positions.Add(pos);
                        }

                        Console.WriteLine("}\n");

                        break;

                    case TagType.Bounds:
                        int numBounds = f.readInt();

                        for (int i = 0; i < numBounds; i++)
                        {
                            bounds.Add(new ShapeBounds(f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()));
                        }
                        break;

                    case TagType.Properties:
                        properties = new UnhandledTag(chunkType, chunkSize, f);
                        break;

                    case TagType.TextureAtlases:
                        int numAtlases = f.readInt();

                        Console.WriteLine("Atlases\n{");

                        for (int i = 0; i < numAtlases; i++)
                        {
                            TextureAtlas atlas = new TextureAtlas();
                            atlas.id = f.readInt();
                            atlas.unk = f.readInt();
                            atlas.width = f.readFloat();
                            atlas.height = f.readFloat();
                            Console.WriteLine("\t{");
                            Console.WriteLine($"\t\tid = 0x{atlas.id:X2}");
                            Console.WriteLine($"\t\tunk = 0x{atlas.unk:X2}");
                            Console.WriteLine($"\t\twidth = {atlas.width}");
                            Console.WriteLine($"\t\theight = {atlas.height}");
                            Console.WriteLine("\t}\n");

                            textureAtlases.Add(atlas);
                        }

                        Console.WriteLine("}\n");

                        break;

                    case TagType.Shape:
						Console.WriteLine("Shape\n{");
						Shape shape = new Shape{
							id = f.readInt(),
							unk1 = f.readInt(),
							boundingBoxID = f.readInt(),
							unk2 = f.readInt()
						};
						Console.WriteLine($"\tID: {shape.id}");
						Console.WriteLine($"\tUnk1: 0x{shape.unk1:X2}");
						Console.WriteLine($"\tBounding Box ID: {shape.boundingBoxID}");
						Console.WriteLine($"\tUnk2: 0x{shape.unk2:X2}");
						//Console.WriteLine($"\tNum Graphics: {shape.numGraphics}");
						Console.WriteLine("}");
						f.skip(0x04);

						break;

					case TagType.Graphic:
						Console.WriteLine("\tGraphic");
						Console.WriteLine("\t\t{");
						f.reverse(0x04);
						Graphic graphic = new Graphic {
							nameId = f.readInt(),
							atlasId = f.readInt(),
							unk1 = f.readShort(),
							numVerts = f.readShort(),
							numIndices = f.readInt(),
						};
						Vertex[] verts = new Vertex[graphic.numVerts];
						short[] indices = new short[graphic.numIndices];
						Console.WriteLine($"\t\tnameId: {graphic.nameId}");
						Console.WriteLine($"\t\tatlasId: 0x{graphic.atlasId:X2}");
						Console.WriteLine($"\t\tunk1: 0x{graphic.unk1:X2}");
						Console.WriteLine($"\t\tnumVerts: {graphic.numVerts}");
						Console.WriteLine($"\t\tnumIndicies: {graphic.numIndices}");

						for (int i = 0; i < graphic.numVerts; i++)
						{
							verts[i] = new Vertex();
							//verts[i].x = f.readFloat();
							Console.WriteLine($"\t\tVert_{i} pos: [" + (verts[i].x = f.readFloat()) + "," + (verts[i].y = f.readFloat()) + "], uv: ["+ (verts[i].u = f.readFloat()) + "," + (verts[i].v = f.readFloat()) + "] }");
							//verts[i].y = f.readFloat();
							//verts[i].u = f.readFloat();
							//verts[i].v = f.readFloat();
						}
						Console.Write("indices: [");
						for (int i = 0; i < graphic.numIndices; i++)
						{

							Console.Write((indices[i] = f.readShort()) + ", ");
						}
						Console.Write("]\n");

						// indices are padded to word boundaries
						if ((graphic.numIndices % 2) != 0)
						{
							f.skip(0x02);
						}

						break;

                    case TagType.DefineEditText:
                        texts.Add(new DynamicText(f));
                        break;

                    case TagType.DefineSprite:
                        sprites.Add(new Sprite(f));
                        break;

					case TagType.FrameLabel:
						break;

					case TagType.Keyframe:
						break;


					default:
                        throw new NotImplementedException($"Unhandled chunk id: 0x{(uint)chunkType:X} @ 0x{chunkOffset:X}");
                }
            }
        }

        #region serialization
        void writeSymbols(OutputBuffer o)
        {
            OutputBuffer chunk = new OutputBuffer();
            chunk.writeInt(symbols.Count);

            foreach (var symbol in symbols)
            {
                chunk.writeInt(symbol.Length);
                chunk.writeString(symbol);

                int padSize = 4 - (chunk.Size % 4);
                for (int i = 0; i < padSize; i++)
                {
                    chunk.writeByte(0);
                }
            }

            o.writeInt((int)TagType.Symbols);
            o.writeInt(chunk.Size / 4);
            o.write(chunk);
        }

        void writeColors(OutputBuffer o)
        {
            o.writeInt((int)TagType.Colors);
            o.writeInt(colors.Count * 2 + 1);
            o.writeInt(colors.Count);

            foreach (var color in colors)
            {
                o.writeShort((short)(color.r * 255));
                o.writeShort((short)(color.g * 255));
                o.writeShort((short)(color.b * 255));
                o.writeShort((short)(color.a * 255));
            }
        }

        void writePositions(OutputBuffer o)
        {
            o.writeInt((int)TagType.Positions);
            o.writeInt(positions.Count * 2 + 1);
            o.writeInt(positions.Count);

            foreach (var position in positions)
            {
                o.writeFloat(position.X);
                o.writeFloat(position.Y);
            }
        }

        void writeTransforms(OutputBuffer o)
        {
            o.writeInt((int)TagType.Transforms);
            o.writeInt(transforms.Count * 6 + 1);
            o.writeInt(transforms.Count);

            foreach (var transform in transforms)
            {
                o.writeFloat(transform.M11);
                o.writeFloat(transform.M12);
                o.writeFloat(transform.M21);
                o.writeFloat(transform.M22);
                o.writeFloat(transform.M31);
                o.writeFloat(transform.M32);
            }
        }

        void writeExtents(OutputBuffer o)
        {
            o.writeInt((int)TagType.Bounds);
            o.writeInt(bounds.Count * 4 + 1);
            o.writeInt(bounds.Count);

            foreach (var extent in bounds)
            {
                o.writeFloat(extent.left);
                o.writeFloat(extent.top);
                o.writeFloat(extent.right);
                o.writeFloat(extent.bottom);
            }
        }

        void writeAtlases(OutputBuffer o)
        {
            o.writeInt((int)TagType.TextureAtlases);
            o.writeInt(textureAtlases.Count * 4 + 1);
            o.writeInt(textureAtlases.Count);

            foreach (var atlas in textureAtlases)
            {
                o.writeInt(atlas.id);
                o.writeInt(atlas.unk);
                o.writeFloat(atlas.width);
                o.writeFloat(atlas.height);
            }
        }

        void writeShapes(OutputBuffer o)
        {
            foreach (var shape in shapes) shape.Write(o);
            {
                
            }
        }

        void writeMovieClips(OutputBuffer o)
        {
            foreach (var mc in sprites)
            {
                mc.Write(o);
            }
        }

        void writeTexts(OutputBuffer o)
        {
            foreach (var text in texts)
            {
                text.Write(o);
            }
        }

        #endregion

        public byte[] Rebuild()
        {
            OutputBuffer o = new OutputBuffer();

            // TODO: write correct filesize in header. 
            // It isn't checked by the game, but what the hell, right?
            header.Write(o);

            writeSymbols(o);
            writeColors(o);
            writeTransforms(o);
            writePositions(o);
            writeExtents(o);
            actionscript.Write(o);
            writeAtlases(o);

            unkF008.Write(o);
            unkF009.Write(o);
            unkF00A.Write(o);
            unk000A.Write(o);
            unkF00B.Write(o);
            properties.Write(o);
            unkF00D.Write(o);

            writeShapes(o);
            writeMovieClips(o);
            writeTexts(o);
            //metadata.Write(o);

            o.writeInt((int)TagType.End);
            o.writeInt(0);

            int padSize = (4 - (o.Size % 4)) % 4;
            for (int i = 0; i < padSize; i++)
            {
                o.writeByte(0);
            }

            return o.getBytes();
        }

        public class Metadata
        {
            public short VersionMajor;
            public short VersionMinor;
            public short VersionPatch;
            public short VersionFlag;

            public Metadata()
            {
                //VersionMajor = Sm4shPlugin.VersionMajor;
                //VersionMinor = Sm4shPlugin.VersionMinor;
                //VersionPatch = Sm4shPlugin.VersionPatch;
                //VersionFlag = Sm4shPlugin.VersionFlag;
            }

            public Metadata(InputBuffer f)
            {
                Read(f);
            }

            public void Read(InputBuffer f)
            {
                VersionMajor = f.readShort();
                VersionMinor = f.readShort();
                VersionPatch = f.readShort();
                VersionFlag = f.readShort();
            }

            public void Write(OutputBuffer o)
            {
                o.writeInt((int)TagType.Metadata);
                o.writeInt(2);

                o.writeShort(VersionMajor);
                o.writeShort(VersionMinor);
                o.writeShort(VersionPatch);
                o.writeShort(VersionFlag);
            }
        }
    }
}