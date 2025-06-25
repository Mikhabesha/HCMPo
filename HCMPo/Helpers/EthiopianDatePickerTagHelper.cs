using Microsoft.AspNetCore.Razor.TagHelpers;
using HCMPo.Helpers;

namespace HCMPo.Helpers
{
    [HtmlTargetElement("ethiopian-date-picker")]
    public class EthiopianDatePickerTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public string For { get; set; }

        [HtmlAttributeName("asp-label")]
        public string Label { get; set; }

        [HtmlAttributeName("asp-required")]
        public bool Required { get; set; } = false;

        [HtmlAttributeName("asp-show-gregorian")]
        public bool ShowGregorian { get; set; } = true;

        [HtmlAttributeName("asp-selected-year")]
        public int? SelectedYear { get; set; }

        [HtmlAttributeName("asp-selected-month")]
        public int? SelectedMonth { get; set; }

        [HtmlAttributeName("asp-selected-day")]
        public int? SelectedDay { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            var currentDate = EthiopianCalendarHelper.GetCurrentEthiopianDate();
            var year = SelectedYear ?? currentDate.Year;
            var month = SelectedMonth ?? currentDate.Month;
            var day = SelectedDay ?? currentDate.Day;

            var viewBag = new Dictionary<string, object>
            {
                ["Id"] = For?.Replace(".", "_"),
                ["Name"] = For,
                ["Label"] = Label ?? For?.Replace(".", " "),
                ["Required"] = Required,
                ["ShowGregorian"] = ShowGregorian,
                ["SelectedYear"] = year,
                ["SelectedMonth"] = month,
                ["SelectedDay"] = day
            };

            // Store ViewBag data in context
            context.Items["EthiopianDatePickerViewBag"] = viewBag;

            // Render the partial view
            output.Content.SetHtmlContent($"@{{ ViewBag.Id = \"{viewBag["Id"]}\"; ViewBag.Name = \"{viewBag["Name"]}\"; ViewBag.Label = \"{viewBag["Label"]}\"; ViewBag.Required = {Required.ToString().ToLower()}; ViewBag.ShowGregorian = {ShowGregorian.ToString().ToLower()}; ViewBag.SelectedYear = {year}; ViewBag.SelectedMonth = {month}; ViewBag.SelectedDay = {day}; }}");
            output.Content.AppendHtml("@await Html.PartialAsync(\"_EthiopianDatePicker\")");
        }
    }
} 