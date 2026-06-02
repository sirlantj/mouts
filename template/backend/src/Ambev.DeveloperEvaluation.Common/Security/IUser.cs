namespace Ambev.DeveloperEvaluation.Common.Security;

/// <summary>
/// Contract for representing a user in the system.
/// </summary>
public interface IUser
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the username.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the user's role in the system.
    /// </summary>
    public string Role { get; }
}
