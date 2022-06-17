namespace Yumiko.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Not with D#+ Command classes")]
    public class Interact : ApplicationCommandModule
    {
        public IConfigurationRoot Configuration { private get; set; } = null!;

        public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ctx.Interaction.Locale!);
            return Task.FromResult(true);
        }

        [SlashCommand("Say", "Replicates a text")]
        [SlashRequirePermissions(Permissions.SendMessages | Permissions.SendMessagesInThreads | Permissions.AccessChannels)]
        public async Task Say(InteractionContext ctx, [Option("Message", "The text you want to replicate")] string texto)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.DeleteResponseAsync();
            await ctx.Channel.SendMessageAsync(texto);
        }

        [SlashCommand("question", "Responds with yes or no")]
        public async Task Question(InteractionContext ctx, [Option("Question", "The question you want to ask")] string texto)
        {
            Random rnd = new();
            int random = rnd.Next(2);
            DiscordEmbed embed = random switch
            {
                0 => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = strings.yes_or_no,
                    Description = $"{Formatter.Bold($"{strings.question}:")} {texto}\n{Formatter.Bold($"{strings.answer}:")} {strings.no.ToUpper()}",
                },
                _ => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Green,
                    Title = strings.yes_or_no,
                    Description = $"{Formatter.Bold($"{strings.question}:")} {texto}\n{Formatter.Bold($"{strings.answer}:")} {strings.yes.ToUpper()}",
                },
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("choose", "Choose from multiple options separated by commas")]
        public async Task Choose(InteractionContext ctx, [Option("Question", "The question you want to ask")] string pregunta, [Option("Options", "Comma Separated Options")] string opc)
        {
            List<string> opciones = opc.Split(',').ToList();
            int random = Common.GetNumeroRandom(0, opciones.Count - 1);
            string options = Formatter.Bold($"{strings.options}:");
            foreach (string msj in opciones)
            {
                options += "\n   - " + msj;
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = Constants.YumikoColor,
                Title = strings.question,
                Description = $"{Formatter.Bold(pregunta)}\n\n{options}\n\n{Formatter.Bold($"{strings.answer} :")} {opciones[random]}",
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("emote", "Gives information about an emote")]
        public async Task Emote(InteractionContext ctx, [Option("Emote", "The emote")] string emoji)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmoji? emote = Common.ToEmoji(emoji);

            if (emote != null)
            {
                if (emote.Id != 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = Constants.YumikoColor,
                        Title = "Emote",
                        ImageUrl = emote.Url,
                    }.AddField("Id", $"{emote.Id}", true)
                    .AddField(strings.name, emote.Name, true)
                    .AddField(strings.animated, emote.IsAnimated ? strings.yes : strings.no, true)
                    .AddField(strings.creation_date, $"{Formatter.Timestamp(emote.CreationTimestamp.UtcDateTime, TimestampFormat.LongDate)}");

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(emoji));
                }
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = Constants.YumikoColor,
                    Title = "Emote",
                    Description = string.Format(strings.emote_not_found, emoji),
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
        }

        [SlashCommand("avatar", "Shows an user's avatar")]
        [SlashRequireGuild]
        public async Task Avatar(
            InteractionContext ctx,
            [Option("User", "The user's avatar to retrieve")] DiscordUser? usuario = null,
            [Option("Guild", "Get guild avatar (nitro users only)")] bool serverAvatar = false,
            [Option("Secret", "If you want to see only you the command")] bool secreto = true)
        {
            usuario ??= ctx.User;
            DiscordMember member = await ctx.Guild.GetMemberAsync(usuario.Id, true);

            string displayAvatar = serverAvatar switch
            {
                true => member.GuildAvatarUrl ?? member.AvatarUrl,
                false => member.AvatarUrl,
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = string.Format(strings.member_avatar, member.DisplayName),
                ImageUrl = displayAvatar,
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = secreto,
            }.AddEmbed(embed));
        }

        [SlashCommand("user", "Shows information about an user")]
        public async Task User(
            InteractionContext ctx,
            [Option("Usuario", "The user you want to retrieve information")] DiscordUser usuario,
            [Option("Secret", "If you want to see only you the command")] bool secreto = false)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = secreto,
            }.AddEmbed(new DiscordEmbedBuilder
            {
                Title = strings.processing,
                Description = $"{strings.processing_desc}..",
            }));

            usuario = await ctx.Client.GetUserAsync(usuario.Id, true);
            DiscordMember member = await ctx.Guild.GetMemberAsync(usuario.Id);

            string roles = string.Empty;
            var rolesCol = member.Roles.OrderByDescending(x => x.Position);
            foreach (var rol in rolesCol)
            {
                roles += $"{rol.Mention} ";
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{member.DisplayName} ({usuario.Username}#{usuario.Discriminator})",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = usuario.AvatarUrl,
                },
                Color = member.Color,
            };
            embed.AddField(strings.registered, $"{Formatter.Timestamp(usuario.CreationTimestamp, TimestampFormat.LongDate)} ({Formatter.Timestamp(usuario.CreationTimestamp, TimestampFormat.RelativeTime)})", true);
            embed.AddField(strings.joined_date, $"{Formatter.Timestamp(member.JoinedAt, TimestampFormat.LongDate)} ({Formatter.Timestamp(member.JoinedAt, TimestampFormat.RelativeTime)})", true);
            embed.AddField("Bot", usuario.IsBot ? strings.yes : strings.no, true);
            if (usuario.Flags != null)
            {
                embed.AddField(strings.badges, usuario.Flags.ToString());
            }

            if (!string.IsNullOrEmpty(roles))
            {
                embed.AddField(strings.roles, roles);
            }

            if (usuario.BannerUrl != null)
            {
                embed.WithImageUrl(usuario.BannerUrl);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("waifu", "My love level to a user")]
        public async Task Waifu(
            InteractionContext ctx,
            [Option("User", "User whose waifu level you want to know")] DiscordUser? usuario = null,
            [Option("Real", "Shows the real percentage, it doesn't change")] bool real = false)
        {
            usuario ??= ctx.User;
            DiscordMember miembro = await ctx.Guild.GetMemberAsync(usuario.Id);
            string nombre;
            string titulo = "Waifu";
            nombre = miembro.DisplayName;
            int waifuLevel;
            if (real)
            {
                Random rnd = new((int)miembro.Id);
                waifuLevel = rnd.Next(0, 100);
                titulo += " (REAL)";
            }
            else
            {
                waifuLevel = Common.GetNumeroRandom(0, 100);
            }

            var builder = waifuLevel switch
            {
                < 25 => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = titulo,
                    Description = $"{string.Format(strings.my_love_to_user_is, nombre, waifuLevel)}\n" +
                                    $"{strings.waifu_level_25}",
                    ImageUrl = "https://i.imgur.com/BOxbruw.png",
                },
                < 50 => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Orange,
                    Title = titulo,
                    Description = $"{string.Format(strings.my_love_to_user_is, nombre, waifuLevel)}\n" +
                                    $"{strings.waifu_level_50}",
                    ImageUrl = "https://i.imgur.com/ys2HoiL.jpg",
                },
                < 75 => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Yellow,
                    Title = titulo,
                    Description = $"{string.Format(strings.my_love_to_user_is, nombre, waifuLevel)}\n" +
                                    $"{strings.waifu_level_75}",
                    ImageUrl = "https://i.imgur.com/h7Ic2rk.jpg",
                },
                < 100 => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Green,
                    Title = titulo,
                    Description = $"{string.Format(strings.my_love_to_user_is, nombre, waifuLevel)}\n" +
                                    $"{strings.waifu_level_99}",
                    ImageUrl = "https://i.imgur.com/dhXR8mV.png",
                },
                _ => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Blue,
                    Title = titulo,
                    Description = $"{string.Format(strings.my_love_to_user_is, nombre, waifuLevel)}\n" +
                                    $"{strings.waifu_level_100}",
                    ImageUrl = "https://i.imgur.com/Vk6JMJi.jpg",
                },
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(builder));
        }

        [SlashCommand("love", "Love percentage between two users")]
        public async Task Love(
            InteractionContext ctx,
            [Option("User1", "First user")] DiscordUser? user1 = null,
            [Option("User2", "Second user")] DiscordUser? user2 = null,
            [Option("Real", "Shows the real percentage, it doesn't change")] bool real = false)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            DiscordWebhookBuilder builder = new();
            int porcentajeAmor;
            string titulo;
            string imagenUrl;
            if (user1 == null)
            {
                user1 = ctx.Member;
            }

            if ((user2 == null && user1.Id == ctx.Member.Id) || (user2 != null && user1.Id == user2.Id))
            {
                titulo = $"{Formatter.Bold($"{user1.FullName()}")}'s self-love";
                imagenUrl = user1.GetAvatarUrl(ImageFormat.Png, 128);
            }
            else
            {
                if (user2 == null)
                {
                    user2 = user1;
                    user1 = ctx.Member;
                }

                titulo = string.Format(strings.love_between, user1.FullName(), user2.FullName());

                string avatar1 = user1.GetAvatarUrl(ImageFormat.Png, 512);
                string avatar2 = user2.GetAvatarUrl(ImageFormat.Png, 512);

                var imagen = await Common.MergeImage(avatar1, avatar2, 1024, 512);
                imagenUrl = Formatter.AttachedImageUrl("imageLove.png");
                builder.AddFile("imageLove.png", imagen);
            }

            if (real)
            {
                Random rnd;
                if (user2 == null)
                {
                    rnd = new((int)(user1.Id + user1.Id)); // Diferente al de /waifu
                }
                else
                {
                    rnd = new((int)(user1.Id + user2.Id));
                }

                porcentajeAmor = rnd.Next(0, 100);
                titulo += " (REAL)";
            }
            else
            {
                porcentajeAmor = Common.GetNumeroRandom(0, 100);
            }

            string descripcion = $"{Formatter.Bold($"{porcentajeAmor}%")} [";

            for (int i = 0; i < porcentajeAmor / 5; i++)
            {
                descripcion += "█";
            }

            for (int i = 0; i < 20 - (porcentajeAmor / 5); i++)
            {
                descripcion += " . ";
            }

            descripcion += "]\n\n";
            if ((user2 == null && user1.Id != ctx.Member.Id) || (user2 != null && user1.Id != user2.Id))
            {
                switch (porcentajeAmor)
                {
                    case 0:
                        descripcion += strings.love_0;
                        break;
                    case <= 10:
                        descripcion += strings.love_10;
                        break;
                    case <= 25:
                        descripcion += strings.love_25;
                        break;
                    case <= 50:
                        descripcion += strings.love_50;
                        break;
                    case <= 75:
                        descripcion += strings.love_75;
                        break;
                    case <= 90:
                        descripcion += strings.love_90;
                        break;
                    case < 100:
                        descripcion += strings.love_99;
                        break;
                    case 100:
                        descripcion += strings.love_100;
                        break;
                }
            }

            builder.AddEmbed(new DiscordEmbedBuilder
            {
                Title = titulo,
                Description = descripcion,
                ImageUrl = imagenUrl,
                Color = Constants.YumikoColor,
            });

            await ctx.EditResponseAsync(builder);
        }
    }
}
