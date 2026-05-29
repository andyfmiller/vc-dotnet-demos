using IssuerApp.Data.Models.OpenBadges;

namespace IssuerApp.Extensions
{
    public static class AddressExtensions
    {
        public static bool IsEmpty(this Address address)
        {
            return string.IsNullOrEmpty(address.AddressCountry)
                   && string.IsNullOrEmpty(address.AddressLocality)
                   && string.IsNullOrEmpty(address.AddressRegion)
                   && string.IsNullOrEmpty(address.PostalCode)
                   && string.IsNullOrEmpty(address.PostOfficeBoxNumber)
                   && string.IsNullOrEmpty(address.StreetAddress);
        }
    }
}