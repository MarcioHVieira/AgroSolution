using AgroSolutions.Identidade.Domain.Entities;
using AgroSolutions.Identidade.Domain.Interfaces;
using AgroSolutions.Identidade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Identidade.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IdentidadeDbContext _context;

    public UsuarioRepository(IdentidadeDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.CodigosValidacao)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.CodigosValidacao)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Cpf == cpf, cancellationToken);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Usuario>> ListarTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .OrderByDescending(u => u.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Usuario>> ObterMarcadosParaExclusaoAsync(DateTime dataLimite, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Where(u => u.Excluido && u.DataExclusao.HasValue && u.DataExclusao.Value <= dataLimite)
            .ToListAsync(cancellationToken);
    }
}
