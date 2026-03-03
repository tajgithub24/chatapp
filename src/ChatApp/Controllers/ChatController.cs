using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

namespace ChatApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _storagePath;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
            Env.Load();
            _storagePath = Environment.GetEnvironmentVariable("CHATA_UPLOAD_PATH") ?? @"C:\ChatUploads\";
            if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
        }

        public async Task<IActionResult> Index()
        {
            var currentId = HttpContext.Session.GetInt32("UserId");
            if (currentId == null) return RedirectToAction("Login", "Account");
            var users = await _context.Users.Where(u => u.Id != currentId).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<JsonResult> GetChatHistory(int receiverId)
        {
            var senderId = HttpContext.Session.GetInt32("UserId");
            var history = await _context.ChatMessages
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            return Json(history);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int receiverId)
        {
            var senderId = HttpContext.Session.GetInt32("UserId");
            if (file != null && senderId != null)
            {
                var uniqueName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var fullPath = Path.Combine(_storagePath, uniqueName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var msg = new ChatMessage
                {
                    SenderId = senderId.Value,
                    ReceiverId = receiverId,
                    FilePath = "/Chat/DownloadFile?fileName=" + uniqueName,
                    FileName = file.FileName
                };
                _context.ChatMessages.Add(msg);
                await _context.SaveChangesAsync();
                return Json(new { fileName = file.FileName, filePath = msg.FilePath });
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            var path = Path.Combine(_storagePath, fileName);
            if (System.IO.File.Exists(path))
            {
                return PhysicalFile(path, "application/octet-stream", fileName);
            }
            return NotFound();
        }
    }
}