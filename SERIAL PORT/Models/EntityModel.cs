namespace SERIAL_PORT.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class EntityModel : DbContext
    {
        public EntityModel()
            : base("name=EntityModel")
        {
        }

        public virtual DbSet<Rework> Reworks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rework>()
                .Property(e => e.line)
                .IsFixedLength();

            modelBuilder.Entity<Rework>()
                .Property(e => e.shift)
                .IsFixedLength();

            modelBuilder.Entity<Rework>()
                .Property(e => e.model)
                .IsFixedLength();

            modelBuilder.Entity<Rework>()
                .Property(e => e.item)
                .IsFixedLength();

            modelBuilder.Entity<Rework>()
                .Property(e => e.error)
                .IsFixedLength();

            modelBuilder.Entity<Rework>()
                .Property(e => e.action)
                .IsFixedLength();
        }
    }
}
