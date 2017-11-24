using DasBlog.Web.UI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DasBlog.Web.UI.Core.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using newtelligence.DasBlog.Runtime;
using newtelligence.DasBlog.Util;
using System.Collections.ObjectModel;

namespace DasBlog.Web.UI.Settings
{
    public class DasBlogSettings : IDasBlogSettings
    {
        public DasBlogSettings(IHostingEnvironment env, IOptions<SiteConfig> siteConfig, IOptions<MetaTags> metaTagsConfig, 
            IOptions<SiteSecurityConfig> siteSecurityConfig)
        {
            this.SiteConfiguration = siteConfig.Value;
            this.MetaTags = metaTagsConfig.Value;
            this.SecurityConfiguration = siteSecurityConfig.Value;

            this.WebRootDirectory = env.WebRootPath;
            this.RssUrl = this.RelativeToRoot("feed/rss");
            this.CategoryUrl = this.RelativeToRoot("category");
            this.ArchiveUrl = this.RelativeToRoot("archive");
            this.SiteMap = this.RelativeToRoot("site/map");
            this.RsdUrl = this.RelativeToRoot("feed/rsd");
            this.ShortCutIconUrl = this.RelativeToRoot("icon.jpg");
        }

        public IMetaTags MetaTags { get; }

        public ISiteConfig SiteConfiguration { get; }

        public ISiteSecurityConfig SecurityConfiguration { get; }

        public string WebRootDirectory { get; }

        public string RssUrl { get; }

        public string CategoryUrl { get; }

        public string ArchiveUrl { get; }

        public string SiteMap { get; }

        public string RsdUrl { get; }

        public string ShortCutIconUrl { get; }

        public string GetBaseUrl()
        {
            return new Uri(this.SiteConfiguration.Root).AbsoluteUri;
        }

        public string RelativeToRoot(string relative)
        {
            return new Uri(new Uri(this.SiteConfiguration.Root), relative).AbsoluteUri;
        }

        public string GetPermaLinkUrl(string entryId)
        {
            string titlePermalink = string.Empty;

            //if (SiteConfiguration.EnableTitlePermaLink)
            //{
            //    replace spaces with nowt, or the configured space replacement string.
            //    string compressedTitle = (SiteConfiguration.EnableTitlePermaLinkUnique) ? titledEntry.CompressedTitleUnique : titledEntry.CompressedTitle;
            //    string spaceReplacement = !SiteConfiguration.EnableTitlePermaLinkSpaces ? "" : SiteConfiguration.TitlePermalinkSpaceReplacement;
            //    titlePermalink = GetCompressedTitleUrl(compressedTitle.Replace("+", spaceReplacement).ToLower());
            //}
            //else
            //{
            //    titlePermalink = RelativeToRoot("post/" + EntryId);
            //}

            return titlePermalink;
        }

        public string GetCompressedTitleUrl(string title)
        {
            return SiteConfiguration.Root + title;
        }

        public string GetCommentViewUrl(string entryId)
        {
            //TODO: Old links vs new links
            return RelativeToRoot("comment/" + entryId);
        }

        public string GetTrackbackUrl(string entryId)
        {
            //TODO: Old links vs new links
            return RelativeToRoot("trackback/" + entryId);
        }

        public string GetEntryCommentsRssUrl(string entryId)
        {
            //TODO: Old links vs new links
            return RelativeToRoot("feed/rss/comments/" + entryId);
        }

        public string GetCategoryViewUrl(string category)
        {
            return RelativeToRoot("category/" + category);
        }

        public User GetUser(string userName)
        {
            if (false == String.IsNullOrEmpty(userName))
            {
                return this.SecurityConfiguration.Users.Find(delegate (User x)
                {
                    return String.Compare(x.Name, userName, StringComparison.InvariantCultureIgnoreCase) == 0;
                });
            }
            return null;
        }

        public TimeZone GetConfiguredTimeZone()
        {
            // Need to figure out how to handle time...
            return new UTCTimeZone();

            //if (SiteConfiguration.AdjustDisplayTimeZone)
            //{
            //    return TimeZone.CurrentTimeZone as WindowsTimeZone;
            //}
        }
    }
}