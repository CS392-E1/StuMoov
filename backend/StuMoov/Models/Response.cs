/**
 * Response.cs
 *
 * Defines a standardized response structure for API responses in the StuMoov application.
 * This model encapsulates the status, message, and data payload for consistent API communication.
 */

namespace StuMoov.Models
{
    /// <summary>
    /// Represents a standardized response structure for API responses.
    /// Used to encapsulate status, message, and data for consistent communication.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// The HTTP status code or custom API status code indicating the response outcome.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// A message providing additional details or context about the response.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The payload containing the actual response data, such as a list of storage locations.
        /// Nullable to accommodate responses without data.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Constructs a Response with the specified status, message, and data.
        /// </summary>
        /// <param name="status">The HTTP or custom API status code.</paramHealth
        /// <param name="message">A descriptive message about the response.</param>
        /// <param name="data">The response data payload, if any.</param>
        public Response(int status, string message, object? data)
        {
            Status = status;
            Message = message;
            Data = data;
        }
    }
}