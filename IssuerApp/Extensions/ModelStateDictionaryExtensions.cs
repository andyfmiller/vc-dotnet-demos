using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace IssuerApp.Extensions
{
    public static class ModelStateDictionaryExtensions
    {
        public static void RemoveKeys(this ModelStateDictionary modelState, string pattern)
        {
            var keys = modelState.Keys.Where(x => x.StartsWith(pattern)).ToList();
            foreach (var key in keys)
            {
                modelState.Remove(key);
            }
        }
    }
}
