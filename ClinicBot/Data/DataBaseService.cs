using ClinicBot.Common.Models.MedicalAppoiment;
using ClinicBot.Common.Models.Qualification;
using ClinicBot.Common.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicBot.Data
{
    public class DataBaseService : DbContext, IDataBaseService
    {
        //@@@ OJO MUY IMPORTANTE SI TE DA ERROR AL MOMENTO DE CREAR LA BD DESDE NET CORE ENTONCES CREALA DESDE AZURE @@@@
        /*public DataBaseService(DbContextOptions options) : base(options)
        {
            //creo la bd si no existe
            Database.EnsureCreatedAsync();
        }

        public DataBaseService()
        {
            //creo la bd si no existe
            Database.EnsureCreatedAsync();
        }
        */

        #region BORRAR SI DESCOMENTO LA FUNCION OnConfiguring Y BORRAR TAMBIEN EN STARTUP.CS
        public DataBaseService(DbContextOptions options) : base(options)
        {
            //creo la bd si no existe
            Database.EnsureCreated();
        }

        public DataBaseService()
        {
            //creo la bd si no existe
            Database.EnsureCreated();
        }

        #endregion

        /* DESCOMENTAR SI BORRO LOS CONSTRUCTORES 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(
        "https://clinicbot-cosmo2.documents.azure.com:443/",
        "3Y2F2xZSdihwvFOv9U8SlLd4pwDTzuT4GABrQYZHP4e61Fm8MbIlLg1JIUJaejt4rFkwpOwDcksFCGsoezubyg==",
        databaseName: "botdb");*/

        //creo el dbSet
        public DbSet<UserModel> User { get; set; }
        public DbSet<QualificationModel> Qualification { get; set; }
        public DbSet<MedicalAppoimentModel> MedicalAppoiment { get; set; }


        //creo esta propiedad para confirmar el guardado de los datos
        public async Task<bool> SaveAsync()
        {
            return (await SaveChangesAsync() > 0);
        }

        //creo lo modelos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().ToContainer("User").
                         HasPartitionKey("channel").HasNoDiscriminator().HasKey("id");
            //modelBuilder.Entity<UserModel>().ToContainer("User").HasPartitionKey("channel");

            modelBuilder.Entity<QualificationModel>().ToContainer("Qualification").
                        HasPartitionKey("idUser").HasNoDiscriminator().HasKey("id");

            modelBuilder.Entity<MedicalAppoimentModel>().ToContainer("MedicalAppoiment").
                        HasPartitionKey("idUser").HasNoDiscriminator().HasKey("id");

        }
    }
}
