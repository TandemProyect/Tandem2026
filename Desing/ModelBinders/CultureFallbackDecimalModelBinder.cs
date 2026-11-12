using System;
using System.Globalization;
using System.Web.Mvc;

namespace Desing.ModelBinders
{
    /// <summary>
    /// Binds decimal / double / float (and nullable variants) accepting both
    /// comma and dot decimal separators, so "12,5" and "12.5" save equally.
    /// </summary>
    public sealed class CultureFallbackNumberModelBinder : IModelBinder
    {
        private const NumberStyles Styles =
            NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
            | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint
            | NumberStyles.AllowExponent;

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == null)
            {
                return null;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

            var raw = valueResult.AttemptedValue;
            var underlying = Nullable.GetUnderlyingType(bindingContext.ModelType);
            var isNullable = underlying != null;
            var targetType = underlying ?? bindingContext.ModelType;

            if (string.IsNullOrWhiteSpace(raw))
            {
                if (isNullable)
                {
                    return null;
                }

                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            var normalized = NormalizeDecimalSeparators(raw.Trim());
            object parsed;
            if (TryParse(normalized, targetType, out parsed))
            {
                return parsed;
            }

            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName,
                string.Format(
                    CultureInfo.CurrentUICulture,
                    "The value '{0}' is not valid for {1}.",
                    raw,
                    bindingContext.ModelMetadata.DisplayName
                    ?? bindingContext.ModelMetadata.PropertyName
                    ?? bindingContext.ModelName));
            return null;
        }

        private static string NormalizeDecimalSeparators(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var lastComma = value.LastIndexOf(',');
            var lastDot = value.LastIndexOf('.');
            if (lastComma >= 0 && lastDot >= 0)
            {
                if (lastComma > lastDot)
                {
                    return value.Replace(".", string.Empty).Replace(",", ".");
                }

                return value.Replace(",", string.Empty);
            }

            if (lastComma >= 0)
            {
                return value.Replace(",", ".");
            }

            return value;
        }

        private static bool TryParse(string raw, Type targetType, out object value)
        {
            value = null;

            if (targetType == typeof(decimal))
            {
                decimal decimalValue;
                if (decimal.TryParse(raw, Styles, CultureInfo.InvariantCulture, out decimalValue))
                {
                    value = decimalValue;
                    return true;
                }
                return false;
            }

            if (targetType == typeof(double))
            {
                double doubleValue;
                if (double.TryParse(raw, Styles, CultureInfo.InvariantCulture, out doubleValue))
                {
                    value = doubleValue;
                    return true;
                }
                return false;
            }

            if (targetType == typeof(float))
            {
                float floatValue;
                if (float.TryParse(raw, Styles, CultureInfo.InvariantCulture, out floatValue))
                {
                    value = floatValue;
                    return true;
                }
                return false;
            }

            return false;
        }
    }
}
