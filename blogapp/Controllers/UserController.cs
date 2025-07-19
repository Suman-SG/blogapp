using blogapp.Data;
using blogapp.Models;
using blogapp.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace blogapp.Controllers
{
    public class UserController : Controller
    {
        private readonly BlogDBContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(BlogDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        //  GET: /User/Profile
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };

            return View(viewModel);
        }

        //  POST: Update Profile
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult UpdateProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Email = model.Email;

            // If user entered a new password, update it
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            _context.SaveChanges();

            // Clear session and redirect to Login page
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }


        //  GET: /User/MyPosts
        public IActionResult MyPosts()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var posts = _context.BlogPosts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var bookmarkedIds = _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Select(b => b.PostId)
                .ToList();

            ViewBag.BookmarkedIds = bookmarkedIds;

            return View(posts);
        }

        //  POST: Delete a post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (post != null)
            {
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.BlogPosts.Remove(post);
                _context.SaveChanges();
            }

            return RedirectToAction("MyPosts");
        }

        // ✏️ GET: Edit post
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (post == null) return NotFound();

            return View("~/Views/User/Edit.cshtml", post); // You can rename Edit1.cshtml to Edit.cshtml later
        }

        // POST: Edit post
        
      
        //public IActionResult Edit(int id, BlogPost updatedPost, IFormFile imageFile)
        //{
        //    var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id);
        //    if (post == null) return NotFound();

        //    post.Title = updatedPost.Title;
        //    post.Content = CleanHtml(updatedPost.Content); // ✅ Clean TinyMCE content
        //    post.Tags = updatedPost.Tags;

        //    if (imageFile != null && imageFile.Length > 0)
        //    {
        //        if (!string.IsNullOrEmpty(post.ImagePath))
        //        {
        //            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImagePath.TrimStart('/'));
        //            if (System.IO.File.Exists(oldPath))
        //                System.IO.File.Delete(oldPath);
        //        }

        //        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        //        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

        //        var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
        //        var filePath = Path.Combine(uploads, fileName);

        //        using var stream = new FileStream(filePath, FileMode.Create);
        //        imageFile.CopyTo(stream);

        //        post.ImagePath = "/uploads/" + fileName;
        //    }

        //    _context.SaveChanges();
        //    TempData["Success"] = "Post updated successfully!";
        //    return RedirectToAction("MyPosts");
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BlogPost updatedPost, IFormFile imageFile, IFormFile videoFile)
        {
            var post = _context.BlogPosts.Find(updatedPost.Id);
            if (post == null)
                return NotFound();

            post.Title = updatedPost.Title;
            post.Content = updatedPost.Content;
            post.Tags = updatedPost.Tags;
            post.Category = updatedPost.Category;

            // Save new image
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var newName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = new FileStream(Path.Combine(uploads, newName), FileMode.Create);
                imageFile.CopyTo(stream);

                post.ImagePath = "/uploads/" + newName;
            }

            // Save new video
            if (videoFile != null && videoFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                if (!string.IsNullOrEmpty(post.VideoPath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, post.VideoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var newName = Guid.NewGuid() + Path.GetExtension(videoFile.FileName);
                using var stream = new FileStream(Path.Combine(uploads, newName), FileMode.Create);
                videoFile.CopyTo(stream);

                post.VideoPath = "/uploads/" + newName;
            }

            _context.SaveChanges();

            TempData["Message"] = "✅ Blog updated successfully!";
            return RedirectToAction("Index", "Feed");
        }


        //  POST: Toggle bookmark
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ToggleBookmark(int id, string redirectToDashboard, string returnUrl = null)
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId == null)
        //        return RedirectToAction("Login", "Auth");

        //    var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id);
        //    if (post == null)
        //    {
        //        TempData["Error"] = "Post not found.";
        //        return RedirectToAction("Index", "BlogPost");
        //    }

        //    var bookmark = _context.Bookmarks.FirstOrDefault(b => b.UserId == userId && b.PostId == id);

        //    if (bookmark != null)
        //    {
        //        _context.Bookmarks.Remove(bookmark);
        //        TempData["BookmarkMessage"] = "Bookmark removed.";
        //    }
        //    else
        //    {
        //        _context.Bookmarks.Add(new Bookmark
        //        {
        //            UserId = userId.Value,
        //            PostId = id,
        //            BookmarkedAt = DateTime.Now
        //        });
        //        TempData["BookmarkMessage"] = "Post bookmarked!";
        //    }

        //    //  Save before redirect
        //    _context.SaveChanges();

        //    //  Redirect after saving
        //    if (redirectToDashboard == "true")
        //        return RedirectToAction("Dashboard");

        //    return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
        //        ? Redirect(returnUrl)
        //        : RedirectToAction("MyPosts");
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleBookmark(int id, string returnUrl = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                TempData["Error"] = "Post not found.";
                return RedirectToAction("Index", "Feed");
            }

            var bookmark = _context.Bookmarks.FirstOrDefault(b => b.UserId == userId && b.PostId == id);

            if (bookmark != null)
            {
                _context.Bookmarks.Remove(bookmark);
                TempData["BookmarkMessage"] = "Bookmark removed.";
            }
            else
            {
                _context.Bookmarks.Add(new Bookmark
                {
                    UserId = userId.Value,
                    PostId = id,
                    BookmarkedAt = DateTime.Now
                });
                TempData["BookmarkMessage"] = "Post bookmarked!";
            }

            _context.SaveChanges();

            // Safely redirect back
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            string referer = Request.Headers["Referer"].ToString();
            return Redirect(referer);

            //return RedirectToAction("Dashboard", "User");
        }



        //  GET: Bookmarked Posts
        public IActionResult Bookmarks()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var posts = _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Post)
                .Select(b => b.Post)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View("BookmarkedPosts", posts);
        }

        //  GET: Dashboard
        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var model = new DashboardViewModel
            {
                TotalPosts = _context.BlogPosts.Count(p => p.UserId == userId),
                TotalComments = _context.Comments.Count(c => c.Post.UserId == userId),
                TotalLikes = _context.Likes.Count(l => l.BlogPost.UserId == userId),
                RecentPosts = _context.BlogPosts
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToList(),
                BookmarkedPosts = _context.Bookmarks
                    .Where(b => b.UserId == userId)
                    .Include(b => b.Post)
                    .Select(b => b.Post)
                    .ToList()
            };

            return View(model);
        }

        //  Password hashing (replace with BCrypt in production)
        private string HashPassword(string password)
        {
            return password;
        }

        /// Clean TinyMCE content
        private string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;
            return Regex.Replace(html, @"\sdata-[\w-]+=""[^""]*""", "", RegexOptions.IgnoreCase);
        }
        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Invalid upload.";
                return RedirectToAction("UploadProfileImage");
            }

            // Save the image to wwwroot/images/profiles/
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"user_{userId}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            //  path
            var relativePath = "/images/profiles/" + uniqueFileName;

            // Update user
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.ProfileImagePath = relativePath;
                _context.SaveChanges();
                HttpContext.Session.SetString("ProfileImagePath", relativePath);
            }

            TempData["Message"] = "Profile uploaded successfully!";
            return RedirectToAction("Dashboard");
        }
        [HttpGet]
        public IActionResult UploadProfile()
        {
            return View();
        }
        //become creator

        [HttpGet]
        public IActionResult BecomeCreator()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BecomeCreatorRequest(string email, string password)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user != null && user.BlogAccessStatus == AccessStatus.None)
            {
                user.Email = email;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.BlogAccessStatus = AccessStatus.Pending;
                _context.SaveChanges();

                TempData["Message"] = "Request submitted to admin.";
            }

            return RedirectToAction("Profile");
        }
        public IActionResult Settings()
        {
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(); // This looks for Views/User/Settings.cshtml
        }

        // You can also add EditProfile if you're linking to it
        public IActionResult EditProfile()
        {
            // Load user info from DB, pass to view, etc.
            return View();
        }
    }
}


