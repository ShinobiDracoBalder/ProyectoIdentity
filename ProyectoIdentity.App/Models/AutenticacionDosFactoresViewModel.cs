using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ProyectoIdentity.App.Models
{
    public class AutenticacionDosFactoresViewModel
    {
        //Para el acceso (login)
        [Required]
        [Display(Name = "Código del autenticador")]
        public string Code { get; set; }

        //Para el registro
        public string Token { get; set; }

        //Para código QR
        public string UrlCodigoQR { get; set; }
    }
}
