using Discord_Bot;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YumikoBot.Data_Access_Layer
{
    public class Imagenes
    {
        public void AddImagen(string url, string comando)
        {
            using (var context = new YumikoEntities())
            {
                Imagen verif = context.Imagenes.FirstOrDefault(x => x.Url == url && x.Comando == comando);
                if(verif == null)
                {
                    var imagen = new Imagen()
                    {
                        Url = url,
                        Comando = comando
                    };
                    context.Imagenes.Add(imagen);
                    context.SaveChanges();
                }
            }
        }

        public List<Imagen> GetImagenes(string comando)
        {
            using (var context = new YumikoEntities())
            {
                return context.Imagenes.ToList().Where(x => x.Comando == comando).ToList();
            }
        }
    }
}
