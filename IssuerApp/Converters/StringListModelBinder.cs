using IssuerApp.Data.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IssuerApp.Converters
{
    public class StringListModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext context)
        {
            var modelName = context.ModelName;
            var valueProviderResult = context.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                context.Result = ModelBindingResult.Success(null);
            }
            else
            {
                var stringList = new StringList();
                foreach (var resultValue in valueProviderResult.Values.ToList())
                {
                    if (resultValue == null) continue;
                    stringList.AddRange(resultValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                }
                context.Result = ModelBindingResult.Success(stringList);
            }

            return Task.CompletedTask;
        }
    }
}
