using Microsoft.EntityFrameworkCore;
using PagueVeloz.Domain.Entities;

using System;

namespace PagueVeloz.TransactionProcessor.Infrastructure.Database
{
    /// <summary>
    /// PagueVelozDbContext para criar o schema inicial do PagueVeloz via EF Core
    /// </summary>
    public class PagueVelozDbContext : DbContext
    {
        public PagueVelozDbContext(DbContextOptions<PagueVelozDbContext> options)
            : base(options)
        { }

        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountModel>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                entity.Property(e => e.AccountNumber)
                      .IsRequired();

                entity.HasIndex(e => e.AccountNumber)
                      .IsUnique(); 

                entity.Property(e => e.Balance).IsRequired();
                entity.Property(e => e.ReservedBalance).IsRequired();
                entity.Property(e => e.CreditLimit).IsRequired();
                entity.Property(e => e.RowVersion)
                      .IsRowVersion()
                      .IsRequired();

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired();

                entity.HasMany<TransactionModel>()
                      .WithOne()
                      .HasForeignKey(t => t.AccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            // Configurações da entidade Transaction
            modelBuilder.Entity<TransactionModel>(entity =>
            {
                entity.HasKey(e => e.TransactionId);

                entity.Property(e => e.Operation)
                      .HasConversion<string>() // salva enum como string
                      .IsRequired();

                entity.Property(e => e.AccountId).IsRequired();

                // Conta de destino (opcional)
                entity.Property(e => e.DestinationAccountId).IsRequired(false);

                entity.Property(e => e.Amount).IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
                entity.Property(e => e.ReferenceId).HasMaxLength(100).IsRequired();
                entity.HasIndex(t => t.ReferenceId).IsUnique();

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired();

                entity.Property(e => e.Balance).IsRequired();
                entity.Property(e => e.ReservedBalance).IsRequired();

                // Saldo da conta destino (opcional)
                entity.Property(e => e.DestinationBalance).IsRequired(false);

                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            });


            // Configurações da entidade IdempotencyRecord
            modelBuilder.Entity<IdempotencyRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired();
                entity.HasIndex(i => i.Key).IsUnique();
                entity.Property(i => i.Response).IsRequired();
                entity.Property(i => i.CreatedAt).IsRequired();
            });


            base.OnModelCreating(modelBuilder);
        }
    }
}
