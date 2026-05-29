using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace WalletApp.Helpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class FormFileExtensionsAttribute : DataTypeAttribute
    {
        private readonly FileExtensionsAttribute _innerAttribute = new();

        public FormFileExtensionsAttribute() : base(DataType.Upload)
        {
            ErrorMessage = _innerAttribute.ErrorMessage;
        }

        //public FormFileExtensionsAttribute(DataType dataType) : base(dataType)
        //{
        //}

        //public FormFileExtensionsAttribute([NotNull] string customDataType) : base(customDataType)
        //{
        //}

        /// <summary>
        ///     Gets or sets the file name extensions.
        /// </summary>
        /// <returns>
        ///     The file name extensions, or the default file extensions (".png", ".jpg", ".jpeg", and ".gif") if the property is not set.
        /// </returns>
        public string Extensions
        {
            get => _innerAttribute.Extensions;
            set => _innerAttribute.Extensions = value;
        }

        /// <summary>
        ///     Applies formatting to an error message, based on the data field where the error occurred.
        /// </summary>
        /// <returns>
        ///     The formatted error message.
        /// </returns>
        /// <param name="name">The name of the field that caused the validation failure.</param>
        public override string FormatErrorMessage(string name)
        {
            return _innerAttribute.FormatErrorMessage(name);
        }

        /// <summary>
        ///     Checks that the specified file name extension or extensions is valid.
        /// </summary>
        /// <returns>
        ///     true if the file name extension is valid; otherwise, false.
        /// </returns>
        /// <param name="value">A comma delimited list of valid file extensions.</param>
        public override bool IsValid(object? value)
        {
            if (value is IFormFile file)
            {
                return _innerAttribute.IsValid(file.FileName);
            }

            return _innerAttribute.IsValid(value);
        }
    }
}
