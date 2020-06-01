using System;
using System.Collections.Generic;
using RazorEngineCore;

namespace BlazorEditor.Compiler
{
    public class IncludeTemplateBase : RazorEngineTemplateBase
    {
        public Func<string, object, string> IncludeCallback;
        public Action<string> LayoutCallback;
        public ViewData ViewData = new ViewData();

        private string _layout;

        public string Include(string key, object model = null)
        {
            return IncludeCallback(key, model);
        }

        public string Layout
        {
            get => _layout;
            set
            {
                _layout = value;
                LayoutCallback(_layout);
            }
        }
    }
}