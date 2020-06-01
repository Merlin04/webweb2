namespace BlazorEditor.Utils
{
    public static class NewItemDefaultValues
    {
        public static string[] ComponentValues = new[]
        {
            string.Empty, string.Empty, string.Empty
        };

        // This can also be used for templates
        public static string[] PageValues = new[]
        {
            "@{ Layout = \"_Layout\"; ViewData[\"Title\"] = \"Page\"; }\n", "", ""
        };

        public static string[] LayoutValues = new[]
        {
            "<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>@ViewData[\"Title\"]</title>\n    <style>@RenderCss()</style>\n</head>\n<body>\n    @RenderBody()\n    <script>\n        @RenderJs()\n    </script>\n</body>\n</html>"
        };
    }
}