using System;
using System.Collections.Generic;
using System.Xml;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;

namespace RomanticWeb.Converters
{
    /// <summary>Converter for xsd:decimal.</summary>
    public class DecimalConverter : XsdConverterBase
    {
        /// <summary>Gets Uri of xsd:decimal.</summary>
        protected override IEnumerable<Uri> SupportedDataTypes { get { yield return Xsd.Decimal; } }

        /// <summary>Converts the decimal value to a literal node.</summary>
        public override INode ConvertBack(object value, IEntityContext context)
        {
            return Node.ForLiteral(XmlConvert.ToString((Decimal)value), Xsd.Decimal);
        }

        /// <inheritdoc />
        public override Uri CanConvertBack(Type type)
        {
            return (type == typeof(decimal) ? Xsd.Decimal : null);
        }

        /// <inheritdoc />
        protected override object ConvertInternal(INode literalNode)
        {
            return XmlConvert.ToDecimal(literalNode.Literal);
        }
    }
}