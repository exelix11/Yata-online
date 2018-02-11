using Bridge.Html5;
using PuyoTools.Modules.Compression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Bridge;
using System.Text.RegularExpressions;
// If building RELEASE manually change scripts names in index.html (bridge.js => bridge.min.js)
namespace YataOnline
{
    public class MainClass
    {
        static Theme t = null;
        static HTMLDivElement ImagesDiv = null;
        static HTMLDivElement ColorsDiv = null;
        static HTMLDivElement loader = null;
        static HTMLParagraphElement LoaderText = null;
        static HTMLSelectElement TopFrameType = null;
        static HTMLSelectElement BotFrameType = null;
        const float AppVersion = 1.3f;

        public static readonly string[] loadingFaces = new string[] {"(ﾉ≧∀≦)ﾉ・‥…━━━★","o͡͡͡╮༼ ಠДಠ ༽╭o͡͡͡━☆ﾟ.*･｡ﾟ",
            "༼∩✿ل͜✿༽⊃━☆ﾟ. * ･ ｡ﾟ","༼(∩ ͡°╭͜ʖ╮͡ ͡°)༽⊃━☆ﾟ. * ･ ｡ﾟ",
            "ᕦ( ✿ ⊙ ͜ʖ ⊙ ✿ )━☆ﾟ.*･｡ﾟ","(∩｀-´)⊃━☆ﾟ.*･｡ﾟ",
            "༼∩☉ل͜☉༽⊃━☆ﾟ. * ･ ｡ﾟ","╰( ͡° ͜ʖ ͡° )つ──☆*:・ﾟ",
            "(∩ ͡° ͜ʖ ͡°)⊃━☆ﾟ","੭•̀ω•́)੭̸*✩⁺˚",
            "(੭ˊ͈ ꒵ˋ͈)੭̸*✧⁺˚","✩°｡⋆⸜(ू｡•ω•｡)",
            "ヽ༼ຈل͜ຈ༽⊃─☆*:・ﾟ","╰(•̀ 3 •́)━☆ﾟ.*･｡ﾟ",
            "(*’▽’)ノ＾—==ΞΞΞ☆","(੭•̀ω•́)੭̸*✩⁺˚",
            "(っ・ω・）っ≡≡≡≡≡≡☆",". * ･ ｡ﾟ☆━੧༼ •́ ヮ •̀ ༽୨",
            "༼∩ •́ ヮ •̀ ༽⊃━☆ﾟ. * ･ ｡ﾟ","(⊃｡•́‿•̀｡)⊃━☆ﾟ.*･｡ﾟ",
            "★≡≡＼（`△´＼）","( ◔ ౪◔)⊃━☆ﾟ.*・",
            "彡ﾟ◉ω◉ )つー☆*","(☆_・)・‥…━━━★",
            "(つ◕౪◕)つ━☆ﾟ.*･｡ﾟ","(つ˵•́ω•̀˵)つ━☆ﾟ.*･｡ﾟ҉̛༽̨҉҉ﾉ",
            "✩°｡⋆⸜(ू˙꒳​˙ )","╰( ⁰ ਊ ⁰ )━☆ﾟ.*･｡ﾟ"}; //Actually i think the strings could still be changed, but it works

        static void DoActionWithloading(Action action)
        {
            LoaderText.TextContent = loadingFaces[new Random().Next(0, loadingFaces.Length)];
            loader.Style.Display = "";
            Window.SetTimeout(()=> { action(); loader.Style.Display = "none"; }, 100);
        }

        static void OnLoaded()
        {
            Document.GetElementById<HTMLParagraphElement>("pVersion").TextContent = "Ver " + AppVersion.ToString("N1");
            Document.GetElementById<HTMLDivElement>("javascriptWarn").Hidden = true;
            
            ImagesDiv = Document.GetElementById<HTMLDivElement>("PlaceImages");
            ColorsDiv = Document.GetElementById<HTMLDivElement>("PlaceColorEditors");
            loader = Document.GetElementById<HTMLDivElement>("loaderDiv");
            LoaderText = Document.GetElementById<HTMLParagraphElement>("LoadingText");

            TopFrameType = Document.GetElementById<HTMLSelectElement>("Top-screen-TYP");
            BotFrameType = Document.GetElementById<HTMLSelectElement>("Bottom-screen-TYP");
            TopFrameType.OnChange += ListBoxChanged;
            BotFrameType.OnChange += ListBoxChanged;

            Window.OnResize += OnResized;

            string useragent = Window.Navigator.UserAgent.ToLower();
            if (useragent.Contains("edge"))
            {
                Document.GetElementById<HTMLDivElement>("EdgeWarn").Hidden = false;
            }
            else if (useragent.Contains("msie") || useragent.Contains("trident"))
            {
                Document.GetElementById<HTMLDivElement>("interwebExplorerWarn").Hidden = false;
            }
        }

