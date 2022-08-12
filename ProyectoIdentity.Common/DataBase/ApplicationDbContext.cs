using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Common.Entities;

namespace ProyectoIdentity.Common.DataBase
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {}
        //Agregamos los diferentes modelos que necesitamos
        public DbSet<AppUsuario> AppUsuario { get; set; }
    }
}
