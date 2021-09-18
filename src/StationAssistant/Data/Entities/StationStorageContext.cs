using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StationAssistant.Data.Entities
{
    public partial class StationStorageContext : DbContext
    {
        public StationStorageContext()
        {
        }

        public StationStorageContext(DbContextOptions<StationStorageContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Direction> Direction { get; set; }
        public virtual DbSet<Path> Path { get; set; }
        public virtual DbSet<Station> Station { get; set; }
        public virtual DbSet<Train> Train { get; set; }
        public virtual DbSet<TrainKind> TrainKind { get; set; }
        public virtual DbSet<Vagon> Vagon { get; set; }
        public virtual DbSet<VagonKind> VagonKind { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Direction>(entity =>
            {
                entity.HasKey(e => e.StationDestination);

                entity.Property(e => e.StationDestination)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.PreferenceAreaArrival)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength();

                entity.Property(e => e.Track)
                    .HasMaxLength(100)
                    .IsFixedLength();
            });

            modelBuilder.Entity<Path>(entity =>
            {
                entity.Property(e => e.Area)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength();

                entity.Property(e => e.Marks).HasMaxLength(200);

                entity.Property(e => e.PathNum)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Pfdirection).HasColumnName("PFDirection");
            });

            modelBuilder.Entity<Station>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("PK_Stations");

                entity.Property(e => e.Code)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Mnemonic)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Train>(entity =>
            {
                entity.HasKey(e => e.Uid);

                entity.Property(e => e.Uid)
                    .HasColumnName("UID");

                entity.Property(e => e.DateOper).HasColumnType("datetime");

                entity.Property(e => e.DestinationStation)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.FormStation)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.FormTime).HasColumnType("datetime");

                entity.Property(e => e.Oversize)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Num)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.DestinationStationNavigation)
                    .WithMany(p => p.Train)
                    .HasForeignKey(d => d.DestinationStation)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Train_Station");

                entity.HasOne(d => d.Path)
                    .WithMany(p => p.Train)
                    .HasForeignKey(d => d.PathId)
                    .HasConstraintName("FK_Train_Path");
            });

            modelBuilder.Entity<TrainKind>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.Property(e => e.Mnemocode)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsFixedLength();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<Vagon>(entity =>
            {
                entity.HasKey(e => e.Num);

                entity.Property(e => e.Num)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.DateOper).HasColumnType("datetime");

                entity.Property(e => e.Destination)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Ksob)
                    .HasColumnName("KSOB")
                    .HasMaxLength(8);

                entity.Property(e => e.PlanForm)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SequenceNum).HasDefaultValueSql("((0))");

                entity.Property(e => e.TrainId);

                entity.Property(e => e.Tvag).HasColumnName("TVAG");

                entity.Property(e => e.WeightNetto).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Path)
                    .WithMany(p => p.Vagon)
                    .HasForeignKey(d => d.PathId)
                    .HasConstraintName("FK_Direction_Path");

                entity.HasOne(d => d.PlanFormNavigation)
                    .WithMany(p => p.Vagon)
                    .HasForeignKey(d => d.PlanForm)
                    .HasConstraintName("FK_Vagon_Station");

                entity.HasOne(d => d.TrainIndexNavigation)
                    .WithMany(p => p.Vagon)
                    .HasForeignKey(d => d.TrainId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Vagon_Train");

                entity.HasOne(d => d.VagonKind)
                    .WithMany(p => p.Vagon)
                    .HasForeignKey(d => d.VagonKindId)
                    .HasConstraintName("FK_Vagon_VagonKind");
            });

            modelBuilder.Entity<VagonKind>(entity =>
            {
                entity.Property(e => e.Mnemocode)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(90);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
