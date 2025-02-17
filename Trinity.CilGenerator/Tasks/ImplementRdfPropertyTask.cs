﻿// LICENSE:
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// AUTHORS:
//
//  Moritz Eberl <moritz@semiodesk.com>
//  Sebastian Faubel <sebastian@semiodesk.com>
//
// Copyright (c) Semiodesk GmbH 2015-2019

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Semiodesk.Trinity.CilGenerator.Extensions;
using System.Runtime.CompilerServices;

namespace Semiodesk.Trinity.CilGenerator.Tasks
{
    /// <summary>
    /// Task to implement the RdfPropertyCustomAttribute for a given property parameter.
    /// </summary>
    public class ImplementRdfPropertyTask : GeneratorTaskBase
    {
        #region Constructors

        public ImplementRdfPropertyTask(ILGenerator generator, TypeDefinition type) : base(generator, type) { }

        #endregion

        #region Methods

        public override bool CanExecute(object parameter = null)
        {
            var property = parameter as PropertyDefinition;
            var canExecute = property != null && property.GetMethod.HasCustomAttribute<CompilerGeneratedAttribute>() && property.SetMethod.HasCustomAttribute<CompilerGeneratedAttribute>();
            return canExecute;
        }

        public override bool Execute(object parameter = null)
        {
            var property = parameter as PropertyDefinition;

            if (property == null) return false;

            var mappingField = ImplementMappingField(property);

            var p = new PropertyGeneratorTaskHelper(property, mappingField);

            if (p.Property.GetMethod != null)
            {
                if (p.Property.GetMethod.HasCustomAttribute<CompilerGeneratedAttribute>() || p.Property.GetMethod.IsCompilerControlled)
                {
                    var getValueGenerator = GetGetValueGenerator(p);

                    if (getValueGenerator.CanExecute())
                    {
                        getValueGenerator.Execute();
                    }
                    else
                    {
                        var msg = "{0}.{1}: Failed to implement property getter.";
                        throw new Exception(string.Format(msg, property.DeclaringType.FullName, property.Name));
                    }
                }
            }

            if (p.Property.SetMethod != null)
            {
                if (p.Property.SetMethod.HasCustomAttribute<CompilerGeneratedAttribute>() || p.Property.SetMethod.IsCompilerControlled)
                {
                    var setValueGenerator = GetSetValueGenerator(p);

                    if (setValueGenerator.CanExecute())
                    {
                        setValueGenerator.Execute();
                    }
                    else
                    {
                        var msg = "{0}.{1}: Failed to implement property setter.";
                        throw new Exception(string.Format(msg, property.DeclaringType.FullName, property.Name));
                    }
                }
            }

            Log.LogMessage("{0}.{1} -> <{2}>", Type.FullName, property.Name, p.Uri);

            return true;
        }


        internal FieldDefinition ImplementMappingField(PropertyDefinition property)
        {
            // Load the property mapping type.
            var mappingType = MainModule.ImportReference(ILGenerator.PropertyMapping);

            if (mappingType == null)
            {
                throw new ArgumentException(string.Format("{0}: Failed to import type {1}.", Assembly.FullName, mappingType.FullName));
            }

            // Add the property type as generic parameter.
            var fieldType = new GenericInstanceType(mappingType);
            fieldType.GenericArguments.Add(property.PropertyType);

            // Generate the name of the private backing field.
            var fieldName = "<" + Char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1) + ">"+"k__"+"MappingField";

            var mappingField = Type.TryGetField(fieldName);

            if( mappingField == null )
            { 
                // Implement the field.
                mappingField = new FieldDefinition(fieldName, FieldAttributes.Family, fieldType);
                Type.Fields.Add(mappingField);
            }

            var backingField = property.TryGetBackingField();

            if (backingField != null)
            {
                Type.Fields.Remove(backingField.Resolve());
            }

            var p = new PropertyGeneratorTaskHelper(property, mappingField);

            if (!Uri.IsWellFormedUriString(p.Uri, UriKind.Absolute))
            {
                throw new UriFormatException("Annotated URI must be in absolute format.");
            }

