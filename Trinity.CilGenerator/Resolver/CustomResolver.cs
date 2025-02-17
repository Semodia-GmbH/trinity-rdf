﻿using ICSharpCode.Decompiler.Metadata;
using Mono.Cecil;

namespace Semiodesk.Trinity.CilGenerator.Resolver
{
    class CustomResolver : UniversalAssemblyResolver
    {
        protected CustomResolver(string mainAssemblyFileName, bool throwOnError, string targetFramework) : base(mainAssemblyFileName, throwOnError, targetFramework, System.Reflection.PortableExecutable.PEStreamOptions.PrefetchEntireImage)
        {
        }

        public static ModuleDefinition LoadMainModule(string mainAssemblyFileName)
        {
            var moduleDefinition = new PEFile(mainAssemblyFileName, System.Reflection.PortableExecutable.PEStreamOptions.PrefetchEntireImage);
            var targetFramework = DotNetCorePathFinderExtensions.DetectTargetFrameworkId(moduleDefinition.Reader);
            var resolver = new CustomResolver(mainAssemblyFileName, true, targetFramework);

            var module = ModuleDefinition.ReadModule(mainAssemblyFileName, new ReaderParameters
            {
                AssemblyResolver = resolver,
                ReadWrite = true
            });

            return module;
        }
    }
}
