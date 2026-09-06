using Soneta.Types;
using Soneta.Types.DynamicApi;
using System.Collections.Generic;

namespace WebAPI
{

    /// <summary>
    /// Serwis udostępniający dane towarowe i stany magazynowe dla sklepu internetowego.
    /// </summary>
    public interface ITowaryService
    {
        /// <summary>
        /// Pobiera listę towarów wraz z ich stanami na poszczególnych magazynach w oparciu o przekazane filtry.
        /// </summary>
        /// <remarks>
        /// Przy TylkoZmienione = true API zdejmuje flagę na zwróconych towarach.
        /// Klient musi więc odpytywać zawsze o NumerStrony: 1 i powtarzać, dopóki
        /// CzyJestNastepnaStrona = true — kolejne strony w klasycznym rozumieniu
        /// nie mają tu zastosowania, bo zestaw wyników zmienia się po każdym wywołaniu.
        /// Świadoma decyzja: pobranie jest jednocześnie potwierdzeniem synchronizacji.
        /// </remarks>
        /// <param name="pars">Parametry filtrowania.</param>
        /// <returns>Obiekt zawierający wyfiltrowane towary oraz informacje o stronicowaniu.</returns>
        [DynamicApiMethod("POST", "PobierzTowary")]
        OdpowiedzTowary PobierzTowary(FiltryTowarow pars);
    }

    /// <summary>
    /// Obiekt zwracany przez API, zawierający wyniki oraz informacje o stronicowaniu.
    /// </summary>
    public class OdpowiedzTowary
    {
        /// <summary>Numer aktualnie zwróconej strony.</summary>
        public int AktualnaStrona { get; set; }

        /// <summary>Informacja, czy w bazie są kolejne rekordy pasujące do filtrów.</summary>
        public bool CzyJestNastepnaStrona { get; set; }

        /// <summary>Lista wyfiltrowanych towarów.</summary>
        public List<TowarDTO> Towary { get; set; } = new List<TowarDTO>();
    }

    /// <summary>
    /// Filtry wyszukiwania towarów. Brak podania filtru oznacza brak ograniczenia w danym kryterium (poza limitem ilościowym).
    /// </summary>
    public class FiltryTowarow
    {
        /// <summary>
        /// Pobiera tylko te towary, które mają zaznaczoną cechę TowarZmieniony.
        /// </summary>
        public bool TylkoZmienione { get; set; }

        /// <summary>
        /// Lista kodów towarów do pobrania. Wpisz tu dokładne kody towarów z Enovy.
        /// </summary>
        public List<string> KodyTowarow { get; set; }

        /// <summary>
        /// Jeśli true, zwróci tylko te towary, których łączny stan na wszystkich magazynach jest większy niż 0.
        /// </summary>
        public bool TylkoDodatnieStany { get; set; }

        /// <summary>
        /// Maksymalna liczba zwróconych wyników. Domyślnie wynosi 100.
        /// </summary>
        public int Limit { get; set; } = 100;

        /// <summary>
        /// Numer strony do pobrania. Domyślnie 1 (pierwsza strona).
        /// </summary>
        public int NumerStrony { get; set; } = 1;
    }

    /// <summary>
    /// Obiekt reprezentujący towar wraz ze szczegółami kartotekowymi i stanami magazynowymi.
    /// </summary>
    public class TowarDTO
    {
        /// <summary>Kod EAN towaru.</summary>
        public string EAN { get; set; }

        /// <summary>Pełna nazwa towaru z kartoteki.</summary>
        public string Nazwa { get; set; }

        /// <summary>Unikalny kod towaru w systemie.</summary>
        public string Kod { get; set; }

        /// <summary>Cena detaliczna brutto towaru.</summary>
        public Currency CenaDetalicznaBrutto { get; set; }

        /// <summary>Nazwa dostawcy towaru.</summary>
        public string DostawcaNazwa { get; set; }

        /// <summary>Kod dostawcy towaru.</summary>
        public string DostawcaKod { get; set; }

        /// <summary>Stan na magazynie 1.</summary>
        public double Stan_1 { get; set; }

        /// <summary>Stan na magazynie 2.</summary>
        public double Stan_2 { get; set; }

        /// <summary>Stan na magazynie 3.</summary>
        public double Stan_3 { get; set; }

        /// <summary>Stan na magazynie 4.</summary>
        public double Stan_4 { get; set; }
    }
}