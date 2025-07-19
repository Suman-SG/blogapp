using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blogapp.Data;
using blogapp.Models;
using System.Linq;

public class FeedController : Controller
{
    private readonly BlogDBContext _context;

    public FeedController(BlogDBContext context)
    {
        _context = context;
    }

    //public IActionResult Index(string search = "", string sort = "newest")
    //{
    //    var userId = HttpContext.Session.GetInt32("UserId");
    //    if (userId == null)
    //    {
    //        return RedirectToAction("Login", "Auth");
    //    }

    //    var posts = _context.BlogPosts
    //        .Include(p => p.User)
    //        .Include(p => p.Likes)
    //        .Include(p => p.Comments)
    //        .ToList();

    //    // Apply search filter
    //    if (!string.IsNullOrWhiteSpace(search))
    //    {
    //        posts = posts
    //            .Where(p => p.Title != null && p.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
    //            .ToList();
    //    }

    //    // Sort logic
    //    ViewBag.SelectedSort = sort;
    //    switch (sort)
    //    {
    //        case "likes":
    //            posts = posts.OrderByDescending(p => p.Likes.Count).ToList();
    //            break;
    //        case "comments":
    //            posts = posts.OrderByDescending(p => p.Comments.Count).ToList();
    //            break;
    //        default:
    //            posts = posts.OrderByDescending(p => p.CreatedAt).ToList();
    //            break;
    //    }

    //    ViewBag.UserId = userId;
    //    ViewBag.SearchQuery = search;

    //    // Bookmarks (optional)
    //    var bookmarkedIds = _context.Bookmarks
    //        .Where(b => b.UserId == userId)
    //        .Select(b => b.Post)
    //        .ToList();

    //    ViewBag.BookmarkedIds = bookmarkedIds;

    //    return View(posts);
    //}


    public IActionResult Index(string search = "", string sort = "newest")
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var query = _context.BlogPosts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsQueryable();

        // 🔍 Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Title != null && p.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        // 🔃 Sort logic
        ViewBag.SelectedSort = sort;
        switch (sort)
        {
            case "likes":
                query = query.OrderByDescending(p => p.Likes.Count);
                break;
            case "comments":
                query = query.OrderByDescending(p => p.Comments.Count);
                break;
            default:
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
        }

        // 🧠 Load only 5 posts after filtering/sorting
        var posts = query.Take(5).ToList();

        // ✅ Pass session info and search query
        ViewBag.UserId = userId;
        ViewBag.SearchQuery = search;

        // ⭐️ Load bookmarked post IDs
        var bookmarkedIds = _context.Bookmarks
            .Where(b => b.UserId == userId)
            .Select(b => b.PostId)
            .ToList();

        ViewBag.BookmarkedIds = bookmarkedIds;

        return View(posts);
    }
    [HttpGet]
    public IActionResult LoadMore(int skip, string search = "", string sort = "newest")
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized();

        var query = _context.BlogPosts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Title != null && p.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

        switch (sort)
        {
            case "likes":
                query = query.OrderByDescending(p => p.Likes.Count);
                break;
            case "comments":
                query = query.OrderByDescending(p => p.Comments.Count);
                break;
            default:
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
        }

        var posts = query.Skip(skip).Take(5).ToList();

        var bookmarkedIds = _context.Bookmarks
            .Where(b => b.UserId == userId)
            .Select(b => b.PostId)
            .ToList();

        ViewBag.BookmarkedIds = bookmarkedIds;
        ViewBag.UserId = userId;

        return PartialView("_BlogPostPartial", posts);
    }

}
