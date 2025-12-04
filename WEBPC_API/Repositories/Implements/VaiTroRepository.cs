using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class VaiTroRepository : IVaiTroRepository
    {
        private readonly DataContext _context;

        public VaiTroRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<VaiTro>> GetAllAsync()
        {
            return await _context.VaiTros.ToListAsync();
        }

        public async Task<VaiTro?> GetByIdAsync(int id)
        {
            return await _context.VaiTros.FindAsync(id);
        }

        public async Task AddAsync(VaiTro vaiTro)
        {
            _context.VaiTros.Add(vaiTro);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VaiTro vaiTro)
        {
            _context.VaiTros.Update(vaiTro);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro != null)
            {
                _context.VaiTros.Remove(vaiTro);
                await _context.SaveChangesAsync();
            }
        }
    }
}