using System.Collections.Generic;
using RazorEngineCore;

namespace BlazorEditor.Compiler
{
    public class LayoutTemplateBase : RazorEngineTemplateBase
    {
        public Dictionary<string, dynamic> ViewData => Model.ViewData;

        public string RenderBody()
        {
            return Model.RenderBody;
        }

        public string RenderCss()
        {
            return Model.RenderCss;
        }

        public string RenderJs()
        {
            return Model.RenderJs;
        }
    }
}