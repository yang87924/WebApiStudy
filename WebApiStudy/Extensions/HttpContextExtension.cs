using System.IdentityModel.Tokens.Jwt;

namespace WebApiStudy.Extensions;

public static class HttpContextExtension
{
    public static string GetSubInToken(this HttpRequest request)
    {
        string authorization = request.Headers["Authorization"]!;
        string token = authorization ?? string.Empty;

        if (token.Length == 0) return string.Empty;
        try
        {
            // https://stackoverflow.com/questions/54778724/token-handler-unable-to-convert-the-token-to-jwt-token
            var sub = new JwtSecurityToken(token.Replace("Bearer ", string.Empty))
                .Claims
                .First(c => c.Type == "sub").Value;
            return sub;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string GetCompanyInToken(this HttpRequest request)
    {
        string authorization = request.Headers["Authorization"];
        string token = authorization ?? string.Empty;

        if (token.Length == 0) return string.Empty;
        try
        {
            // https://stackoverflow.com/questions/54778724/token-handler-unable-to-convert-the-token-to-jwt-token
            var sub = new JwtSecurityToken(token.Replace("Bearer ", string.Empty))
                .Claims
                .First(c => c.Type == "company").Value;

            return sub;
        }
        catch
        {
            return string.Empty;
        }
    }
}

