using System;
using System.Globalization;
using System.Web.Mvc;

namespace Desing.ModelBinders
{
    /// <summary>
    /// Binds <see cref="decimal"/> / <see cref="Nullable{Decimal}"/> using invariant
    /// culture first (dot decimals from JS and hidden fields), then
    /// <see cref="CultureInfo.CurrentCulture"/> so valores como "12,5" sigan
    /// funcionando en UI es-ES.
    /// </summary>
    public sealed class CultureFallbackDecimalModelBinder : IModelBinder
    {
        private const NumberStyles Styles =
            NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
            | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint
            | NumberStyles.AllowThousands;

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

            if (string.IsNullOrWhiteSpace(raw))
            {
                return isNullable ? (object)null : 0m;
            }

            if (TryParse(raw, out var parsed))
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

        private static bool TryParse(string raw, out decimal value)
        {
            if (decimal.TryParse(raw, Styles, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            return decimal.TryParse(raw, Styles, CultureInfo.CurrentCulture, out value);
        }
    }
}