        private static void OnResized(Event arg)
        {
            if (t == null) return;
            HtmlCanvasFitSize(Document.GetElementById<HTMLCanvasElement>(Theme.Name_TopScr), t.TopImageType.s);
            HtmlCanvasFitSize(Document.GetElementById<HTMLCanvasElement>(Theme.Name_BotScr), t.BotImageType.s);
        }

        static void HtmlCanvasFitSize(HTMLCanvasElement em, ImageTool.Size OriginalSize)
        {
            if (em == null) return;
            var width = Window.InnerWidth - 60;

            if (width >= OriginalSize.x)
            {
                em.Style.Width = OriginalSize.x + "px";
                em.Style.Height = OriginalSize.y + "px";
            }
            else
            {
                double ratio = (double)OriginalSize.y / OriginalSize.x;
                int height = (int)(ratio * width);
                em.Style.Width = width + "px";
                em.Style.Height = height + "px";
            }
        }

        static void OpenThemeBtn()
        {
            Document.GetElementById<HTMLInputElement>("openThemeUploader").Click();
        }

        private static void ListBoxChanged(Event<HTMLSelectElement> arg)
        {
            ImageInputSenderID = arg.CurrentTarget.Id;
            ImageInputSenderID = ImageInputSenderID.Substring(0, ImageInputSenderID.Length - 4);

            var tex = t.GetTexture(ImageInputSenderID);
            t.TopScreenFrameType = (uint)TopFrameType.SelectedIndex;
            t.BotScreenFrameType = (uint)BotFrameType.SelectedIndex;

            if (t.TopScreenFrameType == 2) t.TopScreenFrameType = 1; //Invalid value fix            
            
            if (ImageInputSenderID == Theme.Name_TopScr && !(tex.tex.Width == t.TopImageType.s.x && tex.tex.Height == t.TopImageType.s.y))
            {
                tex.tex = ImageTool.WhiteImage(t.TopScreenImageID);
                UpdateShownImg(t.TopImageType, ImageInputSenderID, tex.tex);
                Document.GetElementById<HTMLLabelElement>(ImageInputSenderID + "-LBL").TextContent = ImageInputSenderID.Replace('-', ' ') + " (" + t.TopImageType.s.x.ToString() + "x" + t.TopImageType.s.y.ToString() + ")";
            }
            else if (ImageInputSenderID == Theme.Name_BotScr && !(tex.tex.Width == t.BotImageType.s.x && tex.tex.Height == t.BotImageType.s.y))
            {
                tex.tex = ImageTool.WhiteImage(t.BotScreenImageID);
                UpdateShownImg(t.BotImageType, ImageInputSenderID, tex.tex);
                Document.GetElementById<HTMLLabelElement>(ImageInputSenderID + "-LBL").TextContent = ImageInputSenderID.Replace('-', ' ') + " (" + t.BotImageType.s.x.ToString() + "x" + t.BotImageType.s.y.ToString() + ")";
            }
            OnResized(null);
        }
        
        static void LoadFile(Uint8Array arr) //entrypoint, called from JsFileRead
        {
            LoadThemeFromByteArr(arr.ToArray());            
        }

        static void LoadThemeFromByteArr(byte[] data)
        {
            DoActionWithloading(() =>
            {
                Lz11Compression c = new Lz11Compression();
                MemoryStream inData = new MemoryStream(data);
                MemoryStream decData = new MemoryStream();
                c.Decompress(inData, decData);
                decData.Position = 0;
                t = Theme.ReadTheme(decData);
                if (!(t.TopScreenDrawType == 3 && t.BotScreenDrawType == 3))
                {
                    Window.Alert("Theme draw type not supported");
                    t = null;
                    return;
                }
                UpdateThemeDIV();
            });
        }

