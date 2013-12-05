﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NullGuard;
using RomanticWeb.Entities;

namespace RomanticWeb.ComponentModel
{
    public class EntityIdTypeConverter:TypeConverter
    {
        #region Fields
        private static UriTypeConverter uriTypeConverter=null;
        #endregion

        #region Constructors
        static EntityIdTypeConverter()
        {
            uriTypeConverter=(UriTypeConverter)TypeDescriptor.GetConverter(typeof(Uri));
        }
        #endregion

        #region Public methods
        /// <summary>Returns whether this converter can convert an object of the given type to the type of this converter.</summary>
        /// <param name="context">Type: <see cref="System.ComponentModel.ITypeDescriptorContext" />
        /// An <see cref="System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="sourceType">Type: <see cref="System.Type" />
        /// A <see cref="System.Type" /> that represents the type you want to convert from.</param>
        /// <returns>Type: <see cref="System.Boolean" />
        /// <b>true</b> if this converter can perform the conversion; otherwise, <b>false</b>.</returns>
        public override bool CanConvertFrom([AllowNull] ITypeDescriptorContext context,Type sourceType)
        {
            return (uriTypeConverter.CanConvertFrom(context,sourceType))||(base.CanConvertFrom(context,sourceType));
        }

        /// <summary>Returns whether this converter can convert the object to the specified type, using the specified context.</summary>
        /// <param name="context">Type: <see cref="System.ComponentModel.ITypeDescriptorContext" />
        /// An <see cref="System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="destinationType">Type: <see cref="System.Type" />
        /// A <see cref="System.Type" /> that represents the type you want to convert to.</param>
        /// <returns>Type: <see cref="System.Boolean" />
        /// <b>true</b> if this converter can perform the conversion; otherwise, <b>false</b>.</returns>
        public override bool CanConvertTo([AllowNull] ITypeDescriptorContext context,Type destinationType)
        {
            return (destinationType==typeof(string))||(destinationType==typeof(Uri))||(base.CanConvertTo(context,destinationType));
        }

        /// <summary>Converts the given object to the type of this converter, using the specified context and culture information.</summary>
        /// <param name="context">Type: <see cref="System.ComponentModel.ITypeDescriptorContext" />
        /// An <see cref="System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">Type: <see cref="System.Globalization.CultureInfo" />
        /// The <see cref="System.Globalization.CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">Type: <see cref="System.Object" />
        /// The <see cref="System.Object" /> to convert.</param>
        /// <returns>Type: <see cref="System.Object" />
        /// An <see cref="System.Object" /> that represents the converted value.</returns>
        public override object ConvertFrom([AllowNull] ITypeDescriptorContext context,[AllowNull] CultureInfo culture,[AllowNull] object value)
        {
            object result=null;
            if (value!=null)
            {
                Uri uri=(Uri)uriTypeConverter.ConvertFrom(context,culture,value);
                if (uri!=null)
                {
                    result=new EntityId(uri);
                }
                else
                {
                    result=base.ConvertFrom(context,culture,value);
                }
            }
            else
            {
                result=base.ConvertFrom(context,culture,value);
            }

            return result;
        }

        /// <summary>Converts the given value object to the specified type, using the specified context and culture information.</summary>
        /// <param name="context">Type: <see cref="System.ComponentModel.ITypeDescriptorContext" />
        /// An <see cref="System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">Type: <see cref="System.Globalization.CultureInfo" />
        /// A <see cref="System.Globalization.CultureInfo" />. If <b>null</b> is passed, the current culture is assumed.</param>
        /// <param name="value">Type: <see cref="System.Object" />
        /// The <see cref="System.Object" /> to convert.</param>
        /// <param name="destinationType">Type: <see cref="System.Type" />
        /// The <see cref="System.Type" /> to convert the value parameter to.</param>
        /// <returns>Type: <see cref="System.Object" />
        /// An <see cref="System.Object" /> that represents the converted value.</returns>
        public override object ConvertTo([AllowNull] ITypeDescriptorContext context,[AllowNull] CultureInfo culture,[AllowNull] object value,Type destinationType)
        {
            object result=null;
            if ((value!=null)&&(typeof(EntityId).IsAssignableFrom(value.GetType()))&&((destinationType==typeof(string))||(destinationType==typeof(Uri))))
            {
                if (destinationType==typeof(string))
                {
                    result=((EntityId)value).Uri.ToString();
                }
                else if (destinationType==typeof(Uri))
                {
                    result=((EntityId)value).Uri;
                }
            }
            else
            {
                result=base.ConvertTo(context,culture,value,destinationType);
            }

            return result;
        }

        /// <summary>Returns whether the given value object is valid for this type and for the specified context.</summary>
        /// <param name="context">Type: <see cref="System.ComponentModel.ITypeDescriptorContext" />
        /// An <see cref="System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="value">Type: <see cref="System.Object" />
        /// The <see cref="System.Object" /> to test for validity.</param>
        /// <returns>Type: <see cref="System.Boolean" />
        /// <b>true</b> if the specified value is valid for this object; otherwise <b>false</b>.</returns>
        public override bool IsValid([AllowNull] ITypeDescriptorContext context,[AllowNull] object value)
        {
            return ((value!=null)&&(value is EntityId)&&(uriTypeConverter.IsValid(((EntityId)value).Uri)))||(base.IsValid(context,value));
        }
        #endregion
    }
}