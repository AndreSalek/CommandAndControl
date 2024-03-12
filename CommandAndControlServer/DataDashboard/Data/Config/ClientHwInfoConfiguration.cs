using DataDashboard.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DataDashboard.Data.Config
{
    public class ClientHwInfoConfiguration : IEntityTypeConfiguration<ClientHwInfo>
    {
        public void Configure(EntityTypeBuilder<ClientHwInfo> builder)
        {
            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.MAC)
                .HasMaxLength(256)
                .IsRequired(); 

            builder.Property(ci => ci.OS)
                .HasMaxLength(256)
                .IsRequired(false);

            builder.Property(ci => ci.CpuId)
                .HasMaxLength(256)
                .IsRequired(false);

            builder.Property(ci => ci.RAMCapacity)
                .HasMaxLength(256)
                .IsRequired(false);
        }
    }
}
