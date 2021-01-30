using Discord_Bot;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YumikoBot.Data_Access_Layer
{
    public class UsuariosDiscord
    {
        public List<UserCumple> GetBirthdays(CommandContext ctx, bool month)
        {
            List<UserCumple> lista = new List<UserCumple>();
            using (var context = new YumikoEntities())
            {
                var list = context.UsuariosDiscord.ToList().Where(x => x.guild_id == (long)ctx.Guild.Id);
                var listaVerif = ctx.Guild.Members.Values.ToList();
                list.ToList().ForEach(x =>
                {
                    if (listaVerif.Find(u => u.Id == (ulong)x.Id) != null)
                    {
                        DateTime fchAux = new DateTime(day: x.Birthday.Day, month: x.Birthday.Month, year:DateTime.Now.Year);
                        DateTime nuevoCumple;
                        if (DateTime.Now > new DateTime(day: x.Birthday.Day, month: x.Birthday.Month, year: DateTime.Now.Year))
                            nuevoCumple = new DateTime(day: x.Birthday.Day, month: x.Birthday.Month, year: DateTime.Now.Year + 1);
                        else
                            nuevoCumple = new DateTime(day: x.Birthday.Day, month: x.Birthday.Month, year: DateTime.Now.Year);
                        if (month)
                        {
                            if (fchAux >= DateTime.Now && fchAux <= DateTime.Now.AddMonths(1))
                            {
                                lista.Add(new UserCumple {
                                    Id = x.Id,
                                    Guild_id = x.guild_id,
                                    Birthday = x.Birthday,
                                    BirthdayActual = nuevoCumple,
                                    MostrarYear = x.MostrarYear
                                });
                            }
                        }
                        else
                        {
                            lista.Add(new UserCumple
                            {
                                Id = x.Id,
                                Guild_id = x.guild_id,
                                Birthday = x.Birthday,
                                BirthdayActual = nuevoCumple,
                                MostrarYear = x.MostrarYear
                            });
                        }
                    }
                });
                lista.Sort((x, y) => x.BirthdayActual.CompareTo(y.BirthdayActual));
                return lista;
            }
        }

        public List<UserCumple> GetBirthdaysGuild(long guildId, bool month)
        {
            List<UserCumple> lista = new List<UserCumple>();
            using (var context = new YumikoEntities())
            {
                var list = context.UsuariosDiscord.ToList().Where(x => x.guild_id == guildId);
                list.ToList().ForEach(x =>
                {
                    DateTime fchAux = new DateTime(day: x.Birthday.Day, month: x.Birthday.Month, year: DateTime.Now.Year);
                    DateTime nuevoCumple;
                    if (DateTime.Now > new DateTime(day: x.Birthday.Day, month: x.Birthday.Month, year: DateTime.Now.Year))
                        nuevoCumple = new DateTime(day: x.Birthday.Day, month: x.Birthday.Month, year: DateTime.Now.Year + 1);
                    else
                        nuevoCumple = new DateTime(day: x.Birthday.Day, month: x.Birthday.Month, year: DateTime.Now.Year);
                    if (month)
                    {
                        if (fchAux >= DateTime.Now && fchAux <= DateTime.Now.AddMonths(1))
                        {
                            lista.Add(new UserCumple
                            {
                                Id = x.Id,
                                Guild_id = x.guild_id,
                                Birthday = x.Birthday,
                                BirthdayActual = nuevoCumple,
                                MostrarYear = x.MostrarYear
                            });
                        }
                    }
                    else
                    {
                        lista.Add(new UserCumple
                        {
                            Id = x.Id,
                            Guild_id = x.guild_id,
                            Birthday = x.Birthday,
                            BirthdayActual = nuevoCumple,
                            MostrarYear = x.MostrarYear
                        });
                    }
                });
                lista.Sort((x, y) => x.BirthdayActual.CompareTo(y.BirthdayActual));
                return lista;
            }
        }

        public void SetBirthday(CommandContext ctx, DateTime fecha, bool mostrarEdad)
        {
            using (var context = new YumikoEntities())
            {
                var usuario = context.UsuariosDiscord.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.Id == (long)ctx.Member.Id);
                if(usuario == null)
                {
                    usuario = new UsuarioDiscord()
                    {
                        Id = (long)ctx.Member.Id,
                        guild_id = (long)ctx.Guild.Id,
                        Birthday = fecha,
                        MostrarYear = mostrarEdad
                    };
                    context.UsuariosDiscord.Add(usuario);
                }
                else
                {
                    usuario.Birthday = fecha;
                    usuario.MostrarYear = mostrarEdad;
                }
                context.SaveChanges();
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

        public void SetAnilist(CommandContext ctx,string anilist)
        {
            using (var context = new YumikoEntities())
            {
                var usuario = context.UsuariosDiscord.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.Id == (long)ctx.Member.Id);
                if (usuario == null)
                {
                    usuario = new UsuarioDiscord()
                    {
                        Id = (long)ctx.Member.Id,
                        guild_id = (long)ctx.Guild.Id,
                        Anilist = anilist
                    };
                    context.UsuariosDiscord.Add(usuario);
                }
                else
                {
                    usuario.Anilist = anilist;
                }
                context.SaveChanges();
            }
        }

        public UsuarioDiscord GetUsuario(CommandContext ctx)
        {
            using (var context = new YumikoEntities())
            {
                return context.UsuariosDiscord.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.Id == (long)ctx.Member.Id);
            }
        }
    }
}
