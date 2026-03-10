using System;
using System.Text;

namespace ChargePointNet.Packets.Generator.CodeGen;

internal class CodeGenSerialize : ICodeGen
{
    public const string BytesSizeRef = "bytesSize";
        
    public void WriteU8(StringBuilder builder, string ident, object? value, PacketFieldAttribute attribute)
    {
        value ??= 0;

        builder.AppendFormat("{0}writer.WriteU8({1});\n", ident, value);
    }

    public void WriteU16(StringBuilder builder, string ident, object? value, PacketFieldAttribute attribute)
    {
        value ??= 0;
        
        builder.AppendFormat(attribute.BigEndian 
                ? "{0}writer.WriteU16BE({1});\n" 
                : "{0}writer.WriteU16({1});\n", ident, value);
    }

    public void WriteU32(StringBuilder builder, string ident, object? value, PacketFieldAttribute attribute)
    {
        value ??= 0;
        
        builder.AppendFormat("{0}writer.WriteU32({1});\n", ident, value);
    }

    public void WriteBytes(StringBuilder builder, string ident, string? refName, PacketFieldAttribute attribute)
    {
        if (string.IsNullOrEmpty(refName))
        {
            throw new ArgumentNullException(nameof(refName));
        }
        
        builder.AppendFormat("{0}if ({1} == null)\n", ident, refName);
        builder.AppendFormat("{0}{{\n", ident);
        if (attribute.FixedSize > 0)
        {
            builder.AppendFormat("{0}writer.Skip({1});\n", ident + Constants.DefaultIdent, attribute.FixedSize);
        }
        else
        {
            switch (attribute.LengthSize)
            {
                case 1:
                    WriteU8(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                case 2:
                    WriteU16(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                case 4:
                    WriteU32(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                default:
                    throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
            }
        }
        builder.AppendFormat("{0}}} else {{\n", ident);
        builder.AppendFormat("{0}{1} = {2}.Length;\n", ident + Constants.DefaultIdent, BytesSizeRef, refName);
        if (attribute.FixedSize > 0)
        {
            builder.AppendFormat("{0}if ({1} != {2})\n", ident + Constants.DefaultIdent, BytesSizeRef, attribute.FixedSize);
            builder.AppendFormat("{0}{{\n", ident + Constants.DefaultIdent);
            builder.AppendFormat("{0}throw new InvalidOperationException(\"Bytes length mismatch, should be {1}\");\n", ident + Constants.DefaultIdent + Constants.DefaultIdent, attribute.FixedSize);
            builder.AppendFormat("{0}}}\n", ident + Constants.DefaultIdent);
        }
        else
        {
            switch (attribute.LengthSize)
            {
                case 1:
                    WriteU8(builder, ident + Constants.DefaultIdent, $"Convert.ToByte({BytesSizeRef})", attribute);
                    break;
                case 2:
                    WriteU16(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt16({BytesSizeRef})", attribute);
                    break;
                case 4:
                    WriteU32(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt32({BytesSizeRef})", attribute);
                    break;
                default:
                    throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
            }
        }
        builder.AppendFormat("{0}writer.WriteBytes({1});\n", ident + Constants.DefaultIdent, refName);
        builder.AppendFormat("{0}}}\n", ident);
    }

    public void WriteString(StringBuilder builder, string ident, string? refName, PacketFieldAttribute attribute)
    {
        if (string.IsNullOrEmpty(refName))
        {
            throw new ArgumentNullException(nameof(refName));
        }
        
        builder.AppendFormat("{0}if (string.IsNullOrEmpty({1}))\n", ident, refName);
        builder.AppendFormat("{0}{{\n", ident);
        if (attribute.FixedSize > 0)
        {
            builder.AppendFormat("{0}writer.Skip({1});\n", ident + Constants.DefaultIdent, attribute.FixedSize);
        }
        else
        {
            switch (attribute.LengthSize)
            {
                case 1:
                    WriteU8(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                case 2:
                    WriteU16(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                case 4:
                    WriteU32(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                default:
                    throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
            }
        }
        builder.AppendFormat("{0}}} else {{\n", ident);
        builder.AppendFormat("{0}{1} = Encoding.{2}.GetByteCount({3});\n", ident + Constants.DefaultIdent, BytesSizeRef, attribute.Encoding, refName);
        if (attribute.FixedSize > 0)
        {
            builder.AppendFormat("{0}if ({1} != {2})\n", ident + Constants.DefaultIdent, BytesSizeRef, attribute.FixedSize);
            builder.AppendFormat("{0}{{\n", ident + Constants.DefaultIdent);
            builder.AppendFormat("{0}throw new InvalidOperationException(\"String length mismatch, should be {1}\");\n", ident + Constants.DefaultIdent + Constants.DefaultIdent, attribute.FixedSize);
            builder.AppendFormat("{0}}}\n", ident + Constants.DefaultIdent);
        }
        else
        {
            switch (attribute.LengthSize)
            {
                case 1:
                    WriteU8(builder, ident + Constants.DefaultIdent, $"Convert.ToByte({BytesSizeRef})", attribute);
                    break;
                case 2:
                    WriteU16(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt16({BytesSizeRef})", attribute);
                    break;
                case 4:
                    WriteU32(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt32({BytesSizeRef})", attribute);
                    break;
                default:
                    throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
            }
        }
        builder.AppendFormat("{0}writer.WriteString(Encoding.{1}, {2});\n", ident + Constants.DefaultIdent, attribute.Encoding, refName);
        builder.AppendFormat("{0}}}\n", ident);
    }

    public void WriteObject(StringBuilder builder, string ident, string? refName, string typeName, PacketFieldAttribute attribute)
    {
        if (string.IsNullOrEmpty(refName))
        {
            throw new ArgumentNullException(nameof(refName));
        }
        
        builder.AppendFormat("{0}{1}.Serialize(version, ref writer);\n", ident, refName);
    }

    public void WriteObjectArray(StringBuilder builder, string ident, string? refName, string typeName, PacketFieldAttribute attribute, ArrayElementDelegate writeElement)
    {
        if (string.IsNullOrEmpty(refName))
        {
            throw new ArgumentNullException(nameof(refName));
        }
        
        builder.AppendFormat("{0}if ({1} != null)\n", ident, refName);
        builder.AppendFormat("{0}{{\n", ident);
        
        if (attribute.MaxSize != -1)
        {
            builder.AppendFormat("{0}if ({1}.Length > {2})\n", ident + Constants.DefaultIdent, refName, attribute.MaxSize);
            builder.AppendFormat("{0}{{\n", ident + Constants.DefaultIdent);
            builder.AppendFormat("{0}throw new InvalidOperationException($\"{{nameof({1})}} length should be less than or equal to {2}\");\n", ident + Constants.DefaultIdent + Constants.DefaultIdent, refName, attribute.MaxSize);
            builder.AppendFormat("{0}}}\n", ident + Constants.DefaultIdent);
        }
        
        // Array with items.
        if (attribute.FixedSize > 0)
        {
            builder.AppendFormat("{0}if ({1}.Length != {2})\n", ident + Constants.DefaultIdent, refName, attribute.FixedSize);
            builder.AppendFormat("{0}{{\n", ident + Constants.DefaultIdent);
            builder.AppendFormat("{0}throw new InvalidOperationException($\"{{nameof({1})}} length should be equal to {2}\");\n", ident + Constants.DefaultIdent + Constants.DefaultIdent, refName, attribute.FixedSize);
            builder.AppendFormat("{0}}}\n", ident + Constants.DefaultIdent);
        }
        else
        {
            switch (attribute.LengthSize)
            {
                case 1:
                    WriteU8(builder, ident + Constants.DefaultIdent, $"Convert.ToByte({refName}.Length)", attribute);
                    break;
                case 2:
                    WriteU16(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt16({refName}.Length)", attribute);
                    break;
                case 4:
                    WriteU32(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt32({refName}.Length)", attribute);
                    break;
                default:
                    throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
            }
        }
        
        builder.AppendFormat("{0}for (var i = 0; i < {1}.Length; i++)\n", ident + Constants.DefaultIdent, refName);
        builder.AppendFormat("{0}{{\n", ident + Constants.DefaultIdent);
        writeElement(ident + Constants.DefaultIdent + Constants.DefaultIdent, $"{refName}[i]");
        builder.AppendFormat("{0}}}\n", ident + Constants.DefaultIdent);
        
        builder.AppendFormat("{0}}} else {{\n", ident);
        
        // Null array.
        if (attribute.FixedSize > 0)
        {
            builder.AppendFormat("{0}for (var i = 0; i < {1}; i++)\n", ident + Constants.DefaultIdent, attribute.FixedSize);
            builder.AppendFormat("{0}{{\n", ident + Constants.DefaultIdent);
            // We set the ref to null because there is no array entry to reference.
            // Let WriteObject deal with it, should probably create an instance and get the size of that.
            writeElement(ident + Constants.DefaultIdent + Constants.DefaultIdent, null);
            builder.AppendFormat("{0}}}\n", ident + Constants.DefaultIdent);
        }
        else
        {
            switch (attribute.LengthSize)
            {
                case 1:
                    WriteU8(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                case 2:
                    WriteU16(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                case 4:
                    WriteU32(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                default:
                    throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
            }
        }
        
        builder.AppendFormat("{0}}}\n", ident);
    }
}