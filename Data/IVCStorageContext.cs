using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace GVCServer.Models
{
    public partial class IVCStorageContext : DbContext
    {
        public IVCStorageContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IVCStorageContext(DbContextOptions<IVCStorageContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Cars> Cars { get; set; }
        public virtual DbSet<Messages> Messages { get; set; }
        public virtual DbSet<OpVag> OpVag { get; set; }
        public virtual DbSet<Operations> Operations { get; set; }
        public virtual DbSet<Stations> Stations { get; set; }
        public virtual DbSet<Trains> Trains { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("IVCStorage"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cars>(entity =>
            {
                entity.HasKey(e => e.Nv);

                entity.Property(e => e.Nv)
                    .HasColumnName("NV")
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("ИНВЕНТАРНЫЙ НОМЕР ВАГОНА");

                entity.Property(e => e.Ksob)
                    .HasColumnName("KSOB")
                    .HasDefaultValueSql("((1))")
                    .HasComment("КОД СОБСТВЕННИКА");

                entity.Property(e => e.Otm)
                    .HasColumnName("OTM")
                    .HasComment("НЕГАБАРИТНОСТЬ, ЖИВНОСТЬ, ДЛИННОБАЗНЫЕ ВАГОНЫ, ВАГОНЫ, НЕ ");

                entity.Property(e => e.Pns)
                    .HasColumnName("PNS")
                    .HasComment("НОМЕР ВАГОНА ПО ПОРЯДКУ В СОСТАВЕ");

                entity.Property(e => e.Stnz)
                    .HasColumnName("STNZ")
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("КОД СТАНЦИИ НАЗНАЧЕНИЯ");

                entity.Property(e => e.Tvag)
                    .HasColumnName("TVAG")
                    .HasComment("ТАРА ВАГОНА");

                entity.Property(e => e.Vesgr)
                    .HasColumnName("VESGR")
                    .HasDefaultValueSql("((0))")
                    .HasComment("ВЕС ГРУЗА В ТОННАХ");
            });

            modelBuilder.Entity<Messages>(entity =>
            {
                entity.HasIndex(e => e.Code)
                    .HasName("UK_Messages")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<OpVag>(entity =>
            {
                entity.HasKey(e => e.Uid);

                entity.ToTable("OP_VAG");

                entity.Property(e => e.Uid)
                    .HasColumnName("UID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Datop)
                    .HasColumnName("DATOP")
                    .HasColumnType("datetime")
                    .HasComment("Дата и время операции");

                entity.Property(e => e.Kop)
                    .IsRequired()
                    .HasColumnName("KOP")
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("('Код операции')");

                entity.Property(e => e.Kso)
                    .HasColumnName("KSO")
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("Код станции совершения операции");

                entity.Property(e => e.Msgid)
                    .HasColumnName("MSGID")
                    .HasColumnType("datetime")
                    .HasComment("Дата и время сообщения");

                entity.Property(e => e.Nrs)
                    .HasColumnName("NRS")
                    .HasComment("Номер рейса вагона");

                entity.Property(e => e.Nv)
                    .IsRequired()
                    .HasColumnName("NV")
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("Номер вагона");

                entity.Property(e => e.Snpf)
                    .HasColumnName("SNPF")
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("Станция элементарного назначения ПЛФ");

                entity.Property(e => e.Train)
                    .HasColumnName("TRAIN")
                    .HasComment("Поезд");

                entity.HasOne(d => d.KopNavigation)
                    .WithMany(p => p.OpVag)
                    .HasPrincipalKey(p => p.Code)
                    .HasForeignKey(d => d.Kop)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OP_VAG_Oper");

                entity.HasOne(d => d.KsoNavigation)
                    .WithMany(p => p.OpVag)
                    .HasPrincipalKey(p => p.Code)
                    .HasForeignKey(d => d.Kso)
                    .HasConstraintName("FK_OP_VAG_STA");

                entity.HasOne(d => d.NvNavigation)
                    .WithMany(p => p.OpVag)
                    .HasForeignKey(d => d.Nv)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OP_VAG_Cars");

                entity.HasOne(d => d.U)
                    .WithOne(p => p.OpVag)
                    .HasForeignKey<OpVag>(d => d.Uid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OP_VAG_Trains");
            });

            modelBuilder.Entity<Operations>(entity =>
            {
                entity.HasIndex(e => e.Code)
                    .HasName("UK_Operations")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Mnemonic)
                    .HasMaxLength(7)
                    .IsFixedLength();

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Stations>(entity =>
            {
                entity.HasIndex(e => e.Code)
                    .HasName("UK_Stations")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Mnemonic)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Trains>(entity =>
            {
                entity.HasKey(e => e.Uid);

                entity.Property(e => e.Uid).HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateForm)
                    .HasColumnName("DATE_FORM")
                    .HasColumnType("datetime")
                    .HasComment("ВРЕМЯ ОКОНЧАНИЯ ФОРМИРОВАНИЯ СОСТАВА");

                entity.Property(e => e.Ksfp)
                    .IsRequired()
                    .HasColumnName("KSFP")
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("КОД СТАНЦИИ ФОРМИРОВАНИЯ ПОЕЗДА");

                entity.Property(e => e.Ksnz)
                    .IsRequired()
                    .HasColumnName("KSNZ")
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("КОД СТАНЦИИ НАЗНАЧЕНИЯ ПОЕЗДА");

                entity.Property(e => e.Ksos)
                    .IsRequired()
                    .HasColumnName("KSOS")
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("КОД СТАНЦИИ ПЕРЕДАЧИ ИНФОРМАЦИИ");

                entity.Property(e => e.Ng)
                    .HasColumnName("NG")
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("((0))")
                    .HasComment("ИНДЕКС НЕГАБАРИТНОСТИ");

                entity.Property(e => e.Np)
                    .HasColumnName("NP")
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("((9999))")
                    .HasComment("НОМЕР ПОЕЗДА");

                entity.Property(e => e.Nsos)
                    .IsRequired()
                    .HasColumnName("NSOS")
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("ПОРЯДКОВЫЙ НОМЕР СОСТАВА");

                entity.Property(e => e.Prss)
                    .HasColumnName("PRSS")
                    .HasComment("ПРИЗНАК СПИСЫВАНИЯ СОСТАВА");

                entity.Property(e => e.Usdl)
                    .HasColumnName("USDL")
                    .HasComment("УСЛОВНАЯ ДЛИНА ПОЕЗДА");

                entity.Property(e => e.Vesbr)
                    .HasColumnName("VESBR")
                    .HasComment("ВЕС БРУТТО ПОЕЗДА");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
