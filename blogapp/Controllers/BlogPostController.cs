using blogapp.Data;
using blogapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace blogapp.Controllers
{
    public class BlogPostController : Controller
    {
        private readonly BlogDBContext _context;
        private readonly IWebHostEnvironment _env;

        public BlogPostController(BlogDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        //  Display all blog posts
        public IActionResult Index()
        {
            var posts = _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToList();

            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.UserId = userId;

            if (userId != null)
            {
                var bookmarkedIds = _context.Bookmarks
                    .Where(b => b.UserId == userId.Value)
                    .Select(b => b.PostId)
                    .ToList();

                ViewBag.BookmarkedIds = bookmarkedIds;
            }

            return View(posts);
        }

        // View full post details
        public IActionResult Details(int id)
        {
            var post = _context.BlogPosts
                .Include(p => p.Comments).ThenInclude(c => c.Replies)
                .Include(p => p.Likes)
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.AlreadyLiked = userId != null &&
                _context.Likes.Any(l => l.BlogPostId == id && l.UserId == userId);

            return View(post);
        }

        //  GET: Create blog post form
        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null || user.BlogAccessStatus != AccessStatus.Approved)
                return RedirectToAction("BecomeCreator", "User");

            return View();
        }


        //  POST: Create a new blog post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BlogPost blogPost)
        {
            if (!ModelState.IsValid)
                return View(blogPost);

            // Save image if uploaded
            if (blogPost.imageFile != null && blogPost.imageFile.Length > 0)
            {
                var imageFileName = Guid.NewGuid() + Path.GetExtension(blogPost.imageFile.FileName);
                var imagePath = Path.Combine("wwwroot/images", imageFileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    blogPost.imageFile.CopyTo(stream);
                }
                blogPost.ImagePath = "/images/" + imageFileName;
            }

            // Save video if uploaded
            if (blogPost.videoFile != null && blogPost.videoFile.Length > 0)
            {
                var videoFileName = Guid.NewGuid() + Path.GetExtension(blogPost.videoFile.FileName);
                var videoPath = Path.Combine("wwwroot/videos", videoFileName);
                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    blogPost.videoFile.CopyTo(stream);
                }
                blogPost.VideoPath = "/videos/" + videoFileName;
            }

            blogPost.CreatedAt = DateTime.Now;
            blogPost.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            _context.BlogPosts.Add(blogPost);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }





        //  Like a blog post
        [HttpPost]
        public IActionResult Like(int id, string returnUrl = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (!_context.Likes.Any(l => l.BlogPostId == id && l.UserId == userId))
            {
                var like = new Like
                {
                    BlogPostId = id,
                    UserId = userId.Value,
                    LikedAt = DateTime.Now
                };

                _context.Likes.Add(like);
                _context.SaveChanges();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Details", new { id });
        }

        //  Delete a blog post
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id);
            if (post != null)
            {
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    string fullPath = Path.Combine(_env.WebRootPath, post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                _context.BlogPosts.Remove(post);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        //  Add a comment or reply
        [HttpPost]
        public IActionResult AddComment(int postId, string author, string content, int? parentCommentId)
        {
            var comment = new Comment
            {
                PostId = postId,
                Author = author,
                Content = content,
                CreatedAt = DateTime.Now,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = postId });
        }

        //  GET: Edit post form
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var post = _context.BlogPosts.Find(id);
            if (post == null)
                return NotFound();

            return View(post);
        }

        // POST: Save edited post
        [HttpPost]
        public IActionResult Edit(BlogPost updatedPost, IFormFile imageFile, IFormFile videoFile)
        {
            var post = _context.BlogPosts.Find(updatedPost.Id);
            if (post == null)
                return NotFound();

            // ✏️ Update fields
            post.Title = updatedPost.Title;
            post.Content = updatedPost.Content;
            post.Tags = updatedPost.Tags;
            post.Category = updatedPost.Category; // ✅ Category update

            // 📷 Handle new image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                string imageFileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string imagePath = Path.Combine(uploadsFolder, imageFileName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                post.ImagePath = "/uploads/" + imageFileName;
            }

            // 🎥 Handle new video upload
            if (videoFile != null && videoFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(post.VideoPath))
                {
                    var oldVideoPath = Path.Combine(_env.WebRootPath, post.VideoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldVideoPath))
                        System.IO.File.Delete(oldVideoPath);
                }

                string videoFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(videoFolder);

                string videoFileName = Guid.NewGuid() + Path.GetExtension(videoFile.FileName);
                string videoPath = Path.Combine(videoFolder, videoFileName);

                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    videoFile.CopyTo(stream);
                }

                post.VideoPath = "/uploads/" + videoFileName;
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Feed(string search, string sort)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var posts = _context.BlogPosts
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .AsQueryable();

            // 🔍 Apply search
            if (!string.IsNullOrWhiteSpace(search))
            {
                posts = posts.Where(p => p.Title.Contains(search) || p.Content.Contains(search) || p.Tags.Contains(search));
            }

            // ↕️ Apply sorting
            switch (sort)
            {
                case "newest":
                    posts = posts.OrderByDescending(p => p.CreatedAt);
                    break;
                case "likes":
                    posts = posts.OrderByDescending(p => p.Likes.Count);
                    break;
                case "comments":
                    posts = posts.OrderByDescending(p => p.Comments.Count);
                    break;
                case "editor":
                    posts = posts.Where(p => p.IsAdminChoice).OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    posts = posts.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            // 📌 Bookmarks
            var bookmarkedIds = new List<int>();
            if (userId != null)
            {
                bookmarkedIds = _context.Bookmarks
                    .Where(b => b.UserId == userId)
                    .Select(b => b.PostId)

                    .ToList();
            }

            ViewBag.UserId = userId;
            ViewBag.BookmarkedIds = bookmarkedIds;
            ViewBag.SelectedSort = sort;
            ViewBag.SearchQuery = search;

            return View(posts
                .AsNoTracking() // ✅ Reduces memory usage
                .Take(50)       // ✅ Load only top 50 results
                .ToList());

        }



    }
}
