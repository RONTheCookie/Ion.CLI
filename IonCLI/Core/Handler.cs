using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LLVMSharp;
using Ion.Linking;
using Ion.Parsing;
using Ion.SyntaxAnalysis;
using IonCLI.PackageManagement;
using Ion.Core;
using IonCLI.Engines;
using IonCLI.Integrity;

namespace IonCLI.Core
{
    internal class Handler
    {
        protected readonly Options options;

        protected readonly Processor processor;

        protected OperationType operation;

        public Handler(Options options)
        {
            this.options = options;

            // Create the processor instance.
            this.processor = new Processor(this, options);
        }

        protected void HandleInitOperation()
        {
            // Retrieve the application's base path.
            string basePath = AppContext.BaseDirectory;

            // Combine base path with the default package's path.
            string defaultPackagePath = Path.Combine(basePath, PackageConstants.DefaultPackageFilename);

            // Override path if debug mode is active.
            if (this.options.DebugMode)
            {
                // Use the environment's working directory instead for default package's path.
                defaultPackagePath = Path.Combine(Environment.CurrentDirectory, PackageConstants.DefaultPackageFilename);
            }

            // Ensure default package file exists.
            if (!File.Exists(defaultPackagePath))
            {
                throw new FileNotFoundException("Default package manifest file does not exist");
            }

            // Create the destination path.
            string destination = Path.Combine(this.options.Root, PackageConstants.ManifestFilename);

            // Create the re-initialization flag.
            bool reinitializing = false;

            // Determine if re-initializing.
            if (File.Exists(destination))
            {
                // Activate the re-initialization flag.
                reinitializing = true;

                // Inform the user that re-initialization is taking place.
                Log.Verbose("Package manifest already exists, re-initializing.");

                // Attempt to delete the existing package manifest.
                File.Delete(destination);

                // Ensure that the existing package manifest was deleted.
                if (File.Exists(destination))
                {
                    Log.Error("Unable to delete existing package manifest.");
                }

                // Inform the user that the existing package manifest was deleted.
                Log.Verbose("Existing package manifest was deleted.");
            }

            // Copy the default package manifest.
            File.Copy(defaultPackagePath, destination);

            // Destination manifest should now exist.
            if (File.Exists(destination))
            {
                // Inform the user that the operation completed.
                Log.Success(reinitializing ? "Re-initialized existing package manifest." : "Create a default package manifest file.");
            }
            // Otherwise, report that it does not.
            else
            {
                Log.Error("Could not create default package manifest.");
            }
        }

        public void Process()
        {
            // Retrieve operation value from options.
            string operationValue = this.options.Operation;

            // Inform the user of the requested operation if applicable.
            Log.Verbose($"Using operation: {operationValue}");

            // Resolve operation value.
            OperationType operation = Operation.Resolve(operationValue);

            // Ensure operation type is not unknown.
            if (operation == OperationType.Unknown)
            {
                Log.Error($"Unknown operation: '{operationValue}'.");
            }

            // Inform the user that the requested operation is valid, if applicable.
            Log.Verbose("Requested operation is valid.");

            // Set the operation for future use.
            this.operation = operation;

            // Set the root directory.
            string root = Directory.GetCurrentDirectory();

            // Use specified root directory.
            if (!String.IsNullOrEmpty(this.options.Root))
            {
                // Ensure provided root directory exists.
                if (!Directory.Exists(options.Root))
                {
                    Log.Error("The specified root directory path does not exist.");
                }

                // Use provided root directory path.
                root = Path.GetFullPath(this.options.Root);
            }

            // Inform the user of the final root directory.
            Log.Verbose($"Using root directory: {root}");

            // Ensure root directory exists.
            if (!Directory.Exists(root))
            {
                Log.Error("Root directory does not exist.");
            }

            // TODO: Should never modify options' root path.
            // Apply root directory to options.
            this.options.Root = root;

            // Inform the user that the root directory is valid.
            Log.Verbose("Root directory is valid.");

            // If operation is to initialize, simply initialize and finish.
            if (this.operation == OperationType.Init)
            {
                // Invoke the initialization operation handler.
                this.HandleInitOperation();

                // Terminate this function.
                return;
            }

            // Create a new package loader instance.
            PackageLoader packageLoader = new PackageLoader(root);

            // Ensure package manifest exists.
            if (!packageLoader.DoesManifestExist)
            {
                Log.Error("Package manifest file does not exist.");
            }

            // Inform the user that the package manifest exists.
            Log.Verbose("Package manifest file exists.");

            // Load the package manifest.
            Package package = packageLoader.ReadPackage();

            // Inform the user that the package manifest was loaded.
            Log.Verbose("Package manifest file loaded.");

            // Process package options if applicable.
            if (package.Options != null)
            {
                // Use package's root path option if applicable.
                if (package.Options.SourceRoot != null)
                {
                    // Create the source directory path.
                    string sourcePath = Path.GetFullPath(package.Options.SourceRoot);

                    // Inform the user of the source directory path.
                    Log.Verbose($"Using source directory: {sourcePath}");

                    // Ensure directory path exists.
                    if (!Directory.Exists(sourcePath))
                    {
                        Log.Error("Provided source root directory path in package manifest does not exist.");
                    }

                    // Inform the user that the source directory exists.
                    Log.Verbose("Source directory is valid.");

                    // Override root path.
                    root = Path.GetFullPath(package.Options.SourceRoot);

                    // Inform the user of the action taken.
                    Log.Verbose($"Using source root directory from package manifest: {root}");
                }
            }

            // Process scanner.
            Project project = this.ProcessScanner(root);

            // Summon the corresponding engine.
            this.SummonEngine(this.operation, package, project);
        }

