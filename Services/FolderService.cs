using TheDriveAPI.Models;
using TheDriveAPI.DTOs.Folder;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using TheDriveAPI.Data;

namespace TheDriveAPI.Services
{
    public interface IFolderService
    {
        Task<FolderDto> CreateFolderAsync(string userId, string name, int? parentId);
        Task<IEnumerable<FolderDto>> ListFoldersAsync(string userId, int? parentId);
        Task<bool> DeleteFolderAsync(string userId, int folderId);
        Task<FolderDto> RenameFolderAsync(string userId, int folderId, string name);
    }

    public class FolderService : IFolderService
    {
        private readonly AppDbContext _context;
        public FolderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FolderDto> CreateFolderAsync(string userId, string name, int? parentId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Folder name required");
            var exists = await _context.Folders.AnyAsync(f => f.UserId == userId && f.ParentId == parentId && f.Name == name);
            if (exists) throw new Exception("Folder name must be unique within parent");
            if (parentId.HasValue)
            {
                var parent = await _context.Folders.FirstOrDefaultAsync(f => f.Id == parentId && f.UserId == userId);
                if (parent == null) throw new Exception("Parent folder not found");
            }
            var folder = new Folder
            {
                Name = name,
                ParentId = parentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();
            return new FolderDto
            {
                Id = folder.Id,
                Name = folder.Name,
                ParentId = folder.ParentId,
                UserId = folder.UserId,
                CreatedAt = folder.CreatedAt.ToString("o"),
                UpdatedAt = folder.UpdatedAt.ToString("o")
            };
        }

        public async Task<IEnumerable<FolderDto>> ListFoldersAsync(string userId, int? parentId)
        {
            var query = _context.Folders.AsNoTracking().Where(f => f.UserId == userId);
            if (parentId.HasValue) query = query.Where(f => f.ParentId == parentId);
            else query = query.Where(f => f.ParentId == null);
            var folders = await query.OrderBy(f => f.Name).ToListAsync();
            return folders.Select(f => new FolderDto
            {
                Id = f.Id,
                Name = f.Name,
                ParentId = f.ParentId,
                UserId = f.UserId,
                CreatedAt = f.CreatedAt.ToString("o"),
                UpdatedAt = f.UpdatedAt.ToString("o")
            });
        }

        public async Task<bool> DeleteFolderAsync(string userId, int folderId)
        {
            var folder = await _context.Folders.Include(f => f.Children).FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);
            if (folder == null) throw new Exception("Folder not found");
            // Cascade delete: remove all child folders and files
            await CascadeDeleteFolder(folder);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task CascadeDeleteFolder(Folder folder)
        {
            // Delete files in this folder
            var files = await _context.Files.Where(f => f.FolderId == folder.Id).ToListAsync();
            _context.Files.RemoveRange(files);
            // Delete child folders recursively
            foreach (var child in folder.Children)
            {
                await CascadeDeleteFolder(child);
            }
            _context.Folders.Remove(folder);
        }

        public async Task<FolderDto> RenameFolderAsync(string userId, int folderId, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Folder name required");
            var folder = await _context.Folders.FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);
            if (folder == null) throw new Exception("Folder not found");
            var exists = await _context.Folders.AnyAsync(f => f.UserId == userId && f.ParentId == folder.ParentId && f.Name == name && f.Id != folderId);
            if (exists) throw new Exception("Folder name must be unique within parent");
            folder.Name = name;
            folder.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new FolderDto
            {
                Id = folder.Id,
                Name = folder.Name,
                ParentId = folder.ParentId,
                UserId = folder.UserId,
                CreatedAt = folder.CreatedAt.ToString("o"),
                UpdatedAt = folder.UpdatedAt.ToString("o")
            };
        }
    }
}
