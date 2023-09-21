namespace Yumiko.Enums
{
    public enum MediaUserStatus
    {
        [ChoiceName("Current")]
        CURRENT,
        [ChoiceName("Planning")]
        PLANNING,
        [ChoiceName("Completed")]
        COMPLETED,
        [ChoiceName("Dropped")]
        DROPPED,
        [ChoiceName("Paused")]
        PAUSED,
        [ChoiceName("Repeating")]
        REPEATING
    }
}
