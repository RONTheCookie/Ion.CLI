using System.Collections.Generic;

namespace IonCLI.Encapsulation
{
    public class Package
    {
        /// <summary>
        /// The uniquely identifying package name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The optional description string of the package.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The version string of the package.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The compile version iteration handled
        /// by the compiler, incremented every compilation
        /// step.
        /// </summary>
        public long Build { get; set; }

        /// <summary>
        /// The author's name or contact information.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Packages needed or used within this application.
        /// </summary>
        public Dependency[] Dependencies { get; set; }
    }
}
