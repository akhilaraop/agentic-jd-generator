namespace JobDescriptionAgent.Models
{
    /// <summary>
    /// Response model containing all stages and results of job description generation.
    /// </summary>
    public class JDResponse
    {
        /// <summary>
        /// The initial draft of the job description.
        /// </summary>
        public string InitialDraft { get; set; } = string.Empty;
        /// <summary>
        /// The critique feedback for the job description.
        /// </summary>
        public string CritiqueFeedback { get; set; } = string.Empty;
        /// <summary>
        /// The compliance review notes for the job description.
        /// </summary>
        public string ComplianceReview { get; set; } = string.Empty;
        /// <summary>
        /// The combined feedback from all review stages.
        /// </summary>
        public string CombinedFeedback { get; set; } = string.Empty;
        /// <summary>
        /// The final, polished job description.
        /// </summary>
        public string FinalJobDescription { get; set; } = string.Empty;
        /// <summary>
        /// Any assumptions made during the job description generation process.
        /// </summary>
        public string Assumptions { get; set; } = string.Empty;
    }
}
