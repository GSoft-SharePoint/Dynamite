﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes.Readers;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Handles reading values from a SharePoint list item, from a DataRow obtained from a CAML query
    /// over list items or from a DataRow obtained from a Search query.
    /// </summary>
    public class FieldValueReader : IFieldValueReader 
    {
        private readonly IDictionary<Type, IBaseValueReader> readers = new Dictionary<Type, IBaseValueReader>();

        public FieldValueReader(
            StringValueReader stringValueReader,
            BooleanValueReader boolValueReader,
            IntegerValueReader integerValueReader,
            DoubleValueReader doubleValueReader,
            DateTimeValueReader dateTimeValueReader,
            GuidValueReader guidValueReader,
            TaxonomyValueReader taxonomyValueReader,
            TaxonomyValueCollectionReader taxonomyValueCollectionReader,
            LookupValueReader lookupValueReader,
            LookupValueCollectionReader lookupValueCollectionReader,
            PrincipalValueReader principalValueReader,
            UserValueReader userValueReader,
            UserValueCollectionReader userValueCollectionReader,
            UrlValueReader urlValueReader,
            ImageValueReader imageValueReader,
            MediaValueReader mediaValueReader)
        {
            this.AddToReadersDictionary(stringValueReader);
            this.AddToReadersDictionary(boolValueReader);
            this.AddToReadersDictionary(integerValueReader);
            this.AddToReadersDictionary(doubleValueReader);
            this.AddToReadersDictionary(dateTimeValueReader);
            this.AddToReadersDictionary(guidValueReader);
            this.AddToReadersDictionary(taxonomyValueReader);
            this.AddToReadersDictionary(taxonomyValueCollectionReader);
            this.AddToReadersDictionary(lookupValueReader);
            this.AddToReadersDictionary(lookupValueCollectionReader);
            this.AddToReadersDictionary(principalValueReader);
            this.AddToReadersDictionary(userValueReader);
            this.AddToReadersDictionary(userValueCollectionReader);
            this.AddToReadersDictionary(urlValueReader);
            this.AddToReadersDictionary(imageValueReader);
            this.AddToReadersDictionary(mediaValueReader);
        }

        /// <summary>
        /// Reads a field value from a list item
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="item">The list item we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public T ReadValueFromListItem<T>(SPListItem item, string fieldInternalName)
        {
            IBaseValueReader selectedReader = this.GetReader(typeof(T));
            object valueThatWasRead = selectedReader.GetType().GetMethod("ReadValueFromListItem").Invoke(selectedReader, new object[] { item, fieldInternalName });
            return (T)valueThatWasRead;
        }

        /// <summary>
        /// Reads a field value from a list item version
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="itemVersion">The list item version we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public T ReadValueFromListItemVersion<T>(SPListItemVersion itemVersion, string fieldInternalName)
        {
            IBaseValueReader selectedReader = this.GetReader(typeof(T));
            object valueThatWasRead = selectedReader.GetType().GetMethod("ReadValueFromListItemVersion").Invoke(selectedReader, new object[] { itemVersion, fieldInternalName });
            return (T)valueThatWasRead;
        }

        /// <summary>
        /// Reads a field value from a DataRow returned by a CAML query
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        public T ReadValueFromCamlResultDataRow<T>(DataRow dataRowFromCamlResult, string fieldInternalName)
        {
            IBaseValueReader selectedReader = this.GetReader(typeof(T));
            object valueThatWasRead = selectedReader.GetType().GetMethod("ReadValueFromCamlResultDataRow").Invoke(selectedReader, new object[] { dataRowFromCamlResult, fieldInternalName });
            return (T)valueThatWasRead;
        }

        public IBaseValueReader GetValueReaderForType(Type valueType)
        {
            Type readerTypeArgument = valueType;
            if ((valueType.IsValueType || valueType.IsPrimitive) && !valueType.Name.StartsWith("Nullable", StringComparison.OrdinalIgnoreCase))
            {
                // Readers for primitives or structs always handles the Nullable versions of those value types
                readerTypeArgument = typeof(Nullable<>).MakeGenericType(valueType);
            }

            if (this.readers.ContainsKey(readerTypeArgument))
            {
                return readers[readerTypeArgument];
            }

            return null;
        }

        /// <summary>
        /// Reads a field value from a DataRow returned by a Search query
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        ////T ReadValueFromSearchResultDataRow<T>(DataRow dataRowFromSearchResult, string fieldManagedPropertyName);
        private void AddToReadersDictionary(IBaseValueReader reader)
        {
            this.readers.Add(reader.AssociatedValueType, reader);
        }

        private IBaseValueReader GetReader(Type typeOfValueWeWantToRead)
        {
            Type readerTypeArgument = typeOfValueWeWantToRead;
            if ((typeOfValueWeWantToRead.IsValueType || typeOfValueWeWantToRead.IsPrimitive)
                && !typeOfValueWeWantToRead.Name.StartsWith("Nullable", StringComparison.OrdinalIgnoreCase))
            {
                // Readers for primitives or structs always handles the Nullable versions of those value types
                readerTypeArgument = typeof(Nullable<>).MakeGenericType(typeOfValueWeWantToRead);
            }

            if (this.readers.ContainsKey(readerTypeArgument))
            {
                return readers[readerTypeArgument];
            }
            else
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to find a value reader for the specified AssociatedValueType (valueType={0})",
                    typeOfValueWeWantToRead.ToString()));
            }
        }
    }
}