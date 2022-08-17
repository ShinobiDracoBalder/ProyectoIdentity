using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoIdentity.App.Models;
using ProyectoIdentity.Common.Entities;
using System.Text.Encodings.Web;

namespace ProyectoIdentity.App.Controllers
{
    public class CuentasController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UrlEncoder _urlEncoder;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CuentasController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, UrlEncoder urlEncoder, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
            _roleManager = roleManager;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(string returnurl = null)
        {
            //Para la creación de los roles
            if (!await _roleManager.RoleExistsAsync("Administrador"))
            {
                //Creación de rol usuario Administrador
                await _roleManager.CreateAsync(new IdentityRole("Administrador"));
            }

            //Para la creación de los roles
            if (!await _roleManager.RoleExistsAsync("Registrado"))
            {
                //Creación de rol usuario Registrado
                await _roleManager.CreateAsync(new IdentityRole("Registrado"));
            }



            ViewData["ReturnUrl"] = returnurl;
            RegistroViewModel registroVM = new RegistroViewModel();
            return View(registroVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel rgViewModel, string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var usuario = new AppUsuario { UserName = rgViewModel.Email, Email = rgViewModel.Email, Nombre = rgViewModel.Nombre, Url = rgViewModel.Url, CodigoPais = rgViewModel.CodigoPais, Telefono = rgViewModel.Telefono, Pais = rgViewModel.Pais, Ciudad = rgViewModel.Ciudad, Direccion = rgViewModel.Direccion, FechaNacimiento = rgViewModel.FechaNacimiento, Estado = rgViewModel.Estado };
                var resultado = await _userManager.CreateAsync(usuario, rgViewModel.Password);

                if (resultado.Succeeded)
                {
                    //Esta línea es para la asignación del usuario que se registra al rol "Registrado"
                    await _userManager.AddToRoleAsync(usuario, "Registrado");


                    //Implementación de confirmación de email en el registro
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
                    //var urlRetorno = Url.Action("ConfirmarEmail", "Cuentas", new { userId = usuario.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    //await _emailSender.SendEmailAsync(rgViewModel.Email, "Confirmar su cuenta - Proyecto Identity",
                    //"Por favor confirme su cuenta dando click aquí: <a href=\"" + urlRetorno + "\">enlace</a>");


                    await _signInManager.SignInAsync(usuario, isPersistent: false);
                    //return RedirectToAction("Index", "Home");
                    return LocalRedirect(returnurl);
                }

                ValidarErrores(resultado);
            }

            return View(rgViewModel);
        }

        [AllowAnonymous]
        //Manejador de errores
        private void ValidarErrores(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }

        //Método mostrar fomulario de acceso
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Acceso(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Acceso(AccesoViewModel accViewModel, string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(accViewModel.Email, accViewModel.Password, accViewModel.RememberMe, lockoutOnFailure: true);

                if (resultado.Succeeded)
                {
                    //return RedirectToAction("Index", "Home");
                    return LocalRedirect(returnurl);
                }
                if (resultado.IsLockedOut)
                {
                    return View("Bloqueado");
                }
                //Para autenticación de dos factores
                if (resultado.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(VerificarCodigoAutenticador), new { returnurl, accViewModel.RememberMe });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Acceso inválido");
                    return View(accViewModel);
                }
            }

            return View(accViewModel);
        }

        //Salir o cerrar sesión de la aplicacion (logout)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalirAplicacion()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerificarCodigoAutenticador(bool recordarDatos, string returnurl = null)
        {
            var usuario = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (usuario == null)
            {
                return View("Error");
            }

            ViewData["ReturnUrl"] = returnurl;
            return View(new VerificarAutenticadorViewModel { ReturnUrl = returnurl, RecordarDatos = recordarDatos });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> VerificarCodigoAutenticador(VerificarAutenticadorViewModel vaViewModel)
        {
            vaViewModel.ReturnUrl = vaViewModel.ReturnUrl ?? Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(vaViewModel);
            }

            var resultado = await _signInManager.TwoFactorAuthenticatorSignInAsync(vaViewModel.Code, vaViewModel.RecordarDatos, rememberClient: true);
            if (resultado.Succeeded)
            {
                return LocalRedirect(vaViewModel.ReturnUrl);
            }
            if (resultado.IsLockedOut)
            {
                return View("Bloqueado");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Código Inválido");
                return View(vaViewModel);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Denegado(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            return View();
        }


    }
}
