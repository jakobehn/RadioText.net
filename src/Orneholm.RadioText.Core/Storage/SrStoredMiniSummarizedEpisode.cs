using System;

namespace Orneholm.RadioText.Core.Storage
{
    public class SrStoredMiniSummarizedEpisode
    {
        public int EpisodeId { get; set; }

        public string OriginalAudioUrl { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public EnrichedText? Title_EN { get; set; }

        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime PublishDateUtc { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int ProgramId { get; set; }
        public string ProgramName { get; set; } = string.Empty;

        public EnrichedText? Transcription_Original { get; set; }
        public EnrichedText? Transcription_English { get; set; }

        public string SpeechUrl_EN { get; set; } = string.Empty;
    }
}
