﻿using System.Security.Claims;

namespace ProyectoIdentity.App.Claims
{
    public static class ManejoClaims
    {
        public static List<Claim> listaClaims = new List<Claim>()
        {
            new Claim("Crear", "Crear"),
            new Claim("Editar", "Editar"),
            new Claim("Borrar", "Borrar")
        };
    }
}
