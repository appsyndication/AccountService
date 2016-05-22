using System;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Razor.TagHelpers;

namespace AppSyndication.AccountService.Web.TagHelpers
{
    [HtmlTargetElement("div", Attributes = ErrorSummaryModelAttributeName)]
    public class ErrorSummaryTagHelper : TagHelper
    {
        private const string ErrorSummaryModelAttributeName = "error-summary";
        private const string ErrorSummaryClass = "alert alert-danger";

        [HtmlAttributeName(ErrorSummaryModelAttributeName)]
        public ModelStateDictionary ErrorSummaryModel { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (this.ErrorSummaryModel.IsValid)
            {
                output.SuppressOutput();
                return;
            }

            var message = this.GetErrorMessage() ?? "Please contact support.";

            output.Content.AppendHtml($"<strong>Error:</strong> {message}");

            var classAttribute = output.Attributes["class"];
            output.Attributes["class"] = classAttribute == null ? ErrorSummaryClass : classAttribute.Value + " " + ErrorSummaryClass;
        }

        private string GetErrorMessage()
        {
            ModelStateEntry entry;

            if (!this.ErrorSummaryModel.TryGetValue(String.Empty, out entry) || !entry.Errors.Any())
            {
                entry = this.ErrorSummaryModel.Values.FirstOrDefault(e => e.Errors.Any());
            }

            return entry?.Errors.First().ErrorMessage;
        }
    }
}
