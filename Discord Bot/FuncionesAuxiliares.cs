using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace Discord_Bot
{
    public class FuncionesAuxiliares
    {
        public string GetImagenRandomMeme()
        {
            string[] options = new[] {
                "https://static.wixstatic.com/media/1e9fdf_b9ea2c7f507c45b3ba3771012fc846c5.png", // Roster meme version
                "https://static.wixstatic.com/media/1e9fdf_f6590db0097546adabaeb430d1b576fb.png", // Ser joven no es delito
                "https://static.wixstatic.com/media/1e9fdf_5b71c799b7a8462db416ee320b10004e.jpg", // Roster 2
                "https://static.wixstatic.com/media/1e9fdf_ced8bdb6ddfe4781b7ed44ff11b87531.png", // Cuando no juega harilo
                "https://static.wixstatic.com/media/1e9fdf_0c57c48e84594767b40e0192fdbddcf7.jpg", // Elias putaso bidon
                "https://static.wixstatic.com/media/1e9fdf_c3c917ac524b48c1912894dc816d8e25.png", // elias 11set
                "https://static.wixstatic.com/media/1e9fdf_9562e8f85e7b4687a2765f7600c9cdc9.png", // Dario hairo ejecutar
                "https://static.wixstatic.com/media/1e9fdf_758acfbca72e46ecaa0e4b3afc0ef9ac.png", // Mela fulco de neici
                "https://static.wixstatic.com/media/1e9fdf_3df69a0441c84a5ab6fe9d235b1ab922.png", // Sadi con cancer habitacion
                "https://static.wixstatic.com/media/1e9fdf_aa2f2236f9b24b249d90741734b073f0.png", // Sadi eli yaoi
                "https://static.wixstatic.com/media/1e9fdf_a21af803a6fe409caa5d0d9e968a4e77.jpg", // Faker nai plata 5
                "https://static.wixstatic.com/media/1e9fdf_d2da2775d1f24b8ead0bf2ca2268054e.png", // Hairo main elise
                "https://static.wixstatic.com/media/1e9fdf_64ce1af185ac4556b37b80d0b1908d75~mv2.png", // Rapidoyfurioso fepper
                "https://static.wixstatic.com/media/1e9fdf_99f7e06baa8a403e83b9ceece77734e2~mv2.jpg", // Patada hairo
                "https://static.wixstatic.com/media/1e9fdf_8ded154c00554dd4991c96904d8dfe35~mv2.png", // Suho y eli flores boku no pico
                "https://i.imgur.com/8UWJDBL.png", // Nai waifu dank
                "https://i.imgur.com/7C69lyR.png", // Mela fulco ahegao
                "https://i.imgur.com/sw0vgRb.png", // Eli suicidio dario atras
                "https://i.imgur.com/MM1m1eQ.png", // Dario fortnite
                "https://i.imgur.com/Gk7tzv7.png", // Eli auto con suho
                "https://i.imgur.com/V9YSISj.png", // Hairo sutien waifu
                "https://i.imgur.com/FNUkQ6M.png", // Eli dakimakura 2b
                "https://i.imgur.com/ypuwqV8.jpg", // Eli holocausto
                "https://i.imgur.com/UUVMxJE.jpg", // Eli sakura fanservice
                "https://i.imgur.com/RFHhH4h.png", // Eli como te ve ella
            };

            Random rnd = new Random();
            return options[rnd.Next(options.Length)];
        }

        public string GetEliRandom()
        {
            string[] options = new[] {"DORADO", "geniero", "dividuo", "dignado", "deciso", "fradotado", "coherente", "penetrable", "cestuoso", "accesible", "cognito",
            "adaptado", "centivado", "nombrable", "sensato", "moral", "falible", "enarrable", "putado", "tratable", "deseado", "contagiable", "clusivo", "fumable",
            "postor", "estable", "teligente", "festado", "parable", "oportuno", "cauto", "comodo", "cendios", "conforme", "advertido", "conveniente", "incivilizado",
            "capacitado", "centivado", "comodo", "cagable", "sufrible", "comparable", "quisitor", "cogible"};

            Random rnd = new Random();
            return options[rnd.Next(options.Length)];
        }

        public Boolean TienePermisos(Permissions permiso, IEnumerable<DiscordRole> roles)
        {
            foreach (DiscordRole role in roles)
            {
                if (role.CheckPermission(Permissions.ManageMessages) == PermissionLevel.Allowed)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetCancionByPosicion(int posicion)
        {
            string[] filePaths = Directory.GetFiles(@"C:\Users\Mariano\Music\Yumiko\");
            string elegido = filePaths.ElementAt(posicion - 1);
            string preString = elegido.Remove(0, 30); // Cantidad de caracteres del path original
            return preString.Remove(preString.Length - 4);
        }
    }

    public class ColaReproduccion
    {
        public int posicion { get; set; }
        public string archivo { get; set; }
    }
}
