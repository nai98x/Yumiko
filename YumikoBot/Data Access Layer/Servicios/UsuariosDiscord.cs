using Discord_Bot;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YumikoBot.Data_Access_Layer
{
    public class UsuariosDiscord
    {
        public List<UsuarioDiscord> GetBirthdays(CommandContext ctx, bool month)
        {
            List<UsuarioDiscord> lista = new List<UsuarioDiscord>();
            using (var context = new YumikoEntities())
            {
                var list = context.UsuariosDiscord.ToList().Where(x => x.guild_id == (long)ctx.Guild.Id);
                var listaVerif = ctx.Guild.Members.Values.ToList();
                list.ToList().ForEach(x =>
                {
                    if (listaVerif.Find(u => u.Id == (ulong)x.Id) != null)
                    {
                        DateTime fchAux = new DateTime(day: x.Birthday.Day, month: x.Birthday.Month, year:DateTime.Now.Year);
                        if (month)
                        {
                            if (fchAux >= DateTime.Now && fchAux <= DateTime.Now.AddMonths(1))
                            {
                                lista.Add(x);
                            }
                        }
                        else
                        {
                            lista.Add(x);
                        }
                    }
                });
                lista.Sort((x, y) => y.Birthday.CompareTo(x.Birthday));
                return lista;
            }
        }

        public void CreateBirthday(CommandContext ctx, DateTime fecha, bool mostrarEdad)
        {
            using (var context = new YumikoEntities())
            {
                var usuarioVerif = context.UsuariosDiscord.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.Id == (long)ctx.Member.Id);
                if(usuarioVerif == null)
                {
                    var usuario = new UsuarioDiscord()
                    {
                        Id = (long)ctx.Member.Id,
                        guild_id = (long)ctx.Guild.Id,
                        Birthday = fecha,
                        MostrarYear = mostrarEdad
                    };
                    context.UsuariosDiscord.Add(usuario);
                    context.SaveChanges();
                }
            }
        }

        public void ModifyBirthday(CommandContext ctx, DateTime fecha, bool mostrarEdad)
        {
            using (var context = new YumikoEntities())
            {
                var usuario = context.UsuariosDiscord.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.Id == (long)ctx.Member.Id);
                if (usuario != null)
                {
                    usuario.Birthday = fecha;
                    usuario.MostrarYear = mostrarEdad;
                    context.SaveChanges();
                }
            }
        }

        public void DeleteBirthday(CommandContext ctx)
        {
            using (var context = new YumikoEntities())
            {
                var usuario = context.UsuariosDiscord.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.Id == (long)ctx.Member.Id);
                if (usuario != null)
                {
                    context.UsuariosDiscord.Remove(usuario);
                    context.SaveChanges();
                }
            }
        }

        public UsuarioDiscord GetBirthday(CommandContext ctx)
        {
            using (var context = new YumikoEntities())
            {
                return context.UsuariosDiscord.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.Id == (long)ctx.Member.Id);
            }
        }
    }
}
