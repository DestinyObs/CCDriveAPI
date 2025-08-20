using TheDriveAPI.Models;
using TheDriveAPI.DTOs.File;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using TheDriveAPI.Data;

namespace TheDriveAPI.Services
{
    public interface IFileService
    {
        Task<bool> PermanentDeleteFileAsync(string userId, int fileId);
        Task<FileVersionDto> UploadFileVersionAsync(string userId, int fileId, IFormFile file);
        Task<FileDto> RestoreFileVersionAsync(string userId, int fileId, int versionId);
        Task<bool> DeleteFileVersionAsync(string userId, int fileId, int versionId);
        Task<object> PreviewFileAsync(string userId, int fileId);
        Task<FileDto> CopyFileAsync(string userId, int fileId);
        Task<bool> FavoriteFileAsync(string userId, int fileId);
        Task<IEnumerable<FileDto>> ListFilesAsync(string userId, int? folderId);
        Task<FileDto> UploadFileAsync(string userId, IFormFile file, int? folderId);
        Task<FileDownloadDto> DownloadFileAsync(string userId, int fileId);
        Task<bool> DeleteFileAsync(string userId, int fileId);
        Task<FileDto> RenameFileAsync(string userId, int fileId, string name);
        Task<FileDto> MoveFileAsync(string userId, int fileId, int folderId);
        Task<bool> ShareFileAsync(string userId, int fileId, string email, string permission);
        Task<IEnumerable<FileDto>> ListSharedFilesAsync(string userId);
        Task<IEnumerable<FileDto>> ListTrashedFilesAsync(string userId);
        Task<FileDto> RestoreFileAsync(string userId, int fileId);
        Task<IEnumerable<FileVersionDto>> GetFileVersionsAsync(string userId, int fileId);
    }

    public class FileService : IFileService
    {
        private readonly AppDbContext _context;
        public FileService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<FileDto>> ListFilesAsync(string userId, int? folderId)
        {
            var query = _context.Files.AsNoTracking().Where(f => f.UserId == userId && !f.IsTrashed);
            if (folderId.HasValue) query = query.Where(f => f.FolderId == folderId);
            var files = await query.OrderByDescending(f => f.UpdatedAt).ToListAsync();
            return files.Select(f => new FileDto
            {
                Id = f.Id,
                Name = f.Name,
                Size = f.Size,
                FolderId = f.FolderId,
                S3Key = f.S3Key,
                Trashed = f.IsTrashed,
                Version = f.Version,
                CreatedAt = f.CreatedAt.ToString("o"),
                UpdatedAt = f.UpdatedAt.ToString("o")
            });
        }

        public Task<FileDto> UploadFileAsync(string userId, IFormFile file, int? folderId)
        {
            return Task.FromResult(new FileDto { Id = 1, Name = file.FileName, Size = file.Length });
        }

        public Task<FileDownloadDto> DownloadFileAsync(string userId, int fileId)
        {
            return Task.FromResult(new FileDownloadDto { Id = fileId, Name = "file.txt", DownloadUrl = "https://example.com/file.txt", Size = 1234 });
        }

