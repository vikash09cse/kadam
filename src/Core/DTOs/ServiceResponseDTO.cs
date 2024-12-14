namespace Core.DTOs;
public class ServiceResponseDTO(int statusCode, object result, string message)
{
    public int StatusCode { get; } = statusCode;
    public string Message { get; } = message;
    public object Result { get; } = result;
}