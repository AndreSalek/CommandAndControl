using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataDashboard.Data.Config
{
    public class SessionDataConfiguration : IEntityTypeConfiguration<SessionData>
    {
        public void Configure(EntityTypeBuilder<SessionData> builder)
        {
            builder.HasKey(sd => sd.Id);

            builder.Property(sd => sd.ClientId)
                .IsRequired();

            builder.Property(sd => sd.SessionId)
                .IsRequired();
        }
    }
}
