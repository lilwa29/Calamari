﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Calamari.Common.Features.Processes;
using Calamari.Common.Features.Scripting;
using Calamari.Common.Features.Scripts;
using Calamari.Common.Plumbing.Extensions;
using Calamari.Common.Plumbing.FileSystem;
using Calamari.Common.Plumbing.Variables;
using Newtonsoft.Json;

namespace Calamari.Common.Features.FunctionScriptContributions
{
    class FunctionAppenderScriptWrapper: IScriptWrapper
    {
        readonly IVariables variables;
        readonly ICalamariFileSystem fileSystem;
        readonly CodeGenFunctionsRegistry codeGenFunctionsRegistry;

        public FunctionAppenderScriptWrapper(IVariables variables, ICalamariFileSystem fileSystem, CodeGenFunctionsRegistry codeGenFunctionsRegistry)
        {
            this.variables = variables;
            this.fileSystem = fileSystem;
            this.codeGenFunctionsRegistry = codeGenFunctionsRegistry;
        }

        public int Priority { get; } = ScriptWrapperPriorities.ToolConfigPriority;
        public IScriptWrapper NextWrapper { get; set; }

        public bool IsEnabled(ScriptSyntax syntax)
        {
            if (String.IsNullOrEmpty(variables.Get(ScriptFunctionsVariables.Registration)))
            {
                return false;
            }

            return codeGenFunctionsRegistry.SupportedScriptSyntax.Contains(syntax);
        }

        public CommandResult ExecuteScript(Script script, ScriptSyntax scriptSyntax, ICommandLineRunner commandLineRunner, Dictionary<string, string>? environmentVars)
        {
            var workingDirectory = Path.GetDirectoryName(script.File);

            variables.Set("OctopusFunctionAppenderTargetScript", $"{script.File}");
            variables.Set("OctopusFunctionAppenderTargetScriptParameters", script.Parameters);
            var copyScriptFile = variables.Get(ScriptFunctionsVariables.CopyScriptWrapper);
            var scriptFile = CreateContextScriptFile(workingDirectory, scriptSyntax);

            if (!String.IsNullOrEmpty(copyScriptFile))
            {
                var destinationFile = copyScriptFile;
                if (!Path.IsPathRooted(copyScriptFile))
                {
                    destinationFile = Path.Combine(workingDirectory, copyScriptFile);
                }

                File.Copy(scriptFile, destinationFile, true);
            }

            using (var contextScriptFile = new TemporaryFile(scriptFile))
            {
                return NextWrapper.ExecuteScript(new Script(contextScriptFile.FilePath), scriptSyntax, commandLineRunner, environmentVars);
            }
        }

        string CreateContextScriptFile(string workingDirectory, ScriptSyntax scriptSyntax)
        {
            var registrations = variables.Get(ScriptFunctionsVariables.Registration);
            var results = JsonConvert.DeserializeObject<IList<ScriptFunctionRegistration>>(registrations);

            var azureContextScriptFile = Path.Combine(workingDirectory, $"Octopus.FunctionAppenderContext.{scriptSyntax.FileExtension()}");
            var contextScript = codeGenFunctionsRegistry.GetCodeGenerator(scriptSyntax).Generate(results);
            fileSystem.OverwriteFile(azureContextScriptFile, contextScript);
            return azureContextScriptFile;
        }


    }
}