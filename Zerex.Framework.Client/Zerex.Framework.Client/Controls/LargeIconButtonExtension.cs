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
            {
                return;
            }

            var tooltip = StringUtil.GetString(this.ToolTip);

            var title = tooltip.Length > 0 ? " title=\"" + tooltip + "\"" : string.Empty;

            if (!this.Enabled)
            {
                output.Write("<span class=\"scRibbonToolbarLargeIconButtonDisabled\"" + title + ">");

                output.Write("<span class=\"scRibbonToolbarLargeIconButtonIconGrayed\">");

                output.Write(new ImageBuilder()
                {
                    Src = Images.GetThemedImageSource(this.Icon, ImageDimension.id32x32),
                    Disabled = true,
                    Class = "scRibbonToolbarLargeIconButtonIconDisabled",
                    Alt = tooltip
                }.ToString());

                output.Write("</span>");

                output.Write("</span>");
            }
            else
            {
                var id = this.ID == null || this.ID.Length <= 0 ? string.Empty : " id=\"" + this.ID + "\"";

                var accessKey = this.AccessKey == null || this.AccessKey.Length <= 0 ? string.Empty : " accesskey=\"" + this.AccessKey + "\"";

                output.Write("<a" + id + " href=\"#\" class=\"scRibbonToolbarLargeIconButton" + (this.Down ? "Down" : string.Empty) + "\"" + title + accessKey + " onclick=\"javascript:return scForm.invoke('" + this.Command + "')\">");

                output.Write(new ImageBuilder()
                {
                    Src = Images.GetThemedImageSource(this.Icon, ImageDimension.id32x32),
                    Class = "scRibbonToolbarLargeIconButtonIcon",
                    Alt = tooltip
                }.ToString());

                output.Write("</a>");
            }
        }
    }
}
