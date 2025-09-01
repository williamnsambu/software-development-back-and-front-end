namespace DevPulse.Application.Dtos.OAuth
{
    public sealed record GithubUserDto(
        long Id,
        string Login,
        string Name,
        string AvatarUrl,
        string HtmlUrl,
        string Email
    );
}