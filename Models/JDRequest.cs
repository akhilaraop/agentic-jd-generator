namespace JobDescriptionAgent.Models
{
    /// <summary>
    /// Request model for generating a job description.
    /// </summary>
    public class JDRequest
    {
        /// <summary>
        /// The initial input or requirements for the job description.
        /// </summary>
        public string InitialInput { get; set; } = string.Empty;
    }
}
