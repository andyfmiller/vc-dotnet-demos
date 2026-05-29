#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IssuerApp.Data.Models.OpenBadges
{
    public partial class Profile : Library.Models.OpenBadges.Profile
    {
        [Key]
        [JsonIgnore]
        public int ProfileKey { get; init; }

        /// <summary>
        /// Override Address to use IssuerApp's Address type with AddressKey
        /// </summary>
        public new Address? Address
        {
            get => base.Address as Address;
            set => base.Address = value;
        }

        /// <summary>
        /// Override Image to use IssuerApp's Image type with ImageKey
        /// </summary>
        public new Image? Image
        {
            get => base.Image as Image;
            set => base.Image = value;
        }
    }
}