﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using ProyectoIdentity.Common.DataBase;
using ProyectoIdentity.Common.Entities;

namespace ProyectoIdentity.App.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _contexto;

        public UsuariosController(UserManager<IdentityUser> userManager, ApplicationDbContext contexto)
        {
            _userManager = userManager;
            _contexto = contexto;
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _contexto.AppUsuario.ToListAsync();
            var rolesUsuario = await _contexto.UserRoles.ToListAsync();
            var roles = await _contexto.Roles.ToListAsync();
            foreach (var usuario in usuarios)
            {
                var rol = rolesUsuario.FirstOrDefault(u => u.UserId == usuario.Id);
                if (rol == null)
                {
                    usuario.Rol = "Ninguno";
                }
                else
                {
                    usuario.Rol = roles.FirstOrDefault(u => u.Id == rol.RoleId).Name;
                }
            }

            return View(usuarios);
        }

        //Editar usuario (Asignación de rol)
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Editar(string id)
        {
            var usuarioBD = _contexto.AppUsuario.FirstOrDefault(u => u.Id == id);
            if (usuarioBD == null)
            {
                return NotFound();
            }
            //Obtner los roles actuales del usuario 
            var rolUsuario = _contexto.UserRoles.ToList();
            var roles = _contexto.Roles.ToList();
            var rol = rolUsuario.FirstOrDefault(u => u.UserId == usuarioBD.Id);
            if (rol != null)
            {
                usuarioBD.IdRol = roles.FirstOrDefault(u => u.Id == rol.RoleId).Id;
            }
            usuarioBD.ListaRoles = _contexto.Roles.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });

            return View(usuarioBD);
        }


        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(AppUsuario usuario)
        {

            if (ModelState.IsValid)
            {
                var usuarioBD = _contexto.AppUsuario.FirstOrDefault(u => u.Id == usuario.Id);
                if (usuarioBD == null)
                {
                    return NotFound();
                }

                var rolUsuario = _contexto.UserRoles.FirstOrDefault(u => u.UserId == usuarioBD.Id);
                if (rolUsuario != null)
                {
                    //Obtener el rol actual
                    var rolActual = _contexto.Roles.Where(u => u.Id == rolUsuario.RoleId).Select(e => e.Name).FirstOrDefault();
                    //Eliminar el rol actual
                    await _userManager.RemoveFromRoleAsync(usuarioBD, rolActual);
                }

                //Agregar usuario al nuevo rol seleccionado
                await _userManager.AddToRoleAsync(usuarioBD, _contexto.Roles.FirstOrDefault(u => u.Id == usuario.IdRol).Name);
                _contexto.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            usuario.ListaRoles = _contexto.Roles.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });

            return View(usuario);
        }
        
        //Método bloquear/desbloquear usuario
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BloquearDesbloquear(string idUsuario)
        {

            var usuariBD = _contexto.AppUsuario.FirstOrDefault(u => u.Id == idUsuario);
            if (usuariBD == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (usuariBD.LockoutEnd != null && usuariBD.LockoutEnd > DateTime.Now)
            {
                //El usuario se encuentra bloqueado y lo podemos desbloquear
                usuariBD.LockoutEnd = DateTime.Now;
                TempData["Correcto"] = "Usuario desbloqueado correctamente";
            }
            else
            {
                //El usuario no está bloqueado y lo podemos bloquear
                usuariBD.LockoutEnd = DateTime.Now.AddYears(100);
                TempData["Error"] = "Usuario bloqueado correctamente";
            }

            _contexto.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        //Método para borrar usuario
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Borrar(string idUsuario)
        {

            var usuariBD = _contexto.AppUsuario.FirstOrDefault(u => u.Id == idUsuario);
            if (usuariBD == null)
            {
                return NotFound();
            }

            _contexto.AppUsuario.Remove(usuariBD);
            _contexto.SaveChanges();
            TempData["Correcto"] = "Usuario borrado correctamente";
            return RedirectToAction(nameof(Index));
        }


        //Editar perfil
        [HttpGet]
        public IActionResult EditarPerfil(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioBd = _contexto.AppUsuario.Find(id);
            if (usuarioBd == null)
            {
                return NotFound();
            }

            return View(usuarioBd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(AppUsuario appUsuario)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _contexto.AppUsuario.FindAsync(appUsuario.Id);
                usuario.Nombre = appUsuario.Nombre;
                usuario.Url = appUsuario.Url;
                usuario.CodigoPais = appUsuario.CodigoPais;
                usuario.Telefono = appUsuario.Telefono;
                usuario.Ciudad = appUsuario.Ciudad;
                usuario.Pais = appUsuario.Pais;
                usuario.Direccion = appUsuario.Direccion;
                usuario.FechaNacimiento = appUsuario.FechaNacimiento;

                await _userManager.UpdateAsync(usuario);

                return RedirectToAction(nameof(Index), "Home");
            }
            return View(appUsuario);
        }


    }
}
