using System.Net.Http.Headers;
using System.Text;

namespace AzureContributionsApi.Utils;

public static class AuthHelpers
{
    // Accept Basic <base64(:PAT)> or Bearer <PAT>
    public static (string Scheme, string Value)? ExtractAuthDetails(string authHeader)
    {
        try
        {
            var headerValue = AuthenticationHeaderValue.Parse(authHeader);
            
            if (string.Equals(headerValue.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
            {
                return ("Bearer", headerValue.Parameter?.Trim() ?? string.Empty);
            }
            
            if (string.Equals(headerValue.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(headerValue.Parameter))
                    return null;

                string decoded;
                try { 
                    decoded = Encoding.UTF8.GetString(Convert.FromBase64String(headerValue.Parameter)); 
                } catch { return null; }

                // decoded expected format ":PAT" or "username:PAT"
                var idx = decoded.IndexOf(':');
                if (idx < 0) return ("Basic", decoded); // fallback
                var pat = decoded[(idx + 1)..];
                return string.IsNullOrEmpty(pat) ? null : ("Basic", pat);
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }
}
