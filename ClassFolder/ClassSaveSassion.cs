using EventApp.DataFolder;
using System;
using System.IO;
using System.Linq;

namespace EventApp.ClassFolder
{
    internal class ClassSaveSassion
    {
        public static void SaveSassion(User user)
        {
            string filePath = "session.txt";
            StreamWriter writer = new StreamWriter(filePath, false);
            try
            {
                writer.WriteLine($"IdUser={user.IdUser}");
                writer.WriteLine($"Login={user.Login}");
                writer.WriteLine($"Role={user.IdRole}");
                writer.WriteLine($"SessionStart={DateTime.Now}");
            }
            finally
            {
                writer.Close(); // Явное закрытие
            }
        }
        public static User LoadSession()
        {
            string filePath = "session.txt";
            if (!File.Exists(filePath))
            {
                return null;
            }
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                int idLogin = int.Parse(lines.FirstOrDefault(line => line.StartsWith("IdUser="))?.Split('=')[1] ?? "0");
                string login = lines.FirstOrDefault(line => line.StartsWith("Login="))?.Split('=')[1];
                int idRole = int.Parse(lines.FirstOrDefault(line => line.StartsWith("Role="))?.Split('=')[1] ?? "0");

                var user = EventEntities.GetContext().User.FirstOrDefault(u => u.IdUser == idLogin && u.Login == login);
                return user;
            }
            catch (Exception ex)
            {
                MBClass.ErrorMB(ex);
                return null;
            }
        }
        public static void ClearSession()
        {
            string filePath = "session.txt";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
