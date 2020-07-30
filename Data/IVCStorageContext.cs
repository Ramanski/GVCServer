using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GVCServer.Data.Entities
{
    public partial class IVCStorageContext : DbContext
    {
        public IVCStorageContext()
        {
        }

        public IVCStorageContext(DbContextOptions<IVCStorageContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Direction> Direction { get; set; }
        public virtual DbSet<OpTrain> OpTrain { get; set; }
        public virtual DbSet<OpVag> OpVag { get; set; }
        public virtual DbSet<Operation> Operation { get; set; }
        public virtual DbSet<Pfclaim> Pfclaim { get; set; }
        public virtual DbSet<PlanForm> PlanForm { get; set; }
        public virtual DbSet<Schedule> Schedule { get; set; }
        public virtual DbSet<Station> Station { get; set; }
        public virtual DbSet<Train> Train { get; set; }
        public virtual DbSet<TrainKind> TrainKind { get; set; }
        public virtual DbSet<Vagon> Vagon { get; set; }
        public virtual DbSet<VagonKind> VagonKind { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Direction>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsFixedLength();
            });

            modelBuilder.Entity<OpTrain>(entity =>
            {
                entity.HasKey(e => e.Uid);

                entity.ToTable("OP_TRAIN");

                entity.Property(e => e.Uid)
                    .HasColumnName("UID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Datop)
                    .HasColumnName("DATOP")
                    .HasColumnType("datetime");

                entity.Property(e => e.Kop)
                    .IsRequired()
                    .HasColumnName("KOP")
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.LastOper)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Msgid)
                    .HasColumnName("MSGID")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SourceStation)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.TrainId).HasColumnName("TrainID");

                entity.HasOne(d => d.KopNavigation)
                    .WithMany(p => p.OpTrain)
                    .HasPrincipalKey(p => p.Code)
                    .HasForeignKey(d => d.Kop)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OP_TRAIN_Operation");

                entity.HasOne(d => d.SourceStationNavigation)
                    .WithMany(p => p.OpTrain)
                    .HasForeignKey(d => d.SourceStation)
                    .HasConstraintName("FK_OP_TRAIN_Station");

                entity.HasOne(d => d.Train)
                    .WithMany(p => p.OpTrain)
                    .HasForeignKey(d => d.TrainId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OP_TRAIN_Train");
            });

            modelBuilder.Entity<OpVag>(entity =>
            {
                entity.HasKey(e => e.Uid);

                entity.ToTable("OP_VAG");

                entity.Property(e => e.Uid)
                    .HasColumnName("UID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.CodeOper)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("('Код операции')");

                entity.Property(e => e.DateOper)
                    .HasColumnType("datetime")
                    .HasComment("Дата и время операции");

                entity.Property(e => e.Destination)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.LastOper)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Msgid)
                    .HasColumnName("MSGID")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Дата и время сообщения");

                entity.Property(e => e.PlanForm)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("Станция элементарного назначения ПЛФ");

                entity.Property(e => e.SequenceNum).HasDefaultValueSql("((0))");

                entity.Property(e => e.Source)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("Код станции совершения операции");

                entity.Property(e => e.TrainId).HasComment("Поезд");

                entity.Property(e => e.VagonId)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("Номер вагона");

                entity.Property(e => e.WeightNetto).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.CodeOperNavigation)
                    .WithMany(p => p.OpVag)
                    .HasPrincipalKey(p => p.Code)
                    .HasForeignKey(d => d.CodeOper)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OP_VAG_Oper");

                entity.HasOne(d => d.SourceNavigation)
                    .WithMany(p => p.OpVag)
                    .HasForeignKey(d => d.Source)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OP_VAG_STA");

                entity.HasOne(d => d.Train)
                    .WithMany(p => p.OpVag)
                    .HasForeignKey(d => d.TrainId)
                    .HasConstraintName("FK_OP_VAG_ToTrain");

                entity.HasOne(d => d.Vagon)
                    .WithMany(p => p.OpVag)
                    .HasForeignKey(d => d.VagonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OP_VAG_ToCars");
            });

            modelBuilder.Entity<Operation>(entity =>
            {
                entity.HasIndex(e => e.Code)
                    .HasName("UK_Operations")
                    .IsUnique();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Mnemonic)
                    .HasMaxLength(7)
                    .IsFixedLength();

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Pfclaim>(entity =>
            {
                entity.ToTable("PFclaim");

                entity.Property(e => e.StaDestination)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.StaForm)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.StaFormNavigation)
                    .WithMany(p => p.Pfclaim)
                    .HasForeignKey(d => d.StaForm)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PFclaim_Station");
            });

            modelBuilder.Entity<PlanForm>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.FormStation)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.TargetStation)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.FormStationNavigation)
                    .WithMany(p => p.PlanForm)
                    .HasForeignKey(d => d.FormStation)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlanForm_Sta");

                entity.HasOne(d => d.TrainKindNavigation)
                    .WithMany(p => p.PlanForm)
                    .HasForeignKey(d => d.TrainKind)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlanForm_TrainKind");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.Property(e => e.ArrivalTime).HasColumnType("time(0)");

                entity.Property(e => e.DepartureTime).HasColumnType("time(0)");

                entity.Property(e => e.Station)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.Direction)
                    .WithMany(p => p.Schedule)
                    .HasForeignKey(d => d.DirectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Schedule_Direction");

                entity.HasOne(d => d.StationNavigation)
                    .WithMany(p => p.Schedule)
                    .HasForeignKey(d => d.Station)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Schedule_Station");
            });

            modelBuilder.Entity<Station>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("PK_Stations");

                entity.HasIndex(e => e.Code)
                    .HasName("UK_Stations")
                    .IsUnique();

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
                    .HasColumnName("UID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DestinationNode)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("КОД СТАНЦИИ НАЗНАЧЕНИЯ ПОЕЗДА");

                entity.Property(e => e.FormNode)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("КОД СТАНЦИИ ФОРМИРОВАНИЯ ПОЕЗДА");

                entity.Property(e => e.FormTime)
                    .HasColumnType("datetime")
                    .HasComment("ВРЕМЯ ОКОНЧАНИЯ ФОРМИРОВАНИЯ СОСТАВА");

                entity.Property(e => e.Length).HasComment("УСЛОВНАЯ ДЛИНА ПОЕЗДА");

                entity.Property(e => e.Ordinal).HasComment("ПОРЯДКОВЫЙ НОМЕР СОСТАВА");

                entity.Property(e => e.Oversize)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValueSql("((0))")
                    .HasComment("ИНДЕКС НЕГАБАРИТНОСТИ");

                entity.Property(e => e.TrainNum)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("НОМЕР ПОЕЗДА");

                entity.Property(e => e.WeightBrutto).HasComment("ВЕС БРУТТО ПОЕЗДА");

                entity.HasOne(d => d.TrainKind)
                    .WithMany(p => p.Train)
                    .HasForeignKey(d => d.TrainKindId)
                    .HasConstraintName("FK_Train_TrainKind");
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
                entity.Property(e => e.Id)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasComment("ИНВЕНТАРНЫЙ НОМЕР ВАГОНА");

                entity.Property(e => e.Ksob)
                    .HasColumnName("KSOB")
                    .HasDefaultValueSql("((99))")
                    .HasComment("КОД СОБСТВЕННИКА");

                entity.Property(e => e.Tvag)
                    .HasColumnName("TVAG")
                    .HasComment("ТАРА ВАГОНА");

                entity.HasOne(d => d.KindNavigation)
                    .WithMany(p => p.Vagon)
                    .HasForeignKey(d => d.Kind)
                    .OnDelete(DeleteBehavior.ClientSetNull)
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
