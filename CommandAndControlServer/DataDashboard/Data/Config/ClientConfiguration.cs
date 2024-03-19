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

            builder.Property(c => c.Created)
                .IsRequired();

            builder.HasOne(c => c.ClientHwInfo) // One to one relationship with ClientHwInfo
                .WithOne()
                .HasForeignKey<ClientHwInfo>(c => c.Id)
                .IsRequired();

            builder.HasMany(c => c.ConnectionHistory) //One to many relationship with SessionData
                .WithOne()
                .HasForeignKey(c => c.ClientId)
                .IsRequired();
            
            builder.HasMany(c => c.ScriptResults) //One to many relationship with SessionData
                .WithOne()
                .HasForeignKey(c => c.ClientId)
                .IsRequired();
        }
    }
}
    