            // Finally, implement the field initializers in the constructors of the class.
            foreach (var ctor in Type.GetConstructors())
            {
                // Implementing mapping fields in static constructors results in invalid byte code,
                // since there is no ldarg.0 (this) variable.
                if (ctor.IsStatic) continue;

                var g = new MethodGeneratorTask(ctor);

                var Omit = new List<Instruction>();
                var alreadyInitialized = false;
                foreach (var x in ctor.Body.Instructions)
                {
                    if(backingField != null && x.Operand == backingField)
                    {
                        p.Initializer = new List<Instruction>();
                        Omit.Add(x);
                        var inst = x;
                        while(inst.Previous.OpCode.Code != Code.Ldarg_0)
                        {
                            inst = inst.Previous;
                            p.Initializer.Insert(0, inst);
                            // Remove this initializiation from constructor
                            Omit.Add(inst);
                        }
                        Omit.Add(inst.Previous);
                    }
                    if (mappingField != null && x.Operand != null && x.Operand is FieldDefinition && (x.Operand as FieldDefinition).FullName == mappingField.FullName)
                    { 
                        alreadyInitialized = true;
                        continue;
                    }
                }
                if (alreadyInitialized)
                    continue;

                g.Instructions.AddRange(GetFieldInitializationInstructions(g.Processor, p, fieldType));
                g.Instructions.AddRange(ctor.Body.Instructions.Where((x) => !Omit.Contains(x)));

                if (g.CanExecute())
                {
                    g.Execute();
                }   
            }
            

            return mappingField;
        }

        internal MethodGeneratorTask GetGetValueGenerator(PropertyGeneratorTaskHelper p)
        {
            // Load a reference to the GetValue method for the property mapping.
            var getValue = Type.TryGetGetValueMethod(Assembly, p.Property.PropertyType);

            if(getValue == null)
                throw new ArgumentException("{0}: Type has no GetValue() method.", p.Property.DeclaringType.FullName);

            var generator = new MethodGeneratorTask(p.Property.GetMethod);

            generator.Instructions.AddRange(GetReturnGetValueInstructions(generator.Processor, p, getValue));

            return generator;
        }

        internal MethodGeneratorTask GetSetValueGenerator(PropertyGeneratorTaskHelper p)
        {
            // Load a reference to the SetValue method for the property mapping.
            var setValue = Type.TryGetSetValueMethod(Assembly, PropertyMappingType, p.Property.PropertyType);

            if(setValue == null)
                throw new ArgumentException("{0}: Type has no SetValue() method.", p.Property.DeclaringType.FullName);

            var generator = new MethodGeneratorTask(p.Property.SetMethod);

            generator.Instructions.AddRange(GetCallSetValueInstructions(generator.Processor, p, setValue));

            return generator;
        }

        private IEnumerable<Instruction> GetCallSetValueInstructions(ILProcessor processor, PropertyGeneratorTaskHelper p, MethodReference setValue)
        {
            yield return processor.Create(OpCodes.Nop);
            yield return processor.Create(OpCodes.Ldarg_0);
            yield return processor.Create(OpCodes.Ldarg_0);
            yield return processor.Create(OpCodes.Ldfld, p.BackingField);
            yield return processor.Create(OpCodes.Ldarg_1);
            yield return processor.Create(OpCodes.Callvirt, setValue);
            yield return processor.Create(OpCodes.Nop);
            yield return processor.Create(OpCodes.Ret);
        }

        private IEnumerable<Instruction> GetCallGetValueInstructions(ILProcessor processor, PropertyGeneratorTaskHelper p, MethodReference getValue)
        {
            yield return processor.Create(OpCodes.Nop);
            yield return processor.Create(OpCodes.Ldarg_0);
            yield return processor.Create(OpCodes.Ldarg_0);
            yield return processor.Create(OpCodes.Ldfld, p.BackingField);
            yield return processor.Create(OpCodes.Callvirt, getValue);
        }

        private IEnumerable<Instruction> GetReturnGetValueInstructions(ILProcessor processor, PropertyGeneratorTaskHelper p, MethodReference getValue)
        {
            processor.Create(OpCodes.Ldloc_0);

            foreach (var i in GetCallGetValueInstructions(processor, p, getValue))
            {
                yield return i;
            }
                
            yield return processor.Create(OpCodes.Ret);
        }

        private IEnumerable<Instruction> GetFieldInitializationInstructions(ILProcessor processor, PropertyGeneratorTaskHelper p, TypeReference mappingType)
        {
            MethodDefinition ctorDef;

            if (p.HasDefaultValue)
            {
                ctorDef = GetPropertyMappingConstructorDefinition(mappingType, p.DefaultValue);
            }
            else
            {
                ctorDef = GetPropertyMappingConstructorDefinition(mappingType);
            }

            if (ctorDef == null) yield break;

            var ctor = Generator.Assembly.MainModule.ImportReference(ctorDef);

            // Thanks to: http://stackoverflow.com/questions/4968755/mono-cecil-call-generic-base-class-method-from-other-assembly
            if (mappingType.IsGenericInstance)
            {
                var genericType = mappingType as GenericInstanceType;

                ctor = MakeGeneric(ctor, genericType.GenericArguments.ToArray());
            }

            yield return processor.Create(OpCodes.Ldarg_0);
            yield return processor.Create(OpCodes.Ldstr, p.Property.Name);
            yield return processor.Create(OpCodes.Ldstr, p.Uri);

            if(p.HasDefaultValue)
            { 
                if (p.Initializer == null)
                {
                    foreach (var i in GetLdX(processor, p.DefaultValue))
                    {
                        yield return i;
                    }
                }else
                {
                    foreach (var inst in p.Initializer)
                        yield return inst;
                }
            }

            if (p.LanguageInvariant)
                yield return processor.Create(OpCodes.Ldc_I4_1);
            else
                yield return processor.Create(OpCodes.Ldc_I4_0);

            yield return processor.Create(OpCodes.Newobj, ctor);
            yield return processor.Create(OpCodes.Stfld, p.BackingField);
        }

