//var vTranslations = Aspectize.CreateView('Translations', aas.Controls.TranslationManager.TranslationManager, aas.Zones.Home.ZoneInfo);
//vTranslations.OnLoad.BindCommand(aas.Services.Server.MyTranslationManagerService.LoadTranslations(), aas.Data.TranslationData, true, true);
//vTranslations.BtnExtractTranslation.click.BindCommand(aas.Services.Server.MyTranslationManagerService.ExtractLiterals(), aas.Data.TranslationData, true, true);
//vTranslations.BtnExportToExcel.href.BindData('MyTranslationManagerService.ExportTranslationToExcel.bin.cmd.ashx');
//vTranslations.BtnExportToExcel.target.BindData('_blank');
//vTranslations.UploadExcelFile.OnFileSelected.BindCommand(aas.Services.Server.MyTranslationManagerService.ImportTraductionFromExcel(vTranslations.UploadExcelFile.SelectedFile));
//vTranslations.BtnSave.click.BindCommand(aas.Services.Server.MyTranslationManagerService.SaveTranslations(aas.Data.TranslationData), aas.Data.TranslationData, true, true);
//vTranslations.TxtSearch.keyup.BindCommand(aas.Services.Browser.UIService.SetCustomFilter, { controlName: aas.ViewName.Translations.GridTranslation, customFilter: aas.Expression('(Key).toLowerCase().indexOf("' + vTranslations.TxtSearch.value + '".toLowerCase()) !== -1') });
//vTranslations.TranslationKey.BindData(aas.Data.TranslationData.Translation.Key);
//vTranslations.GridTranslation.BindGrid(aas.Data.TranslationData.Translation, 'Key ASC');
//vTranslations.GridTranslation.PageSize.BindData(20);

//var cClassName = vTranslations.GridTranslation.AddGridColumn('ClassName', 'RowClass');
//cClassName.className.BindData(aas.Expression(IIF(vTranslations.GridTranslation.DataSource.IsNew, 'bg-success', IIF(vTranslations.GridTranslation.DataSource.Ignore, 'bg-danger', ''))));

//var cIgnore = vTranslations.GridTranslation.AddGridColumn('Ignore', 'SpanCheck');
//cIgnore.Text.BindData(vTranslations.GridTranslation.DataSource.Ignore);
//var cIsNew = vTranslations.GridTranslation.AddGridColumn('New', 'SpanCheck');
//cIsNew.Text.BindData(vTranslations.GridTranslation.DataSource.IsNew);
//var cName = vTranslations.GridTranslation.AddGridColumn('Translation', 'Span');
//cName.Text.BindData(vTranslations.GridTranslation.DataSource.Key);
//var cIgnore = vTranslations.GridTranslation.AddGridColumn('Ignore', 'Button');
//cIgnore.HeaderText.BindData('');
//cIgnore.Text.BindData(aas.Expression(IIF(vTranslations.GridTranslation.DataSource.Ignore, 'Add', 'Ignore')));
//cIgnore.Click.BindCommand(aas.Services.Browser.TestingServices.BrowserEcho(aas.Expression(!aas.Data.TranslationData.Translation.Ignore)), aas.Data.TranslationData.Translation.Ignore);
//cIgnore.Click.BindCommand(aas.Services.Server.MyTranslationManagerService.SaveTranslations(aas.Data.TranslationData), '', false, true);

//var vTranslationItemValues = Aspectize.CreateRepeatedView('TranslationItemValues', aas.Controls.TranslationManager.TranslationItemValues, aas.Zones.Translations.RepeaterPanelTranslationItemValues, aas.Data.TranslationData.Translation.Values);
//vTranslationItemValues.Language.BindData(vTranslationItemValues.ParentData.Language);
//vTranslationItemValues.TxtTranslation.value.BindData(vTranslationItemValues.ParentData.Value);
