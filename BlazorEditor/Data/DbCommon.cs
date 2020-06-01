using System.Threading.Tasks;

namespace BlazorEditor.Data
{
    public static class DbCommon
    {
        public static bool TitleInUse(ApplicationDbContext context, string title, string type)
        {
            return type switch
            {
                "Component" => context.Components.Find(title) != null,
                "Page" => context.Pages.Find(title) != null,
                "Template" => context.Templates.Find(title) != null,
                "Layout" => context.Layouts.Find(title) != null,
                _ => false
            };
        }

        public static async Task AddItem(ApplicationDbContext context, dynamic item)
        {
            if (item is NonSpecificItem) return;
            switch (item)
            {
                case Component c:
                {
                    await context.Components.AddAsync(c);
                    break;
                }
                case Page p:
                {
                    await context.Pages.AddAsync(p);
                    break;
                }
                case Template t:
                {
                    await context.Templates.AddAsync(t);
                    break;
                }
                case Layout l:
                {
                    await context.Layouts.AddAsync(l);
                    break;
                }
            }
        
            await context.SaveChangesAsync();
        }
    }
}