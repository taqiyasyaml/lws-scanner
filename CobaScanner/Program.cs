using System.Configuration;
namespace CobaScanner
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            bool MutexCreated = false;
            Mutex AppMutex = new Mutex(true, ConfigurationManager.AppSettings["MutexID"], out MutexCreated);
            if(MutexCreated)
            {
                Application.Run(new Form1());
                AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) =>
                {
                    AppMutex.ReleaseMutex();
                };
            }
            else
            {
                MessageBox.Show("The application is already started. Please check the application icon on right bottom taskbar or end the application with Task Manager or re-run application after few second.", "ERROR!");
            }
        }
    }
}