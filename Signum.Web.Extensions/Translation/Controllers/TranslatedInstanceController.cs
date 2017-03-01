using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Signum.Engine;
using Signum.Engine.Basics;
using Signum.Engine.Translation;
using Signum.Entities;
using Signum.Entities.Authorization;
using Signum.Entities.Basics;
using Signum.Entities.Translation;
using Signum.Utilities;

namespace Signum.Web.Translation.Controllers
{
    [ValidateInputAttribute(false)]
    public class TranslatedInstanceController : Controller
    {
        public ActionResult Index()
        {
            var cultures = TranslationLogic.CurrentCultureInfos(TranslatedInstanceLogic.DefaultCulture);

            var list = TranslatedInstanceLogic.TranslationInstancesStatus();

            return base.View(TranslationClient.ViewPrefix.Formato("IndexInstance"), list.AgGroupToDictionary(a => a.Type, gr => gr.ToDictionary(a => a.CultureInfo)));
        }
        
        [HttpGet]
        public ActionResult EntityStatus(Lite<Entity> instance)
        {
            var cultures = TranslationLogic.CurrentCultureInfos(TranslatedInstanceLogic.DefaultCulture);

            var list = TranslatedInstanceLogic.TranslationSingleInstanceStatus(instance);

            ViewBag.Instance = instance;

            return base.View(TranslationClient.ViewPrefix.Formato("IndexSingleInstance"), list.ToDictionary(a => a.CultureInfo));
        }

        [HttpGet]
        public ActionResult View(string type, string culture, bool searchPressed, string filter)
        {
            Type t = TypeLogic.GetType(type);
            ViewBag.Type = t;
            var c = culture == null ? null : CultureInfo.GetCultureInfo(culture);
            ViewBag.Culture = c;

            ViewBag.Filter = filter;

            if (!searchPressed)
                return base.View(TranslationClient.ViewPrefix.Formato("ViewInstance"));

            Dictionary<LocalizedInstanceKey, string> master = TranslatedInstanceLogic.FromEntities(t);

            ViewBag.Master = master;

            Dictionary<CultureInfo, Dictionary<LocalizedInstanceKey, TranslatedInstanceDN>> support = TranslatedInstanceLogic.TranslationsForType(t, culture: c);

            return base.View(TranslationClient.ViewPrefix.Formato("ViewInstance"), support);
        }

        [HttpGet]
        public ActionResult ViewEntity(Lite<Entity> instance, string culture, bool searchPressed, string filter)
        {
            ViewBag.Instance = instance;

            var c = culture == null ? null : CultureInfo.GetCultureInfo(culture);
            ViewBag.Culture = c;

            ViewBag.Filter = filter;

            if (!searchPressed)
                return base.View(TranslationClient.ViewPrefix.Formato("ViewInstance"));

            Dictionary<LocalizedInstanceKey, string> master = TranslatedInstanceLogic.FromEntity(instance);

            ViewBag.Master = master;

            Dictionary<CultureInfo, Dictionary<LocalizedInstanceKey, TranslatedInstanceDN>> support = TranslatedInstanceLogic.TranslationsForEntity(instance, culture: c);

            return base.View(TranslationClient.ViewPrefix.Formato("ViewInstance"), support);
        }

        public FileContentResult ViewFile(string type, string culture)
        {
             Type t = TypeLogic.GetType(type);
             var c = CultureInfo.GetCultureInfo(culture);

            var file = TranslatedInstanceLogic.ExportExcelFile(t, c);

            return File(file.Content, MimeType.FromFileName(file.FileName), file.FileName);  
        }

        [HttpPost]
        public ActionResult SaveView(string type, string culture, string filter)
        {
            Type t = TypeLogic.GetType(type);

            var records = GetTranslationRecords(t);

            var c = culture == null ? null : CultureInfo.GetCultureInfo(culture);

             TranslatedInstanceLogic.SaveRecords(records, t, c);

            return RedirectToAction("View", new { type = type, culture = culture, filter = filter, searchPressed = true });
        }

        [HttpPost]
        public ActionResult SaveViewEntity(Lite<Entity> instance, string culture, string filter)
        {
            var records = GetEntityTranslationRecords();

            var c = culture == null ? null : CultureInfo.GetCultureInfo(culture);

            TranslatedInstanceLogic.SaveEntityRecords(records, instance, c);

            return RedirectToAction("ViewEntity", new { instance = instance.Key(), culture = culture, filter = filter, searchPressed = true });
        }


        static Regex regexRecord = new Regex(@"^(?<lang>[^#]+)#(?<instance>[^#]+)#(?<route>[^#]+)$");
        static Regex regexIndexer = new Regex(@"\[(?<num>\d+)\]\.");

