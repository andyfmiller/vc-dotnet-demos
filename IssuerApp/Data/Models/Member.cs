#nullable enable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace IssuerApp.Data.Models
{
    public class Member
    {
        [Key]
        public int MemberKey { get; init; }

        public int OrganizationKey { get; set; }

        [Display(Description = "The organization that the member belongs to.")]
        [Required]
        [ValidateNever]
        public virtual Organization Organization { get; set; } = null!;

        /// <summary>
        /// The display name of the member.
        /// </summary>
        [Display(Description = "The name of the member. This name will not appear in verifiable credentials.")]
        [Required]
        public required string Name { get; set; }
    }
}
