using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Aspectize.Core;
using Aspectize;
using Aspectize.Office;
using Excel;
using System.IO;
using System.Linq;
using System.Threading;

namespace TranslationManager
{
    public interface ITranslationManagerService
    {
        DataSet LoadTranslations();

        [Command(BrowserCacheDuration = "30 Days")]
        DataSet LoadTranslationValues(Guid translationId, DateTime dateModified);

        DataSet ExtractLiterals(string applicationName);

        DataSet ImportTraductionFromExcel(UploadedFile excelFile);

        byte[] ExportTranslationToExcel();

        [Command(IsSaveCommand = true)]
        DataSet SaveTranslations(DataSet dataSet);

        void ResetTranslationCache();

        string GetPivotLanguage();

        string[] GetLanguages();
    }

    [Service(Name = "TranslationManagerService", ConfigurationRequired = true)]
    public class TranslationManagerService : ITranslationManagerService, ILocalizationProvider, ISingleton, IApplicationDependent, IMustValidate, IServiceName//, ITranslationTerm
    {
        [Parameter(Optional = false)]
        string DataServiceName = "";

        [Parameter(Optional = false)]
        string KeyLanguage = "";

        [Parameter(Optional = false)]
        string Languages = "";

        Application parentApp;

        string svcName;

        Dictionary<string, Dictionary<string, string>> dictionaries = new Dictionary<string, Dictionary<string, string>>();

        Application IApplicationDependent.Parent
        {
            set { parentApp = value; }
        }

        DataSet ITranslationManagerService.LoadTranslations()
        {
            IDataManager dm = EntityManager.FromDataBaseService(DataServiceName);

            dm.LoadEntities<AspectizeTranslation>();

            return dm.Data;
        }

        DataSet ITranslationManagerService.LoadTranslationValues(Guid translationId, DateTime dateModified)
        {
            IDataManager dm = EntityManager.FromDataBaseService(DataServiceName);

            dm.LoadEntityFields<AspectizeTranslation>(EntityLoadOption.AllFields, translationId);

            return dm.Data;
        }


        DataSet ITranslationManagerService.ExtractLiterals(string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName)) applicationName = parentApp.Name;

            IDataManager dm = EntityManager.FromDataBaseService(DataServiceName);

            IEntityManager em = dm as IEntityManager;

            var dicoTranslations = new Dictionary<string, bool>();

            List<AspectizeTranslation> translations = dm.GetEntities<AspectizeTranslation>();

            foreach(AspectizeTranslation translation in em.GetAllInstances<AspectizeTranslation>())
            {
                dicoTranslations.Add(translation.Key, translation.Ignore);
            }

            List<string> languages = Languages.Split(',').Select(p => p.Trim()).ToList();

            List<string> literals = TranslationHelper.ExtractWebLiterals2(Context.HostHome, applicationName);

            var nbSaved = 0;

            foreach (string l in literals)
            {
                if (!dicoTranslations.ContainsKey(l))
                {
                    var t = em.CreateInstance<AspectizeTranslation>();

                    t.Key = l;
                    t.IsNew = true;

                    foreach (string language in languages)
                    {
                        TranslationValue tv =  t.Values.Add();

                        tv.Language = language.Trim();
                        tv.Value = "";
                    }

                    nbSaved++;
                }
            }

            dm.SaveTransactional();

