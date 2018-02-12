using Bridge.Html5;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YataOnline
{
    public struct Color {
        public byte A, R, G, B;
        public string AsHtmlColor { get { return "#" + R.ToString(16) + G.ToString(16) + B.ToString(16); } }
        public string AsRgbArgs { get { return String.Format("({0},{1},{2})", R.ToString(),G.ToString(),B.ToString()); } }
    }
    
    public class Theme
    {
        public const string Name_TopScr = "Top-screen"; //no spaces because are used as HTML ids
        public const string Name_BotScr = "Bottom-screen";
        public const string Name_Folder = "Folder";
        public const string Name_FolderOpn = "Open-folder";
        public const string Name_IcnSmall = "Small-icon";
        public const string Name_IcnBig = "Big-icon";

        public bool BGM = false;
        public uint TopScreenDrawType;
        public uint TopScreenFrameType;
        public uint BotScreenDrawType;
        public uint BotScreenFrameType;
        public Texture[] textures = new Texture[]
        {
            new Texture(Name_TopScr),
            new Texture(Name_BotScr),
            new Texture(Name_Folder),
            new Texture(Name_FolderOpn),
            new Texture(Name_IcnBig),
            new Texture(Name_IcnSmall)
        };

        public ColorField[] ColorFields = new ColorField[] //IDs must match ColorFlagsDescription
        {
            new ColorField("Cursor",0xC, new int[]{ 0,3,9}), //0x30
            new ColorField("3D-Folder",0xC ), //0x38
            new ColorField("DsiWare-icon",0xD,new int[] {0,3,6}), //0x4C
            new ColorField("ArrowButtons",0xD,new int[] {0,3,6}), //0x60
            new ColorField("Arrows",9), //0x68
            new ColorField("BottomScreenOpenButton",0x20,new int[] {4,7,10,20,23,26}),//0x70
            new ColorField("BottomScreenCloseButton",0x20,new int[] {4,7,10,20,23,26}), //0x74
            new ColorField("BottomScreenTitleTextColor",0xD,new int[] {0,10}), //0x7C
            new ColorField("FolderBackground",0xD,new int[] {0,3,6}), //0x94
            new ColorField("BottomScreenOverlay",0x15 ,new int[] {0,3,6,12,15}), //0xA4
            new ColorField("TopScreenOverlay",0xC, new int[] { 0,9}), //0xAC
            new ColorField("DemoMessage",0x6), //0xB4
            //0x9C unsupported yet
        };

        static readonly int[] ColorFieldsOffsets = 
        {
            0x2C, 0x34, 0x48, 0x5C, 0x64, 0x6C, 0x00, //0x6C enables both 0x70 and 0x74, so the other offset 0x00 is ignored
            0x78, 0x90, 0xA0, 0xA8, 0xB0
        };

        public static Theme EmptyTheme()
        {
            Theme t = new Theme();
            t.TopScreenDrawType = 3;
            t.TopScreenFrameType = 1;
            t.BotScreenDrawType = 3;
            t.BotScreenFrameType = 1;
            t.textures[0].tex = ImageEncoding.WhiteImage(t.TopScreenImageID);
            t.textures[1].tex = ImageEncoding.WhiteImage(t.BotScreenImageID);
            return t;
        }
        
        public static Theme ReadTheme(Stream s)
        {
            Theme t = new Theme();

            BinaryReader bin = new BinaryReader(s);
            bin.BaseStream.Position = 0x5;

            t.BGM = bin.ReadByte() != 0;
            #region ReadImages
            bin.BaseStream.Position = 0xC;
            t.TopScreenDrawType = bin.ReadUInt32();
            t.TopScreenFrameType = bin.ReadUInt32();
            if (t.TopScreenDrawType == 3)
            {
                bin.BaseStream.Position = 0x18;
                uint offset = bin.ReadUInt32();
                bin.BaseStream.Position = offset;
                t.GetTexture(Name_TopScr).tex = ImageEncoding.getImage(bin, t.TopScreenImageID);
            }

            bin.BaseStream.Position = 0x20;
            t.BotScreenDrawType = bin.ReadUInt32();
            t.BotScreenFrameType = bin.ReadUInt32();
            if (t.BotScreenDrawType == 3)
            {
                bin.BaseStream.Position = 0x28;
                uint offset = bin.ReadUInt32();
                bin.BaseStream.Position = offset;
                t.GetTexture(Name_BotScr).tex = ImageEncoding.getImage(bin, t.BotScreenImageID);
            }

            bin.BaseStream.Position = 0x3C;
            if (bin.ReadUInt32() != 0)
            {
                uint offs1 = bin.ReadUInt32();
                uint offs2 = bin.ReadUInt32();
                bin.BaseStream.Position = offs1;
                t.GetTexture(Name_Folder).tex = ImageEncoding.getImage(bin, 7);
                bin.BaseStream.Position = offs2;
                t.GetTexture(Name_FolderOpn).tex = ImageEncoding.getImage(bin, 8);
            }

            bin.BaseStream.Position = 0x50;
            if (bin.ReadUInt32() != 0)
            {
                uint offs1 = bin.ReadUInt32();
                uint offs2 = bin.ReadUInt32();
                bin.BaseStream.Position = offs1;
                t.GetTexture(Name_IcnBig).tex = ImageEncoding.getImage(bin, 9);
                bin.BaseStream.Position = offs2;
                t.GetTexture(Name_IcnSmall).tex = ImageEncoding.getImage(bin, 10);
            }
            #endregion
            #region ReadColorData
            for (int i = 0; i < ColorFieldsOffsets.Length; i++)
            {
                int off = ColorFieldsOffsets[i];
                if (off == 0x00) continue; //ignore if 0x00 because it's second part of 0x6C

                bin.BaseStream.Position = off;
                if (bin.ReadInt32() != 1) continue;
                t.ColorFields[i].ReadFromStream(bin);
                if (off == 0x6C) t.ColorFields[i + 1].ReadFromStream(bin);
            }
            #endregion

            return t;
        }

        public byte[] MakeTheme()
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bin = new BinaryWriter(mem);

            #region WriteHeader
            bin.Write(1);
            bin.Write((byte)0);
            bin.Write((byte)(BGM ? 1 : 0));
            bin.Write(0); 
            bin.Write((short)0); 
            bin.Write(TopScreenDrawType);
            bin.Write(TopScreenFrameType);
            bin.Write(0);
            bin.Write(0);
            bin.Write(0);
            bin.Write(BotScreenDrawType);
            bin.Write(BotScreenFrameType); //0x24
            bin.Write(0);
            bin.Write(0);
            bin.Write(0);
            bin.Write(0);
            bin.Write(0);
            bool writeTex78 = textures[2].tex != null && textures[3].tex != null;
            bin.Write((int)(writeTex78 ? 1 : 0));
            bin.Write(0);
            bin.Write(0);
            bin.Write(0);
            bin.Write(0);
            bool writeTex910 = textures[4].tex != null && textures[5].tex != null;
            bin.Write((int)(writeTex910 ? 1 : 0));
            bin.Write(0);
            bin.Write(0);
            for (int i = 0; i < 27; i++) { bin.Write(0); }
            #endregion

            #region WriteColors
            for (int i = 0; i < ColorFieldsOffsets.Length; i++)
            {
                if (!ColorFields[i].IsEnabled || ColorFieldsOffsets[i] == 0) continue;
                uint offset = (uint)bin.BaseStream.Position;
                bin.Write(ColorFields[i].GetData());
                uint NextOffset = (uint)bin.BaseStream.Position;
                bin.BaseStream.Position = ColorFieldsOffsets[i];
                bin.Write(1); //flag enabled
                bin.Write(offset);
                if (ColorFieldsOffsets[i] == 0x6C) bin.Write(NextOffset); //the second block will be written here
                bin.BaseStream.Position = NextOffset;
                if (ColorFieldsOffsets[i] == 0x6C)
                {
                    bin.Write(ColorFields[i + 1].GetData());
                }
            }
            #endregion
            #region writeImages
            //top 
            {
                if (textures[0].tex == null) textures[0].tex = ImageEncoding.WhiteImage(TopScreenImageID);
                uint offset = (uint)bin.BaseStream.Position;
                bin.Write(ImageEncoding.bitmapToRawImg(textures[0].tex, TopScreenImageID));
                uint NextOffset = (uint)bin.BaseStream.Position;
                bin.BaseStream.Position = 0x18;
                bin.Write(offset);
                bin.BaseStream.Position = NextOffset;
            }

            {
                //bottom screen
                if (textures[1].tex == null) textures[1].tex = ImageEncoding.WhiteImage(BotScreenImageID);
                uint offset = (uint)bin.BaseStream.Position;
                bin.Write(ImageEncoding.bitmapToRawImg(textures[1].tex, BotScreenImageID));
                uint NextOffset = (uint)bin.BaseStream.Position;
                bin.BaseStream.Position = 0x28;
                bin.Write(offset);
                bin.BaseStream.Position = NextOffset;
            }

            if (writeTex78)
            {
                //FolderTextures
                uint offset = (uint)bin.BaseStream.Position;
                bin.Write(ImageEncoding.bitmapToRawImg(textures[2].tex, 7));
                uint NextOffset = (uint)bin.BaseStream.Position;
                bin.BaseStream.Position = 0x40;
                bin.Write(offset);
                bin.BaseStream.Position = NextOffset;

                offset = (uint)bin.BaseStream.Position;
                bin.Write(ImageEncoding.bitmapToRawImg(textures[3].tex, 8));
                NextOffset = (uint)bin.BaseStream.Position;
                bin.BaseStream.Position = 0x44;
                bin.Write(offset);
                bin.BaseStream.Position = NextOffset;
            }

            if (writeTex910)
            {
                //Icon textures
                uint offset = (uint)bin.BaseStream.Position;
                bin.Write(ImageEncoding.bitmapToRawImg(textures[4].tex, 9));
                uint NextOffset = (uint)bin.BaseStream.Position;
                bin.BaseStream.Position = 0x54;
                bin.Write(offset);
                bin.BaseStream.Position = NextOffset;

                offset = (uint)bin.BaseStream.Position;
                bin.Write(ImageEncoding.bitmapToRawImg(textures[5].tex, 10));
                NextOffset = (uint)bin.BaseStream.Position;
                bin.BaseStream.Position = 0x58;
                bin.Write(offset);
                bin.BaseStream.Position = NextOffset;
            }
            #endregion

            while ((bin.BaseStream.Position % 8) != 0) bin.Write((byte)0x00); //padding
            return mem.ToArray();
        }

        public ImageEncoding.ImageType TopImageType { get { return ImageEncoding.ImageTypesByID[TopScreenImageID]; } }
        public ImageEncoding.ImageType BotImageType { get { return ImageEncoding.ImageTypesByID[BotScreenImageID]; } }

        public int TopScreenImageID { get { if (TopScreenFrameType == 0 || TopScreenFrameType == 3) return 1; else return 0; } }
        public int BotScreenImageID { get {
                if (BotScreenFrameType == 0 || BotScreenFrameType == 3) return 4;
                else if (BotScreenFrameType == 2 || BotScreenFrameType == 4) return 3;
                else return 2;
            } }

        public Texture GetTexture(string name)
        {
            foreach (var t in textures) if (t.name == name) return t;
            return null;
        }

        public int NameToImageID(string name)
        {
            switch (name)
            {
                case Name_TopScr:
                    return TopScreenImageID;
                case Name_BotScr:
                    return BotScreenImageID;
                case Name_Folder:
                    return 7;
                case Name_FolderOpn:
                    return 8;
                case Name_IcnBig:
                    return 9;
                case Name_IcnSmall:
                    return 10;
            }
            return -1;
        }

        public class Texture { public ImageData tex; public readonly string name; public Texture(string _name) { name = _name; tex = null; } }

        public class ColorField
        {
            public bool IsEnabled = false;
            byte[] data;
            int[] Offsets = null;
            public string ID;
            public Color[] colors;

            public ColorField(string _id,int byteCount, int[] _Offsets = null)
            {
                IsEnabled = false;
                ID = _id;
                Offsets = _Offsets;
                colors = new Color[_Offsets == null ? byteCount/3 : _Offsets.Length];
                data = new byte[byteCount];
            }

            public void ReadFromStream(BinaryReader bin)
            {
                IsEnabled = true;

                uint InFileOffset = bin.ReadUInt32();
                uint currentPos = (uint)bin.BaseStream.Position;
                bin.BaseStream.Position = InFileOffset;
                bin.Read(data, 0, data.Length);
                bin.BaseStream.Position = currentPos;

                if (Offsets != null)
                {
                    for (int i = 0; i < Offsets.Length; i++)
                    {
                        int off = Offsets[i];
                        colors[i] = new Color { A = 0xFF, R = data[off++], G = data[off++], B = data[off] };
                    }
                }
                else
                {
                    int off = 0;
                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = new Color { A = 0xFF, R = data[off++], G = data[off++], B = data[off++] };
                    }
                }
            }

            public byte[] GetData()
            {
                if (Offsets != null)
                {
                    for (int i = 0; i < Offsets.Length; i++)
                    {
                        int off = Offsets[i];
                        data[off++] = colors[i].R;
                        data[off++] = colors[i].G;
                        data[off] = colors[i].B;
                    }
                }
                else
                {
                    int off = 0;
                    for (int i = 0; i < colors.Length; i++)
                    {
                        data[off++] = colors[i].R;
                        data[off++] = colors[i].G;
                        data[off++] = colors[i].B;
                    }
                }
                return data;
            }
        }
    }

    public static class ImageEncoding
    {
        public enum Encoding : int //the int value is the number of bytes per pixel
        {
            A8 = 1,
            RGB565 = 2,
            BGR888 = 3
        }

        public struct Size { public int x, y; } //width and height

        public struct ImageType { public Encoding e; public Size s; public int length { get { return s.x * s.y * (int)e; } } public Size ActualSize; }

        public static readonly ImageType[] ImageTypesByID = //From 3dbrew
            {
            new ImageType { e = Encoding.RGB565, s = new Size { x = 512, y = 256 } , ActualSize = new Size { x = 410, y = 240 } },
            new ImageType { e = Encoding.RGB565, s = new Size { x = 1024, y = 256 } , ActualSize = new Size { x = 1008, y = 240 }  },
            new ImageType { e = Encoding.RGB565, s = new Size { x = 512, y = 256 } , ActualSize = new Size { x = 320, y = 240 }  },
            new ImageType { e = Encoding.RGB565, s = new Size { x = 1024, y = 256 } , ActualSize = new Size { x = 960, y = 240 }  },
            new ImageType { e = Encoding.RGB565, s = new Size { x = 1024, y = 256 } , ActualSize = new Size { x = 1008, y = 240 }  },
            new ImageType { e = Encoding.A8, s = new Size { x = 64, y = 64 } , ActualSize = new Size { x = 64, y = 64 }  },
            new ImageType { e = Encoding.A8, s = new Size { x = 64, y = 64 }  , ActualSize = new Size { x = 64, y = 64 } },
            new ImageType { e = Encoding.BGR888, s = new Size { x = 128, y = 64 } , ActualSize = new Size { x = 74, y = 64 }  },
            new ImageType { e = Encoding.BGR888, s = new Size { x = 128, y = 64 } , ActualSize = new Size { x = 82, y = 64 }  },
            new ImageType { e = Encoding.BGR888, s = new Size { x = 64, y = 128 } , ActualSize = new Size { x = 36, y = 64 }  },
            new ImageType { e = Encoding.BGR888, s = new Size { x = 32, y = 64 } , ActualSize = new Size { x = 25, y = 50 }  },
        };

        public static ImageData WhiteImage(int ID)
        {
            var img = new ImageData((uint)ImageTypesByID[ID].s.x, (uint)ImageTypesByID[ID].s.y);
            for (int i = 0; i < img.Data.Length; i++) img.Data[i] = 0xFF;
            return img;
        }

        public static ImageData getImage(BinaryReader bin, int ID)
        {
            byte[] data = bin.ReadBytes(ImageTypesByID[ID].length);
            return getImage(data, ID);
        }

        //Binary math is used to make % and / faster
        public static ImageData getImage(byte[] data, int ID)
        {
            ImageType t = ImageTypesByID[ID];
            ImageData Image = new ImageData((uint)t.s.x, (uint)t.s.y);

            uint x = 0, y = 0;
            int p = t.s.x >> 3;
            if (p == 0) p = 1;

            if (!BinaryMath.IsPowerOfTwo(p)) throw new Exception("NOT POWER OF TWO");
            int plog = BinaryMath.Log2(p);
            BinaryReader dec_br = new BinaryReader(new MemoryStream(data));

            int red, blue, green;
            if (t.e == Encoding.RGB565)
            {
                uint i = 0;
                while (dec_br.BaseStream.Position < dec_br.BaseStream.Length)
                {
                    uint pix = dec_br.ReadUInt16();

                    d2xy(i.FastMod(64), out x, out y);
                    uint tile = i >> 6;
                    // Shift Tile Coordinate into Tilemap
                    x += (uint)tile.FastMod(p) * 8;
                    y += (uint)tile.FastDiv(plog) * 8;
                    red = (byte)((pix >> 11) & 0x1f) * 8;
                    green = (byte)(((pix >> 5) & 0x3f)) * 4;
                    blue = (byte)((pix) & 0x1f) * 8;
                    Image.SetPixel(x, y, red, green, blue, 0xFF);
                    i++;
                }
            }
            else if (t.e == Encoding.BGR888)
            {
                uint i = 0;
                while (dec_br.BaseStream.Position < dec_br.BaseStream.Length)
                {
                    d2xy(i.FastMod(64), out x, out y);
                    uint tile = i >> 6;
                    // Shift Tile Coordinate into Tilemap
                    x += (uint)tile.FastMod(p) * 8;
                    y += (uint)tile.FastDiv(plog) * 8;
                    blue = dec_br.ReadByte();
                    green = dec_br.ReadByte();
                    red = dec_br.ReadByte();
                    Image.SetPixel(x, y, red, green, blue, 0xFF);
                    i++;
                }
            }
            else if (t.e == Encoding.A8)
            {
                uint i = 0;
                while (dec_br.BaseStream.Position < dec_br.BaseStream.Length)
                {
                    d2xy(i.FastMod(64), out x, out y);
                    uint tile = i >> 6;
                    // Shift Tile Coordinate into Tilemap
                    x += (uint)tile.FastMod(p) * 8;
                    y += (uint)tile.FastDiv(plog) * 8;
                    byte col = dec_br.ReadByte();
                    Image.SetPixel(x, y, col, col, col, 0xFF);
                    i++;
                }
            }

            return Image;
        }

        public static byte[] bitmapToRawImg(ImageData img, int ID)
        {
            ImageType t = ImageTypesByID[ID];
            int w = (int)img.Width;
            int h = (int)img.Height;
            List<byte> array = new List<byte>(w * h * (int)t.e);
            //w = h = Math.Max(nlpo2(w), nlpo2(h));
            uint x = 0, y = 0;
            Color c;
            int p = gcm(w, 8) >> 3;
            if (p == 0) p = 1;
            if (!BinaryMath.IsPowerOfTwo(p)) throw new Exception("NOT POWER OF TWO");
            int plog = BinaryMath.Log2(p);
            for (uint i = 0; i < w * h; i++)
            {
                d2xy(i.FastMod(64), out x, out y);
                uint tile = i >> 6;
                x += (uint)tile.FastMod(p) * 8;
                y += (uint)tile.FastDiv(plog) * 8;

                c = img.GetPixel(x, y);
                //if (c.A == 0) c = new Color { A = 0, R = 0xFF, G = 0xFF, B = 0xFF };
                if (t.e == Encoding.RGB565)
                {
                    uint val = (uint)((c.R >> 3) & 0x1f) << 11;
                    val += (uint)(((c.G >> 2) & 0x3f) << 5);
                    val += (uint)((c.B >> 3) & 0x1f);
                    array.Add((byte)(val & 0xFF));
                    array.Add((byte)((val >> 8) & 0xFF));
                }
                else if (t.e == Encoding.BGR888)
                {
                    array.AddRange(new byte[3] { c.B, c.G, c.R });
                }
                else if (t.e == Encoding.A8)
                {
                    array.Add((byte)c.R);
                }
            }

            return array.ToArray();
        }

        /// <summary>
        /// Greatest common multiple (to round up)
        /// </summary>
        /// <param name="n">Number to round-up.</param>
        /// <param name="m">Multiple to round-up to.</param>
        /// <returns>Rounded up number.</returns>
        private static int gcm(int n, int m)
        {
            return ((n + m - 1) / m) * m;
        }
        /// <summary>
        /// Next Largest Power of 2
        /// </summary>
        /// <param name="x">Input to round up to next 2^n</param>
        /// <returns>2^n > x && x > 2^(n-1) </returns>
        private static int nlpo2(int x)
        {
            x--; // comment out to always take the next biggest power of two, even if x is already a power of two
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (x + 1);
        }
        // Morton Translation
        /// <summary>
        /// Combines X/Y Coordinates to a decimal ordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static uint xy2d(uint x, uint y)
        {
            x &= 0x0000ffff;
            y &= 0x0000ffff;
            x |= (x << 8);
            y |= (y << 8);
            x &= 0x00ff00ff;
            y &= 0x00ff00ff;
            x |= (x << 4);
            y |= (y << 4);
            x &= 0x0f0f0f0f;
            y &= 0x0f0f0f0f;
            x |= (x << 2);
            y |= (y << 2);
            x &= 0x33333333;
            y &= 0x33333333;
            x |= (x << 1);
            y |= (y << 1);
            x &= 0x55555555;
            y &= 0x55555555;
            return x | (y << 1);
        }
        /// <summary>
        /// Decimal Ordinate In to X / Y Coordinate Out
        /// </summary>
        /// <param name="d">Loop integer which will be decoded to X/Y</param>
        /// <param name="x">Output X coordinate</param>
        /// <param name="y">Output Y coordinate</param>
        private static void d2xy(uint d, out uint x, out uint y)
        {
            x = d;
            y = (x >> 1);
            x &= 0x55555555;
            y &= 0x55555555;
            x |= (x >> 1);
            y |= (y >> 1);
            x &= 0x33333333;
            y &= 0x33333333;
            x |= (x >> 2);
            y |= (y >> 2);
            x &= 0x0f0f0f0f;
            y &= 0x0f0f0f0f;
            x |= (x >> 4);
            y |= (y >> 4);
            x &= 0x00ff00ff;
            y &= 0x00ff00ff;
            x |= (x >> 8);
            y |= (y >> 8);
            x &= 0x0000ffff;
            y &= 0x0000ffff;
        }
    }

    public static class ColorFlagsDescription
    {
        public struct ColorFieldDescriptor
        {
            public string ID;
            public string name;
            public string[] colors;

            public string ColorAt(int index)
            {
                if (colors == null || index < 0 || index >= colors.Length) return "unknown";
                else return colors[index];
            }
        }

        static readonly ColorFieldDescriptor[] descriptors = new ColorFieldDescriptor[]
        {
            new ColorFieldDescriptor{ ID = "____unknownField", name = "unknown", colors = new string[] { } },
            new ColorFieldDescriptor{ ID = "Cursor", name = "Cursor", colors = new string[] {"Shadow", "Main color", "Glow" } },
            new ColorFieldDescriptor{ ID = "3D-Folder", name = "3D Folder", colors = new string[] { "Main color", "Light color", "Highlight", "Shadow"} },
            new ColorFieldDescriptor{ ID = "DsiWare-icon", name = "DsiWare icon", colors = new string[] { "Bottom shadow", "Main color", "Highlight" } },
            new ColorFieldDescriptor{ ID = "ArrowButtons", name = "Bottom screen arrow buttons", colors = new string[] { "Shading" , "Main color", "Glow" } },
            new ColorFieldDescriptor{ ID = "Arrows", name = "Bottom screen arrows", colors = new string[] { "Border", "Unpressed color", "Pressed color"} },
            new ColorFieldDescriptor{ ID = "BottomScreenOpenButton", name = "Launch button", colors = new string[] { "Pressed color", "Unpressed color", "Border", "Text shadow", "Unpressed text color", "Pressed text color"} },
            new ColorFieldDescriptor{ ID = "BottomScreenCloseButton", name = "Close button", colors = new string[] { "Pressed color", "Unpressed color", "Border", "Text shadow", "Unpressed text color", "Pressed text color" } },
            new ColorFieldDescriptor{ ID = "BottomScreenTitleTextColor", name = "Game text", colors = new string[] { "Background color", "Text color"} },
            new ColorFieldDescriptor{ ID = "FolderBackground", name = "Folder Background", colors = new string[] { "Highlight", "Background color", "Border" } },
            new ColorFieldDescriptor{ ID = "BottomScreenOverlay", name = "Bottom screen overlay", colors = new string[] { "Borders", "Background color", "Highlight", "Icon gradient color (bottom)", "Icon gradient color (top)" } },
            new ColorFieldDescriptor{ ID = "TopScreenOverlay", name = "Top screen overlay", colors = new string[] { "Background color", "Text color"} },
            new ColorFieldDescriptor{ ID = "DemoMessage", name = "Demo message", colors = new string[] { "Background color", "Text color" } }
        };

        public static ColorFieldDescriptor getByID(string ID)
        {
            foreach (var d in descriptors) if (d.ID == ID) return d;
            return descriptors[0];
        }
    }
}
