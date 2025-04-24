namespace JobDescriptionAgent.Models
{
    public class JDResponse
    {
        public string InitialDraft { get; set; } = string.Empty;
        public string CritiqueFeedback { get; set; } = string.Empty;
        public string ComplianceReview { get; set; } = string.Empty;
        public string CombinedFeedback { get; set; } = string.Empty;
        public string FinalJobDescription { get; set; } = string.Empty;
        public string Assumptions { get; set; } = string.Empty;
    }
}
