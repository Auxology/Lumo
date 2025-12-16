namespace SharedKernel;

public enum FaultKind
{
    None = 0,
    Failure = 1,
    Problem = 2,
    Validation = 3,
    Conflict = 4,
    NotFound = 5,
    Unauthorized = 6,
    Forbidden = 7,
    TooManyRequests = 8
}