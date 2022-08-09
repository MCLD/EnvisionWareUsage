using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace its.TagHelpers
{
    [HtmlTargetElement("progressbar", Attributes = "pb-percent")]
    public class ProgressBarTagHelper : TagHelper
    {
        [HtmlAttributeName("pb-percent")] public string Percent { get; set; }
        [HtmlAttributeName("pb-class")] public string Class { get; set; }
        [HtmlAttributeName("pb-label")] public string Label { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.AddClass("progress", HtmlEncoder.Default);
            var progressBar = new TagBuilder("div");
            progressBar.AddCssClass("progress-bar");
            if (!string.IsNullOrEmpty(Class))
            {
                progressBar.AddCssClass(Class);
            }
            progressBar.Attributes.Add("role", "progressbar");
            progressBar.Attributes.Add("aria-valuemin", "0");
            progressBar.Attributes.Add("aria-valuemax", "0");
            progressBar.Attributes.Add("aria-valuenow", Percent);
            progressBar.Attributes.Add("style", $"width: {Percent}%;");
            if (!string.IsNullOrEmpty(Label))
            {
                progressBar.InnerHtml.Append(Label);
            }
            output.Content.SetHtmlContent(progressBar);
        }
    }
}
