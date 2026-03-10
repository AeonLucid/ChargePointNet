using System.Text;

namespace ChargePointNet.Packets.Generator.CodeGen;

internal interface ICodeGen
{
    void WriteU8(StringBuilder builder, string ident, object? value, PacketFieldAttribute attribute);
    void WriteU16(StringBuilder builder, string ident, object? value, PacketFieldAttribute attribute);
    void WriteU32(StringBuilder builder, string ident, object? value, PacketFieldAttribute attribute);
    void WriteBytes(StringBuilder builder, string ident, string? refName, PacketFieldAttribute attribute);
    void WriteString(StringBuilder builder, string ident, string? refName, PacketFieldAttribute attribute);
    void WriteObject(StringBuilder builder, string ident, string? refName, string typeName, PacketFieldAttribute attribute);
    void WriteObjectArray(StringBuilder builder, string ident, string? refName, string typeName, PacketFieldAttribute attribute, ArrayElementDelegate writeElement);
}