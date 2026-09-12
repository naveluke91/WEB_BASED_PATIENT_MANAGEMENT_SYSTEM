using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data
{
    /// <summary>
    /// ApplicationDbContext — kini ang pultahan sa sistema padulong sa database.
    /// Ang matag DbSet nagrepresenta sa usa ka table sa SQL Server.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        // Gipasa ang options gikan sa Program.cs (connection string, provider, etc.)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Representasyon sa Patient table sa database
        public DbSet<Patient> Patients { get; set; }

        // Representasyon sa Service table sa database
        public DbSet<Service> Services { get; set; }

        // Representasyon sa PrenatalRecords table sa database
        public DbSet<PrenatalRecord> PrenatalRecords { get; set; }

        public DbSet<NewbornRecord> NewbornRecords { get; set; }

        public DbSet<FamilyPlanningRecord> FamilyPlanningRecords { get; set; }

        // Representasyon sa Appointments table sa database
        public DbSet<Appointment> Appointments { get; set; }

        // Representasyon sa actual walk-in or appointment consultation encounter
        public DbSet<Consultation> Consultations { get; set; }

        /// <summary>
        /// I-configure ang relasyon tali sa Patient ug PrenatalRecord.
        /// Kung ma-delete ang Patient, ma-delete pud ang iyang PrenatalRecords (cascade).
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usa ka Patient mahimong naa'y daghang PrenatalRecords (one-to-many relationship)
            modelBuilder.Entity<PrenatalRecord>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.PrenatalRecords)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<FamilyPlanningRecord>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.FamilyPlanningRecords)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Usa ka Patient mahimong makapili ug daghang Services (one-to-many relationship).
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Patient)
                .WithMany(p => p.Services)
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment -> Patient relationship (optional FK)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            // Service -> Appointment: descriptive only (which appointment, if any,
            // this service was created for). Null means Walk-In.
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Appointment)
                .WithMany()
                .HasForeignKey(s => s.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Consultation -> Service: only set for a Walk-In consultation, so the
            // originating Service record can be identified without ambiguity.
            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Service)
                .WithMany()
                .HasForeignKey(c => c.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Patient)
                .WithMany()
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Appointment)
                .WithMany()
                .HasForeignKey(c => c.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // An advance appointment can create only one consultation encounter.
            modelBuilder.Entity<Consultation>()
                .HasIndex(c => c.AppointmentId)
                .IsUnique()
                .HasFilter("[AppointmentId] IS NOT NULL");
        }
    }
}
