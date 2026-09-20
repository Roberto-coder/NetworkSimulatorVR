using System;
using System.IO;

namespace Systems.Save
{
    /// <summary>Escribe primero un temporal; un fallo no deja el guardado anterior truncado.</summary>
    public static class AtomicLocalFile
    {
        public static void Write(string path, string contents)
        {
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporary, contents);
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            catch
            {
                // No ocultar el error original si tampoco se puede limpiar el temporal.
                try { if (File.Exists(temporary)) File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                throw;
            }
        }
    }
}
