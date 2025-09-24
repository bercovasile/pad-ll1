
using System.Net;
using System.Text.Json.Serialization;

namespace Broker.Context.Response;
public class Response
{
	public bool Success { get; set; }
	public string Message { get; set; } = string.Empty;
	public object? Errors { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
	public HttpStatusCode Status { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Details { get; set; }

	// -----------------------------
	// Factory methods for success
	// -----------------------------
	public static Response Ok(string? details = null)
		=> new SimpleResponse { Success = true, Status = HttpStatusCode.OK, Details = details };

	public static Response Created(string? details = null)
		=> new SimpleResponse { Success = true, Status = HttpStatusCode.Created, Details = details };

	public static Response NoContent(string? details = null)
		=> new SimpleResponse { Success = true, Status = HttpStatusCode.NoContent, Details = details };

	// -----------------------------
	// Factory methods for failure
	// -----------------------------
	public static Response Fail(string? message = null, object? errors = null, HttpStatusCode status = HttpStatusCode.BadRequest, string? details = null)
		=> new SimpleResponse { Success = false, Status = status, Message = message, Errors = errors, Details = details };

	public static Response BadRequest(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.BadRequest, details);

	public static Response Unauthorized(string? details = null)
		=> Fail(null, null, HttpStatusCode.Unauthorized, details);

	public static Response Forbidden(string? details = null)
		=> Fail(null, null, HttpStatusCode.Forbidden, details);

	public static Response NotFound(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.NotFound, details);

	public static Response Conflict(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.Conflict, details);

	public static Response TooEarly(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, (HttpStatusCode)425, details);

	public static Response ServerError(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.InternalServerError, details);

	public static Response NotImplemented(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.NotImplemented, details);

	public static Response ServiceUnavailable(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.ServiceUnavailable, details);
}

// Non-generic simple response
public sealed class SimpleResponse : Response { }

// Generic response with inner data
public class Response<T> : Response
{
	public T? Data { get; set; }

	// -----------------------------
	// Factory methods for success
	// -----------------------------
	public static Response<T> Ok(T data, string? details = null)
		=> new() { Success = true, Data = data, Status = HttpStatusCode.OK, Details = details };

	public static Response<T> Created(T data, string? details = null)
		=> new() { Success = true, Data = data, Status = HttpStatusCode.Created, Details = details };

	public static Response<T> NoContent(string? details = null)
		=> new() { Success = true, Status = HttpStatusCode.NoContent, Details = details };

	// -----------------------------
	// Factory methods for failure
	// -----------------------------
	public static Response<T> Fail(string? message = null, object? errors = null, HttpStatusCode status = HttpStatusCode.BadRequest, string? details = null)
		=> new() { Success = false, Data = default, Message = message, Errors = errors, Status = status, Details = details };

	public static Response<T> Fail(T data, string? message = null, object? errors = null, HttpStatusCode status = HttpStatusCode.BadRequest, string? details = null)
	=> new() { Success = false, Data = data, Message = message, Errors = errors, Status = status, Details = details };

	public static Response<T> BadRequest(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.BadRequest, details);

	public static Response<T> Unauthorized(string? details = null)
		=> Fail(null, null, HttpStatusCode.Unauthorized, details);

	public static Response<T> Forbidden(string? details = null)
		=> Fail(null, null, HttpStatusCode.Forbidden, details);

	public static Response<T> NotFound(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.NotFound, details);

	public static Response<T> Conflict(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.Conflict, details);

	public static Response<T> TooEarly(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, (HttpStatusCode)425, details);

	public static Response<T> ServerError(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.InternalServerError, details);

	public static Response<T> NotImplemented(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.NotImplemented, details);

	public static Response<T> ServiceUnavailable(string? message = null, object? errors = null, string? details = null)
		=> Fail(message, errors, HttpStatusCode.ServiceUnavailable, details);
}
