#nullable enable

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssuerApp.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The user's selected issuer organization.
        /// </summary>
        public virtual Organization? SelectedOrganization { get; set; }
        [ForeignKey(nameof(SelectedOrganization))]
        public int? SelectedOrganizationKey { get; set; }

        /// <summary>
        /// The member the user is acting as (used when the role is Holder).
        /// </summary>
        public virtual Member? SelectedMember { get; set; }
        [ForeignKey(nameof(SelectedMember))]
        public int? SelectedMemberKey { get; set; }
    }
}
