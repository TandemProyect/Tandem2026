namespace Tandem2026.Common.Utils
{
    /// <summary>
    /// Utilidades para manejo de archivos compartidas entre proyectos
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Verifica si un archivo existe y tiene permisos de lectura
        /// </summary>
        public static bool CanReadFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return false;

            try
            {
                using (var fs = System.IO.File.OpenRead(filePath))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
