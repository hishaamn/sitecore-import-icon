using Sitecore;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Resources;
using Sitecore.Shell.Applications.ContentManager.Dialogs.SetIcon;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web.UI;
using Sitecore.Data;
using Sitecore.SecurityModel;

namespace Zerex.Framework.Client.Dialogs
{
    public class IconFeaturedForm : DialogForm
    {
        private UrlHandle _urlHandle;

        protected UrlHandle Handle => _urlHandle ?? (_urlHandle = UrlHandle.Get(new UrlString(WebUtil.GetRawUrl()), "hdl", false));

        protected Scrollbox CustomIconList { get; set; }

        protected Listview IconList { get; set; }

        protected Edit IconFile { get; set; }

        protected Literal IconFileName { get; set; }

        protected override void OnLoad(EventArgs args)
        {
            base.OnLoad(args);

            if (Context.ClientPage.IsEvent)
            {
                return;
            }

            var iconFilename = this.Handle["iconfilename"];

            IconFileName.Text = iconFilename;

            RenderIcons(CustomIconList, iconFilename);
        }

        private static void RenderIcons(Scrollbox scrollbox, string prefix)
        {
            Assert.ArgumentNotNull(scrollbox, nameof(scrollbox));
            Assert.ArgumentNotNullOrEmpty(prefix, nameof(prefix));

            var filename = GetFilename(prefix);

            var str = Path.ChangeExtension(filename, ".html");

            if (!File.Exists(filename) || !File.Exists(str))
            {
                DrawIcons(prefix, filename, str);
            }

            var output = new HtmlTextWriter(new StringWriter());

            output.Write(FileUtil.ReadFromFile(str));

            WriteImageTag(filename, prefix, output);

            scrollbox.InnerHtml = output.InnerWriter.ToString();
        }

        public void AddItem()
        {
            var selectedIcon = IconFile.Value;

            Context.ClientPage.ClientResponse.DisableOutput();

            try
            {
                var id = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("ListItem");

                var listviewItem = new ListviewItem
                {
                    ID = id,
                    Icon = selectedIcon,
                    Header = selectedIcon,
                    Value = selectedIcon
                };

                Context.ClientPage.AddControl(IconList, listviewItem);
            }
            finally
            {
                Context.ClientPage.ClientResponse.EnableOutput();
            }
            Context.ClientPage.ClientResponse.Refresh(IconList);
        }

        public void Remove()
        {
            var listviewItemArray = IconList.SelectedItems;

            foreach (var listviewItem in listviewItemArray)
            {
                if (listviewItem != null)
                {
                    listviewItem.Parent.Controls.Remove(listviewItem);

                    Context.ClientPage.ClientResponse.Remove(listviewItem.ID, true);
                }
            }
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            var featuredIcons = this.IconList.Items;

            var iconPaths = new StringBuilder();

            foreach (var featuredIcon in featuredIcons)
            {
                iconPaths.Append($"{featuredIcon.Value}|");
            }

            var id = this.Handle["iconItemId"];

            var iconItem = Factory.GetDatabase("master").GetItem(new ID(id));

            using (new SecurityDisabler())
            {
                iconItem.Editing.BeginEdit();
                iconItem.Fields["Featured Icons"].Value = iconPaths.ToString();
                iconItem.Editing.EndEdit();
            }

            base.OnOK(sender, args);
        }

        private static string GetFilename(string prefix)
        {
            return FileUtil.MapPath(FileUtil.MakePath(TempFolder.Folder, "icons_" + prefix + ".png"));
        }

        private static void DrawIcons(string prefix, string img, string area)
        {
            var files = GetFiles(prefix);

            var num1 = files.Length;

            if (num1 == 0)
            {
                num1 = 1;
            }

            var height = (num1 / 24 + (num1 % 24 == 0 ? 0 : 1)) * 40;

            using (var bitmap1 = new Bitmap(960, height, PixelFormat.Format32bppArgb))
            {
                if (prefix == "OfficeWhite")
                {
                    Graphics.FromImage(bitmap1).FillRectangle(Brushes.DarkGray, 0, 0, 960, height);
                }

                var htmlTextWriter = new HtmlTextWriter(new StringWriter());

                htmlTextWriter.WriteLine("<map name=\"" + prefix + "\">");

                var relativeIconPath = prefix + "/32x32/";

                var num2 = 0;

                using (Graphics graphics = Graphics.FromImage(bitmap1))
                {
                    foreach (string path in files)
                    {
                        int num3 = num2 % 24;
                        int num4 = num2 / 24;
                        string themedImageSource = Images.GetThemedImageSource(relativeIconPath + path, ImageDimension.id32x32);
                        try
                        {
                            using (Bitmap bitmap2 = Settings.Icons.UseZippedIcons ? new Bitmap(ZippedIcon.GetStream(relativeIconPath + path, ZippedIcon.GetZipFile(relativeIconPath))) : new Bitmap(FileUtil.MapPath(themedImageSource)))
                            { graphics.DrawImage(bitmap2, num3 * 40 + 4, num4 * 40 + 4, 32, 32); }
                            string str1 = $"{ (num3 * 40 + 4)},{ (num4 * 40 + 4)},{ (num3 * 40 + 36)},{ (num4 * 40 + 36)}";
                            string str2 = StringUtil.Capitalize(Path.GetFileNameWithoutExtension(path)?.Replace("_", " "));
                            htmlTextWriter.WriteLine("<area shape=\"rect\" coords=\"{0}\" href=\"#\" alt=\"{1}\" sc_path=\"{2}\"/>", str1, str2, (relativeIconPath + path));
                            ++num2;
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("Unable to open icon " + themedImageSource, ex, typeof(SetIconForm));
                        }
                    }
                }
                htmlTextWriter.WriteLine("</map>");
                FileUtil.WriteToFile(area, htmlTextWriter.InnerWriter.ToString());
                bitmap1.Save(img, ImageFormat.Png);
            }
        }

        private static string[] GetFiles(string prefix)
        {
            Assert.ArgumentNotNullOrEmpty(prefix, nameof(prefix));
            if (!Settings.Icons.UseZippedIcons)
                return GetFolderFiles(prefix);
            return GetZippedFiles(prefix);
        }

        private static string[] GetZippedFiles(string prefix)
        {
            return ZippedIcon.GetFiles(prefix, "/sitecore/shell/themes/standard/" + prefix + ".zip");
        }

        private static string[] GetFolderFiles(string prefix)
        {
            string[] files = Directory.GetFiles(FileUtil.MapPath("/sitecore/shell/themes/standard/" + prefix + "/32x32"));
            for (int index = 0; index < files.Length; ++index)
                files[index] = Path.GetFileName(files[index]);
            return files;
        }

        private static void WriteImageTag(string img, string prefix, HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(img, nameof(img));
            Assert.ArgumentNotNull(prefix, nameof(prefix));
            Assert.ArgumentNotNull(output, nameof(output));
            ImageBuilder imageBuilder;

            using (Bitmap bitmap = new Bitmap(img))
            {
                imageBuilder = new ImageBuilder()
                {
                    Src = FileUtil.UnmapPath(img),
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    Usemap = "#" + prefix
                };
            }

            output.WriteLine(imageBuilder.ToString());
        }
    }
}
