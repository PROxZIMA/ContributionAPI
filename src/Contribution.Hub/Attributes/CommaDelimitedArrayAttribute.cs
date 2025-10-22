using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;

namespace Contribution.Hub.Attributes;

/// <summary>
/// Attribute that enables comma-delimited array binding for query parameters.
/// Allows both "?param=value1,value2" and "?param=value1&param=value2" formats.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class CommaDelimitedArrayAttribute : Attribute, IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType.IsArray || 
            (context.Metadata.ModelType.IsGenericType && 
             context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        {
            return new CommaDelimitedArrayModelBinder();
        }

        return null;
    }
}

/// <summary>
/// Model binder that handles comma-delimited values in query parameters.
/// </summary>
public class CommaDelimitedArrayModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var values = valueProviderResult.Values.ToArray();
        
        if (values.Length == 0)
        {
            return Task.CompletedTask;
        }

        // Split comma-delimited values and flatten the array
        var allValues = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .SelectMany(v => v!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(v => v.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToArray();

        if (allValues.Length == 0)
        {
            return Task.CompletedTask;
        }

        var elementType = bindingContext.ModelType.IsArray 
            ? bindingContext.ModelType.GetElementType()
            : bindingContext.ModelType.GetGenericArguments().FirstOrDefault();

        if (elementType == null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        try
        {
            var convertedValues = allValues
                .Select(v => ConvertValue(v, elementType))
                .Where(v => v != null)
                .ToArray();

            if (bindingContext.ModelType.IsArray)
            {
                var array = Array.CreateInstance(elementType, convertedValues.Length);
                for (int i = 0; i < convertedValues.Length; i++)
                {
                    array.SetValue(convertedValues[i], i);
                }
                bindingContext.Result = ModelBindingResult.Success(array);
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(convertedValues);
            }
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }

    private static object? ConvertValue(string value, Type targetType)
    {
        if (targetType == typeof(string))
        {
            return value;
        }

        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(typeof(string)))
        {
            return converter.ConvertFrom(value);
        }

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            var underlyingConverter = TypeDescriptor.GetConverter(underlyingType);
            if (underlyingConverter.CanConvertFrom(typeof(string)))
            {
                return underlyingConverter.ConvertFrom(value);
            }
        }

        return Convert.ChangeType(value, targetType);
    }
}