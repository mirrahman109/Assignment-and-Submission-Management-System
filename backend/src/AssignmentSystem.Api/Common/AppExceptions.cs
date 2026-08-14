namespace AssignmentSystem.Api.Common;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>Authentication failed (bad credentials) — distinct from <see cref="ForbiddenException"/>,
/// which means "authenticated, but not allowed to touch this resource".</summary>
public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message) : base(message)
    {
    }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

public class ValidationAppException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors) : base("Validation failed")
    {
        Errors = errors;
    }

    public ValidationAppException(string field, string message)
        : base("Validation failed")
    {
        Errors = new Dictionary<string, string[]> { [field] = new[] { message } };
    }
}
