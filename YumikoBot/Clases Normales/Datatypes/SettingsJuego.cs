using System;
using System.Collections.Generic;
using System.Text;

namespace Discord_Bot
{
    public class SettingsJuego
    {
        public bool Ok { get; set; }
        public string MsgError { get; set; }
        public int Rondas { get; set; }
        public int  IterIni { get; set; }
        public int IterFin { get; set; }
        public string Dificultad { get; set; }
        public string Tag { get; set; }
        public string TagDesc { get; set; }
        public int PorcentajeTag { get; set; }
        public string Genero { get; set; }
    }
}
