using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Sitecore;
using Sitecore.Resources;
using Sitecore.Shell.Web.UI.WebControls;
using Sitecore.Web.UI;

namespace Zerex.Framework.Client.Controls
{
    public class LargeIconButtonExtension : LargeIconButton
    {
        protected override void Render(HtmlTextWriter output)
        {
            if (!this.Visible)
                return;
            string str1 = StringUtil.GetString(this.ToolTip);
            string str2 = str1.Length > 0 ? " title=\"" + str1 + "\"" : string.Empty;
            if (!this.Enabled)
            {
                output.Write("<span class=\"scRibbonToolbarLargeIconButtonDisabled\"" + str2 + ">");
                output.Write("<span class=\"scRibbonToolbarLargeIconButtonIconGrayed\">");
                output.Write(new ImageBuilder()
                {
                    Src = Images.GetThemedImageSource(this.Icon, ImageDimension.id32x32),
                    Disabled = true,
                    Class = "scRibbonToolbarLargeIconButtonIconDisabled",
                    Alt = str1
                }.ToString());
                output.Write("</span>");
                output.Write("</span>");
            }
            else
            {
                string str3 = this.ID == null || this.ID.Length <= 0 ? string.Empty : " id=\"" + this.ID + "\"";
                string str4 = this.AccessKey == null || this.AccessKey.Length <= 0 ? string.Empty : " accesskey=\"" + this.AccessKey + "\"";
                output.Write("<a" + str3 + " href=\"#\" class=\"scRibbonToolbarLargeIconButton" + (this.Down ? "Down" : string.Empty) + "\"" + str2 + str4 + " onclick=\"javascript:return scForm.invoke('" + this.Command + "')\">");
                output.Write(new ImageBuilder()
                {
                    Src = Images.GetThemedImageSource(this.Icon, ImageDimension.id32x32),
                    Class = "scRibbonToolbarLargeIconButtonIcon",
                    Alt = str1
                }.ToString());
                output.Write("</a>");
            }
        }
    }
}
