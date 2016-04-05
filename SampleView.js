//var vTranslations = Aspectize.CreateView('Translations', aas.Controls.TranslationManager.TranslationManager, aas.Zones.Home.ZoneInfo);

//vTranslations.OnLoad.BindCommand(aas.Services.Server.MyTranslationManagerService.LoadTranslations(), aas.Data.TranslationData, true, true);
//vTranslations.BtnExtractTranslation.click.BindCommand('Server/MyTranslationManagerService.ExtractLiterals');
//vTranslations.BtnExportToExcel.href.BindData('MyTranslationManagerService.ExportTranslationToExcel.bin.cmd.ashx');
//vTranslations.BtnExportToExcel.target.BindData('_blank');
//vTranslations.UploadExcelFile.OnFileSelected.BindCommand(aas.Services.Server.MyTranslationManagerService.ImportTraductionFromExcel(vTranslations.UploadExcelFile.SelectedFile));
//vTranslations.BtnSave.click.BindCommand(aas.Services.Server.MyTranslationManagerService.SaveTranslations(aas.Data.TranslationData), '', false, true);
//vTranslations.TxtSearch.keyup.BindCommand(aas.Services.Browser.UIService.SetCustomFilter, { controlName: aas.ViewName.Translations.GridTranslation, customFilter: aas.Expression('(Key).toLowerCase().indexOf("' + vTranslations.TxtSearch.value + '".toLowerCase()) !== -1') });

//vTranslations.GridTranslation.BindGrid(aas.Data.TranslationData.Translation, 'Key ASC');
//vTranslations.GridTranslation.PageSize.BindData(20);
//var cName = vTranslations.GridTranslation.AddGridColumn('Translation', 'Span');
//cName.Text.BindData(vTranslations.GridTranslation.DataSource.Key);

//var vTranslationItemValues = Aspectize.CreateRepeatedView('TranslationItemValues', aas.Controls.TranslationManager.TranslationItemValues, aas.Zones.Translations.RepeaterPanelTranslationItemValues, aas.Data.TranslationData.Translation.Values);
//vTranslationItemValues.Language.BindData(vTranslationItemValues.ParentData.Language);
//vTranslationItemValues.TxtTranslation.value.BindData(vTranslationItemValues.ParentData.Value);