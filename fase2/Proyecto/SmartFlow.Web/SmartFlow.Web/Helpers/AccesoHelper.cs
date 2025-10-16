using Microsoft.AspNetCore.Http;

namespace SmartFlow.Web.Helpers
{
    public static class AccesoHelper
    {
        public static bool EsAdmin(ISession session)
        {
            return session.GetString("UsuarioRol") == "Admin";
        }

        public static bool EsUsuario(ISession session)
        {
            return session.GetString("UsuarioRol") == "Usuario";
        }

        public static bool TieneSesion(ISession session)
        {
            return !string.IsNullOrEmpty(session.GetString("UsuarioRol"));
        }
        public static bool EsDirector(ISession session)
        {
            return session.GetString("UsuarioRol") == "Director";
        }

        public static bool EsCoordinador(ISession session)
        {
            return session.GetString("UsuarioRol") == "Coordinador";
        }

    }
}
