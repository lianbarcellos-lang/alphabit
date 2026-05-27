using System.Net.Http.Json;
using System.Text.Json;
using Alphabit.App.Models;

namespace Alphabit.App.Services;

public class AlphabitApiClient(HttpClient httpClient)
{
    public async Task<AuthResponseViewModel> RegisterUserAsync(UserCreateRequest request)
    {
        return await PostJsonAsync<UserCreateRequest, AuthResponseViewModel>("api/auth/usuarios/cadastro", request);
    }

    public async Task<AuthResponseViewModel> LoginUserAsync(UserLoginRequest request)
    {
        return await PostJsonAsync<UserLoginRequest, AuthResponseViewModel>("api/auth/usuarios/login", request);
    }

    public async Task<AuthResponseViewModel> LoginAdminAsync(AdminLoginRequest request)
    {
        return await PostJsonAsync<AdminLoginRequest, AuthResponseViewModel>("api/auth/admin/login", request);
    }

    public async Task<PasswordRecoveryResponseViewModel> RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        return await PostJsonAsync<ForgotPasswordRequest, PasswordRecoveryResponseViewModel>("api/auth/usuarios/recuperar-senha", request);
    }

    public async Task<PasswordRecoveryResponseViewModel> ResetPasswordAsync(ResetPasswordRequest request)
    {
        return await PostJsonAsync<ResetPasswordRequest, PasswordRecoveryResponseViewModel>("api/auth/usuarios/redefinir-senha", request);
    }

    public async Task<List<EventViewModel>> GetEventsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<EventViewModel>>("api/eventos") ?? [];
    }

    public async Task<EventViewModel?> GetEventByIdAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<EventViewModel>($"api/eventos/{id}");
    }

    public async Task<SeatAvailabilityViewModel> GetOccupiedSeatsAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<SeatAvailabilityViewModel>($"api/eventos/{id}/assentos-ocupados")
            ?? new SeatAvailabilityViewModel { EventoId = id };
    }

    public async Task<List<TicketTypeViewModel>> GetTicketTypesAsync(int eventId)
    {
        return await httpClient.GetFromJsonAsync<List<TicketTypeViewModel>>($"api/eventos/{eventId}/tipos-ingresso") ?? [];
    }

    public async Task<List<ActivityViewModel>> GetActivitiesAsync(int eventId, string? userCpf = null)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/eventos/{eventId}/atividades");
        if (!string.IsNullOrWhiteSpace(userCpf))
            message.Headers.Add("X-User-Cpf", userCpf);

        var response = await httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<ActivityViewModel>>() ?? [];
    }

    public async Task<List<GuestViewModel>> GetEventGuestsAsync(int eventId)
    {
        return await httpClient.GetFromJsonAsync<List<GuestViewModel>>($"api/eventos/{eventId}/convidados") ?? [];
    }

    public async Task<List<EventReviewViewModel>> GetEventReviewsAsync(int eventId)
    {
        return await httpClient.GetFromJsonAsync<List<EventReviewViewModel>>($"api/eventos/{eventId}/avaliacoes") ?? [];
    }

    public async Task<ApiResult> CreateEventReviewAsync(EventReviewRequest request, string userCpf)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/avaliacoes")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-User-Cpf", userCpf);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Avaliação registrada com sucesso." : "Não foi possível registrar a avaliação.")
        };
    }

    public async Task<List<GuestViewModel>> GetGuestsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<GuestViewModel>>("api/convidados") ?? [];
    }

    public async Task<ApiResult> CreateGuestAsync(GuestViewModel request, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/convidados")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Convidado cadastrado com sucesso." : "Não foi possível cadastrar o convidado.")
        };
    }

    public async Task<ApiResult> LinkGuestToEventAsync(int eventId, int guestId, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/eventos/{eventId}/convidados")
        {
            Content = JsonContent.Create(new EventGuestLinkRequest { ConvidadoId = guestId })
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Convidado associado ao evento." : "Não foi possível associar o convidado.")
        };
    }

    public async Task<ApiResult> UnlinkGuestFromEventAsync(int eventId, int guestId, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/eventos/{eventId}/convidados/{guestId}");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Convidado removido do evento." : "Não foi possível remover o convidado do evento.")
        };
    }

    public async Task<CheckinResponseViewModel> ValidateCheckinAsync(string qrCode, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/checkin")
        {
            Content = JsonContent.Create(new CheckinRequest { QrCode = qrCode })
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var payload = await response.Content.ReadFromJsonAsync<CheckinResponseViewModel>();
        if (payload is not null)
            return payload;

        var content = await response.Content.ReadAsStringAsync();
        return new CheckinResponseViewModel
        {
            Sucesso = response.IsSuccessStatusCode,
            Mensagem = TrimMessage(content, response.IsSuccessStatusCode ? "Check-in validado." : "Não foi possível validar o check-in.")
        };
    }

    public async Task<ApiResult> SignUpForActivityAsync(int activityId, string userCpf)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/atividades/{activityId}/inscricao")
        {
            Content = JsonContent.Create(new ActivitySignupRequest { UsuarioCpf = userCpf })
        };
        message.Headers.Add("X-User-Cpf", userCpf);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Inscrição realizada com sucesso." : "Não foi possível realizar a inscrição.")
        };
    }

    public async Task<ApiResult> CancelActivitySignupAsync(int activityId, string userCpf)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/atividades/{activityId}/inscricao/{Uri.EscapeDataString(userCpf)}");
        message.Headers.Add("X-User-Cpf", userCpf);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Inscrição cancelada com sucesso." : "Não foi possível cancelar a inscrição.")
        };
    }

    public async Task<List<EventViewModel>> GetAdminEventsAsync(string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/admin/eventos");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<EventViewModel>>() ?? [];
    }

    public async Task<List<string>> GetAdminGenresAsync(string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/admin/generos");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<string>>() ?? [];
    }

    public async Task<List<string>> GetAdminCitiesAsync(string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/admin/cidades");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<string>>() ?? [];
    }

    public async Task<ApiResult> CreateGenreAsync(MusicGenreCreateRequest request, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/admin/generos")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Categoria geek cadastrada com sucesso." : "Não foi possível cadastrar a categoria geek.")
        };
    }

    public async Task<ApiResult> UpdateGenreAsync(string currentName, MusicGenreCreateRequest request, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/admin/generos/{Uri.EscapeDataString(currentName)}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Categoria geek atualizada com sucesso." : "Não foi possível atualizar a categoria geek.")
        };
    }

    public async Task<ApiResult> DeleteGenreAsync(string currentName, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/generos/{Uri.EscapeDataString(currentName)}");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Categoria geek excluida com sucesso." : "Não foi possível excluir a categoria geek.")
        };
    }

    public async Task<ApiResult> CreateCityAsync(CityCreateRequest request, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/admin/cidades")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Cidade cadastrada com sucesso." : "Não foi possível cadastrar a cidade.")
        };
    }

    public async Task<ApiResult> UpdateCityAsync(string currentName, CityCreateRequest request, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/admin/cidades/{Uri.EscapeDataString(currentName)}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Cidade atualizada com sucesso." : "Não foi possível atualizar a cidade.")
        };
    }

    public async Task<ApiResult> DeleteCityAsync(string currentName, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/cidades/{Uri.EscapeDataString(currentName)}");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Cidade excluida com sucesso." : "Não foi possível excluir a cidade.")
        };
    }

    public async Task<ApiResult> CreateEventAsync(EventCreateRequest request, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/eventos")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Evento criado com sucesso." : "Não foi possível criar o evento.")
        };
    }

    public async Task<ApiResult> CreateCouponAsync(CouponCreateRequest request, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/cupons")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Cupom criado com sucesso." : "Não foi possível criar o cupom.")
        };
    }

    public async Task<List<CouponViewModel>> GetAdminCouponsAsync(string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/admin/cupons");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<CouponViewModel>>() ?? [];
    }

    public async Task<AdminSalesDashboardViewModel?> GetAdminSalesDashboardAsync(string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/admin/vendas/dashboard");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AdminSalesDashboardViewModel>();
    }

    public async Task<ApiResult> DeleteReviewAsync(int reviewId, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/avaliacoes/{reviewId}");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Avaliação removida com sucesso." : "Não foi possível remover a avaliação.")
        };
    }

    public async Task<ApiResult> UpdateCouponAsync(string codigo, CouponCreateRequest request, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/admin/cupons/{Uri.EscapeDataString(codigo)}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Cupom atualizado com sucesso." : "Não foi possível atualizar o cupom.")
        };
    }

    public async Task<ApiResult> DeleteCouponAsync(string codigo, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/cupons/{Uri.EscapeDataString(codigo)}");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Cupom removido com sucesso." : "Não foi possível remover o cupom.")
        };
    }

    public async Task<ApiResult> UpdateEventAsync(int id, AdminEventUpdateRequest request, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/admin/eventos/{id}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Evento atualizado com sucesso." : "Não foi possível atualizar o evento.")
        };
    }

    public async Task<ApiResult> DeleteEventAsync(int id, string adminToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/eventos/{id}");
        message.Headers.Add("X-Admin-Token", adminToken);

        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            Message = TrimMessage(content, response.IsSuccessStatusCode ? "Evento removido com sucesso." : "Não foi possível remover o evento.")
        };
    }

    public async Task<ReservationCheckoutResponseViewModel> CheckoutReservationsAsync(ReservationCheckoutRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/reservas", request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<ReservationCheckoutResponseViewModel>() ?? new ReservationCheckoutResponseViewModel
            {
                Sucesso = true,
                Mensagem = "Compra concluida com sucesso."
            };

        var content = await response.Content.ReadAsStringAsync();
        return new ReservationCheckoutResponseViewModel
        {
            Sucesso = false,
            Mensagem = TrimMessage(content, "Não foi possível concluir a compra.")
        };
    }

    public async Task<ReservationCheckoutResponseViewModel> PreviewCouponAsync(ReservationCheckoutRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/cupons/preview", request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<ReservationCheckoutResponseViewModel>() ?? new ReservationCheckoutResponseViewModel
            {
                Sucesso = true,
                Mensagem = "Resumo calculado com sucesso."
            };

        var content = await response.Content.ReadAsStringAsync();
        return new ReservationCheckoutResponseViewModel
        {
            Sucesso = false,
            Mensagem = TrimMessage(content, "Não foi possível validar o cupom.")
        };
    }

    public async Task<List<ReservationViewModel>> GetReservationsAsync(string cpf)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/reservas/{cpf}");
        message.Headers.Add("X-User-Cpf", cpf);

        var response = await httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<ReservationViewModel>>() ?? [];
    }

    public async Task<CustomerProfileViewModel?> GetUserProfileAsync(string cpf)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/usuarios/{cpf}/perfil");
        message.Headers.Add("X-User-Cpf", cpf);

        var response = await httpClient.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<CustomerProfileViewModel>();
    }

    public async Task<ApiResult<CustomerProfileViewModel>> UpdateUserProfileAsync(string cpf, CustomerProfileViewModel request)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{cpf}/perfil")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-User-Cpf", cpf);

        var response = await httpClient.SendAsync(message);

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<CustomerProfileViewModel>();
            return new ApiResult<CustomerProfileViewModel>
            {
                Success = true,
                Message = "Perfil atualizado com sucesso.",
                Data = payload
            };
        }

        var content = await response.Content.ReadAsStringAsync();
        return new ApiResult<CustomerProfileViewModel>
        {
            Success = false,
            Message = TrimMessage(content, "Não foi possível atualizar o perfil.")
        };
    }

    private async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string url, TRequest request)
        where TResponse : AuthResponseViewModel, new()
    {
        var response = await httpClient.PostAsJsonAsync(url, request);

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<TResponse>();
            return payload ?? new TResponse
            {
                Sucesso = true,
                Mensagem = "Operacao concluida."
            };
        }

        var content = await response.Content.ReadAsStringAsync();
        return new TResponse
        {
            Sucesso = false,
            Mensagem = TrimMessage(content, "Não foi possível concluir a operação.")
        };
    }

    private static string TrimMessage(string rawMessage, string fallback)
    {
        if (string.IsNullOrWhiteSpace(rawMessage))
            return fallback;

        var trimmed = rawMessage.Trim();

        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if (document.RootElement.TryGetProperty("mensagem", out var mensagemElement) &&
                    mensagemElement.ValueKind == JsonValueKind.String)
                {
                    var mensagem = mensagemElement.GetString();
                    if (!string.IsNullOrWhiteSpace(mensagem))
                        return mensagem;
                }

                if (document.RootElement.TryGetProperty("Mensagem", out var mensagemUpperElement) &&
                    mensagemUpperElement.ValueKind == JsonValueKind.String)
                {
                    var mensagem = mensagemUpperElement.GetString();
                    if (!string.IsNullOrWhiteSpace(mensagem))
                        return mensagem;
                }
            }
            catch
            {
                // Fallback to raw text below when the payload is not valid JSON.
            }
        }

        return trimmed.Trim('"');
    }
}

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
