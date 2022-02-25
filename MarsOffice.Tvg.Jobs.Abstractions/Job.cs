using System;

namespace MarsOffice.Tvg.Jobs.Abstractions
{
    public class Job
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string Name { get; set; }
        public long? FinalFileDurationInMillis { get; set; }
        public bool? TrimGracefullyToMaxDuration { get; set; }
        public bool? Disabled { get; set; }
        public string Cron { get; set; }
        public string ContentType { get; set; }
        public string ContentTopic { get; set; }
        public string SelectedContent { get; set; }
        public bool? ContentGetLatestPosts { get; set; }
        public DateTimeOffset? ContentStartDate { get; set; }
        public int? ContentMinChars { get; set; }
        public int? ContentMaxChars { get; set; }
        public string ContentTranslateFromLanguage { get; set; }
        public string ContentTranslateToLanguage { get; set; }
        public int? ContentNoOfIncludedTopComments { get; set; }
        public bool? ContentIncludeLinks { get; set; }
        public int? ContentMinPosts { get; set; }
        public int? ContentMaxPosts { get; set; }
        public int? SpeechPitch { get; set; }
        public int? SpeechSpeed { get; set; }
        public string SpeechType { get; set; }
        public string SpeechLanguage {get;set;}
        public long? SpeechPauseBeforeInMillis { get; set; }
        public long? SpeechPauseAfterInMillis { get; set; }
        public int? AudioBackgroundQuality { get; set; }
        public string SelectedAudioBackground { get; set; }
        public int? AudioBackgroundVolumeInPercent { get; set; }
        public string VideoBackgroundResolution { get; set; }
        public string SelectedVideoBackground { get; set; }
        public string TextFontFamily { get; set; }
        public int? TextFontSize { get; set; }
        public string TextBoxColor { get; set; }
        public string TextColor { get; set; }
        public int? TextBoxOpacity { get; set; }
        public string TextBoxBorderColor { get; set; }
        public bool? DisabledAutoUpload { get; set; }
        public string PostDescription { get; set; }
        public string EditorVideoResolution { get; set; }
        public string AutoUploadTikTokAccounts { get; set; }
    }
}