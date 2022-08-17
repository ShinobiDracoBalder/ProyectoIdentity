using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace ProyectoIdentity.Common.Entities
{
    public class AppUsuario : IdentityUser
    {
        public string Nombre { get; set; }
        public string Url { get; set; }
        public Int32 CodigoPais { get; set; }
        public string Telefono { get; set; }
        public string Pais { get; set; }
        public string Ciudad { get; set; }
        [DataType(DataType.MultilineText)]
        [MaxLength(512)]
        [Display(Name = "Direccion")]
        public string Direccion { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public bool Estado { get; set; }

        //Nuevas propiedades para usar roles y asignación de un rol a un usuario
        [NotMapped]
        [Display(Name = "Rol para el usuario")]
        public string IdRol { get; set; }
        [NotMapped]
        public string Rol { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> ListaRoles { get; set; }

    }
}
