﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DasBlog.Web.UI.Models;
using newtelligence.DasBlog.Runtime;
using newtelligence.DasBlog.Web.Core;
using DasBlog.Web.UI.Models.BlogViewModels;
using Microsoft.Extensions.Options;
using DasBlog.Web.Core;
using DasBlog.Web.Repositories.Interfaces;
using newtelligence.DasBlog.Util;
using Microsoft.Extensions.FileProviders;
using DasBlog.Web.Core.Configuration;
using System.IO;

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
            lpvm.Posts = _blogRepository.GetFrontPagePosts()
                            .Select(entry => new PostViewModel
                                {
                                    Author = entry.Author,
                                    Content = entry.Content,
                                    Categories = entry.Categories,
                                    Description = entry.Description,
                                    EntryId = entry.EntryId,
                                    AllowComments = entry.AllowComments,
                                    IsPublic = entry.IsPublic,
                                    PermaLink = entry.Link,
                                    Title = entry.Title,
                                    CreatedDateTime = entry.CreatedLocalTime
                                }).ToList();
            DefaultPage();

            return ThemedView("Page", lpvm);
        }

		//TODO: Maybe a helped for all?
		private ViewResult ThemedView(string v, ListPostsViewModel lpvm)
		{
			return View(string.Format("/Themes/{0}/{1}.cshtml", 
						_dasBlogSettings.SiteConfiguration.Theme, v), lpvm);
		}

		public IActionResult Post(string posttitle)
        {
            ListPostsViewModel lpvm = new ListPostsViewModel();

            if (!string.IsNullOrEmpty(posttitle))
            {
                var entry = _blogRepository.GetBlogPost(posttitle);
                if (entry != null)
                {
                    lpvm.Posts = new List<PostViewModel>() {
                        new PostViewModel {
                        Author = entry.Author,
                        Content = entry.Content,
                        Categories = entry.Categories,
                        Description = entry.Description,
                        EntryId = entry.EntryId,
                        AllowComments = entry.AllowComments,
                        IsPublic = entry.IsPublic,
                        PermaLink = entry.Link,
                        Title = entry.Title,
						CreatedDateTime = entry.CreatedLocalTime
						} };

                    SinglePost(lpvm.Posts.First());

					return ThemedView("Page", lpvm);
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
            lpvm.Posts = new List<PostViewModel> {
                    new PostViewModel
                    {
                        Author = entry.Author,
                        Content = entry.Content,
                        Categories = entry.Categories,
                        Description = entry.Description,
                        EntryId = entry.EntryId,
                        AllowComments = entry.AllowComments,
                        IsPublic = entry.IsPublic,
                        PermaLink = entry.Link,
                        Title = entry.Title
                    }
                };

            SinglePost(lpvm.Posts.First());

			return ThemedView("Page", lpvm);
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

            ViewData["Message"] = string.Format("Page...{0}", index);

            ListPostsViewModel lpvm = new ListPostsViewModel();
            lpvm.Posts = _blogRepository.GetEntriesForPage(index)
                                .Select(entry => new PostViewModel
                                {
                                    Author = entry.Author,
                                    Content = entry.Content,
                                    Categories = entry.Categories,
                                    Description = entry.Description,
                                    EntryId = entry.EntryId,
                                    AllowComments = entry.AllowComments,
                                    IsPublic = entry.IsPublic,
                                    PermaLink = entry.Link,
                                    Title = entry.Title
                                }).ToList();
            DefaultPage();

			return ThemedView("Page", lpvm);
        }

        [HttpGet("blogger")]
        public ActionResult Blogger()
        {
            // https://www.poppastring.com/blog/blogger.aspx
            // Implementation of Blogger XML-RPC Api
            // blogger
            // metaWebLog
            // mt

            // return NoContent();

            return View();
        }

        [Route("blogger")]
        [HttpPost]
        [Produces("text/xml")]
        public IActionResult Blogger([FromBody] string xmlrpcpost)
        {
            return this.Content(_blogRepository.XmlRpcInvoke(this.HttpContext.Request.Body));
        }

        public IActionResult About()
        {
            DefaultPage();

            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            DefaultPage();

            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void SinglePost(PostViewModel post)
        {
            ViewData["Title"] = post.Title;
            ViewData["Description"] = post.Description;
            ViewData["Keywords"] = post.Categories;
            ViewData["Canonical"] = post.PermaLink;
            ViewData["Author"] = post.Author;
        }

        private void DefaultPage()
        {
            ViewData["Title"] = _dasBlogSettings.SiteConfiguration.Title;
            ViewData["Description"] = _dasBlogSettings.SiteConfiguration.Description;
            ViewData["Keywords"] = _dasBlogSettings.MetaTags.MetaKeywords;
            ViewData["Canonical"] = _dasBlogSettings.SiteConfiguration.Root;
            ViewData["Author"] = _dasBlogSettings.SiteConfiguration.Copyright;
        }
    }
}

