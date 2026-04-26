// File: HttpClients/BookingServiceClient.cs
using System.Text.Json;

namespace MedicalAPI.HttpClients;

/// <summary>HTTP client for communicating with Booking Service.</summary>
public class BookingServiceClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public BookingServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>Fetches appointment details from Booking Service.</summary>
    /// <returns>AppointmentDto if found, null if not found.</returns>
    public async Task<AppointmentDto?> GetAppointmentAsync(Guid appointmentId)
    {
        var response = await _httpClient.GetAsync($"appointments/{appointmentId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<BookingResponseWrapper>(content, _jsonOptions);
        return wrapper?.Data;
    }
}

public class BookingResponseWrapper
{
    public bool Success { get; set; }
    public AppointmentDto? Data { get; set; }
}

public class AppointmentDto
{
    public Guid AppointmentId { get; set; }
    public int Status { get; set; }
}