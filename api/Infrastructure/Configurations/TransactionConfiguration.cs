using Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.MerchantName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Description).HasMaxLength(200);
        builder.Property(t => t.Amount).IsRequired().HasPrecision(18, 2);
        builder.Property(t => t.AmountInPLN).IsRequired().HasPrecision(18, 2);
        builder.Property(t => t.Currency).IsRequired().HasConversion<string>().HasMaxLength(3);
        builder.Property(t => t.Type).IsRequired().HasConversion<string>();
        builder.Property(t => t.Date).IsRequired();

        builder.HasIndex(t => t.Date);
    }
}