        static void ExampleTheme() 
        {
            DoActionWithloading(() =>
            {
                t = Theme.EmptyTheme();
                UpdateThemeDIV();
            });
        }

        static void UpdateThemeDIV()
        {
            foreach (var c in ImagesDiv.ChildNodes) ImagesDiv.RemoveChild(c);
            foreach (var c in ColorsDiv.ChildNodes) ColorsDiv.RemoveChild(c);
            TopFrameType.SelectedIndex = (int)t.TopScreenFrameType;
            BotFrameType.SelectedIndex = (int)t.BotScreenFrameType;
            Document.GetElementById<HTMLInputElement>("EnableBGMchb").Checked = t.BGM;
            Document.GetElementById<HTMLDivElement>("LoadFile").Remove();
            foreach (var tex in t.textures) AddImageToPage(tex.tex, tex.name);
            foreach (var col in t.ColorFields) AddColorToPage(col);
            Document.GetElementById<HTMLDivElement>("EditFile").Hidden = false;
        }

        #region ImageEditing
        static string ImageInputSenderID = "";
        static void LoadImage(Uint8Array arr)
        {
            Document.GetElementById<HTMLInputElement>("ImageUpload").Value = ""; //Clear file to fix MS Edge bug
            if (arr.Length < 6)
            {
                Window.Alert("file too small");
                return;
            }
            if (!(
                (arr[0] == 0x89 && arr[1] == 0x50 && arr[2] == 0x4e && arr[3] == 0x47 && arr[4] == 0x0d) || //Png header
                (arr[0] == 0xFF && arr[1] == 0xD8 && arr[2] == 0xFF && (arr[3] == 0xE0 || arr[3] == 0xE1)) //JPEG arr[3] is 0xE1 if there is EXIF metadata attached
                ))
            {
                Window.Alert("unsupported file");
                return;
            }
            DoActionWithloading(() =>
            {
                HTMLCanvasElement tmpImg = new HTMLCanvasElement();
                if (arr[0] == 0x89) //check just first byte since the format was already checked
                {
                    Script.Write("PNG.load(arr,tmpImg)");
                }
                else if (arr[0] == 0xFF)
                {
                    Uint8Array decarr = new Uint8Array(0);
                    int wid = 0,hei = 0;
                    Script.Write("var decoded = JpegDecode(arr,true); wid = decoded.width;hei = decoded.height; decarr = decoded.data;");
                    tmpImg.Width = wid;
                    tmpImg.Height = hei;
                    ImageData img = new ImageData(new Uint8ClampedArray(decarr),(uint)wid, (uint)hei);
                    ((CanvasRenderingContext2D)tmpImg.GetContext("2d")).PutImageData(img, 0, 0);
                }
                ApplyImage(tmpImg);
            });
        }

        static void ApplyImage(HTMLCanvasElement tmpImg)
        {
            ImageTool.ImageType imgType = ImageTool.ImageTypesByID[t.NameToImageID(ImageInputSenderID)];
            HTMLCanvasElement FinalCanvas = new HTMLCanvasElement();
            FinalCanvas.Width = imgType.s.x;
            FinalCanvas.Height = imgType.s.y;

            if (tmpImg == null)
            {
                t.GetTexture(ImageInputSenderID).tex = null;
                var context = ((CanvasRenderingContext2D)FinalCanvas.GetContext("2d"));
                context.FillRect(0, 0, imgType.s.x, imgType.s.y);
            }
            else if (tmpImg.Width == imgType.ActualSize.x && tmpImg.Height == imgType.ActualSize.y)
            {
                var context = ((CanvasRenderingContext2D)FinalCanvas.GetContext("2d"));
                context.FillRect(0, 0, imgType.s.x, imgType.s.y);
                context.DrawImage(tmpImg, 0, 0);
            }
            else if (tmpImg.Width == imgType.s.x && tmpImg.Height == imgType.s.y) FinalCanvas = tmpImg;
            else
            {
                var context = ((CanvasRenderingContext2D)FinalCanvas.GetContext("2d"));
                context.FillRect(0, 0, imgType.s.x, imgType.s.y);
                context.DrawImage(tmpImg, 0, 0, imgType.ActualSize.x, imgType.ActualSize.y);
            }

            var img = ((CanvasRenderingContext2D)FinalCanvas.GetContext("2d")).GetImageData(0, 0, imgType.s.x, imgType.s.y);
            t.GetTexture(ImageInputSenderID).tex = tmpImg == null ? null : img;

            UpdateShownImg(imgType, ImageInputSenderID, img);
        }

