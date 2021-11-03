using System;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Web.Mvc.Html;
using EPiServer;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlloyTemplates.Business.Rendering
{
    /// <summary>
    /// Extends the default <see cref="ContentAreaRenderer"/> to apply custom CSS classes to each <see cref="ContentFragment"/>.
    /// </summary>
    public class AlloyContentAreaRenderer : ContentAreaRenderer
    {
        protected override string GetContentAreaItemCssClass(IHtmlHelper htmlHelper, ContentAreaItem contentAreaItem)
        {
            var baseItemClass = base.GetContentAreaItemCssClass(htmlHelper, contentAreaItem);

            var tag = GetContentAreaItemTemplateTag(htmlHelper, contentAreaItem);
            return $"alloy-renderer {baseItemClass} {GetTypeSpecificCssClasses(contentAreaItem, ContentRepository)} {GetCssClassForTag(tag)} {tag}";
        }

        /// <summary>
        /// Gets a CSS class used for styling based on a tag name (ie a Bootstrap class name)
        /// </summary>
        /// <param name="tagName">Any tag name available, see <see cref="Global.ContentAreaTags"/></param>
        private static string GetCssClassForTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return "";
            }
            switch (tagName.ToLower())
            {
                case Global.ContentAreaTags.FullWidth:
                    return "w-full flex-shrink-0";
                case Global.ContentAreaTags.TwoThirdsWidth:
                    return "w-full sm:w-2/3";
                case Global.ContentAreaTags.HalfWidth:
                    return "w-full sm:w-1/2";
                case Global.ContentAreaTags.OneThirdWidth:
                    return "w-full sm:w-1/3";
                default:
                    return string.Empty;
            }
        }

        private static string GetTypeSpecificCssClasses(ContentAreaItem contentAreaItem, IContentRepository contentRepository)
        {
            var content = contentAreaItem.GetContent();
            var cssClass = content == null ? String.Empty : content.GetOriginalType().Name.ToLowerInvariant();

            var customClassContent = content as ICustomCssInContentArea;
            if (customClassContent != null && !string.IsNullOrWhiteSpace(customClassContent.ContentAreaCssClass))
            {
                cssClass += string.Format(" {0}", customClassContent.ContentAreaCssClass);
            }

            return cssClass;
        }
    }
}
