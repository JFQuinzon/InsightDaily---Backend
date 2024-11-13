using System.ComponentModel.DataAnnotations;

namespace backend.Model.DTO
{
    public class UpdateBookmarkDto
    {
        public string User_Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string UrlToImage { get; set; }

        [Required]
        public string Note { get; set; }
    }
}
