using Microsoft.AspNetCore.Http;

namespace SmartFlow.Web.Helpers
{
    public static class AccesoHelper
    {
        /// <summary>
        /// Verifica si el usuario tiene sesión activa.
        /// </summary>
        public static bool TieneSesion(ISession session)
        {
            return !string.IsNullOrEmpty(session.GetString("UsuarioRol"));
        }

        /// <summary>
        /// Devuelve true si el usuario actual tiene el rol Admin.
        /// </summary>
        public static bool EsAdmin(ISession session)
        {
            return session.GetString("UsuarioRol") == "Admin";
        }

        /// <summary>
        /// Devuelve true si el usuario actual tiene el rol Usuario.
        /// </summary>
        public static bool EsUsuario(ISession session)
        {
            return session.GetString("UsuarioRol") == "Usuario";
        }

        /// <summary>
        /// Devuelve true si el usuario actual tiene el rol Director.
        /// </summary>
        public static bool EsDirector(ISession session)
        {
            return session.GetString("UsuarioRol") == "Director";
        }

        public static bool EsCoordinador(ISession session)
        {
            return session.GetString("UsuarioRol") == "Coordinador";
        }


        /// <summary>
        /// Verifica si el rol actual del usuario está dentro de los roles permitidos.
        /// </summary>
        /// <param name="session">Objeto de sesión actual</param>
        /// <param name="rolesPermitidos">Array de roles con permiso</param>
        /// <returns>True si el rol tiene acceso, False en caso contrario</returns>
        public static bool TieneAcceso(ISession session, string[] rolesPermitidos)
        {
            var rolActual = session.GetString("UsuarioRol");

            if (string.IsNullOrEmpty(rolActual))
                return false;

            foreach (var rol in rolesPermitidos)
            {
                if (rolActual.Equals(rol, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
