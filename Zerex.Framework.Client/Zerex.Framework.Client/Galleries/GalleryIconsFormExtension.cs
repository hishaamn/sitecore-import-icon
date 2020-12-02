using System;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Resources;
using Sitecore.Shell.Applications.ContentManager.Galleries;
using Sitecore.Shell.Applications.ContentManager.Galleries.Icons;
using Sitecore.Shell.Web.UI.WebControls;
using Sitecore.StringExtensions;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Xml;
using Zerex.Framework.Client.Controls;

namespace Zerex.Framework.Client.Galleries
{
    public class GalleryIconsFormExtension : GalleryForm
    {
        protected Scrollbox Icons;

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull((object)message, nameof(message));
            this.Invoke(message, true);
            message.CancelBubble = true;
            message.CancelDispatch = true;
        }

        protected override void OnLoad(EventArgs args)
        {
            base.OnLoad(args);

            if (Context.ClientPage.IsEvent)
            {
                return;
            }

            var configNodes = Factory.GetConfigNodes("icons/collection");
            
            var iconList = Registry.GetString("/Current_User/RecentIcons");
            
            if (iconList.Length > 0)
            {
                this.GetIcons(Translate.Text("Recently Used Icons"), iconList);
            }

            var customFeaturedIcons = Factory.GetDatabase("master").GetItem("/sitecore/system/Modules/Custom Icons")
                .GetChildren().Where(w => w.Fields["Is Featured Icon"].Value.Equals("1") && !w.Fields["Featured Icons"].Value.IsNullOrEmpty()).ToList();

            foreach (var customFeaturedIcon in customFeaturedIcons)
            {
                var customIconList = customFeaturedIcon.Fields["Featured Icons"].Value;

                this.GetIcons(customFeaturedIcon.Fields["Header"].Value, customIconList);
            }

            foreach (XmlNode node in configNodes)
            {
                this.GetIcons(XmlUtil.GetAttribute("name", node), StringUtil.GetString(node.InnerText));
            }
        }

        private void GetIcons(string header, string iconList)
        {
            var border1 = new Border
            {
                Class = "scMenuHeader"
            };

            this.Icons.Controls.Add(border1);

            border1.InnerHtml = Translate.Text(header);

            var border3 = new Border();

            this.Icons.Controls.Add(border3);

            border3.Class = "scMenuItem";

            foreach (var str1 in new ListString(iconList))
            {
                var updatedString = str1.Replace("\r\n    ", string.Empty);
                
                if (!updatedString.IsNullOrEmpty())
                {
                    var source = Images.GetThemedImageSource(updatedString, ImageDimension.id32x32);
                            
                    var isExists = File.Exists(FileUtil.MapPath(source));

                    if (isExists)
                    {
                        var largeIconButton = new LargeIconButtonExtension();

                        Context.ClientPage.AddControl(border3, largeIconButton);

                        largeIconButton.Icon = source;

                        var str2 = WebUtil.SafeEncode(str1);

                        largeIconButton.Command = "item:seticon(icon=" + str2.Trim() + ")";
                    }
                }
            }
        }
    }
}
