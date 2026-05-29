using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable UnusedMember.Global

namespace WalletApp.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The user's selected holder.
        /// </summary>
        public virtual Holder? SelectedHolder { get; set; }

        [ForeignKey(nameof(SelectedHolder))]
        public int? SelectedHolderKey { get; set; }
    }
}