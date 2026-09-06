using Soneta.Business;
using Soneta.Business.App;
using Soneta.Magazyny;
using Soneta.Towary;
using Soneta.Types;
using Soneta.Types.DynamicApi;
using System.Collections.Generic;

[assembly: Service(typeof(WebAPI.ITowaryService), typeof(WebAPI.TowaryService), ServiceScope.Session)]
[assembly: DynamicApiController(typeof(WebAPI.ITowaryService), typeof(WebAPI.TowaryService))]

namespace WebAPI
{
    public class TowaryService : ITowaryService
    {
        private readonly Login _login;
        public TowaryService(Login login) => _login = login;

        public OdpowiedzTowary PobierzTowary(FiltryTowarow pars)
        {
            var odpowiedz = new OdpowiedzTowary();

            int limit = pars != null && pars.Limit > 0 ? pars.Limit : 100;
            int strona = pars != null && pars.NumerStrony > 0 ? pars.NumerStrony : 1;
            odpowiedz.AktualnaStrona = strona;

            int doPominieciaValid = (strona - 1) * limit;

            using (Session session = _login.CreateSession(false, false, "WebAPI_PobierzTowary"))
            {

                var tm = TowaryModule.GetInstance(session);
                var mm = MagazynyModule.GetInstance(session);

                /* Magazyny wskazane na sztywno - konkretny podzbiór nie wystawiamy tego jako konfiguracji
                użytkownika: przypadkowa zmiana po stronie enovy oznaczałaby błędne stany */
                var mag1 = mm.Magazyny.WgSymbol["1"];
                var mag2 = mm.Magazyny.WgSymbol["2"];
                var mag3 = mm.Magazyny.WgSymbol["3"];
                var mag4 = mm.Magazyny.WgSymbol["4"];

                var widok = tm.Towary.CreateView();

                if (pars != null)
                {
                    if (pars.KodyTowarow != null && pars.KodyTowarow.Count > 0)
                        widok.Condition &= new FieldCondition.In("Kod", pars.KodyTowarow);

                    if (pars.TylkoZmienione)
                        widok.Condition &= new FieldCondition.Equal("Features.TowarZmieniony", true);
                }

                int przeskoczoneValid = 0;
                int dodane = 0;

                var towaryDoOdznaczenia = new List<Towar>();

                foreach (Towar t in widok)
                {
                    var worker = new StanMagazynuWorker { Towar = t };

                    double stan1 = PobierzStan(worker, mag1);
                    double stan2 = PobierzStan(worker, mag2);
                    double stan3 = PobierzStan(worker, mag3);
                    double stan4 = PobierzStan(worker, mag4);

                    double sumaStanow = stan1 + stan2 + stan3 + stan4;

                    if (pars != null)
                    {
                        if (pars.TylkoDodatnieStany && sumaStanow <= 0) continue;
                    }

                    if (przeskoczoneValid < doPominieciaValid)
                    {
                        przeskoczoneValid++;
                        continue;
                    }

                    Currency cenaDetal = 0m;
                    var cenaDef = t.Ceny["Detaliczna"];
                    if (cenaDef != null) cenaDetal = cenaDef.Brutto;

                    odpowiedz.Towary.Add(new TowarDTO
                    {
                        Kod = t.Kod,
                        Nazwa = t.Nazwa,
                        EAN = t.EAN,
                        CenaDetalicznaBrutto = cenaDetal,
                        DostawcaKod = t.Dostawca?.Kod,
                        DostawcaNazwa = t.Dostawca?.Nazwa,
                        Stan_1 = stan1,
                        Stan_2 = stan2,
                        Stan_3 = stan3,
                        Stan_4 = stan4,
                    });

                    if (pars != null && pars.TylkoZmienione)
                    {
                        towaryDoOdznaczenia.Add(t);
                    }

                    dodane++;

                    if (dodane > limit)
                    {
                        odpowiedz.CzyJestNastepnaStrona = true;
                        odpowiedz.Towary.RemoveAt(odpowiedz.Towary.Count - 1);
                        if (towaryDoOdznaczenia.Count > limit)
                            towaryDoOdznaczenia.RemoveAt(towaryDoOdznaczenia.Count - 1);
                        break;
                    }
                }

                // TRANSAKCJA ZAPISU 
                if (towaryDoOdznaczenia.Count > 0)
                {
                    using (ITransaction trans = session.Logout(true))
                    {
                        foreach (var towar in towaryDoOdznaczenia)
                        {
                            towar.Features["TowarZmieniony"] = false;
                        }
                        trans.Commit();
                    }
                    session.Save();
                }

            }

            return odpowiedz;
        }

        private double PobierzStan(StanMagazynuWorker worker, Magazyn mag)
        {
            if (mag == null) return 0;
            worker.Magazyn = mag;
            return worker.Stan.Value;
        }
    }
}
