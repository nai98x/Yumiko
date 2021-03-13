using Discord_Bot;
using DSharpPlus.CommandsNext;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YumikoBot.DAL
{
    public class UsuarioDiscordo
    {
        public int Id { get; set; }
        public long user_id { get; set; }
        public long guild_id { get; set; }
        public DateTime Birthday { get; set; }
        public bool MostrarYear { get; set; }
    }

    public class UsuariosDiscordo
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        public async Task<List<UsuarioDiscordo>> GetListaUsuarios()
        {
            var client = await funciones.getClienteFirebase();
            FirebaseResponse response = await client.GetTaskAsync("UsuariosDiscord/");
            var listaFirebase = response.ResultAs<List<UsuarioDiscordo>>();
            return listaFirebase.Where(x => x != null).ToList();
        }

        public async Task<int> GetLastId()
        {
            var lista = await GetListaUsuarios();
            return lista.Last().Id;
        }

        public async Task<List<UserCumple>> GetBirthdays(CommandContext ctx, bool month)
        {
            List<UserCumple> lista = new List<UserCumple>();
            var listaFirebase = await GetListaUsuarios();
            var list = listaFirebase.Where(x => x.guild_id == (long)ctx.Guild.Id).ToList();
            var listaVerif = ctx.Guild.Members.Values.ToList();
            list.ToList().ForEach(x =>
            {
                if (listaVerif.Find(u => u.Id == (ulong)x.user_id) != null)
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
                                Id = x.user_id,
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
                            Id = x.user_id,
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
        
        public async Task<List<UserCumple>> GetBirthdaysGuild(long guildId, bool month)
        {
            List<UserCumple> lista = new List<UserCumple>();
            var listaFirebase = await GetListaUsuarios();
            var list = listaFirebase.Where(x => x != null && x.guild_id == guildId).ToList();
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
                            Id = x.user_id,
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
                        Id = x.user_id,
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
        
        public async Task SetBirthday(CommandContext ctx, DateTime fecha, bool mostrarEdad)
        {
            var client = await funciones.getClienteFirebase();
            var listaFirebase = await GetListaUsuarios();
            var usuario = listaFirebase.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.user_id == (long)ctx.Member.Id);
            if (usuario == null)
            {
                int nuevoId = await GetLastId() + 1;
                await client.SetTaskAsync("UsuariosDiscord/" + nuevoId, new UsuarioDiscordo { 
                    Id = nuevoId,
                    user_id = (long)ctx.Member.Id,
                    guild_id = (long)ctx.Guild.Id,
                    Birthday = fecha,
                    MostrarYear = mostrarEdad
                });
            }
            else
            {
                await client.UpdateTaskAsync("UsuariosDiscord/" + usuario.Id, new UsuarioDiscordo
                {
                    Id = usuario.Id,
                    user_id = (long)ctx.Member.Id,
                    guild_id = (long)ctx.Guild.Id,
                    Birthday = fecha,
                    MostrarYear = mostrarEdad
                });
            }
        }
        
        public async Task DeleteBirthday(CommandContext ctx)
        {
            var client = await funciones.getClienteFirebase();
            var listaFirebase = await GetListaUsuarios();
            var usuario = listaFirebase.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.user_id == (long)ctx.Member.Id);
            if(usuario != null)
            {
                await client.DeleteTaskAsync("UsuariosDiscord/" + usuario.Id);
            }
        }
    }
}
