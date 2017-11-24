using newtelligence.DasBlog.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace DasBlog.Web.UI.Repositories.Interfaces
{
    public interface IBlogRepository
    {
        bool IsLastPage(int pageindex);

        Entry GetBlogPost(string postid);

        EntryCollection GetFrontPagePosts();

        EntryCollection GetEntriesForPage(int pageIndex);
    }
}