        private MethodDefinition GetPropertyMappingConstructorDefinition(TypeReference mappingType)
        {
            return mappingType.Resolve().GetConstructors().FirstOrDefault(
                m => m.Parameters.Count == 3
                    && m.Parameters[0].ParameterType.MetadataType == MetadataType.String
                    && m.Parameters[1].ParameterType.MetadataType == MetadataType.String
                    && m.Parameters[2].ParameterType.MetadataType == MetadataType.Boolean);
        }

        private MethodDefinition GetPropertyMappingConstructorDefinition(TypeReference mappingType, CustomAttributeArgument defaultValue)
        {
            var ctors = mappingType.Resolve().GetConstructors();

            return ctors.FirstOrDefault(
                m => m.Parameters.Count == 4
                && m.Parameters[0].ParameterType.MetadataType == MetadataType.String
                && m.Parameters[1].ParameterType.MetadataType == MetadataType.String
                && m.Parameters[2].ParameterType.MetadataType == MetadataType.Var
                && m.Parameters[3].ParameterType.MetadataType == MetadataType.Boolean);
        }

        private IEnumerable<Instruction> GetLdX(ILProcessor processor, CustomAttributeArgument defaultValue)
        {
            var valueType = defaultValue.Type.MetadataType;

            if (valueType == MetadataType.String)
            {
                yield return processor.Create(OpCodes.Ldstr, (string)defaultValue.Value);
            }
            else if (valueType == MetadataType.Boolean)
            {
                yield return processor.CreateLdc_I4((bool)defaultValue.Value ? 1 : 0);
            }
            else if (valueType == MetadataType.Int16 || valueType == MetadataType.Int32)
            {
                yield return processor.CreateLdc_I4((int)defaultValue.Value);
            }
            else if (valueType == MetadataType.UInt16 || valueType == MetadataType.UInt32)
            {
                yield return processor.CreateLdc_I4((uint)defaultValue.Value);
            }
            else if (valueType == MetadataType.Int64)
            {
                var v = (long)defaultValue.Value;

                if (Int32.MinValue <= v && v <= Int32.MaxValue)
                {
                    yield return processor.CreateLdc_I4((int)v);
                    yield return processor.Create(OpCodes.Conv_I8);
                }
                else
                {
                    yield return processor.Create(OpCodes.Ldc_I8, (long)defaultValue.Value);
                }
            }
            else if (valueType == MetadataType.UInt64)
            {
                var v = (ulong)defaultValue.Value;

                if (UInt32.MinValue <= v && v >= UInt32.MaxValue)
                {
                    yield return processor.CreateLdc_I4((uint)v);
                    yield return processor.Create(OpCodes.Conv_I8);
                }
                else
                {
                    yield return processor.Create(OpCodes.Ldc_I8, (ulong)defaultValue.Value);
                }
            }
            else if (valueType == MetadataType.Single)
            {
                yield return processor.Create(OpCodes.Ldobj, (float)defaultValue.Value);
            }
            else if (valueType == MetadataType.Double)
            {
                yield return processor.Create(OpCodes.Ldobj, (double)defaultValue.Value);
            }
            else
            {
                throw new Exception("Unsupported data type for default value: {0}" + defaultValue.Value.GetType());
            }
        }

        private MethodReference MakeGeneric(MethodReference self, params TypeReference[] arguments)
        {
            var reference = new MethodReference(self.Name, self.ReturnType)
            {
                DeclaringType = MakeGenericType(self.DeclaringType, arguments),
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention,
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var generic_parameter in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));
            }

            return reference;
        }

        private TypeReference MakeGenericType(TypeReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
            {
                throw new ArgumentException();
            }

            var instance = new GenericInstanceType(self);

            foreach (var argument in arguments)
            {
                instance.GenericArguments.Add(argument);
            }

            return instance;
        }

        #endregion
    }
}
