namespace Library.Models.OpenBadges
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The status of an achievement result.
    /// </summary>
    public enum ResultStatusType
    {
        /// <summary>
        /// The achievement has been completed.
        /// </summary>
        [EnumMember(Value = "Completed")]
        Completed = 0,

        /// <summary>
        /// The recipient is enrolled in the achievement.
        /// </summary>
        [EnumMember(Value = "Enrolled")]
        Enrolled = 1,

        /// <summary>
        /// The recipient has failed the achievement.
        /// </summary>
        [EnumMember(Value = "Failed")]
        Failed = 2,

        /// <summary>
        /// The achievement is in progress.
        /// </summary>
        [EnumMember(Value = "InProgress")]
        InProgress = 3,

        /// <summary>
        /// The achievement is on hold.
        /// </summary>
        [EnumMember(Value = "OnHold")]
        OnHold = 4,

        /// <summary>
        /// The achievement status is provisional.
        /// </summary>
        [EnumMember(Value = "Provisional")]
        Provisional = 5,

        /// <summary>
        /// The recipient has withdrawn from the achievement.
        /// </summary>
        [EnumMember(Value = "Withdrew")]
        Withdrew = 6
    }
}