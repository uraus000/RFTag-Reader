namespace RF_Tag_Reader
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // // To customize application configuration such as set high DPI settings or default font,
            // // see https://aka.ms/applicationconfiguration.
            // ApplicationConfiguration.Initialize();
            // Application.Run(new MainForm());
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            Main_cls m_Main = new Main_cls();
            Application.Run();
        }    
    }
}