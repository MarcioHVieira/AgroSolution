namespace AgroSolutions.SharedKernel.Domain.Entities;

/// <summary>
/// Entidade base com propriedades comuns
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Data de criação do registro (UTC)
    /// </summary>
    public DateTime DataCriacao { get; protected set; }

    /// <summary>
    /// Data da última atualização (UTC)
    /// </summary>
    public DateTime? DataAtualizacao { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
        DataCriacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza a data de modificação
    /// </summary>
    public void AtualizarDataModificacao()
    {
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica igualdade baseada no Id
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Retorna hash code baseado no Id
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
}
