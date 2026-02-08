using AgroSolutions.Notificacoes.Domain.Entities;
using AgroSolutions.Notificacoes.Domain.Enums;
using AgroSolutions.Notificacoes.Domain.Interfaces;
using AgroSolutions.Notificacoes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Notificacoes.Infrastructure.Repositories;

public class NotificacaoRepository : INotificacaoRepository
{
    private readonly NotificacoesDbContext _context;

    public NotificacaoRepository(NotificacoesDbContext context) => _context = context;

    public async Task<Notificacao?> ObterPorIdAsync(Guid id) => await _context.Notificacoes.FindAsync(id);

    public async Task<IEnumerable<Notificacao>> ObterTodasAsync() => 
        await _context.Notificacoes.OrderByDescending(n => n.DataCriacao).ToListAsync();

    public async Task<IEnumerable<Notificacao>> ObterPorDestinatarioAsync(Guid destinatarioId) =>
        await _context.Notificacoes.Where(n => n.DestinatarioId == destinatarioId)
            .OrderByDescending(n => n.DataCriacao).ToListAsync();

    public async Task<IEnumerable<Notificacao>> ObterPorStatusAsync(StatusNotificacao status) =>
        await _context.Notificacoes.Where(n => n.Status == status).ToListAsync();

    public async Task<IEnumerable<Notificacao>> ObterPendentesAsync() =>
        await _context.Notificacoes
            .Where(n => n.Status == StatusNotificacao.Pendente || n.Status == StatusNotificacao.Reenviando)
            .OrderBy(n => n.Prioridade).ThenBy(n => n.DataCriacao).ToListAsync();

    public async Task<Notificacao> AdicionarAsync(Notificacao notificacao)
    {
        _context.Notificacoes.Add(notificacao);
        await _context.SaveChangesAsync();
        return notificacao;
    }

    public async Task AtualizarAsync(Notificacao notificacao)
    {
        _context.Notificacoes.Update(notificacao);
        await _context.SaveChangesAsync();
    }

    public async Task DeletarAsync(Guid id)
    {
        var notificacao = await ObterPorIdAsync(id);
        if (notificacao != null)
        {
            _context.Notificacoes.Remove(notificacao);
            await _context.SaveChangesAsync();
        }
    }
}
