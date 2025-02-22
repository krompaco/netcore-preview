using System.Collections.Generic;
using System.Linq;
using AlloyMvcTemplates.Models.ViewModels;
using AlloyTemplates.Models.Pages;
using AlloyTemplates.Models.ViewModels;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AlloyTemplates.Business
{
    [ServiceConfiguration]
    public class PageViewContextFactory
    {
        private readonly IContentLoader _contentLoader;
        private readonly UrlResolver _urlResolver;
        private readonly IDatabaseMode _databaseMode;
        private readonly IContextModeResolver _contextModeResolver;

        public PageViewContextFactory(IContentLoader contentLoader, UrlResolver urlResolver, IDatabaseMode databaseMode, IContextModeResolver contextModeResolver,  IOptionsMonitor<CookieAuthenticationOptions> optionMonitor)
        {
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
            _databaseMode = databaseMode;
            _contextModeResolver = contextModeResolver;
        }

        public virtual LayoutModel CreateLayoutModel(ContentReference currentContentLink, HttpContext httpContext)
        {
            var startPageContentLink = SiteDefinition.Current.StartPage;

            // Use the content link with version information when editing the startpage,
            // otherwise the published version will be used when rendering the props below.
            if (currentContentLink.CompareToIgnoreWorkID(startPageContentLink))
            {
                startPageContentLink = currentContentLink;
            }

            var startPage = _contentLoader.Get<StartPage>(startPageContentLink);

            // Process menu
            var menuItems = ProcessMenu(startPageContentLink, 1);

            return new LayoutModel
            {
                Logotype = startPage.SiteLogotype,
                LogotypeLinkUrl = new HtmlString(_urlResolver.GetUrl(SiteDefinition.Current.StartPage)),
                ProductPages = startPage.ProductPageLinks,
                CompanyInformationPages = startPage.CompanyInformationPageLinks,
                NewsPages = startPage.NewsPageLinks,
                CustomerZonePages = startPage.CustomerZonePageLinks,
                LoggedIn = httpContext.User.Identity.IsAuthenticated,
                LoginUrl = new HtmlString(GetLoginUrl(currentContentLink)),
                SearchActionUrl = new HtmlString(_urlResolver.GetUrl(startPage.SearchPageLink)),
                IsInReadonlyMode = _databaseMode.DatabaseMode == DatabaseMode.ReadOnly,
                ContextMode = _contextModeResolver.CurrentMode,
                MenuItems = menuItems,
            };
        }

        private List<MenuItemViewModel> ProcessMenu(ContentReference parentLink, int level)
        {
            var menuItems = new List<MenuItemViewModel>();
            var children = _contentLoader.GetChildren<PageData>(parentLink).Where(x => x.VisibleInMenu);

            foreach (var pageData in children)
            {
                var url = _urlResolver.GetUrl(pageData.ContentLink);

                if (url == null)
                {
                    continue;
                }

                menuItems.Add(new MenuItemViewModel
                {
                    Level = level,
                    LinkUrl = url,
                    Text = pageData.PageName,
                    ChildItems = ProcessMenu(pageData.ContentLink, level + 1),
                });
            }

            return menuItems;
        }

        private string GetLoginUrl(ContentReference returnToContentLink)
        {
            return $"{Global.LoginPath}?ReturnUrl={_urlResolver.GetUrl(returnToContentLink)}";
        }

        public virtual IContent GetSection(ContentReference contentLink)
        {
            var currentContent = _contentLoader.Get<IContent>(contentLink);
            if (currentContent.ParentLink != null && currentContent.ParentLink.CompareToIgnoreWorkID(SiteDefinition.Current.StartPage))
            {
                return currentContent;
            }

            return _contentLoader.GetAncestors(contentLink)
                .OfType<PageData>()
                .SkipWhile(x => x.ParentLink == null || !x.ParentLink.CompareToIgnoreWorkID(SiteDefinition.Current.StartPage))
                .FirstOrDefault();
        }
    }
}
