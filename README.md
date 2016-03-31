# TranslationManager
Aspectize Extension to manage Translation of your App.

## 1 - Download

Download extension package from aspectize.com:
- in the portal, goto extension section
- browse extension, and find TranslationManager
- download package and unzip it into your local WebHost Applications directory; you should have a TranslationManager directory next to your app directory.

## 2 - Configuration

a/ Add TranslationManager as Shared Application in your application configuration file.
In your Visual Studio Project, find the file Application.js in the Configuration folder.

Add TranslationManager in the Directories list :
```javascript
app.Directories = "TranslationManager";
```

b/ Add a new Configured service.
In your Visual Studio Project, find the file Service.js in the Configuration/Services folder.
Add a new service definition:
```javascript
var myTranslationManagerService = Aspectize.ConfigureNewService("MyTranslationManagerService", aas.ConfigurableServices.TranslationManagerService);
myTranslationManagerService.DataServiceName = "MyDataService";
myTranslationManagerService.KeyLanguage = "en-US";
myTranslationManagerService.Languages = "fr-FR";
```

The configurable Service has 3 parameters:
- DataServiceName: name of your DataService in which Translation are stored. Your DataService should have the property BuildNewTableOnSave set to true, otherwise the extraction of literals will failed.
- KeyLanguage: should be the default and reference language of your App.  
- Languages: should be the list of different languages your App supports, separated by a |.

Language should be defined with the CultureInfo Name; for example, 'fr-FR', 'en-US' or 'it-IT'.

c/ Make your App localizable
In your Application.js file add the following configuration on your app:

```javascript
app.Localizable = true;
app.LocalizationPivotLanguage = 'en-US';
app.LocalizationServiceName = 'MyTranslationManagerService';
```
Rebuild your App.

## 3 - Extract Literals from your App

In your app, run the following command:
MyTranslationManagerService.ExtractLiterals

Just copy the command url in your browser:
http://[MylocalWebHost]/[MyApp]/MyTranslationManagerService.ExtractLiterals.json.cmd.ashx

You should see the following return message:
('26 translations has been extracted, 26 translations has been saved in your storage DataService')

Check your DataBase and notice the Translation and Translation1Values new tables.

Each time, you add new UX elements that should require some translations, run the command to update your translations.

## 4 - Export and Import Translation





