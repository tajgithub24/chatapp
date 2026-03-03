using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string? Message { get; set; }
        public string? FilePath { get; set; } // Stores the path to the file
        public string? FileName { get; set; } // Stores the original name
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}