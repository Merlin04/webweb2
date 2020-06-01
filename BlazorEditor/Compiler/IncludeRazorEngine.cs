using System.Collections.Generic;
using System.Linq;
using RazorEngineCore;

namespace BlazorEditor.Compiler
{
    // @include is not natively supported in RazorEngineCore but it is possible to implement it
    // https://github.com/adoconnection/RazorEngineCore/wiki/@Include-section
    // This has had some modifications to let the engine track which includes are being used
    
    public class IncludeRazorEngine
    {
        public List<string> Assemblies;
        
        public IncludeRazorEngine(List<string> assemblies)
        {
            Assemblies = assemblies;
        }
        
        public Dictionary<string, RazorEngineCompiledTemplate<IncludeTemplateBase>> GetCompiledIncludes(
            IDictionary<string, string> includes)
        {
            RazorEngine razorEngine = new RazorEngine();
            return includes.ToDictionary(
                k => k.Key,
                v => razorEngine.Compile<IncludeTemplateBase>(v.Value, GetOptionsBuilder));
        }

        public RenderedTemplate Render(RazorEngineCompiledTemplate<IncludeTemplateBase> compiledTemplate,
            IDictionary<string, RazorEngineCompiledTemplate<IncludeTemplateBase>> compiledIncludes, object model)
        {
            List<string> usedIncludes = new List<string>();
            string templateLayout = string.Empty;
            Dictionary<string, dynamic> templateViewData = new Dictionary<string, dynamic>();
            string templateOutput = compiledTemplate.Run(instance =>
            {
                if (!(model is AnonymousTypeWrapper))
                {
                    model = new AnonymousTypeWrapper(model);
                }

                instance.Model = model;
                instance.IncludeCallback = (key, includeModel) =>
                {
                    usedIncludes.Add(key);
                    RenderedTemplate renderedTemplate = Render(compiledIncludes[key], compiledIncludes, includeModel);
                    renderedTemplate.ViewData.ToList().ForEach(item => { templateViewData[item.Key] = item.Value; });
                    return renderedTemplate.HtmlContents;
                };
                instance.LayoutCallback = key =>
                {
                    templateLayout = key;
                };
                instance.ViewData.Callback = newViewData =>
                {
                    templateViewData = newViewData;
                };
            });

            return new RenderedTemplate
            {
                HtmlContents = templateOutput,
                ComponentDependencies = usedIncludes,
                Layout = templateLayout,
                ViewData = templateViewData
            };
        }

        public string RenderLayout(RazorEngineCompiledTemplate<LayoutTemplateBase> compiledLayout, object model)
        {
            return compiledLayout.Run(instance =>
            {
                if (!(model is AnonymousTypeWrapper))
                {
                    model = new AnonymousTypeWrapper(model);
                }

                instance.Model = model;
            });
        }

        public void GetOptionsBuilder(RazorEngineCompilationOptionsBuilder builder)
        {
            Assemblies.ForEach(builder.AddAssemblyReferenceByName);
        }
    }
}