namespace AgroSolutions.Propriedades.Domain.Enums;

public enum TipoPropriedade
{
    Fazenda = 1,
    Sitio = 2,
    Chacara = 3,
    GranjaAvicola = 4,
    GranjaSuina = 5,
    Horta = 6,
    Pomar = 7,
    Pasto = 8,
    Outros = 99
}

public enum StatusPropriedade
{
    Ativa = 1,
    Inativa = 2,
    EmManutencao = 3
}

public enum StatusTalhao
{
    Disponivel = 1,
    EmUso = 2,
    EmDescanso = 3,
    EmRecuperacao = 4
}

public enum TipoCultura
{
    Soja = 1,
    Milho = 2,
    Trigo = 3,
    Arroz = 4,
    Feijao = 5,
    Cafe = 6,
    CanaDeAcucar = 7,
    Algodao = 8,
    Mandioca = 9,
    Tomate = 10,
    Alface = 11,
    Cenoura = 12,
    Batata = 13,
    Cebola = 14,
    Laranja = 15,
    Banana = 16,
    Uva = 17,
    Manga = 18,
    Abacaxi = 19,
    Outros = 99
}

public enum StatusCultura
{
    Ativa = 1,
    Colhida = 2,
    Cancelada = 3,
    Perdida = 4
}
