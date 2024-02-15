using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataDashboard.Data.Config
{
    public class ConnectionDataConfiguration : IEntityTypeConfiguration<ConnectionData>
    {
        public void Configure(EntityTypeBuilder<ConnectionData> builder)
        {
            builder.HasKey(sd => sd.Id);

            builder.Property(sd => sd.ClientId)
                .IsRequired();

            builder.Property(sd => sd.ConnectionId)
                .IsRequired();

			builder.Property(sd => sd.IP)
                .HasMaxLength(15)
                .IsRequired();

			builder.Property(sd => sd.ConnectedAt)
				.IsRequired();
		}
    }
}