        public void SummonEngine(OperationType operation, Package package, Project project)
        {
            // Ensure operation is valid.
            if (this.operation == OperationType.Unknown)
            {
                throw new InvalidOperationException("Unexpected operation to be unknown.");
            }

            // Create the engine context.
            EngineContext context = new EngineContext
            {
                Options = this.options,
                Project = project,
                Package = package
            };

            // Create the engine buffer.
            OperationEngine engine = null;

            // Create a new build engine instance.
            if (this.operation == OperationType.Build)
            {
                engine = new BuildEngine(context);
            }
            // Create a new execution engine instance.
            else if (this.operation == OperationType.Run)
            {
                engine = new Engines.ExecutionEngine(context);
            }
            // At this point, the provided operation is invalid.
            else
            {
                throw new ArgumentException($"Unknown requested operation: {operation}");
            }

            // Ensure the engine buffer is not null.
            if (engine == null)
            {
                throw new Exception("Unexpected engine to be null.");
            }

            // Invoke the engine buffer.
            engine.Invoke();
        }

        protected string Build(Ion.CodeGeneration.Module module)
        {
            // Create the resulting string.
            string result;

            // Create the full, target output path.
            string targetPath;

            // Default to IR file extension.
            string extension = FileExtension.IR;

            // Print the resulting LLVM IR code to the output target if applicable.
            if (!this.options.Bitcode)
            {
                // TODO: Make use of this.
                string error;

                // Create the target path.
                targetPath = Path.Join(this.options.Output, $"{module.FileName}.{extension}");

                // TODO: Should not write to file/create file.
                // Emit IR to target path.
                LLVM.PrintModuleToFile(module.Target, targetPath, out error);
            }
            // Otherwise, emit LLVM Bitcode result.
            else
            {
                // Set the extension to Bitcode.
                extension = FileExtension.Bitcode;

                // Create the target path.
                targetPath = Path.Join(this.options.Output, $"{module.FileName}.{extension}");

                // TODO: Should not write to file/create file.
                // Write bitcode to target path.
                if (LLVM.WriteBitcodeToFile(module.Target, targetPath) != 0)
                {
                    Log.Error($"There was an error writing LLVM bitcode to '{targetPath}'.");
                }
            }

            // Read and obtain emitted data.
            result = File.ReadAllText(targetPath);

            // Return result.
            return result;
        }

        protected Project ProcessScanner(string root)
        {
            // Create the scanner.
            Scanner scanner = new Scanner(root);

            // Scan for files.
            string[] files = scanner.Scan();

            // No matching files.
            if (files.Length == 0)
            {
                Log.Warning("No matching files discovered.");
                Environment.Exit(0);
            }

            // Ensure output directory exists, otherwise create it.
            if (!Directory.Exists(this.options.Output))
            {
                Directory.CreateDirectory(this.options.Output);
                Log.Verbose("Created output directory.");
            }

            // Create a new project instance.
            Project project = new Project();

            // Process files.
            foreach (string file in files)
            {
                // Inform the user that of the file being processed.
                Log.Compose($"Processing {file} ...");

                // TODO: Process output file path if possible.
                // Process file and obtain the resulting module.
                Ion.CodeGeneration.Module module = this.processor.ProcessFile(file);

                // Append the module to the project.
                project.Modules.Add(module);
            }

            // Inform the user that the operation completed successfully.
            Log.Verbose($"Processed {files.Length} file(s).");

            // Return the project.
            return project;
        }
    }
}
