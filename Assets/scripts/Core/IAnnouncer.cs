namespace CrystalFlux.UISystem
{
    public interface IAnnouncer
    {
        public static IAnnouncer Current { get; set; }
        void DisableTitle();
        void DisableSubtitle();
        void SetTitleForDuration(string text, float duration, float fadeIn, float fadeOut);
        void SetSubtitleForDuration(string text, float duration, float fadeIn, float fadeOut);
    }
}