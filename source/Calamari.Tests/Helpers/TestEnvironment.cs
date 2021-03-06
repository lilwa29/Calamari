﻿using System;
using System.IO;
using System.Reflection;
using Assent;
using Assent.Namers;
using Calamari.Integration.Processes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Calamari.Tests.Helpers
{
    public static class TestEnvironment 
    {
        public static readonly string AssemblyLocalPath = typeof(TestEnvironment).Assembly.FullLocalPath();
        public static readonly string CurrentWorkingDirectory = Path.GetDirectoryName(AssemblyLocalPath);
        public static readonly bool IsCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));

        public static readonly Configuration AssentConfiguration = new Configuration()
            .UsingNamer(IsCI ? (INamer) new CIAssentNamer() : new SubdirectoryNamer("Approved"))
            .SetInteractive(!IsCI);
        
        public static readonly Configuration AssentJsonConfiguration = new Configuration()
            .UsingNamer(IsCI ? (INamer) new CIAssentNamer() : new SubdirectoryNamer("Approved"))
            .SetInteractive(!IsCI)
            .UsingExtension("json");

        public static readonly Configuration AssentYamlConfiguration = new Configuration()
                                                                       .UsingNamer(IsCI ? (INamer)new CIAssentNamer() : new SubdirectoryNamer("Approved"))
                                                                       .SetInteractive(!IsCI)
                                                                       .UsingExtension("yaml");

        public static readonly Configuration AssentXmlConfiguration = new Configuration()
            .UsingNamer(IsCI ? (INamer)new CIAssentNamer() : new SubdirectoryNamer("Approved"))
            .SetInteractive(!IsCI)
            .UsingExtension("xml");

        public static readonly Configuration AssentPropertiesConfiguration = new Configuration()
                                                                             .UsingNamer(IsCI ? (INamer)new CIAssentNamer() : new SubdirectoryNamer("Approved"))
                                                                             .SetInteractive(!IsCI)
                                                                             .UsingExtension("properties");

        public static string GetTestPath(params string[] paths)
        {
            return Path.Combine(CurrentWorkingDirectory, Path.Combine(paths));
        }

        public static string ConstructRootedPath(params string[] paths)
        {
            return Path.Combine(Path.GetPathRoot(CurrentWorkingDirectory), Path.Combine(paths));
        }
    }
}

