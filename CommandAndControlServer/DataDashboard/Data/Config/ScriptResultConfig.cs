using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataDashboard.Data.Config
{
    
    public class ScriptResultConfig : IEntityTypeConfiguration<ScriptResult>
    {
        public void Configure(EntityTypeBuilder<ScriptResult> builder)
        {
            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.CommandId)
                .IsRequired();

            builder.Property(ci => ci.ClientId)
                .IsRequired(false);

            builder.Property(ci => ci.Content)
                .IsRequired(false);

            builder.Property(ci => ci.IsError)
                .IsRequired(false);
        }
    }
    
}
