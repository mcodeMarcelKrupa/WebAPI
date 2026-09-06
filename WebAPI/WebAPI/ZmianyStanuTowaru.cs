using Soneta.Business;
using Soneta.Handel;
using Soneta.Towary;
using System.Linq;

[assembly: ProgramInitializer(typeof(WebAPI.Init))]

namespace WebAPI
{
    public class Init : IProgramInitializer
    {
        public void Initialize()
        {
            HandelModule.DokumentHandlowySchema.AddOnDeleting(DokumentHandlowyOnChange);
            HandelModule.DokumentHandlowySchema.AddOnEditing(DokumentHandlowyOnChange);
            HandelModule.DokumentHandlowySchema.AddOnAdded(DokumentHandlowyOnChange);
        }

        internal static void DokumentHandlowyOnChange(HandelModule.DokumentHandlowyRow row)
        {
            var dokument = (DokumentHandlowy)row;

            var towaryIds = dokument.Pozycje
                .Select(p => p.Towar as Towar)
                .Where(t => t != null)
                .Select(t => t.ID)
                .Distinct()
                .ToList();

            if (!towaryIds.Any()) return;

            using (var nowaSesja = dokument.Session.Login.CreateSession(false, false))
            {
                var towaryModul = TowaryModule.GetInstance(nowaSesja);

                using (var trans = nowaSesja.Logout(true))
                {
                    foreach (var id in towaryIds)
                    {
                        var towarWNowejSesji = towaryModul.Towary[id];

                        if (towarWNowejSesji != null)
                        {
                            towarWNowejSesji.Features["TowarZmieniony"] = true;
                        }
                    }

                    trans.Commit();
                }

                nowaSesja.Save();
            }
        }
    }
}