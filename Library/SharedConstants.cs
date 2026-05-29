// ReSharper disable UnusedMember.Global
namespace Library
{
    public static class SharedConstants
    {
        public static class JsonLd
        {
            public static string ClrContext = "https://purl.imsglobal.org/spec/clr/v1p0/context/clr_v1p0.jsonld";
            public static string VcContext = "https://www.w3.org/2018/credentials/v1";
        }

        public static class MediaTypes
        {
            public const string Jose = "application/jose";
            public const string JsonMediaType = "application/json";
            public const string JsonLdMediaType = "application/ld+json";
        }

        public static class Scopes
        {
            public const string Delete = "https://purl.imsglobal.org/spec/clr/v1p0/scope/delete";
            public const string Readonly = "https://purl.imsglobal.org/spec/clr/v1p0/scope/readonly";
            public const string Replace = "https://purl.imsglobal.org/spec/clr/v1p0/scope/replace";

            public static string[] AllScopes =
            {
                Delete,
                Readonly,
                Replace
            };
        }
    }
}
