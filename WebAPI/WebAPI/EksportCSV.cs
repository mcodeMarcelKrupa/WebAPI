using Soneta.Business;
using Soneta.Magazyny;
using Soneta.Towary;
using System.IO;
using System.Text;

namespace WebAPI
{
    public class EksportCSV
    {
        [Context]
        public Session Session { get; set; }

        public void GenerujPlikCsv()
        {
            string folder = @"C:\WebAPI_csv";
            string plik = Path.Combine(folder, "TowaryApi.csv");

            // Zabezpieczenie na wypadek braku folderu
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var tm = TowaryModule.GetInstance(Session);
            var mm = MagazynyModule.GetInstance(Session);

            var mag1 = mm.Magazyny.WgSymbol["1"];
            var mag2 = mm.Magazyny.WgSymbol["2"];
            var mag3 = mm.Magazyny.WgSymbol["3"];
            var mag4 = mm.Magazyny.WgSymbol["4"];

            // Nadpisujemy plik
            using (var sw = new StreamWriter(plik, false, Encoding.UTF8))
            {
                // Nagłówki CSV
                sw.WriteLine("Kod;Nazwa;EAN;CenaDetalicznaBrutto;DostawcaKod;Stan_1;Stan_2;Stan_3;Stan_4");

                var widokTowarow = tm.Towary.CreateView();

                var worker = new StanMagazynuWorker();

                foreach (Towar t in widokTowarow)
                {
                    worker.Towar = t;

                    double stan1 = PobierzStanFizyczny(worker, mag1);
                    double stan2 = PobierzStanFizyczny(worker, mag2);
                    double stan3 = PobierzStanFizyczny(worker, mag3);
                    double stan4 = PobierzStanFizyczny(worker, mag4);

                    double cenaDetal = 0d;
                    var cenaDef = t.Ceny["Detaliczna"];
                    if (cenaDef != null) cenaDetal = cenaDef.Brutto.Value;

                    string linia = $"{t.Kod};{t.Nazwa?.Replace(";", ",")};{t.EAN};{cenaDetal};{t.Dostawca?.Kod};{stan1};{stan2};{stan3};{stan4}";

                    sw.WriteLine(linia);
                }
            }
        }

        private double PobierzStanFizyczny(StanMagazynuWorker worker, Magazyn mag)
        {
            if (mag == null) return 0;
            worker.Magazyn = mag;
            return worker.StanFizyczny.Value;
        }
    }
}