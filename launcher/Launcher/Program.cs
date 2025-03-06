using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.ExitCode = 1;
            string appName = "hacktv-gui launcher";
            string noJRE = "Unable to find a Java Runtime Environment.";
            string unsupportedJRE = "Unsupported Java Runtime Environment found. Version 11 or higher is required.";
            string jreErr = "An error occurred while launching the Java Runtime Environment.";
            string jreVer;
            string runPath;
            string appDir = AppDomain.CurrentDomain.BaseDirectory;

            if (Directory.Exists(Path.Combine(appDir, "jre")))
            {
                runPath = Path.Combine(appDir, "jre");
                // jreVer = FileVersionInfo.GetVersionInfo(Path.Combine(runPath, "bin", "java.exe")).FileVersion;
            }
            else
            {
                // Try to get JRE or JDK version from registry. Try 64-bit first, then 32-bit.
                string regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\JRE";
                jreVer = QueryRegistry(regPath);
                if (jreVer == null)
                {
                    regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\JavaSoft\JRE";
                    jreVer = QueryRegistry(regPath);
                }
                if (jreVer == null)
                {
                    regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\JDK";
                    jreVer = QueryRegistry(regPath);
                }
                if (jreVer == null)
                {
                    regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\JavaSoft\JDK";
                    jreVer = QueryRegistry(regPath);
                }

                if (jreVer == null)
                {
                    // Try to read JAVA_HOME environment variable
                    runPath = Environment.GetEnvironmentVariable("JAVA_HOME");
                    if (runPath != null)
                    {
                        // Get Java version from java.exe in the directory we found
                        jreVer = FileVersionInfo.GetVersionInfo(Path.Combine(runPath, "bin", "java.exe")).FileVersion;
                    }
                }
                else
                {
                    // Get JRE path from JavaHome registry variable
                    runPath = (string)Microsoft.Win32.Registry.GetValue(regPath + "\\" + jreVer, "JavaHome", null);
                }

                if (jreVer == null)
                {
                    // No JRE version found, exit
                    MessageBox.Show(noJRE, appName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (int.Parse(jreVer.Substring(0, jreVer.IndexOf("."))) < 11)
                {
                    // JRE version is less than 11, exit
                    MessageBox.Show(unsupportedJRE, appName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    if (runPath == null)
                    {
                        MessageBox.Show(noJRE, appName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            // Handle arguments
            string javaEXE;
            string cmdArgs;
            if (args.Length > 0)
            {
                if (args.Length == 1)
                {
                    if (args[0].ToLower() == "/console")
                    {
                        javaEXE = @"\bin\java.exe";
                        cmdArgs = "";
                    }
                    else
                    {
                        javaEXE = @"\bin\javaw.exe";
                        cmdArgs = " " + "\"" + args[0] + "\"";
                    }
                }
                else
                {
                    if (args[0].ToLower() == "/console")
                    {
                        javaEXE = @"\bin\java.exe";
                        cmdArgs = " " + "\"" + args[1] + "\"";
                    }
                    else if (args[1].ToLower() == "/console")
                    {
                        javaEXE = @"\bin\java.exe";
                        cmdArgs = " " + "\"" + args[0] + "\"";
                    }
                    else
                    {
                        javaEXE = @"\bin\javaw.exe";
                        cmdArgs = " " + "\"" + args[0] + "\"";
                    }
                }
            }
            else
            {
                javaEXE = @"\bin\javaw.exe";
                cmdArgs = "";
            }

            // Check that the JAR file exists, as well as if FlatLaf exists
            string binPath = Path.Combine(appDir, "bin");
            string jarFileName = "hacktv-gui.jar";
            string flatLafWildCard = "flatlaf-?.*.jar";
            string flatLafFileName = null;
            if (Directory.Exists(binPath))
            {
                string p = Directory.GetFiles(binPath, flatLafWildCard).FirstOrDefault();
                flatLafFileName = Path.GetFileName(p);
            }
            Process run = new Process();
            // If FlatLaf exists, execute JRE with -cp (classpath) instead of -jar
            if (!string.IsNullOrEmpty(flatLafFileName) && File.Exists(Path.Combine(binPath, jarFileName)))
            {
                try
                {
                    run.StartInfo.UseShellExecute = true;
                    run.StartInfo.FileName = runPath + javaEXE;
                    run.StartInfo.Arguments = "-cp " + jarFileName + ";" + flatLafFileName + " com.steeviebops.hacktvgui.GUI" + cmdArgs;
                    run.StartInfo.WorkingDirectory = binPath;
                    run.Start();
                    Environment.ExitCode = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(jreErr + "\r\n\r\n" + ex.Message, appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // Execute JRE with -jar
            else if (File.Exists(Path.Combine(binPath, jarFileName)))
            {
                try
                {
                    run.StartInfo.UseShellExecute = true;
                    run.StartInfo.FileName = runPath + javaEXE;
                    run.StartInfo.Arguments = "-jar " + jarFileName + cmdArgs;
                    run.StartInfo.WorkingDirectory = binPath;
                    run.Start();
                    Environment.ExitCode = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(jreErr + "\r\n\r\n" + ex.Message, appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            // JAR not found, exit
            {
                MessageBox.Show("Unable to find " + Path.Combine(binPath, jarFileName) + ".", appName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        static string QueryRegistry(string regPath)
        {
            return (string)Microsoft.Win32.Registry.GetValue(regPath, "CurrentVersion", null);
        }
    }
}