using backend.Data;
using backend.Model;
using backend.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookmarkController : Controller
    {
        private readonly AppDbContext dbContext;

        public BookmarkController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("Bookmarks/{userID}")]
        [Authorize]
        public ActionResult<IEnumerable<BookmarkModel>> GetNewsByUserID(string userID)
        {
            var userNews = dbContext.Bookmarks
                                .Where(news => news.User_Id == userID)
                                .ToArray();

            if (userNews == null || userNews.Length == 0)
            {
                return NotFound("No news found.");
            }

            return Ok(userNews);
        }

        //CREATE
        [HttpPost("Create")]
        [Authorize]
        public IActionResult CreateBookmark(AddBookmarkDto addBookmarkDto)
        {
            var bookmarkEntity = new BookmarkModel()
            {
                User_Id = addBookmarkDto.User_Id,
                Title = addBookmarkDto.Title,
                Description = addBookmarkDto.Description,
                PublishedAt = addBookmarkDto.PublishedAt,
                Author = addBookmarkDto.Author,
                Url = addBookmarkDto.Url,
                UrlToImage = addBookmarkDto.UrlToImage,
                Note = addBookmarkDto.Note,
            };

            dbContext.Bookmarks.Add(bookmarkEntity);
            dbContext.SaveChanges();

            return Ok(bookmarkEntity);
        }

        //UPDATE
        [HttpPut("Update/{id}")]
        [Authorize]
        public IActionResult UpdateBookmark(int id, UpdateBookmarkDto updateBookmarkDto)
        {
            var bookmark = dbContext.Bookmarks.Find(id);
            if (bookmark == null)
            {
                return NotFound();
            }
            bookmark.User_Id = updateBookmarkDto.User_Id;
            bookmark.Url = updateBookmarkDto.Url;
            bookmark.Title = updateBookmarkDto.Title;
            bookmark.Description = updateBookmarkDto.Description;
            bookmark.PublishedAt = updateBookmarkDto.PublishedAt;
            bookmark.Author = updateBookmarkDto.Author;
            bookmark.UrlToImage = updateBookmarkDto.UrlToImage;
            bookmark.Note = updateBookmarkDto.Note;

            dbContext.SaveChanges();

            return Ok(bookmark);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize]
        public IActionResult DeleteBookmark(int id)
        {
            var bookmark = dbContext.Bookmarks.Find(id);
            if (bookmark == null)
            {
                return NotFound();
            }
            dbContext.Remove(bookmark);
            dbContext.SaveChanges();
            return Ok();
        }

    }
}
