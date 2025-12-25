using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Invoice.Application.DTO;

namespace Invoice.Application.Services;

public static class ERBIntegrationService
{
    private const string BaseUrl = "http://localhost:8080/api";

    public static async Task SendInvoiceToErpAsync(InvoiceERBDTO dto)
    {
        var token = await LoginAndGetTokenAsync("admin@gmail.com", "root");

        await SendInvoice(dto, token);
    }

    private static async Task<string> LoginAndGetTokenAsync(string username, string password)
    {
        using var client = new HttpClient();

        var loginRequest = new ERBLoginRequest
        {
            Username = username,
            Password = password
        };

        var response = await client.PostAsJsonAsync(
            $"{BaseUrl}/auth/login",
            loginRequest);

        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<ERBLoginResponse>();

        return loginResponse!.Token;
    }

    private static async Task SendInvoice(
    InvoiceERBDTO dto,
    string jwtToken)
    {
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwtToken);

        using var form = new MultipartFormDataContent();

        form.Add(new StringContent(dto.InvoiceId.ToString()), "invoiceId");
        form.Add(new StringContent(dto.TicketId.ToString()), "ticketId");
        form.Add(new StringContent(dto.TicketSerial), "ticketSerial");
        form.Add(new StringContent(dto.BookingFrom.ToString("O")), "from");
        form.Add(new StringContent(dto.BookingTo.ToString("O")), "to");
        form.Add(new StringContent(dto.NumOfHours.ToString()), "numberOfHours");
        form.Add(new StringContent(dto.PlateNumber), "licensePlate");
        form.Add(new StringContent(dto.TotalAmountBeforeTax.ToString()), "amountWithoutTax");

        var tax = dto.TotalAmountAfterTax - dto.TotalAmountBeforeTax;
        form.Add(new StringContent(tax.ToString()), "taxAmount");

        form.Add(new StringContent(dto.TotalAmountAfterTax.ToString()), "amount");

        //make html as a fille
        var htmlBytes = Encoding.UTF8.GetBytes(dto.InvoiceHTMLDoc);
        var fileContent = new ByteArrayContent(htmlBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");

        form.Add(fileContent, "invoiceDoc", $"invoice_{dto.InvoiceId}.html");

        var response = await client.PostAsync(
            $"{BaseUrl}/invoices",
            form);

        response.EnsureSuccessStatusCode();
    }
}
