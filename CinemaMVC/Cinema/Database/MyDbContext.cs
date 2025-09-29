using Cinema.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Database
{
    public partial class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public virtual DbSet<FilmInformationModel> Films { get; set; }
        public virtual DbSet<ShowingModel> Showing { get; set; }
        public virtual DbSet<CinemaHallModel> CinemaHall { get; set; }
        public virtual DbSet<SeatsModel> Seats { get; set; }
        public virtual DbSet<TicketsModel> Tickets { get; set; }
        public virtual DbSet<LoggingModel> Logging { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalendarTime>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<CinemaHallModel>(entity =>
            {
                entity.ToTable("CinemaHall");

                entity.Property(e => e.CinemaHall_Id)
                    .ValueGeneratedNever()
                    .HasColumnName("CinemaHall_Id");
                entity.Property(e => e.HallNumber)
                    .HasMaxLength(10)
                    .IsFixedLength();
                entity.Property(e => e.Lightening)
                    .HasMaxLength(20)
                    .IsFixedLength();
            });

            modelBuilder.Entity<FilmInformationModel>(entity =>
            {
                entity.Property(e => e.Film_Id).HasColumnName("Film_Id");
                entity.Property(e => e.Age_category)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .HasColumnName("Age_category");
                entity.Property(e => e.Cast)
                    .HasMaxLength(500)
                    .IsFixedLength();
                entity.Property(e => e.Category)
                    .HasMaxLength(100)
                    .IsFixedLength();
                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsFixedLength();
                entity.Property(e => e.Director)
                    .HasMaxLength(200)
                    .IsFixedLength();
                entity.Property(e => e.Duration)
                    .HasMaxLength(20)
                    .IsFixedLength();
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsFixedLength();
                entity.Property(e => e.Production)
                    .HasMaxLength(50)
                    .IsFixedLength();
                entity.Property(e => e.Release_date).HasColumnName("Release_date");
                entity.Property(e => e.MainPicture).HasMaxLength(50);
                entity.Property(e => e.BackgroundPicture).HasMaxLength(50);
            });

            modelBuilder.Entity<LoggingModel>(entity =>
            {
                entity.HasKey(e => e.User_Id);

                entity.ToTable("Logging");

                entity.Property(e => e.User_Id)
                    .HasColumnName("User_Id");
                entity.Property(e => e.Email)
                    .HasMaxLength(30)
                    .IsFixedLength();
                entity.Property(e => e.LastLogin).HasColumnType("datetime");
                entity.Property(e => e.Login)
                    .HasMaxLength(20)
                    .IsFixedLength();
                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsFixedLength();
            });

            modelBuilder.Entity<SeatsModel>(entity =>
            {
                entity.HasKey(e => e.Seats_Id);

                entity.Property(e => e.Seats_Id)
                    .ValueGeneratedNever()
                    .HasColumnName("Seats_Id");
                entity.Property(e => e.CinemaHall_Id).HasColumnName("CinemaHall_Id");
                entity.Property(e => e.Row).HasColumnName("Row");
                entity.Property(e => e.Seat).HasColumnName("Seat");

                entity.HasOne(d => d.CinemaHall).WithMany(p => p.Seats)
                    .HasForeignKey(d => d.CinemaHall_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Seats_CinemaHall");
            });

            modelBuilder.Entity<ShowingModel>(entity =>
            {
                entity.ToTable("Showing");

                entity.Property(e => e.DateTime).HasColumnType("datetime");
                entity.Property(e => e.Showing_Id)
                    .HasColumnName("Showing_Id");
                entity.Property(e => e.CinemaHall_Id).HasColumnName("CinemaHall_Id");
                entity.Property(e => e.Film_Id).HasColumnName("Film_Id");

                entity.HasOne(d => d.CinemaHall).WithMany(p => p.Showing)
                    .HasForeignKey(d => d.CinemaHall_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Showing_CinemaHall");

                entity.HasOne(d => d.Films).WithMany(p => p.Showing)
                    .HasForeignKey(d => d.Film_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Showing_Films");
            });

            modelBuilder.Entity<TicketsModel>(entity =>
            {
                entity.Property(e => e.Ticket_Id)
                    .HasColumnName("Ticket_Id");
                entity.Property(e => e.Seats_Id).HasColumnName("Seats_Id");
                entity.Property(e => e.Showing_Id).HasColumnName("Showing_Id");
                entity.Property(e => e.User_Id).HasColumnName("User_Id");
                entity.Property(e => e.Price)
                    .HasColumnType("float")
                    .HasColumnName("Price");

                entity.HasOne(d => d.Seats).WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.Seats_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tickets_Seats");

                entity.HasOne(d => d.Showing).WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.Showing_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tickets_Showing");

                entity.HasOne(d => d.Logging).WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.User_Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tickets_Logging");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
