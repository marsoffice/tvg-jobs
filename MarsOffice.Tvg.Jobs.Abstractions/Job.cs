using System;

namespace MarsOffice.Tvg.Jobs.Abstractions
{
    public class Job
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public int? PreferredDurationInSeconds { get; set; }
        public bool? Disabled { get; set; }
        public string Cron { get; set; }
        public string ContentType { get; set; }
        public string ContentTopic { get; set; }
        public DateTimeOffset? ContentStartDate { get; set; }
        public int? ContentMinChars { get; set; }
        public int? ContentMaxChars { get; set; }
        public bool? ContentIncludeLinks { get; set; }
        public int? MinPosts { get; set; }
        public int? MaxPosts { get; set; }
        public float? SpeechPitch { get; set; }
        public float? SpeechSpeed { get; set; }
        public string SpeechLanguage { get; set; }
        public string SpeechType { get; set; }
        public long? PauseBeforeInMillis { get; set; }
        public long? PauseAfterInMillis { get; set; }
        public int? AudioBackgroundQuality { get; set; }
        public float? AudioBackgroundVolumeInPercent { get; set; }
        public string VideoBackgroundResolution { get; set; }
        public string TextFontFamily { get; set; }
        public float? TextFontSize { get; set; }
        public string TextBoxColor { get; set; }
        public float? TextBoxOpacity { get; set; }
        public string TextBoxBorderColor { get; set; }
        public string AutoUpload { get; set; }
        public string PostDescription { get; set; }
        public string EditorVideoResolution { get; set; }
    }
}