using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord_Bot
{
    class FuncionesAuxiliares
    {
        public string GetImagenRandomMeme()
        {
            string[] options = new[] {
                "https://static.wixstatic.com/media/1e9fdf_b9ea2c7f507c45b3ba3771012fc846c5.png/v1/fill/w_580,h_543,al_c,lg_1,q_85/1e9fdf_b9ea2c7f507c45b3ba3771012fc846c5.webp",
                "https://static.wixstatic.com/media/1e9fdf_f6590db0097546adabaeb430d1b576fb.png/v1/fill/w_580,h_417,al_c,q_85/1e9fdf_f6590db0097546adabaeb430d1b576fb.webp",
                "https://static.wixstatic.com/media/1e9fdf_5b71c799b7a8462db416ee320b10004e.jpg/v1/fill/w_580,h_362,al_c,q_80/1e9fdf_5b71c799b7a8462db416ee320b10004e.webp",
                "https://static.wixstatic.com/media/1e9fdf_ced8bdb6ddfe4781b7ed44ff11b87531.png/v1/fill/w_580,h_488,al_c,q_85/1e9fdf_ced8bdb6ddfe4781b7ed44ff11b87531.webp",
                "https://static.wixstatic.com/media/1e9fdf_0c57c48e84594767b40e0192fdbddcf7.jpg/v1/fill/w_580,h_512,al_c,lg_1,q_80/1e9fdf_0c57c48e84594767b40e0192fdbddcf7.webp",
                "https://static.wixstatic.com/media/1e9fdf_c3c917ac524b48c1912894dc816d8e25.png/v1/fill/w_580,h_327,al_c,lg_1,q_85/1e9fdf_c3c917ac524b48c1912894dc816d8e25.webp",
                "https://static.wixstatic.com/media/1e9fdf_9562e8f85e7b4687a2765f7600c9cdc9.png/v1/fill/w_580,h_383,al_c,q_85/1e9fdf_9562e8f85e7b4687a2765f7600c9cdc9.webp",
                "https://static.wixstatic.com/media/1e9fdf_758acfbca72e46ecaa0e4b3afc0ef9ac.png/v1/fill/w_580,h_773,al_c,q_90/1e9fdf_758acfbca72e46ecaa0e4b3afc0ef9ac.webp",
                "https://static.wixstatic.com/media/1e9fdf_3df69a0441c84a5ab6fe9d235b1ab922.png/v1/fill/w_580,h_303,al_c,lg_1,q_85/1e9fdf_3df69a0441c84a5ab6fe9d235b1ab922.webp",
                "https://static.wixstatic.com/media/1e9fdf_aa2f2236f9b24b249d90741734b073f0.png/v1/fill/w_480,h_597,al_c,lg_1,q_85/1e9fdf_aa2f2236f9b24b249d90741734b073f0.webp",
                "https://static.wixstatic.com/media/1e9fdf_a21af803a6fe409caa5d0d9e968a4e77.jpg/v1/fill/w_580,h_768,al_c,q_85/1e9fdf_a21af803a6fe409caa5d0d9e968a4e77.webp",
                "https://static.wixstatic.com/media/1e9fdf_d2da2775d1f24b8ead0bf2ca2268054e.png/v1/fill/w_600,h_338,al_c,lg_1,q_85/1e9fdf_d2da2775d1f24b8ead0bf2ca2268054e.webp",
                "https://static.wixstatic.com/media/1e9fdf_64ce1af185ac4556b37b80d0b1908d75~mv2.png/v1/fill/w_480,h_720,al_c,q_85/1e9fdf_64ce1af185ac4556b37b80d0b1908d75~mv2.webp",
                "https://static.wixstatic.com/media/1e9fdf_99f7e06baa8a403e83b9ceece77734e2~mv2.jpg/v1/fill/w_600,h_681,al_c,q_85,usm_0.66_1.00_0.01/1e9fdf_99f7e06baa8a403e83b9ceece77734e2~mv2.webp",
                "https://static.wixstatic.com/media/1e9fdf_8ded154c00554dd4991c96904d8dfe35~mv2.png/v1/fill/w_600,h_600,al_c,q_85,usm_0.66_1.00_0.01/1e9fdf_8ded154c00554dd4991c96904d8dfe35~mv2.webp",
            //    "",
            };

            Random rnd = new Random();
            int random = rnd.Next(options.Length);
            return options[random];
        }

        public string GetEliRandom()
        {
            string[] options = new[] {"DORADO", "geniero", "dividuo", "dignado", "deciso", "fradotado", "coherente", "penetrable", "cestuoso", "accesible", "cognito",
            "adaptado", "centivado", "nombrable", "sensato", "moral", "falible", "enarrable", "putado", "tratable", "deseado", "contagiable", "clusivo", "fumable",
            "postor", "estable", "teligente", "festado", "parable", "oportuno", "cauto", "comodo", "cendios", "conforme", "advertido", "conveniente", "incivilizado",
            "capacitado", "centivado", "comodo"};

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
    }
}
