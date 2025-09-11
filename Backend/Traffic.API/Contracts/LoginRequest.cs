namespace Traffic.API.Contracts
{
    public record LoginRequest(
        string Username,
        string Password);

    public record ChangePasswordRequest(
        string OldPassword,
        string NewPassword);
}
