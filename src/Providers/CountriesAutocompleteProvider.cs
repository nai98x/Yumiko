namespace Yumiko.Providers
{
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class CountriesAutocompleteProvider : IAutocompleteProvider
    {
        private List<Country> paises = new();

        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            List<DiscordAutoCompleteChoice> lista = new();

            if (paises.Count == 0)
            {
                paises = await GetPaisesAsync();
            }

            string valor = (string)ctx.FocusedOption.Value;

            var paisesFiltrado = paises
                                    .Where(p => p.name_en.ToLower().Contains(valor.ToLower()) || p.name_es.ToLower().Contains(valor.ToLower()))
                                    .Take(10);

            foreach (var item in paisesFiltrado)
            {
                lista.Add(new(item.name_en, item.code));
            }

            return lista;
        }

        private static async Task<List<Country>> GetPaisesAsync()
        {
            List<Country> ret = new();

            using (StreamReader r = new(Path.Combine("res", "countries.json")))
            {
                string json = await r.ReadToEndAsync();
                var items = JsonConvert.DeserializeObject<dynamic>(json);
                if (items != null)
                {
                    foreach (var item in items.countries)
                    {
                        string name_es = item.name_es;
                        string name_en = item.name_en;
                        string dial_code = item.dial_code;
                        string code = item.code;

                        ret.Add(new Country
                        {
                            name_es = name_es,
                            name_en = name_en,
                            code = code,
                            dial_code = dial_code,
                        });
                    }
                }
            }

            return ret;
        }
    }
}
