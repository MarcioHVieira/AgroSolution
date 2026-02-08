namespace AgroSolutions.SharedKernel.Application.DTOs;

/// <summary>
/// Resultado paginado genérico
/// </summary>
/// <typeparam name="T">Tipo dos itens</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Lista de itens da página atual
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Número da página atual (baseado em 1)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Quantidade de itens por página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de itens em todas as páginas
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Indica se há página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se há próxima página
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Cria resultado paginado vazio
    /// </summary>
    public static PaginatedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PaginatedResult<T>
        {
            Items = new List<T>(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 0
        };
    }

    /// <summary>
    /// Cria resultado paginado
    /// </summary>
    public static PaginatedResult<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
