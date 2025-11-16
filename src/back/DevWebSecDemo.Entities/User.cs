namespace DevWebSecDemo.Entities
{
    public class User : BaseEntity
    {
        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        public string? Password { get; set; }
    }
}
