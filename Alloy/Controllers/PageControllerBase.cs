using AlloyTemplates.Business;
using AlloyTemplates.Models.Pages;
using AlloyTemplates.Models.ViewModels;
using EPiServer.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace AlloyTemplates.Controllers
{
    /// <summary>
    /// All controllers that renders pages should inherit from this class so that we can
    /// apply action filters, such as for output caching site wide, should we want to.
    /// </summary>
    public abstract class PageControllerBase<T> : PageController<T>, IModifyLayout
        where T : SitePageData
    {
        public async Task<IActionResult> Login()
        {
            var redirectUri = HttpContext.RequestServices.GetService<UrlResolver>().GetUrl(PageContext.ContentLink, PageContext.LanguageID);
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> Logout()
        {
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public virtual void ModifyLayout(LayoutModel layoutModel)
        {
            var page = PageContext.Page as SitePageData;
            if (page != null)
            {
                layoutModel.HideHeader = page.HideSiteHeader;
                layoutModel.HideFooter = page.HideSiteFooter;
            }
        }
    }
}
