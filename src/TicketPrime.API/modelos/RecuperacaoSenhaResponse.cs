namespace TicketPrime.API.modelos;

public class RecuperacaoSenhaResponse
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string EmailMascarado { get; set; } = string.Empty;
}
