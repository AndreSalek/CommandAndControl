using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataDashboard.Data.Config
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasOne(c => c.clientHwInfo) // One to one relationship with ClientHwInfo
                .WithOne()
                .HasForeignKey<ClientHwInfo>(c => c.Id)
                .IsRequired();

            builder.HasMany(c => c.SessionsHistory) //One to many relationship with SessionData
                .WithOne()
                .HasForeignKey(c => c.ClientId)
                .IsRequired();
        }
    }
}
    

