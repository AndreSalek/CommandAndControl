using DataDashboard.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using DataDashboard.ViewModels;

namespace DataDashboard.Data.Config
{
    public class ScriptConfiguration : IEntityTypeConfiguration<Script>
    {
        public void Configure(EntityTypeBuilder<Script> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Shell)
                .HasConversion<string>()
                .IsRequired();

            builder.Ignore(c => c.Lines);

            builder.Property(c => c.Name)
                .IsRequired();
        }
    }
}
