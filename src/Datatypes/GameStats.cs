namespace Yumiko.Datatypes
{
    public class GameStats
    {
        public long UserId { get; set; }

        public int PorcentajeAciertos { get; set; }

        public int PartidasTotales { get; set; }

        public int RondasAcertadas { get; set; }

        public int RondasTotales { get; set; }

        public string? Dificultad { get; set; }
    }
}
