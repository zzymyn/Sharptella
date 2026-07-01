using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Sharptella.Gen;

namespace Sharptella.Gen;

[Generator]
public sealed class Generator
    : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitializationOutput);

        var cpuInstructionDefs = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Sharptella.Gen.CpuInstructionAttribute",
                predicate: IsMethodWithAttribute,
                transform: GetCpuInstructionTx)
            .SelectMany((ctx, _) => ctx);

        context.RegisterSourceOutput(
            cpuInstructionDefs.Collect(),
            GenCpuInstruction);
    }

    private void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("Attributes.g.cs", SourceText.From(Resources.Attributes, Encoding.UTF8));
    }

    private static bool IsMethodWithAttribute(SyntaxNode node, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (node is MethodDeclarationSyntax method)
        {
            return method.AttributeLists.Count > 0;
        }
        return false;
    }

    private static ImmutableArray<CpuInstructionData> GetCpuInstructionTx(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ctx.TargetNode is not MethodDeclarationSyntax methodDeclaration)
            return ImmutableArray<CpuInstructionData>.Empty;

        if (ctx.SemanticModel.GetDeclaredSymbol(methodDeclaration, ct) is not IMethodSymbol methodSymbol)
            return ImmutableArray<CpuInstructionData>.Empty;
        if (methodSymbol.ContainingType is not INamedTypeSymbol classSymbol)
            return ImmutableArray<CpuInstructionData>.Empty;

        var nsName = classSymbol.ContainingNamespace?.ToDisplayString();
        var className = GetClassFullName(classSymbol);
        var methodName = GetMethodFullName(methodSymbol);
        var opName = methodSymbol.Name;

        var matchingAttributes = ImmutableArray.CreateBuilder<CpuInstructionData>();

        foreach (var attr in ctx.Attributes)
        {
            if (attr.AttributeClass is not INamedTypeSymbol attrClass)
                continue;

            if (attrClass.ToDisplayString() != "Sharptella.Gen.CpuInstructionAttribute")
                continue;

            int opcode = 0;
            ReadWriteMode readWriteMode = ReadWriteMode.Read;
            AddressingMode addressingMode = AddressingMode.Implied;

            for (int i = 0; i < attr.ConstructorArguments.Length; i++)
            {
                var arg = attr.ConstructorArguments[i];
                switch (i)
                {
                    case 0:
                        if (arg.Value is int op)
                            opcode = op;
                        break;
                    case 1:
                        if (arg.Value is int rw)
                            readWriteMode = (ReadWriteMode)rw;
                        break;
                    case 2:
                        if (arg.Value is int am)
                            addressingMode = (AddressingMode)am;
                        break;
                }
            }

            foreach (var namedArg in attr.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "OpCode":
                        if (namedArg.Value.Value is int op)
                            opcode = op;
                        break;
                    case "ReadWriteMode":
                        if (namedArg.Value.Value is int rw)
                            readWriteMode = (ReadWriteMode)rw;
                        break;
                    case "AddressingMode":
                        if (namedArg.Value.Value is int am)
                            addressingMode = (AddressingMode)am;
                        break;
                    default:
                        break;
                }
            }

            matchingAttributes.Add(new(nsName, className, methodName, opName, opcode, readWriteMode, addressingMode));
        }

        return matchingAttributes.ToImmutable();
    }

    private static void GenCpuInstruction(SourceProductionContext context, ImmutableArray<CpuInstructionData> array)
    {
        var groupBy = array
            .GroupBy(a => (a.Namespace, a.ClassName));

        foreach (var group in groupBy)
        {
            var nsName = group.Key.Namespace;
            var className = group.Key.ClassName;

            var sb = new StringBuilder();

            if (nsName != null)
            {
                sb.AppendLine($"namespace {nsName};");
                sb.AppendLine("");
            }
            sb.AppendLine($"public partial class {className}");
            sb.AppendLine("{");

            foreach (var op in group)
            {
                var template = op.ReadWriteMode switch
                {
                    ReadWriteMode.Read => op.AddressingMode switch
                    {
                        AddressingMode.Immediate => Resources.ReadImmediate,
                        _ => "",
                    },
                    _ => "",
                };

                if (!string.IsNullOrEmpty(template))
                {
                    template = template.Replace("$OP$", op.OpName);
                    sb.AppendLine(template);
                }
            }

            sb.AppendLine("}");

            var nameSb = new StringBuilder();
            if (nsName != null)
            {
                nameSb.Append(nsName);
                nameSb.Append('-');
            }
            nameSb.Append(className);
            nameSb.Replace('<', '-').Replace('>', '-').Replace(',', '-');

            context.AddSource($"{nameSb}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    private static string GetClassFullName(INamedTypeSymbol classSymbol)
    {
        var sb = new StringBuilder();
        sb.Append(classSymbol.Name);
        if (classSymbol.IsGenericType)
        {
            sb.Append("<");
            for (var i = 0; i < classSymbol.TypeParameters.Length; ++i)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(classSymbol.TypeParameters[i].Name);
            }
            sb.Append(">");
        }
        return sb.ToString();
    }

    private static string GetMethodFullName(IMethodSymbol methodSymbol)
    {
        var sb = new StringBuilder();
        sb.Append(methodSymbol.Name);

        if (methodSymbol.IsGenericMethod)
        {
            sb.Append("<");
            for (var i = 0; i < methodSymbol.TypeParameters.Length; ++i)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(methodSymbol.TypeParameters[i].Name);
            }
            sb.Append(">");
        }

        return sb.ToString();
    }

    internal enum ReadWriteMode
    {
        Implied,
        Read,
        Write,
        ReadWrite
    }

    internal enum AddressingMode
    {
        Implied,
        Accumulator,
        Immediate,
        Zeropage,
        ZeropageXIndexed,
        ZeropageYIndexed,
        Absolute,
        AbsoluteXIndexed,
        AbsoluteYIndexed,
        Indirect,
        IndirectXIndexed,
        IndirectYIndexed,
        Relative
    }

    private readonly struct CpuInstructionData
    {
        public readonly string? Namespace;
        public readonly string ClassName;
        public readonly string MethodName;
        public readonly string OpName;
        public readonly int Opcode;
        public readonly ReadWriteMode ReadWriteMode;
        public readonly AddressingMode AddressingMode;

        public CpuInstructionData(string? nsName, string className, string methodName, string opName, int opcode, ReadWriteMode readWriteMode, AddressingMode addressingMode)
        {
            Namespace = nsName;
            ClassName = className;
            MethodName = methodName;
            OpName = opName;
            Opcode = opcode;
            ReadWriteMode = readWriteMode;
            AddressingMode = addressingMode;
        }
    }
}
