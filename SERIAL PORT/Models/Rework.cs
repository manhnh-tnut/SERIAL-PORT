namespace SERIAL_PORT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Rework")]
    public partial class Rework
    {
        public Guid id { get; set; }

        public int gen { get; set; }

        [Required]
        [StringLength(100)]
        public string fullName { get; set; }

        [Required]
        [StringLength(20)]
        public string line { get; set; }

        [Required]
        [StringLength(20)]
        public string shift { get; set; }

        [Column(TypeName = "date")]
        public DateTime date { get; set; }

        public DateTime dateTime { get; set; }

        [Required]
        [StringLength(30)]
        public string model { get; set; }

        [Required]
        [StringLength(30)]
        public string item { get; set; }

        [Required]
        [StringLength(30)]
        public string error { get; set; }

        [Required]
        [StringLength(100)]
        public string barcode { get; set; }

        [StringLength(100)]
        public string remark { get; set; }

        [StringLength(30)]
        public string group { get; set; }

        [StringLength(10)]
        public string action { get; set; }

        public int week { get; set; }

        public int month { get; set; }

        public int year { get; set; }
    }
}
