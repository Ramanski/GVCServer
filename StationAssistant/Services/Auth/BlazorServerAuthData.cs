using System;

namespace StationAssistant
{
    public class BlazorServerAuthData
    {
        public string SubjectId;
        public DateTimeOffset Expiration;
        public string AccessToken;
        public string RefreshToken;
    }
}