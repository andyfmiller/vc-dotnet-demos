using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
// ReSharper disable UnusedMember.Global

namespace WalletApp.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent? DescriptionFor<TModel, TValue>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            var body = (MemberExpression)expression.Body;
            var descriptionAttribute = (DescriptionAttribute?)body.Member
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault();

            if (descriptionAttribute != null)
            {
                var content = $"<description><span data-toggle='tooltip' title=\"{descriptionAttribute.Description}\"><i class=\"fas fa-info-circle text-muted\"></i></span></description>";
                return new HtmlString(content);
            }

            return null;
        }

        public static IHtmlContent? DescriptionTextFor<TModel, TValue>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            var body = (MemberExpression)expression.Body;
            var descriptionAttribute = (DescriptionAttribute?)body.Member
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault();

            if (descriptionAttribute != null)
            {
                var content = descriptionAttribute.Description;
                return new HtmlString(content);
            }

            return null;
        }

        public static string DisplayJsonNameFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var body = (MemberExpression)expression.Body;
            var jsonPropertyNameAttribute = (JsonPropertyNameAttribute?)body.Member
                .GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                .FirstOrDefault();
            if (jsonPropertyNameAttribute != null) return jsonPropertyNameAttribute.Name;

            return html.DisplayNameFor(expression);
        }
    }
}
