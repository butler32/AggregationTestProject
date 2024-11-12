using AggregationTestProject.DTOs;
using AngleSharp.Html.Parser;

namespace AggregationTestProject.Services.Utilities
{
    public static class AppHtmlParser
    {
        public static string GetCsrfTokenValue(string htmlString)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(htmlString);
            var inputElement = document.QuerySelector("input[name='_csrf_token']");
            return inputElement?.GetAttribute("value")!;
        }

        public static BoxWorkplace GetBoxWorkplace(string htmlString)
        {
            var boxWorkplace = new BoxWorkplace();

            var parser = new HtmlParser();
            var document = parser.ParseDocument(htmlString);

            var boxCounterNode = document.QuerySelector("span.box-counter");
            if (boxCounterNode != null)
            {
                boxWorkplace.BoxId = int.Parse(boxCounterNode.TextContent.Trim());
            }

            var knobCounterNode = document.QuerySelector("input#knobCounter");
            if (knobCounterNode != null && knobCounterNode.HasAttribute("value"))
            {
                boxWorkplace.ItemsCount = int.Parse(knobCounterNode.GetAttribute("value")!);
            }

            var operatorIdNode = document.QuerySelector("h4.card-title");
            if (operatorIdNode != null)
            {
                var operatorIdText = operatorIdNode.TextContent.Trim();
                var parts = operatorIdText.Split('#');
                if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int operatorId))
                {
                    boxWorkplace.OperatorId = operatorId;
                }
            }

            return boxWorkplace;
        }

        public static PalletWorkplace GetPalletWorkplace(string htmlString)
        {
            var boxWorkplace = new PalletWorkplace();

            var parser = new HtmlParser();
            var document = parser.ParseDocument(htmlString);

            var boxCounterNode = document.QuerySelector("span.pallet-counter");
            if (boxCounterNode != null)
            {
                boxWorkplace.PalletId = int.Parse(boxCounterNode.TextContent.Trim());
            }

            var knobCounterNode = document.QuerySelector("input#knobCounter");
            if (knobCounterNode != null && knobCounterNode.HasAttribute("value"))
            {
                boxWorkplace.ItemsCount = int.Parse(knobCounterNode.GetAttribute("value")!);
            }

            return boxWorkplace;
        }
    }
}