        static void UpdateShownImg(ImageTool.ImageType imgType, string ID, ImageData img)
        {
            HTMLCanvasElement ShownImg = Document.GetElementById<HTMLCanvasElement>(ID);
            ShownImg.Width = imgType.s.x;
            ShownImg.Height = imgType.s.y;
            ((CanvasRenderingContext2D)ShownImg.GetContext("2d")).PutImageData(img, 0, 0);
        }

        static void AddImageToPage(ImageData tex,string Id)
        {
            HTMLDivElement div = new HTMLDivElement();
            div.ClassName = "col";
            div.Style.Padding = "5px";
            HTMLCanvasElement c = new HTMLCanvasElement();
            c.Id = Id;
            c.ClassName = "CustomCanvas";
            c.Style.BackgroundColor = "#FAFAFA";
            ImageTool.ImageType imgType = ImageTool.ImageTypesByID[t.NameToImageID(Id)];
            c.Height = imgType.s.y;
            c.Width = imgType.s.x;
            c.OnClick = ImageCanvasClick;
            c.Style.Margin = "auto";
            c.Style.Border = "black 3px solid";
            div.AppendChild(MakeImageLabel(Id, imgType.s.x.ToString() + "x" + imgType.s.y.ToString()));
            if (Id != Theme.Name_TopScr && Id != Theme.Name_BotScr)
            {
                div.AppendChild(MakeImageEnableCheckBox(Id, tex != null));
            }
            else HtmlCanvasFitSize(c,imgType.s);
            div.AppendChild(c);
            ImagesDiv.AppendChild(div);
            if (tex != null) ((CanvasRenderingContext2D)c.GetContext("2d")).PutImageData(tex, 0, 0);
        }

        static HTMLDivElement MakeImageLabel(string ID, string size)
        {
            HTMLLabelElement _a = new HTMLLabelElement();
            _a.TextContent = ID.Replace('-', ' ') + " (" + size + ")";
            _a.Id = ID + "-LBL";
            HTMLDivElement _div = new HTMLDivElement();
            _div.AppendChild(_a);
            return _div;
        }

        static HTMLDivElement MakeImageEnableCheckBox(string id, bool check)
        {
            HTMLDivElement container = new HTMLDivElement();
            container.ClassName = "form-check";
            HTMLInputElement chb = new HTMLInputElement();
            chb.Type = InputType.Checkbox;
            chb.Checked = check;
            chb.Id = id + "-CHB";
            chb.OnChange = chb_ImageEnable_changed;
            chb.ClassName = "form-check-input";
            container.AppendChild(chb);
            HTMLLabelElement lab = new HTMLLabelElement();
            lab.ClassName = "form-check-label";
            lab.TextContent = "Enabled";

            container.AppendChild(lab);
            return container;
        }

        private static void chb_ImageEnable_changed(Event<HTMLInputElement> arg)
        {
            ImageInputSenderID = arg.CurrentTarget.Id;
            ImageInputSenderID = ImageInputSenderID.Substring(0, ImageInputSenderID.Length - 4);
            ApplyImage(null);
        }