        public async Task<bool> DeleteFileAsync(string userId, int fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);
            if (file == null) throw new Exception("File not found");
            file.IsTrashed = true;
            file.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FileDto> RenameFileAsync(string userId, int fileId, string name)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);
            if (file == null) throw new Exception("File not found");
            file.Name = name;
            file.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new FileDto
            {
                Id = file.Id,
                Name = file.Name,
                Size = file.Size,
                FolderId = file.FolderId,
                S3Key = file.S3Key,
                Trashed = file.IsTrashed,
                Version = file.Version,
                CreatedAt = file.CreatedAt.ToString("o"),
                UpdatedAt = file.UpdatedAt.ToString("o")
            };
        }

        public async Task<FileDto> MoveFileAsync(string userId, int fileId, int folderId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);
            if (file == null) throw new Exception("File not found");
            file.FolderId = folderId;
            file.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new FileDto
            {
                Id = file.Id,
                Name = file.Name,
                Size = file.Size,
                FolderId = file.FolderId,
                S3Key = file.S3Key,
                Trashed = file.IsTrashed,
                Version = file.Version,
                CreatedAt = file.CreatedAt.ToString("o"),
                UpdatedAt = file.UpdatedAt.ToString("o")
            };
        }

        public async Task<bool> ShareFileAsync(string userId, int fileId, string email, string permission)
        {
            // Find user to share with
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new Exception("User to share with not found");
            var shared = new SharedFile
            {
                FileId = fileId,
                SharedWithUserId = user.Id,
                Permission = permission,
                CreatedAt = DateTime.UtcNow
            };
            _context.SharedFiles.Add(shared);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FileDto>> ListSharedFilesAsync(string userId)
        {
            var shared = await _context.SharedFiles.Include(sf => sf.File)
                .Where(sf => sf.SharedWithUserId == userId)
                .Select(sf => sf.File)
                .ToListAsync();
            return shared.Select(f => new FileDto
            {
                Id = f.Id,
                Name = f.Name,
                Size = f.Size,
                FolderId = f.FolderId,
                S3Key = f.S3Key,
                Trashed = f.IsTrashed,
                Version = f.Version,
                CreatedAt = f.CreatedAt.ToString("o"),
                UpdatedAt = f.UpdatedAt.ToString("o")
            });
        }

        public async Task<IEnumerable<FileDto>> ListTrashedFilesAsync(string userId)
        {
            var files = await _context.Files.AsNoTracking().Where(f => f.UserId == userId && f.IsTrashed).OrderByDescending(f => f.UpdatedAt).ToListAsync();
            return files.Select(f => new FileDto
            {
                Id = f.Id,
                Name = f.Name,
                Size = f.Size,
                FolderId = f.FolderId,
                S3Key = f.S3Key,
                Trashed = f.IsTrashed,
                Version = f.Version,
                CreatedAt = f.CreatedAt.ToString("o"),
                UpdatedAt = f.UpdatedAt.ToString("o")
            });
        }

        public async Task<FileDto> RestoreFileAsync(string userId, int fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);
            if (file == null) throw new Exception("File not found");
            file.IsTrashed = false;
            file.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new FileDto
            {
                Id = file.Id,
                Name = file.Name,
                Size = file.Size,
                FolderId = file.FolderId,
                S3Key = file.S3Key,
                Trashed = file.IsTrashed,
                Version = file.Version,
                CreatedAt = file.CreatedAt.ToString("o"),
                UpdatedAt = file.UpdatedAt.ToString("o")
            };
        }

        public Task<IEnumerable<FileVersionDto>> GetFileVersionsAsync(string userId, int fileId)
        {
            return Task.FromResult<IEnumerable<FileVersionDto>>(new List<FileVersionDto>());
        }

        public Task<bool> PermanentDeleteFileAsync(string userId, int fileId)
        {
            return Task.FromResult(true);
        }

        public Task<FileVersionDto> UploadFileVersionAsync(string userId, int fileId, IFormFile file)
        {
            return Task.FromResult(new FileVersionDto { Id = 1, FileId = fileId, Version = 2, S3Key = "v2/file.txt" });
        }

        public Task<FileDto> RestoreFileVersionAsync(string userId, int fileId, int versionId)
        {
            return Task.FromResult(new FileDto { Id = fileId });
        }

        public Task<bool> DeleteFileVersionAsync(string userId, int fileId, int versionId)
        {
            return Task.FromResult(true);
        }

        public Task<object> PreviewFileAsync(string userId, int fileId)
        {
            return Task.FromResult<object>(new { url = "https://example.com/preview.png" });
        }

        public Task<FileDto> CopyFileAsync(string userId, int fileId)
        {
            return Task.FromResult(new FileDto { Id = fileId + 1, Name = "Copy of file" });
        }

        public async Task<bool> FavoriteFileAsync(string userId, int fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);
            if (file == null) throw new Exception("File not found");
            file.Permissions = "favorite";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
