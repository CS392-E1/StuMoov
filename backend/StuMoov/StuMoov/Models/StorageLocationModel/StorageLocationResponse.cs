namespace StuMoov.Models.StorageLocationModel;

// Represents a standardized response structure for storage location API responses
public class StorageLocationResponse
{
    public int Status { get; set; } // HTTP status code or custom API status code

    public string Message { get; set; } // Message providing additional details about the response

    public object Data { get; set; } // Payload containing the actual response data (e.g., list of storage locations)

    // Constructor to initialize response properties
    public StorageLocationResponse(int status, string message, object data)
    {
        Status = status;
        Message = message;
        Data = data;
    }
}