        private static void ImageCanvasClick(MouseEvent<HTMLCanvasElement> arg)
        {
            if (arg.Target.Id != Theme.Name_TopScr && arg.Target.Id != Theme.Name_BotScr)
            {
                HTMLInputElement chb = Document.GetElementById<HTMLInputElement>(arg.Target.Id + "-CHB");
                if (!chb.Checked)
                {
                    Window.Alert("This image is not enabled");
                    return;
                }
            }
            ImageInputSenderID = arg.Target.Id;
            Document.GetElementById<HTMLInputElement>("ImageUpload").Click();
        }
        #endregion
        #region ColorEditing
        static void AddColorToPage(Theme.ColorField c)
        {
            HTMLDivElement flagDiv = new HTMLDivElement();
            flagDiv.Id = c.name + "-COLDIV";
            flagDiv.ClassName = "form-check";
            
            HTMLInputElement chb = new HTMLInputElement();
            chb.Style.MarginTop = "7px";
            chb.Type = InputType.Checkbox;
            chb.Checked = c.IsEnabled;
            chb.Id = c.name + "-COLDIV-CHB";
            chb.ClassName = "form-check-input";
            flagDiv.AppendChild(chb);           
            
            HTMLLabelElement label = new HTMLLabelElement();
            label.TextContent = c.name;
            flagDiv.AppendChild(label);
            for (int i = 0; i < c.colors.Length; i++)
            {
                HTMLButtonElement colorbtn = new HTMLButtonElement();
                colorbtn.Id = c.name + "-COLBTN-" + i.ToString();
                colorbtn.Style.Border = "1px solid black";
                colorbtn.Style.MarginLeft = "5px";
                colorbtn.Style.MarginTop = "-5px";
                colorbtn.Style.Width = "20px";
                colorbtn.Style.Height = "20px";
                Script.Write("var picker = new jscolor(colorbtn);picker.valueElement= null;picker.fromRGB(c.colors[i].R,c.colors[i].G,c.colors[i].B);");
                colorbtn.TextContent = "";
                flagDiv.AppendChild(colorbtn);
            }

            ColorsDiv.AppendChild(flagDiv);
        }        

        static void ApplyColorsToTheme()
        {
            foreach (var field in t.ColorFields)
            {
                field.IsEnabled = Document.GetElementById<HTMLInputElement>(field.name + "-COLDIV-CHB").Checked;
                if (!field.IsEnabled) continue;
                for (int i = 0; i < field.colors.Length; i++)
                {
                    var lbl = Document.GetElementById<HTMLInputElement>(field.name + "-COLBTN-" + i.ToString());
                    field.colors[i].A = 0xff;
                    Regex rgx = new Regex(@"^rgb\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)$", RegexOptions.IgnoreCase); //regex magic
                    MatchCollection matches = rgx.Matches(lbl.Style.BackgroundColor);
                    Console.WriteLine(lbl.Style.BackgroundColor);
                    if (matches.Count == 1 && matches[0].Groups.Count == 4)
                    {
                        field.colors[i].R = byte.Parse(matches[0].Groups[1].Value);
                        field.colors[i].G = byte.Parse(matches[0].Groups[2].Value);
                        field.colors[i].B = byte.Parse(matches[0].Groups[3].Value);
                    }
                    else
                    {
                        Window.Alert("Error while parsing color flags, this could be related to you browser, color data might not be saved");
                        return;
                    }
                }
            }
        }
        #endregion

        static void SaveTheme()
        {
            if (!Window.Confirm("Save theme ?\r\nThis might take up to a minute, don't close this page")) return;
            HTMLInputElement bgm = Document.GetElementById<HTMLInputElement>("EnableBGMchb");
            t.BGM = bgm.Checked;

            t.TopScreenFrameType = (uint)TopFrameType.SelectedIndex;
            t.BotScreenFrameType = (uint)BotFrameType.SelectedIndex;

            ApplyColorsToTheme();

            DoActionWithloading(() =>
            {
                byte[] themedata = t.MakeTheme();
                Lz11Compression c = new Lz11Compression();
                MemoryStream inData = new MemoryStream(themedata);
                MemoryStream compData = new MemoryStream();

                if (Document.GetElementById<HTMLInputElement>("Fastbuild").Checked)
                    c.ArchiveNoCompression(inData, compData);
                else
                    c.Compress(inData, compData);

                Uint8Array dwn = new Uint8Array(compData.ToArray());
                Script.Write("downloadBlob(dwn,'Body_LZ.bin','application/octet-stream');");
            });
        }

        static void GoBack()
        {
            if (!Window.Confirm("Go back ?")) return;
            Document.Location.Reload();
        }
    }
}
