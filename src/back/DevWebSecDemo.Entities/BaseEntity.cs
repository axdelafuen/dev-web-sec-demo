using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevWebSecDemo.Entities
{
    /// <summary>
    /// Base class for every entity used in Shizima database
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity's id.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}
