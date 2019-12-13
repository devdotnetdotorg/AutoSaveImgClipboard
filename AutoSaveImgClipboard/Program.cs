using AutoSaveImgClipboard.Helper;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace AutoSaveImgClipboard
{
    static class Program
    {
        public static Logger log;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                //Logs
                log = LogManager.GetCurrentClassLogger();
                log.Trace("Version: {0}", Environment.Version.ToString());
                log.Trace("OS: {0}", Environment.OSVersion.ToString());
                log.Trace("Command: {0}", Environment.CommandLine.ToString());
            }
            catch (Exception e)
            {
                    MessageBox.Show("Ошибка работы с логом!\n" + e.Message);
            }
            // ловим все не обработанные исключения
            Application.ThreadException += new ThreadExceptionEventHandler(Application_Exception);
            //Run
            //Lang
            List<string> listAvailableLang = new List<string>() { "en-US", "ru-RU" };
            //test
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            //end test
            //Zero index for default language
            //If there is no language available, the default setting
            CultureInfo cultureCurrent = Thread.CurrentThread.CurrentCulture;
            //
            if (!listAvailableLang.Contains(cultureCurrent.Name))
            {
                //set default language
                var hlpLang = new LanguageChange();
                hlpLang.ChangeLanguage(listAvailableLang[0]);
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmConfig(listAvailableLang));
        }
        static void Application_Exception(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            log.Error(e.Exception,e.Exception.Message);
            MessageBox.Show(e.Exception.Message,"Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
            //Exit App
            Application.Exit();
        }
    }
}
