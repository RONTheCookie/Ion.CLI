using System;
using System.IO;
using System.Runtime.InteropServices;
using IonCLI.Core;

namespace IonCLI.Integrity
{
    public class IntegrityVerifier
    {
        protected bool isWindowsOs;

        protected readonly string root;

        protected readonly Options options;

        public IntegrityVerifier(Options options, string root)
        {
            this.options = options;
            this.root = root;
            this.isWindowsOs = false;
        }

        /// <summary>
        /// Begin the verification process.
        /// </summary>
        public void Invoke()
        {
            // Perform OS check.
            this.PerformOsCheck();

            // Inform the user that the OS check completed.
            Log.Verbose("OS check completed successfully.");

            // Ensure tools, if applicable.
            this.TestTools();

            // Inform the user that the integrity check completed, if applicable.
            if (!this.options.NoIntegrity)
            {
                Log.Verbose("Integrity check completed successfully.");
            }
        }

        /// <summary>
        /// Performs a check on the current OS platform
        /// to ensure everything is as expected.
        /// </summary>
        public void PerformOsCheck()
        {
            // MacOS is not currently supported.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Log.Error("MacOS X is not currently supported.");
            }
            // Set the Windows flag if current OS is Windows.
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.isWindowsOs = true;
            }
        }

        /// <summary>
        /// If applicable, ensure tools exist and are
        /// in an expected state.
        /// </summary>
        public void TestTools()
        {
            // Ensure tools have been downloaded on Windows.
            if (this.isWindowsOs)
            {
                // Resolve tools path directory.
                string toolsPath = Paths.BaseDirectory(this.options.ToolsPath);

                // Inform the user of the tools path being used.
                Log.Verbose($"Using tools directory: {toolsPath}");

                // Tools directory must exist on Windows.
                if (!Directory.Exists(toolsPath))
                {
                    Log.Error("Tools directory does not exist. You may have a corrupt installation.");
                }

                // Ensure all tools exist.
                foreach ((ToolType type, ToolDefinition tool) in VerifierConstants.Tools)
                {
                    // Ensure required properties are set.
                    if (String.IsNullOrEmpty(tool.FileName))
                    {
                        throw new Exception($"Tool definition for '{type}' must contain a filename.");
                    }

                    // Resolve the path for the tool.
                    string path = this.options.PathResolver.Tool(type);

                    // Ensure tool exists.
                    if (!File.Exists(path))
                    {
                        Log.Error($"Required tool executable '{path}' is missing. You may have a corrupt installation.");
                    }
                }

                // Do not continue at this point.
                return;
            }
        }
    }
}
