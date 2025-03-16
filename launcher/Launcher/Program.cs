using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.ExitCode = 1;
            string appName = "hacktv-gui launcher";
            string runPath;
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string binPath = Path.Combine(appDir, "bin");
            string jarFileName = "hacktv-gui.jar";
            string noJRE = "Unable to find a Java Runtime Environment.";
            string unsupportedJRE = "Unsupported Java Runtime Environment found. Version 11 or higher is required.";
            string jreErr = "An error occurred while launching the Java Runtime Environment.";
            string jreVer;
            string flatLafClassPath = null;

            // Check that the hacktv-gui.jar file exists
            if (!File.Exists(Path.Combine(binPath, jarFileName)))
            {
                // JAR not found, exit
                MessageBox.Show("Unable to find " + Path.Combine(binPath, jarFileName) + ".", appName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;   
            }

            // Check for the presence of a JRE
            if (Directory.Exists(Path.Combine(appDir, "jre")))
            {
                runPath = Path.Combine(appDir, "jre");
                jreVer = FileVersionInfo.GetVersionInfo(Path.Combine(runPath, "bin", "java.exe")).FileVersion;
            }
            else
            {
                // Try to get JRE or JDK version from JavaSoft key in registry. Try 64-bit first, then 32-bit.
                string regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\JRE";
                jreVer = QueryRegistry(regPath, "CurrentVersion");
                if (jreVer == null)
                {
                    regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\JavaSoft\JRE";
                    jreVer = QueryRegistry(regPath, "CurrentVersion");
                }
                if (jreVer == null)
                {
                    regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\JDK";
                    jreVer = QueryRegistry(regPath, "CurrentVersion");
                }
                if (jreVer == null)
                {
                    regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\JavaSoft\JDK";
                    jreVer = QueryRegistry(regPath, "CurrentVersion");
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
                    // Check the .jar file association
                    string jarAssoc = QueryRegistry(@"HKEY_CLASSES_ROOT\.jar", "");
                    if (!(jarAssoc == null))
                    {
                        var v = QueryRegistry(@"HKEY_CLASSES_ROOT\" + jarAssoc + @"\shell\open\command", "");
                        v = Regex.Match(v, "(?<=\")[^\"]*").Value;
                        if ((v != null) && (File.Exists(v)))
                        {
                            jreVer = FileVersionInfo.GetVersionInfo(v).FileVersion;
                            runPath = Path.GetDirectoryName(Path.GetDirectoryName(v));
                        }
                    }
                }
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
            

            // Check for Flatlaf package(s)
            if (Directory.Exists(Path.Combine(appDir, binPath)))
            {
                string[] fl = Directory.GetFiles(binPath, "flatlaf-*.jar");
                string[] cp = {"", ""};
                string flFileName = null;
                string ijFileName = null;
                var f1 = new Regex(@"flatlaf-(\d.\d).jar");
                var f2 = new Regex(@"flatlaf-(\d.\d.\d).jar");
                foreach (string f in fl)
                {
                    var m1 = f1.Match(f);
                    var m2 = f2.Match(f);
                    if ((m1.Success) && (flFileName is null)) {
                        flFileName = m1.Value;
                    }
                    else if ((m2.Success) && (flFileName is null)) {
                        flFileName = m2.Value;
                    }
                    if (!(flFileName is null))
                    {
                        // Add the first match
                        if (string.IsNullOrEmpty(cp[0])) cp[0] = flFileName;
                        // Set the name of the flatlaf-intellij-themes JAR file corresponding to the version we found
                        ijFileName = "flatlaf-intellij-themes-" + flFileName.Substring(8, 5) + ".jar";
                    }
                }
                // Try to find the flatlaf-intellij-themes JAR file
                if ((!(ijFileName is null)) && File.Exists(Path.Combine(binPath, ijFileName)))
                {
                    cp[1] = ijFileName;
                }
                if ((!string.IsNullOrEmpty(cp[0])) && string.IsNullOrEmpty(cp[1]))
                {
                    flatLafClassPath = cp[0];
                }
                else if ((!string.IsNullOrEmpty(cp[0])) && (!string.IsNullOrEmpty(cp[1])))
                {
                    flatLafClassPath = cp[0] + ";" + cp[1];
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
                        javaEXE = @"bin\java.exe";
                        cmdArgs = "";
                    }
                    else
                    {
                        javaEXE = @"bin\javaw.exe";
                        cmdArgs = " " + "\"" + args[0] + "\"";
                    }
                }
                else
                {
                    if (args[0].ToLower() == "/console")
                    {
                        javaEXE = @"bin\java.exe";
                        cmdArgs = " " + "\"" + args[1] + "\"";
                    }
                    else if (args[1].ToLower() == "/console")
                    {
                        javaEXE = @"bin\java.exe";
                        cmdArgs = " " + "\"" + args[0] + "\"";
                    }
                    else
                    {
                        javaEXE = @"bin\javaw.exe";
                        cmdArgs = " " + "\"" + args[0] + "\"";
                    }
                }
            }
            else
            {
                javaEXE = @"bin\javaw.exe";
                cmdArgs = "";
            }

            // Get ready to run
            Process run = new Process();
            // If FlatLaf exists, execute JRE with -cp (classpath) instead of -jar
            if (!string.IsNullOrEmpty(flatLafClassPath))
            {
                String a = "-cp " + jarFileName + ";" + flatLafClassPath + " com.steeviebops.hacktvgui.GUI" + cmdArgs;
                try
                {
                    run.StartInfo.UseShellExecute = true;
                    run.StartInfo.FileName = Path.Combine(runPath, javaEXE);
                    run.StartInfo.Arguments = a;
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
                    run.StartInfo.FileName = Path.Combine(runPath, javaEXE);
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
        }

        static string QueryRegistry(string regPath, string value)
        {
            return (string)Microsoft.Win32.Registry.GetValue(regPath, value, null);
        }
    }
}