﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using ILCompiler.DependencyAnalysisFramework;

using Internal.JitInterface;
using Internal.Runtime;
using Internal.Text;
using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;

using ILCompiler.DependencyAnalysis;

namespace ILCompiler.DependencyAnalysis.ReadyToRun
{
    class RvaEmbeddedPointerIndirectionNode<TTarget> : EmbeddedPointerIndirectionNode<TTarget>, ISymbolDefinitionNode, ISortableSymbolNode
        where TTarget : ISortableSymbolNode
    {
        private readonly string _callSite;

        public RvaEmbeddedPointerIndirectionNode(TTarget target, string callSite)
            : base(target)
        {
            _callSite = callSite;
        }

        protected override string GetName(NodeFactory factory)
        {
            Utf8StringBuilder sb = new Utf8StringBuilder();
            AppendMangledName(factory.NameMangler, sb);
            return sb.ToString();
        }

        public override IEnumerable<DependencyListEntry> GetStaticDependencies(NodeFactory factory)
        {
            return new[]
            {
                new DependencyListEntry(Target, "reloc"),
            };
        }

        public override void EncodeData(ref ObjectDataBuilder dataBuilder, NodeFactory factory, bool relocsOnly)
        {
            dataBuilder.RequireInitialPointerAlignment();
            dataBuilder.EmitReloc(Target, RelocType.IMAGE_REL_BASED_ADDR32NB);
        }

        public override void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append("RVA_");
            Target.AppendMangledName(nameMangler, sb);
            if (_callSite != null)
            {
                sb.Append(" @ ");
                sb.Append(_callSite);
            }
        }

        protected override int ClassCode => -66002498;

        public int Offset => OffsetFromBeginningOfArray;
    }

    public class ImportSectionNode : EmbeddedObjectNode
    {
        private readonly ArrayOfEmbeddedDataNode<Import> _imports;
        // TODO: annoying - today there's no way to put signature RVA's into R/O data section
        private readonly ArrayOfEmbeddedPointersNode<Signature> _signatures;

        private readonly CorCompileImportType _type;
        private readonly CorCompileImportFlags _flags;
        private readonly byte _entrySize;
        private readonly string _name;
        private readonly bool _emitPrecode;

        public ImportSectionNode(string name, CorCompileImportType importType, CorCompileImportFlags flags, byte entrySize, bool emitPrecode)
        {
            _name = name;
            _type = importType;
            _flags = flags;
            _entrySize = entrySize;
            _emitPrecode = emitPrecode;

            _imports = new ArrayOfEmbeddedDataNode<Import>(_name + "_ImportBegin", _name + "_ImportEnd", null);
            _signatures = new ArrayOfEmbeddedPointersNode<Signature>(_name + "_SigBegin", _name + "_SigEnd", null);
        }

        public void AddImport(NodeFactory factory, Import import)
        {
            _imports.AddEmbeddedObject(import);
            _signatures.AddEmbeddedObject(import.ImportSignature);
        }

        public string Name => _name;

        public bool EmitPrecode => _emitPrecode;

        public override bool StaticDependenciesAreComputed => true;

        protected override int ClassCode => -62839441;

        public bool ShouldSkipEmittingTable(NodeFactory factory)
        {
            return _imports.ShouldSkipEmittingObjectNode(factory);
        }

        public override void EncodeData(ref ObjectDataBuilder dataBuilder, NodeFactory factory, bool relocsOnly)
        {
            if (!relocsOnly && _imports.ShouldSkipEmittingObjectNode(factory))
            {
                // Don't emit import section node at all if there are no entries in it
                return;
            }

            dataBuilder.EmitReloc(_imports.StartSymbol, RelocType.IMAGE_REL_BASED_ADDR32NB, 0);
            if (!relocsOnly)
            {
                dataBuilder.EmitInt(_imports.GetData(factory, false).Data.Length);

                dataBuilder.EmitShort((short)_flags);
                dataBuilder.EmitByte((byte)_type);
                dataBuilder.EmitByte(_entrySize);
            }
            if (!_signatures.ShouldSkipEmittingObjectNode(factory))
            {
                dataBuilder.EmitReloc(_signatures.StartSymbol, RelocType.IMAGE_REL_BASED_ADDR32NB, 0);
            }
            else
            {
                dataBuilder.EmitUInt(0);
            }

            // Todo: Auxilliary data
            dataBuilder.EmitUInt(0);
        }

        public override IEnumerable<DependencyListEntry> GetStaticDependencies(NodeFactory context)
        {
            yield return new DependencyListEntry(_imports, "Import section fixup data");
            yield return new DependencyListEntry(_signatures, "Import section signatures");
        }

        protected override string GetName(NodeFactory context)
        {
            return _name;
        }
    }
}
