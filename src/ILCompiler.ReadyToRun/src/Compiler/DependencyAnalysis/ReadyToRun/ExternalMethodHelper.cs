
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

using Internal.JitInterface;
using Internal.TypeSystem;

namespace ILCompiler.DependencyAnalysis.ReadyToRun
{
    public class ExternalMethodHelper : DelayLoadHelper, IMethodNode
    {
        private readonly MethodDesc _methodDesc;

        private readonly mdToken _token;

        private readonly MethodWithGCInfo _localMethod;

        public ExternalMethodHelper(
            ReadyToRunCodegenNodeFactory factory,
            ReadyToRunFixupKind fixupKind,
            MethodDesc methodDesc,
            mdToken token,
            MethodWithGCInfo localMethod)
            : base(ReadyToRunHelper.READYTORUN_HELPER_DelayLoad_MethodCall, factory, new MethodFixupSignature(fixupKind, methodDesc, token))
        {
            _methodDesc = methodDesc;
            _token = token;
            _localMethod = localMethod;
        }

        public MethodDesc Method => _methodDesc;

        int ISortableSymbolNode.ClassCode => 458823351;

        public int CompareToImpl(ISortableSymbolNode other, CompilerComparer comparer)
        {
            throw new NotImplementedException();
        }

        public override ObjectNode.ObjectData GetData(NodeFactory factory, bool relocsOnly)
        {
            ObjectData data = base.GetData(factory, relocsOnly);
            if (relocsOnly && _localMethod != null)
            {
                Relocation[] augmentedRelocs = new Relocation[data.Relocs.Length + 1];
                Array.Copy(data.Relocs, 0, augmentedRelocs, 1, data.Relocs.Length);
                augmentedRelocs[0] = new Relocation(RelocType.IMAGE_REL_BASED_ABSOLUTE, 0, _localMethod);
                data = new ObjectData(data.Data, augmentedRelocs, data.Alignment, data.DefinedSymbols);
            }
            return data;
        }
    }
}
