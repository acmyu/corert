// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

using Internal.JitInterface;
using Internal.Text;
using Internal.TypeSystem;

namespace ILCompiler.DependencyAnalysis.ReadyToRun
{
    public class MethodFixupSignature : Signature
    {
        private readonly ReadyToRunFixupKind _fixupKind;

        private readonly MethodDesc _methodDesc;
        
        private readonly mdToken _methodToken;

        public MethodFixupSignature(ReadyToRunFixupKind fixupKind, MethodDesc methodDesc, mdToken methodToken)
        {
            _fixupKind = fixupKind;
            _methodDesc = methodDesc;
            _methodToken = methodToken;
        }

        protected override int ClassCode => 150063499;

        public override ObjectData GetData(NodeFactory factory, bool relocsOnly = false)
        {
            ReadyToRunCodegenNodeFactory r2rFactory = (ReadyToRunCodegenNodeFactory)factory;
            ObjectDataBuilder dataBuilder = new ObjectDataBuilder();
            dataBuilder.AddSymbol(this);

            dataBuilder.EmitByte((byte)_fixupKind);
            SignatureBuilder.MethodSigKind sigKind;
            switch (_fixupKind)
            {
                case ReadyToRunFixupKind.READYTORUN_FIXUP_MethodEntry:
                case ReadyToRunFixupKind.READYTORUN_FIXUP_VirtualEntry:
                    sigKind = SignatureBuilder.MethodSigKind.General;
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_MethodEntry_DefToken:
                // case ReadyToRunFixupKind.READYTORUN_FIXUP_VirtualEntry_DefToken:
                    sigKind = SignatureBuilder.MethodSigKind.DefToken;
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_MethodEntry_RefToken:
                // case ReadyToRunFixupKind.READYTORUN_FIXUP_VirtualEntry_RefToken:
                    sigKind = SignatureBuilder.MethodSigKind.RefToken;
                    break;

                default:
                    throw new NotImplementedException();
            }

            SignatureBuilder.EmitMethod(ref dataBuilder, _methodDesc, _methodToken, sigKind);

            return dataBuilder.ToObjectData();
        }

        public override void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append(nameMangler.CompilationUnitPrefix);
            sb.Append($@"MethodFixupSignature({_fixupKind.ToString()} {(uint)_methodToken:X8}): {_methodDesc.ToString()}");
        }

        protected override int CompareToImpl(SortableDependencyNode other, CompilerComparer comparer)
        {
            return _methodToken.CompareTo(((MethodFixupSignature)other)._methodToken);
        }
    }
}
