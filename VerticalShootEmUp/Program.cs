using FlagConsole.Drawing;
using Microsoft.GotDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VerticalShootEmUp
{
    class Program
    {
        const int CasUderuSrdce = 30; // v milisekundach
        const int PocetUderuSrdceNaPosunuti = 10;

        static int PocetUderuSrdce = 0;
        static int PocetUderuSrdceCelkem = 0;
        static int Skore = 0;

        static int PoziceHraceX = 0;
        static Pole[,] HraciPlocha;
        static bool[,] HraciPlochaStrel;

        static GraphicBuffer Buffer;

        static Random GeneratorNahodnychCisel = new Random();
        static int PravdepodobnostAsteroidu = 20;
        static int PravdepodobnostBodu = 5;
        static int ZakladniPravdepodobnost = 1000;

        static SoundPlayer ZvukExploze;
        static SoundPlayer ZvukBod;

        static void Main(string[] args)
        {
            InicializovatHru();

            while (true)
            {
                Prikaz prikaz = ZjistitPrikaz();
                ProvestPrikaz(prikaz);

                PocetUderuSrdce++;
                PocetUderuSrdceCelkem++;
                if (PocetUderuSrdce > PocetUderuSrdceNaPosunuti)
                {
                    Posunout();
                    PocetUderuSrdce = 0;
                }

                DetekovatKolizi();
                Vykreslit();

                if (prikaz == Prikaz.Zadny)
                    Thread.Sleep(CasUderuSrdce);
            }
        }

        static void InicializovatHru()
        {
            Console.WindowWidth = 50;
            Console.WindowHeight = 20;

            PoziceHraceX = Console.WindowWidth / 2;
            HraciPlocha = new Pole[Console.WindowHeight, Console.WindowWidth];
            HraciPlochaStrel = new bool[Console.WindowHeight, Console.WindowWidth];
            Buffer = new GraphicBuffer(new Size(Console.WindowWidth, Console.WindowHeight));
            ConsoleEx.CursorVisible = false;

            ZvukExploze = new SoundPlayer(Zvuky.Exploze);
            ZvukBod = new SoundPlayer(Zvuky.Bod);
        }

        static Prikaz ZjistitPrikaz()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo klavesa = new ConsoleKeyInfo();
                while (Console.KeyAvailable)
                    klavesa = Console.ReadKey(true);
                if (klavesa.Key == ConsoleKey.Escape)
                    return Prikaz.Konec;
                else if (klavesa.Key == ConsoleKey.Spacebar)
                    return Prikaz.Vystrel;
                else if (klavesa.Key == ConsoleKey.LeftArrow)
                    return Prikaz.Doleva;
                else if (klavesa.Key == ConsoleKey.RightArrow)
                    return Prikaz.Doprava;
            }

            return Prikaz.Zadny;
        }

        static void Konec()
        {
            ConsoleEx.DrawRectangle(BorderStyle.LineDouble, 5, 5, Console.WindowWidth - 10, 6, true);
            ConsoleEx.DrawRectangle(BorderStyle.LineDouble, 5, 5, Console.WindowWidth - 10, 6, false);
            VypsatDoprostredRadku(7, string.Format("Tvoje skore je {0}.", Skore));
            VypsatDoprostredRadku(9, "Stiskni klavesu pro konec...");

            while (Console.KeyAvailable)
                Console.ReadKey(true);
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        static void ProvestPrikaz(Prikaz prikaz)
        {
            if (prikaz == Prikaz.Konec)
            {
                Konec();
            }
            else if (prikaz == Prikaz.Doleva)
            {
                if (PoziceHraceX > 1)
                    PoziceHraceX--;
            }
            else if (prikaz == Prikaz.Doprava)
            {
                if (PoziceHraceX < Console.WindowWidth - 2)
                    PoziceHraceX++;
            }
            else if (prikaz == Prikaz.Vystrel)
            {
                HraciPlochaStrel[Console.WindowHeight - 3, PoziceHraceX] = true;
            }
        }

        static void VypsatDoprostredRadku(int radek, string text)
        {
            var x = (Console.WindowWidth - text.Length) / 2;
            ConsoleEx.WriteAt(x, radek, text);
        }

        static void DetekovatKolizi()
        {
            for (int j = 0; j < Console.WindowWidth; j++)
                HraciPlochaStrel[Console.WindowHeight - 1, j] = false;

            for (int i = 0; i < Console.WindowHeight; i++)
                for (int j = 0; j < Console.WindowWidth; j++)
                    if (HraciPlochaStrel[i, j] && (HraciPlocha[i, j] != Pole.Prazdno))
                    {
                        ZvukExploze.Play();
                        HraciPlocha[i, j] = Pole.Vybuch;
                        HraciPlochaStrel[i, j] = false;
                    }

            DetekovatKoliziVBode(Console.WindowHeight - 2, PoziceHraceX);
            DetekovatKoliziVBode(Console.WindowHeight - 1, PoziceHraceX - 1);
            DetekovatKoliziVBode(Console.WindowHeight - 1, PoziceHraceX);
            DetekovatKoliziVBode(Console.WindowHeight - 1, PoziceHraceX + 1);

            for (int i = 1; i < Console.WindowHeight; i++)
                for (int j = 0; j < Console.WindowWidth; j++)
                    HraciPlochaStrel[i - 1, j] = HraciPlochaStrel[i, j];
        }

        static void DetekovatKoliziVBode(int radek, int sloupec)
        {
            Pole pole = HraciPlocha[radek, sloupec];
            if (pole == Pole.Bod)
            {
                ZvukBod.Play();
                HraciPlocha[radek, sloupec] = Pole.Prazdno;
                Skore++;
            }
            else if (pole == Pole.Asteroid)
            {
                ZvukExploze.Play();
                Konec();
            }
        }

        static void Posunout()
        {
            for (int i = Console.WindowHeight - 2; i >= 0; i--)
                for (int j = 0; j < Console.WindowWidth; j++)
                { 
                    Pole pole = HraciPlocha[i, j];
                    if (pole == Pole.Vybuch)
                        HraciPlocha[i + 1, j] = Pole.Prazdno;
                    else
                        HraciPlocha[i + 1, j] = pole;
                }

            for (int j = 0; j < Console.WindowWidth; j++)
            {
                if (GeneratorNahodnychCisel.Next(ZakladniPravdepodobnost / PravdepodobnostBodu) == 0)
                    HraciPlocha[0, j] = Pole.Bod;
                else if (GeneratorNahodnychCisel.Next(ZakladniPravdepodobnost / PravdepodobnostAsteroidu) == 0)
                    HraciPlocha[0, j] = Pole.Asteroid;
                else
                    HraciPlocha[0, j] = Pole.Prazdno;
            }
        }

        static void Vykreslit()
        {
            for (int i = 0; i < Console.WindowHeight; i++)
            {
                for (int j = 0; j < Console.WindowWidth; j++)
                {
                    if (HraciPlochaStrel[i, j])
                        Buffer.DrawPixel('¡', new Coordinate(j, i));
                    else
                    {
                        Pole pole = HraciPlocha[i, j];
                        if (pole == Pole.Asteroid)
                            Buffer.DrawPixel('@', new Coordinate(j, i));
                        else if (pole == Pole.Bod)
                            Buffer.DrawPixel('$', new Coordinate(j, i));
                        else if (pole == Pole.Vybuch)
                            Buffer.DrawPixel('*', new Coordinate(j, i));
                        else
                            Buffer.DrawPixel(' ', new Coordinate(j, i));
                    }
                }
            }

            Buffer.DrawLine("▄", new Coordinate(PoziceHraceX, Console.WindowHeight - 2));
            Buffer.DrawLine("▐█▌", new Coordinate(PoziceHraceX - 1, Console.WindowHeight - 1));

            Buffer.DrawToScreen(new Coordinate(0, 0));
        }
    }

    enum Prikaz
    {
        Zadny = 0,
        Konec,
        Doleva,
        Doprava,
        Vystrel,
    }

    enum Pole
    {
        Prazdno = 0,
        Asteroid,
        Bod,
        Vybuch
    }
}
