namespace BlazorEditor.Utils
{
    public class InitialItemValues
    {
        public static string[] ComponentValues = new[]
        {
            "<p class=\"blue-text\">This is a component!</p>", ".blue-text {\n    color: blue;\n}", ""
        };

        public static string[] PageValues = new[]
        {
            "@{ Layout = \"_Layout\"; ViewData[\"Title\"] = \"Index\"; }\n\n<h1>Welcome to your new webweb2 installation!</h1>\n<p>To get started, go to the documentation <a href=\"https://github.com/merlin04/webweb2\">here</a>.</p>\n\n<p>Here's an example of a component:</p>\n@Include(\"TestComponent\")", "", ""
        };

        public static string[] LayoutValues = new[]
        {
            "<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>@ViewData[\"Title\"]</title>\n    <style>@RenderCss()</style>\n</head>\n<body>\n    @RenderBody()\n    <script>\n        @RenderJs()\n    </script>\n</body>\n</html>"
        };
    }
}