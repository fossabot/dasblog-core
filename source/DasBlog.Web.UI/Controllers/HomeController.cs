using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DasBlog.Web.UI.Models;
using newtelligence.DasBlog.Runtime;
using newtelligence.DasBlog.Web.Core;
using DasBlog.Web.UI.Models.BlogViewModels;
using Microsoft.Extensions.Options;
using DasBlog.Web.UI.Core;
using DasBlog.Web.UI.Repositories.Interfaces;
using newtelligence.DasBlog.Util;
using Microsoft.Extensions.FileProviders;
using DasBlog.Web.UI.Core.Configuration;

namespace DasBlog.Web.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IDasBlogSettings _dasBlogSettings;

        public HomeController(IBlogRepository blogRepository, IDasBlogSettings settings)
        {
            _blogRepository = blogRepository;
            _dasBlogSettings = settings;
        }

        public IActionResult Index()
        {
            ListPostsViewModel lpvm = new ListPostsViewModel();
            lpvm.PageIndex = 0;
            lpvm.Posts = _blogRepository.GetFrontPagePosts()
                            .Select(entry => new PostViewModel
                                {
                                    Author = entry.Author,
                                    Content = entry.Content,
                                    Categories = entry.Categories.Split(";").Select(category => new CategoryViewModel { CategoryName = category, CategoryNameUrlEncoded = WebUtility.UrlEncode(category) }).ToList(),
                                    Description = entry.Description,
                                    EntryId = entry.EntryId,
                                    AllowComments = entry.AllowComments,
                                    IsPublic = entry.IsPublic,
                                    PermaLink = entry.CompressedTitle.Replace("+", _dasBlogSettings.SiteConfiguration.TitlePermalinkSpaceReplacement).ToLower(),
                                    Title = entry.Title,
                                    Created = entry.CreatedLocalTime,
                                    Modified = entry.ModifiedLocalTime
                            }).ToList();

            MultiplePosts();

            return View(string.Format("/Themes/{0}/Page.cshtml", _dasBlogSettings.SiteConfiguration.Theme), lpvm);
        }

        public IActionResult Post(string posttitle)
        {
            ListPostsViewModel lpvm = new ListPostsViewModel();
            lpvm.PageIndex = -1;

            if (!string.IsNullOrEmpty(posttitle))
            {
                var entry = _blogRepository.GetBlogPost(posttitle);
                if (entry != null)
                {
                    lpvm.Posts = new List<PostViewModel>() {
                        new PostViewModel {
                            Author = entry.Author,
                            Content = entry.Content,
                            Categories = entry.Categories.Split(";").Select(category => new CategoryViewModel { CategoryName = category, CategoryNameUrlEncoded = WebUtility.UrlEncode(category) }).ToList(),
                            Description = entry.Description,
                            EntryId = entry.EntryId,
                            AllowComments = entry.AllowComments,
                            IsPublic = entry.IsPublic,
                            PermaLink = entry.CompressedTitle.Replace("+", _dasBlogSettings.SiteConfiguration.TitlePermalinkSpaceReplacement).ToLower(),
                            Title = entry.Title,
                            Created = entry.CreatedLocalTime,
                            Modified = entry.ModifiedLocalTime
                        }
                    };

                    SinglePost(lpvm.Posts.First());

                    return View(string.Format("/Themes/{0}/Page.cshtml", _dasBlogSettings.SiteConfiguration.Theme), lpvm);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return Index();
            }
        }

        [Route("comment/{Id:guid}")]
        public IActionResult Comment(Guid Id)
        {
            // ~/CommentView.aspx?title=GeneralPatternsusedtoDetectaLeak

            // Get post by GUID bbecae4b-e3a3-47a2-b6a6-b4cc405f8663
            Entry entry = _blogRepository.GetBlogPost(Id.ToString());

            ListPostsViewModel lpvm = new ListPostsViewModel();
            lpvm.PageIndex = -1;
            lpvm.Posts = new List<PostViewModel> {
                    new PostViewModel
                    {
                        Author = entry.Author,
                        Content = entry.Content,
                        Categories = entry.Categories.Split(";").Select(category => new CategoryViewModel { CategoryName = category, CategoryNameUrlEncoded = WebUtility.UrlEncode(category) }).ToList(),
                        Description = entry.Description,
                        EntryId = entry.EntryId,
                        AllowComments = entry.AllowComments,
                        IsPublic = entry.IsPublic,
                        PermaLink = entry.CompressedTitle.Replace("+", _dasBlogSettings.SiteConfiguration.TitlePermalinkSpaceReplacement).ToLower(),
                        Title = entry.Title,
                        Created = entry.CreatedLocalTime,
                        Modified = entry.ModifiedLocalTime
                    }
                };
            SinglePost(lpvm.Posts.First());

            return View(string.Format("/Themes/{0}/Page.cshtml", _dasBlogSettings.SiteConfiguration.Theme), lpvm);
        }

        [Route("page")]
        public IActionResult Page()
        {
            return Index();
        }

        [Route("page/{index:int}")]
        public IActionResult Page(int index)
        {
            if (index == 0)
            {
                return Index();
            }
            
            ListPostsViewModel lpvm = new ListPostsViewModel();
            lpvm.PageIndex = index;
            lpvm.Posts = _blogRepository.GetEntriesForPage(index)
                                .Select(entry => new PostViewModel
                                {
                                    Author = entry.Author,
                                    Content = entry.Content,
                                    Categories = entry.Categories.Split(";").Select(category => new CategoryViewModel { CategoryName = category, CategoryNameUrlEncoded = WebUtility.UrlEncode(category) }).ToList(),
                                    Description = entry.Description,
                                    EntryId = entry.EntryId,
                                    AllowComments = entry.AllowComments,
                                    IsPublic = entry.IsPublic,
                                    PermaLink = entry.CompressedTitle.Replace("+", _dasBlogSettings.SiteConfiguration.TitlePermalinkSpaceReplacement).ToLower(),
                                    Title = entry.Title,
                                    Created = entry.CreatedLocalTime,
                                    Modified = entry.ModifiedLocalTime
                                }).ToList();
            MultiplePosts();

            return View(string.Format("/Themes/{0}/Page.cshtml", _dasBlogSettings.SiteConfiguration.Theme), lpvm);
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void SinglePost(PostViewModel post)
        {
            ViewData["Title"] = post.Title;
            ViewData["Description"] = post.Content;
            ViewData["Keywords"] = post.Categories;
            ViewData["Canonical"] = post.PermaLink;
        }

        private void MultiplePosts()
        {
            ViewData["SiteTitle"] = _dasBlogSettings.SiteConfiguration.Title;
            ViewData["Title"] = _dasBlogSettings.SiteConfiguration.Title;
            ViewData["Description"] = _dasBlogSettings.SiteConfiguration.Description;
            ViewData["Keywords"] = _dasBlogSettings.MetaTags.MetaKeywords;
            ViewData["Canonical"] = _dasBlogSettings.SiteConfiguration.Root;
            ViewData["Author"] = _dasBlogSettings.SiteConfiguration.Copyright;
        }
    }
}

