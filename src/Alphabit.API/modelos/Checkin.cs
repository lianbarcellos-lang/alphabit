namespace Alphabit.API.modelos;

public class Checkin
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public DateTime? DataCheckin { get; set; }
    public string Status { get; set; } = "Pendente";
}