            return dm.Data;
            //return string.Format("{0} translations has been extracted, {1} translations has been saved in your storage {2}", literals.Count, nbSaved, DataServiceName);
        }

        DataSet ITranslationManagerService.ImportTraductionFromExcel(UploadedFile excelFile)
        {
            List<string> languages = Languages.Split(',').Select(p => p.Trim()).ToList(); 

            IDataManager dm = EntityManager.FromDataBaseService(DataServiceName);

            IEntityManager em = dm as IEntityManager;

            IExcelDataReader excelReader = null;

            if (excelFile.ContentLength > 0)
            {
                string extension = Path.GetExtension(excelFile.Name);

                if (string.IsNullOrEmpty(extension) || extension.ToLower() == ".xlsx")
                {
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(excelFile.Stream);
                }
                else throw new SmartException(100, @"File is not a valid Excel File, only valid Excel 2007 format (*.xlsx) is supported !");

                excelReader.IsFirstRowAsColumnNames = true;

                DataSet result = null;

                try
                {
                    result = excelReader.AsDataSet();
                }
                catch (Exception e)
                {
                    throw new SmartException(200, @"Import Error: {0}", e.Message);
                }

                if (result != null && result.Tables.Count == 1)
                {
                    List<AspectizeTranslation> translations = dm.GetEntities<AspectizeTranslation>();

                    DataTable dt = result.Tables[0];

                    if (!dt.Columns.Contains(KeyLanguage)) throw new SmartException(100, "Invalid file, there should be a column named {0}", KeyLanguage);

                    var nbLine = 1;

                    foreach (DataRow dr in dt.Rows)
                    {
                        string key = dr[KeyLanguage].ToString();

                        AspectizeTranslation t = translations.Find(item => item.Key == key);

                        if (t == null)
                        {
                            throw new SmartException(200, @"Invalid translation key {0} at line {1}", key, nbLine);
                        }

                        foreach(DataColumn dc in dt.Columns)
                        {
                            var langImport = dc.ColumnName;

                            if (languages.Contains(langImport))
                            {
                                TranslationValue tv = t.Values.GetList().Find(item => item.Language == langImport);

                                if (tv == null)
                                {
                                    tv = t.Values.Add();
                                    tv.Language = langImport;
                                }

                                tv.Value = dr[dc].ToString();
                            }
                        }

                        t.IsNew = false;

                        nbLine++;
                    }

                    dm.SaveTransactional();
                }
                else throw new SmartException(200, @"File is not valid, should not have more than 1 sheet !");
            }

            return dm.Data;
        }

        Dictionary<string, string> ILocalizationProvider.GetTranslator(string fromLanguage, string toLanguage)
        {
            var dictionaryName = String.Format("{0} {1}", fromLanguage, toLanguage);

            if (!dictionaries.ContainsKey(dictionaryName))
            {
                List<string> languages = Languages.Split(',').Select(p => p.Trim()).ToList();

                foreach (string language in languages)
                {
                    Dictionary<string, string> dico = new Dictionary<string, string>();

                    string dicoName = string.Format("{0} {1}", KeyLanguage, language);
                    
                    dictionaries.Add(dicoName, dico);
                }

                IDataManager dm = EntityManager.FromDataBaseService(DataServiceName);

                IEntityManager em = dm as IEntityManager;

                List<AspectizeTranslation> translations = dm.GetEntitiesFields<AspectizeTranslation>(EntityLoadOption.AllFields, new QueryCriteria(AspectizeTranslation.Fields.Ignore, ComparisonOperator.Equal, false));

                foreach (AspectizeTranslation translation in translations)
                {
                    if (!translation.Ignore)
                    {
                        var key = translation.Key;

                        foreach (string language in languages)
                        {
                            var value = translation.Values.Find(item => item.Language == language);

                            if (value != null && !string.IsNullOrWhiteSpace(value.Value))
                            {
                                string dicoName = string.Format("{0} {1}", KeyLanguage, language);

                                dictionaries[dicoName].Add(key, value.Value);
                            }
                        }
                    }
                }
            }

            return dictionaries[dictionaryName];
        }


        byte[] ITranslationManagerService.ExportTranslationToExcel()
        {
            IDataManager dm = EntityManager.FromDataBaseService(DataServiceName);

            IEntityManager em = dm as IEntityManager;

            dm.LoadEntitiesFields<AspectizeTranslation>(EntityLoadOption.AllFields, new QueryCriteria(AspectizeTranslation.Fields.Ignore, ComparisonOperator.Equal, false));

            IAspectizeExcel aspectizeExcel = ExecutingContext.GetService<IAspectizeExcel>("AspectizeExcel");

            IEntityManager emExport = EntityManager.FromDataSet(new DataSet());

            var dtTranslation = emExport.Data.Tables.Add();

            dtTranslation.Columns.Add(KeyLanguage, typeof(string));

            List<string> languages = Languages.Split(',').Select(p => p.Trim()).ToList();

            foreach (string language in languages)
            {
                dtTranslation.Columns.Add(language, typeof(string));
            }

            foreach(AspectizeTranslation translation in em.GetAllInstances<AspectizeTranslation>())
            {
                var row = dtTranslation.NewRow();

                row[KeyLanguage] = translation.Key;

                foreach (TranslationValue translationValue in translation.Values.GetList())
                {
                    row[translationValue.Language] = translationValue.Value;
                }

                dtTranslation.Rows.Add(row);
            }

            emExport.Data.AcceptChanges();

            ExecutingContext.SetHttpDownloadFileName(string.Format("Translation_{0}_{1:ddMMyyyyHHmm}.xlsx", parentApp.Name, DateTime.Now));

            var bytes = aspectizeExcel.ToExcel(emExport.Data, null);

            return bytes as byte[];
        }


        DataSet ITranslationManagerService.SaveTranslations(DataSet dataSet)
        {
            IDataManager dm = EntityManager.FromDataSetAndBaseService(dataSet, DataServiceName);

            IEntityManager em = dm as IEntityManager;

            foreach (AspectizeTranslation translation in em.GetAllInstances<AspectizeTranslation>())
            {
                if (translation.IsNew)
                {
                    translation.IsNew = false;
                    translation.data.AcceptChanges();
                    translation.data.SetAdded();
                }
            }

            dm.SaveTransactional();

            return dm.Data;
        }

        void ITranslationManagerService.ResetTranslationCache()
        {
            dictionaries.Clear();
        }

        string IMustValidate.ValidateConfiguration()
        {
            if (String.IsNullOrWhiteSpace(KeyLanguage)) return String.Format("Parameter KeyLanguage can not be NullOrWhiteSpace on TranslationManagerService '{0}'. The parameter should be a .Net language  culture name as 'en-US' !", svcName);
            if (String.IsNullOrWhiteSpace(DataServiceName)) return String.Format("Parameter DataServiceName can not be NullOrWhiteSpace on TranslationManagerService '{0}'. The parameter should be a valid Data Service Name !", svcName);
            if (String.IsNullOrWhiteSpace(Languages)) return String.Format("Parameter Languages can not be NullOrWhiteSpace on TranslationManagerService '{0}'. The parameter should be list of .Net language  culture names separated by , !", svcName);

            var languages = Languages.Split(',').Select(p => p.Trim()).ToList();

            foreach(string language in languages)
            {
                if (String.IsNullOrWhiteSpace(language)) return String.Format("Parameter Languages can not contains empty langauage on TranslationManagerService '{0}'. The parameter should be list of .Net language  culture names separated by , !", svcName);
            }

            return null;
        }

        void IServiceName.SetServiceName(string name)
        {
            svcName = name;
        }

        string ITranslationManagerService.GetPivotLanguage()
        {
            return KeyLanguage;
        }

        string[] ITranslationManagerService.GetLanguages()
        {
            return Languages.Split(',').Select(p => p.Trim()).ToArray();
        }

        //string ITranslationTerm.Translate(string term)
        //{
        //    string toLanguage = Thread.CurrentThread.CurrentCulture.Name;

        //    var dico = ((ILocalizationProvider)this).GetTranslator(KeyLanguage, toLanguage);

        //    if (dico != null && dico.ContainsKey(term)) {
        //        return dico[term];
        //    }

        //    return term;
        //}
    }

}
