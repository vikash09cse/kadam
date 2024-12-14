namespace Core.DTOs;
public class ServiceResponseDTO(bool success, int statusCode, object result, string message)
{
    public bool Success { get; } = success;
    public int StatusCode { get; } = statusCode;
    public string Message { get; } = message;
    public object Result { get; } = result;
}