        private List<TranslationRecord> GetEntityTranslationRecords()
        {
            var list = (from k in Request.Form.AllKeys
                        let m = regexRecord.Match(k)
                        where m.Success
                        let route = m.Groups["route"].Value
                        let instance = Lite.Parse(m.Groups["instance"].Value)
                        select new TranslationRecord
                        {
                            Culture = CultureInfo.GetCultureInfo(m.Groups["lang"].Value),
                            Key = new LocalizedInstanceKey(
                                PropertyRoute.Parse(instance.EntityType, regexIndexer.Replace(route, "/")),
                                instance,
                                regexIndexer.Match(route).Let(mi => mi.Success ? PrimaryKey.Parse(mi.Groups["num"].Value, instance.EntityType) : (PrimaryKey?)null)
                                ),
                            TranslatedText = Request.Form[k].DefaultText(null),
                        }).ToList();

            var master = list.Extract(a => a.Culture.Name == TranslatedInstanceLogic.DefaultCulture.Name).ToDictionary(a => a.Key);

            list.ForEach(r => r.OriginalText = master.GetOrThrow(r.Key).TranslatedText);

            return list;
        }

        private List<TranslationRecord> GetTranslationRecords(Type type)
        {
            var list = (from k in Request.Form.AllKeys
                        let m = regexRecord.Match(k)
                        where m.Success
                        let route = m.Groups["route"].Value
                        select new TranslationRecord
                        {
                            Culture = CultureInfo.GetCultureInfo(m.Groups["lang"].Value),
                            Key = new LocalizedInstanceKey(
                                PropertyRoute.Parse(type, regexIndexer.Replace(route, "/")),
                                Lite.Parse(m.Groups["instance"].Value),
                                regexIndexer.Match(route).Let(mi => mi.Success ? PrimaryKey.Parse(mi.Groups["num"].Value, type) : (PrimaryKey?)null)
                                ),
                            TranslatedText = Request.Form[k].DefaultText(null),
                        }).ToList();

            var master = list.Extract(a => a.Culture.Name == TranslatedInstanceLogic.DefaultCulture.Name).ToDictionary(a=>a.Key);

            list.ForEach(r => r.OriginalText = master.GetOrThrow(r.Key).TranslatedText);

            return list;
        }

      

        public ActionResult Sync(string type, string culture)
        {
            Type t = TypeLogic.GetType(type);

            var c = CultureInfo.GetCultureInfo(culture);

            int totalInstances; 
            var changes = TranslatedInstanceSynchronizer.GetTypeInstanceChangesTranslated(TranslationClient.Translator, t, c, out totalInstances);

            ViewBag.TotalInstances = totalInstances; 
            ViewBag.Culture = c;
            return base.View(TranslationClient.ViewPrefix.Formato("SyncInstance"), changes);
        }

        public ActionResult SyncEntity(Lite<Entity> entity, string culture)
        {
            var c = CultureInfo.GetCultureInfo(culture);

            int totalInstances;
            var changes = TranslatedInstanceSynchronizer.GetSingleInstanceChangesTranslated(TranslationClient.Translator, entity, c, out totalInstances);

            ViewBag.Instance = entity;
            ViewBag.TotalInstances = totalInstances;
            ViewBag.Culture = c;
            return base.View(TranslationClient.ViewPrefix.Formato("SyncInstance"), changes);
        }

        public FileContentResult SyncFile(string type, string culture)
        {
            Type t = TypeLogic.GetType(type);

            CultureInfo c = CultureInfo.GetCultureInfo(culture);

            var file = TranslatedInstanceLogic.ExportExcelFileSync(t, c);

            return File(file.Content, MimeType.FromFileName(file.FileName), file.FileName);  
        }

        [HttpPost]
        public ActionResult SaveSync(string type, string culture)
        {
            Type t = TypeLogic.GetType(type);

            var c = CultureInfo.GetCultureInfo(culture);

            List<TranslationRecord> records = GetTranslationRecords(t);

            TranslatedInstanceLogic.SaveRecords(records, t, c);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SaveSyncEntity(Lite<Entity> instance, string culture)
        {
            var c = CultureInfo.GetCultureInfo(culture);

            List<TranslationRecord> records = GetEntityTranslationRecords();

            TranslatedInstanceLogic.SaveEntityRecords(records, instance, c);

            return RedirectToAction("EntityStatus", new { instance = instance.Key() });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadFile()
        {
            HttpPostedFileBase hpf = Request.Files[Request.Files.Cast<string>().Single()];

            var type = TypeLogic.GetType(hpf.FileName.Before('.'));
            var culture = CultureInfo.GetCultureInfo(hpf.FileName.After('.').Before('.'));

            var pair = TranslatedInstanceLogic.ImportExcelFile(hpf.InputStream, hpf.FileName);

            return RedirectToAction("View", new { type = TypeLogic.GetCleanName(pair.Type), culture = pair.Culture, searchPressed = false });
        }

    }
}
