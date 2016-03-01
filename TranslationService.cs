using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Aspectize.Core;
using Aspectize;
using Aspectize.Office;
using Excel;
using System.IO;

namespace TranslationManager
{
    public interface ITranslationManagerService
    {
        void ExtractLiterals(string applicationName);
        DataSet ImportTraductionFromExcel(UploadedFile excelFile);
        byte[] ExportTranslation();
    }

    [Service(Name = "TranslationManagerService")]
    public class TranslationManagerService : ITranslationManagerService, ILocalizationProvider, ISingleton, IApplicationDependent
    {
        //[Parameter(Optional = false)]
        string DataServiceName = "";

        //[Parameter(Optional = false)]
        string KeyLanguage = "";

        //[Parameter(Optional = false)]
        string Languages = "";

        Application parentApp;

        Dictionary<string, Dictionary<string, string>> dictionaries = new Dictionary<string, Dictionary<string, string>>();

        Application IApplicationDependent.Parent
        {
            set { parentApp = value; }
        }

        void ITranslationManagerService.ExtractLiterals(string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName)) applicationName = parentApp.Name;

            IDataManager dm = EntityManager.FromDataBaseService(DataServiceName);

            IEntityManager em = dm as IEntityManager;

            var dicoTranslations = new Dictionary<string, bool>();

            List<Translation> translations = dm.GetEntities<Translation>();

            foreach(Translation translation in em.GetAllInstances<Translation>())
            {
                dicoTranslations.Add(translation.Key, translation.Ignore);
            }

            List<string> languages = new List<string>(Languages.Split('|'));

            Dictionary<string, bool> one = new Dictionary<string, bool>();

            List<string> literals = TranslationHelper.ExtractWebLiterals2(Context.HostHome, applicationName, one);

            foreach (string l in literals)
            {
                if (!dicoTranslations.ContainsKey(l))
                {
                    var t = em.CreateInstance<Translation>();

                    t.Key = l;

                    foreach (string language in languages)
                    {
                        TranslationValue tv =  t.Values.Add();

                        tv.Language = language;
                        tv.Value = "";
                    }
                }
            }

            dm.SaveTransactional();
        }

        DataSet ITranslationManagerService.ImportTraductionFromExcel(UploadedFile excelFile)
        {
            List<string> languages = new List<string>(Languages.Split('|'));

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
                    List<Translation> translations = dm.GetEntities<Translation>();

                    DataTable dt = result.Tables[0];

                    if (!dt.Columns.Contains(KeyLanguage)) throw new SmartException(100, "Invalid file, there should be a column named {0}", KeyLanguage);

                    var nbLine = 1;

                    foreach (DataRow dr in dt.Rows)
                    {
                        string key = dr[KeyLanguage].ToString();

                        Translation t = translations.Find(item => item.Key == key);

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
            var dictionaryName = String.Format("{0} {1} {2}", parentApp.Name, fromLanguage, toLanguage);

            if (!dictionaries.ContainsKey(dictionaryName))
            {
                List<string> languages = new List<string>(Languages.Split('|'));

                foreach (string language in languages)
                {
                    Dictionary<string, string> dico = new Dictionary<string, string>();

                    string dicoName = string.Format("{0} {1}", KeyLanguage, language);
                    
                    dictionaries.Add(dicoName, dico);
                }

                IDataManager dm = EntityManager.FromDataBaseService(DataServiceName);

                IEntityManager em = dm as IEntityManager;

                List<Translation> translations = dm.GetEntities<Translation>(new QueryCriteria(Translation.Fields.Ignore, ComparisonOperator.Equal, false));

                foreach (Translation translation in translations)
                {
                    if (!translation.Ignore)
                    {
                        var key = translation.Key;

                        foreach (string language in languages)
                        {
                            var value = translation.Values.Find(item => item.Language == language);

                            if (value != null)
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


        byte[] ITranslationManagerService.ExportTranslation()
        {
            IDataManager dm = EntityManager.FromDataBaseService(DataServiceName);

            IEntityManager em = dm as IEntityManager;

            dm.LoadEntities<Translation>(new QueryCriteria(Translation.Fields.Ignore, ComparisonOperator.Equal, false));

            IAspectizeExcel aspectizeExcel = ExecutingContext.GetService<IAspectizeExcel>("AspectizeExcel");

            IEntityManager emExport = EntityManager.FromDataSet(new DataSet());

            var dtTranslation = emExport.Data.Tables.Add();

            dtTranslation.Columns.Add(KeyLanguage, typeof(string));

            List<string> languages = new List<string>(Languages.Split('|'));

            foreach (string language in languages)
            {
                dtTranslation.Columns.Add(language, typeof(string));
            }

            foreach(Translation translation in em.GetAllInstances<Translation>())
            {
                var row = dtTranslation.NewRow();

                row[KeyLanguage] = translation.Key;

                foreach (TranslationValue translationValue in translation.Values.GetList())
                {
                    row[translationValue.Language] = translationValue.Value;
                }
            }
            
            var bytes = aspectizeExcel.ToExcel(emExport.Data, null);

            ExecutingContext.SetHttpDownloadFileName(string.Format("Translation_{0:ddMMyyyyHHmm}.xlsx", DateTime.Now));

            return bytes as byte[];
        }
    }

}