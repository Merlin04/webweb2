using System.Collections.Generic;
using System.Linq;
using System.Xml;
using BlazorEditor.Data;
using RazorEngineCore;

namespace BlazorEditor.Compiler
{
    public static class Compiler
    {
        public static void CompileAndStore(ApplicationDbContext context)
        {
            // Get assemblies
            List<string> assemblies = context.Configuration.Find("assemblies").Contents.Split("\n").ToList();
            
            IncludeRazorEngine engine = new IncludeRazorEngine(assemblies);

            Dictionary<string, RazorEngineCompiledTemplate<IncludeTemplateBase>> includes =
                engine.GetCompiledIncludes(context.Components.ToDictionary(component => component.Title,
                    component => component.HtmlContents));

            Dictionary<string, string> renderedPages =
                context.Pages.ToDictionary(page => page.Title, page => RenderPage(context, includes, assemblies, page));

            // Delete all previously compiled pages
            context.CompiledPages.RemoveRange(context.CompiledPages);
            
            foreach (KeyValuePair<string, string> renderedPage in renderedPages)
            {
                context.CompiledPages.Add(new CompiledPage
                {
                    Title = renderedPage.Key,
                    Contents = renderedPage.Value
                });
            }

            context.SaveChanges();
        }

        private static string RenderPage(ApplicationDbContext context,
            Dictionary<string, RazorEngineCompiledTemplate<IncludeTemplateBase>> compiledIncludes, List<string> assemblies, Page page)
        {
            IncludeRazorEngine engine = new IncludeRazorEngine(assemblies);
            RazorEngine razorEngine = new RazorEngine();

            RazorEngineCompiledTemplate<IncludeTemplateBase> compiledTemplate =
                razorEngine.Compile<IncludeTemplateBase>(page.HtmlContents, engine.GetOptionsBuilder);

            RenderedTemplate renderedTemplate = engine.Render(compiledTemplate, compiledIncludes, new { });

            // Remove any inline script elements and store them separately
            List<string> inlineScripts = new List<string>();
            XmlDocument doc = new XmlDocument();
            // Wrap the page in a PageContainer so that including text outside of any element doesn't throw an error
            doc.AppendChild(doc.CreateElement("PageContainer"));
            doc.GetElementsByTagName("PageContainer")[0].InnerXml = renderedTemplate.HtmlContents;
            while(doc.GetElementsByTagName("script").Count > 0) {
                var node = doc.GetElementsByTagName("script")[0];
                inlineScripts.Add(node.InnerText);
                node.ParentNode.RemoveChild(node);
            }

            renderedTemplate.HtmlContents = doc.GetElementsByTagName("PageContainer")[0].InnerXml;
            renderedTemplate.InnerJsContents =
                inlineScripts.Aggregate(string.Empty, (current, script) => current + "\n" + script);
            
            return renderedTemplate.Layout is null ? renderedTemplate.HtmlContents : AddLayoutToPage(context, renderedTemplate, assemblies, page);
        }

        private static string AddLayoutToPage(ApplicationDbContext context, RenderedTemplate renderedTemplate, List<string> assemblies, Page page)
        {
            IncludeRazorEngine engine = new IncludeRazorEngine(assemblies);
            RazorEngine razorEngine = new RazorEngine();

            RazorEngineCompiledTemplate<LayoutTemplateBase> compiledLayout =
                razorEngine.Compile<LayoutTemplateBase>(context.Layouts.Find(renderedTemplate.Layout).Contents,
                    engine.GetOptionsBuilder);

            string renderCss = page.CssContents;
            string renderJs = page.JsContents;
            foreach (var component in renderedTemplate.ComponentDependencies.Distinct().Select(componentDependency =>
                context.Components.Find(componentDependency)))
            {
                renderCss += "\n" + component.CssContents;
                renderJs += "\n" + component.JsContents;
            }
            
            return engine.RenderLayout(compiledLayout, new
            {
                RenderBody = renderedTemplate.HtmlContents,
                RenderCss = renderCss,
                // InnerJsContents already has a \n at the beginning of it
                RenderJs = renderJs + renderedTemplate.InnerJsContents,
                ViewData = renderedTemplate.ViewData
            });
        }
    }
}