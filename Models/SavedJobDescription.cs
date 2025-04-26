using System;

namespace JobDescriptionAgent.Models
{
    /// <summary>
    /// Model representing a saved job description in the database.
    /// </summary>
    public class SavedJobDescription
    {
        /// <summary>
        /// The unique identifier for the saved job description.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The title of the job description.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// The generated job description text.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// The initial input or requirements provided by the user.
        /// </summary>
        public string InitialInput { get; set; } = string.Empty;
        /// <summary>
        /// The date and time when the job description was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// The dictionary containing all workflow stages and their outputs.
        /// </summary>
        public Dictionary<string, string> Stages { get; set; } = new Dictionary<string, string>();
    }
} 