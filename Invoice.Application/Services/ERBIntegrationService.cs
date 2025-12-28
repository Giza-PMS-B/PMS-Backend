using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Invoice.Application.DTO;

namespace Invoice.Application.Services;

public static class ERBIntegrationService
{
    private const string BaseUrl = "http://host.docker.internal:5230";
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri(BaseUrl)
    };

    public static async Task SendInvoiceToErpAsync(InvoiceERBDTO dto)
    {
        var token = await LoginAndGetTokenAsync("admin@gmail.com", "root");
        Console.WriteLine("Token" + token);
        await SendInvoice(dto, token);
    }

    private static async Task<string> LoginAndGetTokenAsync(string email, string password)
    {
        var loginPayload = new
        {
            email,
            password
        };

        var json = JsonSerializer.Serialize(loginPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/Auth/login", content);
        
        //response.EnsureSuccessStatusCode();
        if (!response.IsSuccessStatusCode)
        {
            var errormess = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{errormess}  ---- {response.StatusCode}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);
        if (!doc.RootElement.TryGetProperty("token", out var tokenElement))
            throw new Exception("Token not found in login response.");

        return tokenElement.GetString()!;
    }

    private static async Task SendInvoice(
    InvoiceERBDTO dto,
    string jwtToken)
    {
        var erpRequest = new
        {
            InvoiceHTMLDocument = dto.InvoiceHTMLDoc,
            InvoiceId = dto.InvoiceId,
            TicketId = dto.TicketId,
            TicketSerial = dto.TicketSerial,
            FromDate = dto.BookingFrom,
            ToDate = dto.BookingTo,
            PlateNumber = dto.PlateNumber,
            TotalAmountWithOutTax = dto.TotalAmountBeforeTax,
            TaxAmount = dto.TotalAmountAfterTax - dto.TotalAmountBeforeTax
        };

        var json = JsonSerializer.Serialize(erpRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/Invoice")
        {
            Content = content
        };
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", jwtToken);

        var response = await _httpClient.SendAsync(request);
        //response.EnsureSuccessStatusCode();
        if (!response.IsSuccessStatusCode)
        {
            var errormess=await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{errormess}  ---- {response.StatusCode}" );
        }
    }

}
