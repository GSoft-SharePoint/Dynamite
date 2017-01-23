﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Autofac;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Caml;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Folders;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Lists.Constants;
using GSoft.Dynamite.Pages;
using GSoft.Dynamite.Serializers;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Publishing.Fields;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Binding
{
    /// <summary>
    /// Validates the behavior of the default-configured implementation 
    /// of <see cref="ISharePointEntityBinder"/>, the mapper interface.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class SharePointEntityBinderTest
    {
        /// <summary>
        /// Validates that using the ISharePointEntityBinder to map and entity's properties from
        /// a list item which was created in a list where field definitions contain default values works.
        /// </summary>
        [TestMethod]
        public void ToEntity_WhenMappingFromListItem_AndFieldsHaveDefaultValues_ShouldMapEntityWithAllFieldDefaultValues()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey")
                {
                    DefaultValue = 555
                };

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey")
                {
                    DefaultValue = 5.5
                };

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey")
                {
                    DefaultValue = 500.95,
                    LocaleId = 3084 // fr-CA
                };

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                    "TestInternalNameBoolTrue",
                    new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                    "NameKeyBoolTrue",
                    "DescriptionKeyBoolTrue",
                    "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfoDefault = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey")
                {
                    DefaultValue = new DateTime(2005, 10, 21)
                };

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = "Text default value"
                };

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = "Note default value"
                };

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey")
                {
                    DefaultValue = "<p class=\"some-css-class\">HTML default value</p>"
                };

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey")
                {
                    DefaultValue = new ImageValue()
                    {
                        Hyperlink = "http://github.com/GSoft-SharePoint/",
                        ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                    }
                };

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey")
                {
                    DefaultValue = new UrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    }
                };

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image,
                    DefaultValue = new UrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    }
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    // ShowField should be Title by default
                    DefaultValue = new LookupValue(1, "Test Item 1")
                };

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID",
                    DefaultValue = new LookupValue(2, "2")
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey")
                {
                    DefaultValue = new LookupValueCollection() { new LookupValue(1, "Test Item 1"), new LookupValue(2, "Test Item 2") }
                };

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(WindowsIdentity.GetCurrent().Name);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey")
                {
                    DefaultValue = new UserValue(ensuredUser1)
                };

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey")
                {
                    DefaultValue = new UserValueCollection() { new UserValue(ensuredUser1), new UserValue(ensuredUser2) }
                };

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey")
                {
                    DefaultValue = new MediaValue()
                    {
                        Title = "Some media file title",
                        Url = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                        IsAutoPlay = true,
                        IsLoop = true,
                        PreviewImageUrl = "/_layouts/15/Images/logo.png"
                    }
                };

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValue(levelOneTermB),
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    DefaultValue = new TaxonomyValueCollection(
                        new List<TaxonomyValue>()
                            {
                                new TaxonomyValue(levelTwoTermAA),
                                new TaxonomyValue(levelTwoTermAB)
                            }),
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultTrue,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfoDefault,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo,
                        userFieldInfo,
                        userMultiFieldInfo,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;
                    lookupFieldInfoAlt.ListId = lookupList.ID;
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    list.EnableVersioning = true;
                    list.Update();

                    // Create item on list
                    var itemOnList = list.AddItem();
                    itemOnList.Update();    // force DefaultValue to be applied

                    // Force the generation of a version
                    itemOnList["Title"] = "Item under test";
                    itemOnList.Update();

                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entityMappedFromSingleItem = new TestItemEntityWithLookups();
                    var entityMappedFromItemVersion = new TestItemEntityWithLookups();

                    // Act

                    // Map from SPListItem
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromSingleItem, itemOnList);

                    // Map from SPListItemVersion
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromItemVersion, itemOnList.Versions[0]);

                    // Map from DataRow/SPListItemCollection
                    var entitiesMappedFromItemCollection = entityBinder.Get<TestItemEntity>(list.Items);

                    // Assert
                    // #1 Validate straight single list item to entity mappings
                    Assert.AreEqual(555, entityMappedFromSingleItem.IntegerProperty);
                    Assert.AreEqual(5.5, entityMappedFromSingleItem.DoubleProperty);
                    Assert.AreEqual(500.95, entityMappedFromSingleItem.CurrencyProperty);
                    Assert.IsFalse(entityMappedFromSingleItem.BoolProperty.HasValue);
                    Assert.IsTrue(entityMappedFromSingleItem.BoolDefaultTrueProperty);
                    Assert.IsFalse(entityMappedFromSingleItem.BoolDefaultFalseProperty);
                    Assert.AreEqual(DateTime.Today, entityMappedFromSingleItem.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(2005, 10, 21), entityMappedFromSingleItem.DateTimeProperty);
                    Assert.AreEqual("Text default value", entityMappedFromSingleItem.TextProperty);
                    Assert.AreEqual("Note default value", entityMappedFromSingleItem.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML default value</p>", entityMappedFromSingleItem.HtmlProperty);

                    Assert.IsNotNull(entityMappedFromSingleItem.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entityMappedFromSingleItem.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.UrlProperty.Url);
                    ////Assert.AreEqual("patate!", urlFieldVal.Description);     // proper Url description is missing because it can't be applied through SPField default value

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.UrlImageProperty.Url);
                    ////Assert.AreEqual("patate!", urlImageFieldVal.Description);     // proper Url description is missing because it can't be applied through SPField default value

                    Assert.AreEqual(1, entityMappedFromSingleItem.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromSingleItem.LookupProperty.Value);

                    Assert.AreEqual(2, entityMappedFromSingleItem.LookupAltProperty.Id);
                    Assert.AreEqual("2", entityMappedFromSingleItem.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, entityMappedFromSingleItem.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromSingleItem.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, entityMappedFromSingleItem.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", entityMappedFromSingleItem.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromSingleItem.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromSingleItem.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", entityMappedFromSingleItem.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", entityMappedFromSingleItem.MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entityMappedFromSingleItem.MediaProperty.Url);
                    Assert.IsTrue(entityMappedFromSingleItem.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entityMappedFromSingleItem.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entityMappedFromSingleItem.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entityMappedFromSingleItem.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entityMappedFromSingleItem.TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entityMappedFromSingleItem.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entityMappedFromSingleItem.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entityMappedFromSingleItem.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entityMappedFromSingleItem.TaxonomyMultiProperty[1].Label);

                    // #2 Validate list item version mappings
                    Assert.AreEqual(555, entityMappedFromItemVersion.IntegerProperty);
                    Assert.AreEqual(5.5, entityMappedFromItemVersion.DoubleProperty);
                    Assert.AreEqual(500.95, entityMappedFromItemVersion.CurrencyProperty);
                    Assert.IsFalse(entityMappedFromItemVersion.BoolProperty.HasValue);
                    Assert.IsTrue(entityMappedFromItemVersion.BoolDefaultTrueProperty);
                    Assert.IsFalse(entityMappedFromItemVersion.BoolDefaultFalseProperty);
                    Assert.AreEqual(DateTime.Today, entityMappedFromItemVersion.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(2005, 10, 21), entityMappedFromItemVersion.DateTimeProperty);
                    Assert.AreEqual("Text default value", entityMappedFromItemVersion.TextProperty);
                    Assert.AreEqual("Note default value", entityMappedFromItemVersion.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML default value</p>", entityMappedFromItemVersion.HtmlProperty);

                    Assert.IsNotNull(entityMappedFromItemVersion.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entityMappedFromItemVersion.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.UrlProperty.Url);
                    ////Assert.AreEqual("patate!", urlFieldVal.Description);     // proper Url description is missing because it can't be applied through SPField default value

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.UrlImageProperty.Url);
                    ////Assert.AreEqual("patate!", urlImageFieldVal.Description);     // proper Url description is missing because it can't be applied through SPField default value

                    Assert.AreEqual(1, entityMappedFromItemVersion.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromItemVersion.LookupProperty.Value);

                    Assert.AreEqual(2, entityMappedFromItemVersion.LookupAltProperty.Id);
                    Assert.AreEqual("2", entityMappedFromItemVersion.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, entityMappedFromItemVersion.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromItemVersion.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, entityMappedFromItemVersion.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", entityMappedFromItemVersion.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromItemVersion.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromItemVersion.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", entityMappedFromItemVersion.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", entityMappedFromItemVersion.MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entityMappedFromItemVersion.MediaProperty.Url);
                    Assert.IsTrue(entityMappedFromItemVersion.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entityMappedFromItemVersion.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entityMappedFromItemVersion.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entityMappedFromItemVersion.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entityMappedFromItemVersion.TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entityMappedFromItemVersion.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entityMappedFromItemVersion.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entityMappedFromItemVersion.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entityMappedFromItemVersion.TaxonomyMultiProperty[1].Label);

                    // #3 Validate straight list item collection to entity mappings
                    Assert.AreEqual(555, entitiesMappedFromItemCollection[0].IntegerProperty);
                    Assert.AreEqual(5.5, entitiesMappedFromItemCollection[0].DoubleProperty);
                    Assert.AreEqual(500.95, entitiesMappedFromItemCollection[0].CurrencyProperty);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].BoolProperty.HasValue);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].BoolDefaultTrueProperty);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].BoolDefaultFalseProperty);
                    Assert.AreEqual(DateTime.Today, entitiesMappedFromItemCollection[0].DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(2005, 10, 21), entitiesMappedFromItemCollection[0].DateTimeProperty);
                    Assert.AreEqual("Text default value", entitiesMappedFromItemCollection[0].TextProperty);
                    Assert.AreEqual("Note default value", entitiesMappedFromItemCollection[0].NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML default value</p>", entitiesMappedFromItemCollection[0].HtmlProperty);

                    Assert.IsNotNull(entitiesMappedFromItemCollection[0].ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entitiesMappedFromItemCollection[0].ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].UrlProperty.Url);
                    ////Assert.AreEqual("patate!", urlFieldVal.Description);     // proper Url description is missing because it can't be applied through SPField default value

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].UrlImageProperty.Url);
                    ////Assert.AreEqual("patate!", urlImageFieldVal.Description);     // proper Url description is missing because it can't be applied through SPField default value

                    // No lookups or User fields because DataRow formatting screws up lookup values (we lose the lookup IDs)
                    Assert.AreEqual("Some media file title", entitiesMappedFromItemCollection[0].MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entitiesMappedFromItemCollection[0].MediaProperty.Url);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entitiesMappedFromItemCollection[0].MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entitiesMappedFromItemCollection[0].TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entitiesMappedFromItemCollection[0].TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[1].Label);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that using the ISharePointEntityBinder to map and entity's properties from
        /// a list item which was created in a list where field definitions don't contain default
        /// values and where the list item was updated with field values through the SharePoint API.
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void ToEntity_WhenMappingFromListItem_AndItemPropertiesAreFilledWithValues_ShouldMapEntityWithAllItemValues()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey");

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                    "TestInternalNameBoolTrue",
                    new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                    "NameKeyBoolTrue",
                    "DescriptionKeyBoolTrue",
                    "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfoDefault = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey");

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey");

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey");

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey");

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(WindowsIdentity.GetCurrent().Name);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey");

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey");

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultTrue,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfoDefault,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo,
                        userFieldInfo,
                        userMultiFieldInfo,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;
                    lookupFieldInfoAlt.ListId = lookupList.ID;
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    list.EnableVersioning = true;
                    list.Update();

                    // Create item on list
                    var itemOnList = list.AddItem();

                    // Update with the field values through the SharePoint API
                    itemOnList["Title"] = "Item under test";
                    itemOnList["TestInternalNameInteger"] = 555;
                    itemOnList["TestInternalNameNumber"] = 5.5;
                    itemOnList["TestInternalNameCurrency"] = 500.95;
                    itemOnList["TestInternalNameBool"] = true;
                    itemOnList["TestInternalNameBoolTrue"] = false;
                    itemOnList["TestInternalNameBoolFalse"] = true;
                    itemOnList["TestInternalNameDateFormula"] = new DateTime(1977, 7, 7);
                    itemOnList["TestInternalNameDateDefault"] = new DateTime(1977, 7, 7);
                    itemOnList["TestInternalNameText"] = "Text value";
                    itemOnList["TestInternalNameNote"] = "Note value";
                    itemOnList["TestInternalNameHtml"] = "<p class=\"some-css-class\">HTML value</p>";
                    itemOnList["TestInternalNameImage"] = new ImageFieldValue()
                    {
                        Hyperlink = "http://github.com/GSoft-SharePoint/",
                        ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                    };
                    itemOnList["TestInternalNameUrl"] = new SPFieldUrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    };
                    itemOnList["TestInternalNameUrlImg"] = new SPFieldUrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    };

                    itemOnList["TestInternalNameLookup"] = new SPFieldLookupValue(1, "Test Item 1");
                    itemOnList["TestInternalNameLookupAlt"] = new SPFieldLookupValue(2, "2");
                    itemOnList["TestInternalNameLookupM"] = new SPFieldLookupValueCollection() { new SPFieldLookupValue(1, "Test Item 1"), new SPFieldLookupValue(2, "Test Item 2") };
                    itemOnList["TestInternalNameUser"] = new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name);
                    itemOnList["TestInternalNameUserMulti"] = new SPFieldUserValueCollection()
                        {
                            new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name),
                            new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser2.ID, ensuredUser2.Name)
                        };
                    itemOnList["TestInternalNameMedia"] = new MediaFieldValue()
                    {
                        Title = "Some media file title",
                        MediaSource = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                        AutoPlay = true,
                        Loop = true,
                        PreviewImageSource = "/_layouts/15/Images/logo.png"
                    };

                    var taxonomyField = (TaxonomyField)itemOnList.Fields.GetFieldByInternalName("TestInternalNameTaxo");
                    taxonomyField.SetFieldValue(itemOnList, createdTermB);

                    var taxonomyMultiField = (TaxonomyField)itemOnList.Fields.GetFieldByInternalName("TestInternalNameTaxoMulti");
                    taxonomyMultiField.SetFieldValue(itemOnList, new[] { createdTermAA, createdTermAB });

                    itemOnList.Update();

                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entityMappedFromSingleItem = new TestItemEntityWithLookups();
                    var entityMappedFromItemVersion = new TestItemEntityWithLookups();

                    // Act

                    // Map from SPListItem
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromSingleItem, itemOnList);

                    // Map from SPListItemVersion
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromItemVersion, itemOnList.Versions[0]);

                    // Map from DataRow/SPListItemCollection
                    var entitiesMappedFromItemCollection = entityBinder.Get<TestItemEntity>(list.Items);

                    // Assert
                    // #1 Validate straight single list item to entity mappings
                    Assert.AreEqual(555, entityMappedFromSingleItem.IntegerProperty);
                    Assert.AreEqual(5.5, entityMappedFromSingleItem.DoubleProperty);
                    Assert.AreEqual(500.95, entityMappedFromSingleItem.CurrencyProperty);
                    Assert.IsTrue(entityMappedFromSingleItem.BoolProperty.Value);
                    Assert.IsFalse(entityMappedFromSingleItem.BoolDefaultTrueProperty);
                    Assert.IsTrue(entityMappedFromSingleItem.BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromSingleItem.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromSingleItem.DateTimeProperty);
                    Assert.AreEqual("Text value", entityMappedFromSingleItem.TextProperty);
                    Assert.AreEqual("Note value", entityMappedFromSingleItem.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", entityMappedFromSingleItem.HtmlProperty);

                    Assert.IsNotNull(entityMappedFromSingleItem.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entityMappedFromSingleItem.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.UrlProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromSingleItem.UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.UrlImageProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromSingleItem.UrlProperty.Description);

                    Assert.AreEqual(1, entityMappedFromSingleItem.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromSingleItem.LookupProperty.Value);

                    Assert.AreEqual(2, entityMappedFromSingleItem.LookupAltProperty.Id);
                    Assert.AreEqual("2", entityMappedFromSingleItem.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, entityMappedFromSingleItem.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromSingleItem.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, entityMappedFromSingleItem.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", entityMappedFromSingleItem.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromSingleItem.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromSingleItem.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", entityMappedFromSingleItem.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", entityMappedFromSingleItem.MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entityMappedFromSingleItem.MediaProperty.Url);
                    Assert.IsTrue(entityMappedFromSingleItem.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entityMappedFromSingleItem.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entityMappedFromSingleItem.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entityMappedFromSingleItem.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entityMappedFromSingleItem.TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entityMappedFromSingleItem.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entityMappedFromSingleItem.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entityMappedFromSingleItem.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entityMappedFromSingleItem.TaxonomyMultiProperty[1].Label);

                    // #2 Validate list item version mappings
                    Assert.AreEqual(555, entityMappedFromItemVersion.IntegerProperty);
                    Assert.AreEqual(5.5, entityMappedFromItemVersion.DoubleProperty);
                    Assert.AreEqual(500.95, entityMappedFromItemVersion.CurrencyProperty);
                    Assert.IsTrue(entityMappedFromItemVersion.BoolProperty.Value);
                    Assert.IsFalse(entityMappedFromItemVersion.BoolDefaultTrueProperty);
                    Assert.IsTrue(entityMappedFromItemVersion.BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromItemVersion.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromItemVersion.DateTimeProperty);
                    Assert.AreEqual("Text value", entityMappedFromItemVersion.TextProperty);
                    Assert.AreEqual("Note value", entityMappedFromItemVersion.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", entityMappedFromItemVersion.HtmlProperty);

                    Assert.IsNotNull(entityMappedFromItemVersion.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entityMappedFromItemVersion.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.UrlProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromItemVersion.UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.UrlImageProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromItemVersion.UrlProperty.Description);

                    Assert.AreEqual(1, entityMappedFromItemVersion.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromItemVersion.LookupProperty.Value);

                    Assert.AreEqual(2, entityMappedFromItemVersion.LookupAltProperty.Id);
                    Assert.AreEqual("2", entityMappedFromItemVersion.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, entityMappedFromItemVersion.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromItemVersion.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, entityMappedFromItemVersion.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", entityMappedFromItemVersion.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromItemVersion.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromItemVersion.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", entityMappedFromItemVersion.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", entityMappedFromItemVersion.MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entityMappedFromItemVersion.MediaProperty.Url);
                    Assert.IsTrue(entityMappedFromItemVersion.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entityMappedFromItemVersion.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entityMappedFromItemVersion.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entityMappedFromItemVersion.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entityMappedFromItemVersion.TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entityMappedFromItemVersion.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entityMappedFromItemVersion.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entityMappedFromItemVersion.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entityMappedFromItemVersion.TaxonomyMultiProperty[1].Label);

                    // #3 Validate straight list item collection to entity mappings
                    Assert.AreEqual(555, entitiesMappedFromItemCollection[0].IntegerProperty);
                    Assert.AreEqual(5.5, entitiesMappedFromItemCollection[0].DoubleProperty);
                    Assert.AreEqual(500.95, entitiesMappedFromItemCollection[0].CurrencyProperty);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].BoolProperty.Value);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].BoolDefaultTrueProperty);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entitiesMappedFromItemCollection[0].DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entitiesMappedFromItemCollection[0].DateTimeProperty);
                    Assert.AreEqual("Text value", entitiesMappedFromItemCollection[0].TextProperty);
                    Assert.AreEqual("Note value", entitiesMappedFromItemCollection[0].NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", entitiesMappedFromItemCollection[0].HtmlProperty);

                    Assert.IsNotNull(entitiesMappedFromItemCollection[0].ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entitiesMappedFromItemCollection[0].ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].UrlProperty.Url);
                    Assert.AreEqual("patate!", entitiesMappedFromItemCollection[0].UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].UrlImageProperty.Url);
                    Assert.AreEqual("patate!", entitiesMappedFromItemCollection[0].UrlImageProperty.Description);

                    // No lookups or User fields because DataRow formatting screws up lookup values (we lose the lookup IDs)
                    Assert.AreEqual("Some media file title", entitiesMappedFromItemCollection[0].MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entitiesMappedFromItemCollection[0].MediaProperty.Url);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entitiesMappedFromItemCollection[0].MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entitiesMappedFromItemCollection[0].TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entitiesMappedFromItemCollection[0].TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[1].Label);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that using the ISharePointEntityBinder to map and entity's properties from
        /// an item which was created in a document library where field definitions don't contain default
        /// values and where the list item was updated with field values through the SharePoint API.
        /// </summary>
        [TestMethod]
        public void ToEntity_WhenMappingFromDocumentLibraryItem_AndItemPropertiesAreFilledWithValues_ShouldMapEntityWithAllDocumentValues()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey");

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                    "TestInternalNameBoolTrue",
                    new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                    "NameKeyBoolTrue",
                    "DescriptionKeyBoolTrue",
                    "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfoDefault = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey");

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey");

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey");

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey");

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(WindowsIdentity.GetCurrent().Name);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey");

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey");

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultTrue,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfoDefault,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo,
                        userFieldInfo,
                        userMultiFieldInfo,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;
                    lookupFieldInfoAlt.ListId = lookupList.ID;
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    list.EnableVersioning = true;
                    list.Update();

                    // Create item on list (Upload an empty file in root folder)
                    var fileInLib = list.RootFolder.Files.Add("SomeRootFile.txt", new byte[0]);
                    fileInLib.Update();
                    var itemOnList = fileInLib.Item;

                    // Update with the field values through the SharePoint API
                    itemOnList["Title"] = "Item under test";
                    itemOnList["TestInternalNameInteger"] = 555;
                    itemOnList["TestInternalNameNumber"] = 5.5;
                    itemOnList["TestInternalNameCurrency"] = 500.95;
                    itemOnList["TestInternalNameBool"] = true;
                    itemOnList["TestInternalNameBoolTrue"] = false;
                    itemOnList["TestInternalNameBoolFalse"] = true;
                    itemOnList["TestInternalNameDateFormula"] = new DateTime(1977, 7, 7);
                    itemOnList["TestInternalNameDateDefault"] = new DateTime(1977, 7, 7);
                    itemOnList["TestInternalNameText"] = "Text value";
                    itemOnList["TestInternalNameNote"] = "Note value";
                    itemOnList["TestInternalNameHtml"] = "<p class=\"some-css-class\">HTML value</p>";
                    itemOnList["TestInternalNameImage"] = new ImageFieldValue()
                    {
                        Hyperlink = "http://github.com/GSoft-SharePoint/",
                        ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                    };
                    itemOnList["TestInternalNameUrl"] = new SPFieldUrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    };
                    itemOnList["TestInternalNameUrlImg"] = new SPFieldUrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    };

                    itemOnList["TestInternalNameLookup"] = new SPFieldLookupValue(1, "Test Item 1");
                    itemOnList["TestInternalNameLookupAlt"] = new SPFieldLookupValue(2, "2");
                    itemOnList["TestInternalNameLookupM"] = new SPFieldLookupValueCollection() { new SPFieldLookupValue(1, "Test Item 1"), new SPFieldLookupValue(2, "Test Item 2") };
                    itemOnList["TestInternalNameUser"] = new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name);
                    itemOnList["TestInternalNameUserMulti"] = new SPFieldUserValueCollection()
                        {
                            new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name),
                            new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser2.ID, ensuredUser2.Name)
                        };
                    itemOnList["TestInternalNameMedia"] = new MediaFieldValue()
                    {
                        Title = "Some media file title",
                        MediaSource = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                        AutoPlay = true,
                        Loop = true,
                        PreviewImageSource = "/_layouts/15/Images/logo.png"
                    };

                    var taxonomyField = (TaxonomyField)itemOnList.Fields.GetFieldByInternalName("TestInternalNameTaxo");
                    taxonomyField.SetFieldValue(itemOnList, createdTermB);

                    var taxonomyMultiField = (TaxonomyField)itemOnList.Fields.GetFieldByInternalName("TestInternalNameTaxoMulti");
                    taxonomyMultiField.SetFieldValue(itemOnList, new[] { createdTermAA, createdTermAB });

                    itemOnList.Update();

                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entityMappedFromSingleItem = new TestItemEntityWithLookups();
                    var entityMappedFromItemVersion = new TestItemEntityWithLookups();

                    // Act

                    // Map from SPListItem
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromSingleItem, itemOnList);

                    // Map from SPListItemVersion
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromItemVersion, itemOnList.Versions[0]);

                    // Map from DataRow/SPListItemCollection
                    var entitiesMappedFromItemCollection = entityBinder.Get<TestItemEntity>(list.Items);

                    // Assert
                    // #1 Validate straight single list item to entity mappings
                    Assert.AreEqual(555, entityMappedFromSingleItem.IntegerProperty);
                    Assert.AreEqual(5.5, entityMappedFromSingleItem.DoubleProperty);
                    Assert.AreEqual(500.95, entityMappedFromSingleItem.CurrencyProperty);
                    Assert.IsTrue(entityMappedFromSingleItem.BoolProperty.Value);
                    Assert.IsFalse(entityMappedFromSingleItem.BoolDefaultTrueProperty);
                    Assert.IsTrue(entityMappedFromSingleItem.BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromSingleItem.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromSingleItem.DateTimeProperty);
                    Assert.AreEqual("Text value", entityMappedFromSingleItem.TextProperty);
                    Assert.AreEqual("Note value", entityMappedFromSingleItem.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", entityMappedFromSingleItem.HtmlProperty);

                    Assert.IsNotNull(entityMappedFromSingleItem.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entityMappedFromSingleItem.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.UrlProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromSingleItem.UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.UrlImageProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromSingleItem.UrlProperty.Description);

                    Assert.AreEqual(1, entityMappedFromSingleItem.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromSingleItem.LookupProperty.Value);

                    Assert.AreEqual(2, entityMappedFromSingleItem.LookupAltProperty.Id);
                    Assert.AreEqual("2", entityMappedFromSingleItem.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, entityMappedFromSingleItem.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromSingleItem.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, entityMappedFromSingleItem.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", entityMappedFromSingleItem.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromSingleItem.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromSingleItem.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", entityMappedFromSingleItem.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", entityMappedFromSingleItem.MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entityMappedFromSingleItem.MediaProperty.Url);
                    Assert.IsTrue(entityMappedFromSingleItem.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entityMappedFromSingleItem.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entityMappedFromSingleItem.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entityMappedFromSingleItem.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entityMappedFromSingleItem.TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entityMappedFromSingleItem.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entityMappedFromSingleItem.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entityMappedFromSingleItem.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entityMappedFromSingleItem.TaxonomyMultiProperty[1].Label);

                    // #2 Validate list item version mappings
                    Assert.AreEqual(555, entityMappedFromItemVersion.IntegerProperty);
                    Assert.AreEqual(5.5, entityMappedFromItemVersion.DoubleProperty);
                    Assert.AreEqual(500.95, entityMappedFromItemVersion.CurrencyProperty);
                    Assert.IsTrue(entityMappedFromItemVersion.BoolProperty.Value);
                    Assert.IsFalse(entityMappedFromItemVersion.BoolDefaultTrueProperty);
                    Assert.IsTrue(entityMappedFromItemVersion.BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromItemVersion.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromItemVersion.DateTimeProperty);
                    Assert.AreEqual("Text value", entityMappedFromItemVersion.TextProperty);
                    Assert.AreEqual("Note value", entityMappedFromItemVersion.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", entityMappedFromItemVersion.HtmlProperty);

                    Assert.IsNotNull(entityMappedFromItemVersion.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entityMappedFromItemVersion.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.UrlProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromItemVersion.UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.UrlImageProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromItemVersion.UrlProperty.Description);

                    Assert.AreEqual(1, entityMappedFromItemVersion.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromItemVersion.LookupProperty.Value);

                    Assert.AreEqual(2, entityMappedFromItemVersion.LookupAltProperty.Id);
                    Assert.AreEqual("2", entityMappedFromItemVersion.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, entityMappedFromItemVersion.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromItemVersion.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, entityMappedFromItemVersion.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", entityMappedFromItemVersion.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromItemVersion.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromItemVersion.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", entityMappedFromItemVersion.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", entityMappedFromItemVersion.MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entityMappedFromItemVersion.MediaProperty.Url);
                    Assert.IsTrue(entityMappedFromItemVersion.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entityMappedFromItemVersion.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entityMappedFromItemVersion.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entityMappedFromItemVersion.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entityMappedFromItemVersion.TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entityMappedFromItemVersion.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entityMappedFromItemVersion.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entityMappedFromItemVersion.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entityMappedFromItemVersion.TaxonomyMultiProperty[1].Label);

                    // #3 Validate straight list item collection to entity mappings
                    Assert.AreEqual(555, entitiesMappedFromItemCollection[0].IntegerProperty);
                    Assert.AreEqual(5.5, entitiesMappedFromItemCollection[0].DoubleProperty);
                    Assert.AreEqual(500.95, entitiesMappedFromItemCollection[0].CurrencyProperty);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].BoolProperty.Value);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].BoolDefaultTrueProperty);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entitiesMappedFromItemCollection[0].DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entitiesMappedFromItemCollection[0].DateTimeProperty);
                    Assert.AreEqual("Text value", entitiesMappedFromItemCollection[0].TextProperty);
                    Assert.AreEqual("Note value", entitiesMappedFromItemCollection[0].NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", entitiesMappedFromItemCollection[0].HtmlProperty);

                    Assert.IsNotNull(entitiesMappedFromItemCollection[0].ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entitiesMappedFromItemCollection[0].ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].UrlProperty.Url);
                    Assert.AreEqual("patate!", entitiesMappedFromItemCollection[0].UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].UrlImageProperty.Url);
                    Assert.AreEqual("patate!", entitiesMappedFromItemCollection[0].UrlImageProperty.Description);

                    // No lookups or User fields because DataRow formatting screws up lookup values (we lose the lookup IDs)
                    Assert.AreEqual("Some media file title", entitiesMappedFromItemCollection[0].MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entitiesMappedFromItemCollection[0].MediaProperty.Url);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entitiesMappedFromItemCollection[0].MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entitiesMappedFromItemCollection[0].TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entitiesMappedFromItemCollection[0].TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[1].Label);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that using the ISharePointEntityBinder to map and entity's properties from
        /// an item which was created in a Pages library where field definitions don't contain default
        /// values and where the list item was created through a PageInfo object definition.
        /// </summary>
        [TestMethod]
        public void ToEntity_WhenMappingFromPagesLibraryItem_AndItemPropertiesAreFilledWithValues_ShouldMapEntityWithAllPageValues()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey");

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                    "TestInternalNameBoolTrue",
                    new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                    "NameKeyBoolTrue",
                    "DescriptionKeyBoolTrue",
                    "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfoDefault = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey");

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey");

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey");

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey");

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(WindowsIdentity.GetCurrent().Name);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey");

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey");

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultTrue,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfoDefault,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo,
                        userFieldInfo,
                        userMultiFieldInfo,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ContentTypeInfo pageContentType = new ContentTypeInfo(
                    ContentTypeIdBuilder.CreateChild(ContentTypeId.ArticlePage, Guid.NewGuid()),
                    "PageCTNameKey",
                    "PageCTDescrKey",
                    "ContentGroupKey")
                {
                    Fields = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var folderHelper = injectionScope.Resolve<IFolderHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;
                    lookupFieldInfoAlt.ListId = lookupList.ID;
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Get the Pages library and add our CT to it
                    SPList list = testScope.SiteCollection.RootWeb.GetPagesLibrary();
                    list.EnableVersioning = true;
                    list.Update();

                    SPContentType myEnsuredContentType = contentTypeHelper.EnsureContentType(list.ContentTypes, pageContentType);

                    // Create page at root of pages lib
                    var masterPageGallery = testScope.SiteCollection.RootWeb.GetCatalog(SPListTemplateType.MasterPageCatalog);
                    var item = masterPageGallery.Items.Cast<SPListItem>().First(i => i.Name == "ArticleLeft.aspx");
                    var serverRelativePageUrl = SPUtility.ConcatUrls(masterPageGallery.RootFolder.ServerRelativeUrl, "MyArticleLeftTest.aspx");
                    item.CopyTo(SPUtility.ConcatUrls(testScope.SiteCollection.Url, serverRelativePageUrl)); // assumes site collection doesn't have managed path (i.e. we're dealing with a host-name site coll.)

                    PublishingWeb publishingWeb = PublishingWeb.GetPublishingWeb(testScope.SiteCollection.RootWeb);
                    PageLayout pageLayout = publishingWeb.GetAvailablePageLayouts().First(pl => pl.Name == "MyArticleLeftTest.aspx");
                    pageLayout.AssociatedContentType = myEnsuredContentType;

                    var myArticleLeftPageLayout = new PageLayoutInfo("MyArticleLeftTest.aspx", myEnsuredContentType.Id);
                    folderHelper.EnsureFolderHierarchy(
                        list,
                            new FolderInfo("Root")
                            {
                                Pages = new[]
                                {
                                new PageInfo("MyTestPage", myArticleLeftPageLayout)
                                {
                                    FieldValues = new[]
                                    {
                                        new FieldValueInfo(integerFieldInfo, 555),
                                        new FieldValueInfo(numberFieldInfo, 5.5),
                                        new FieldValueInfo(currencyFieldInfo, 500.95),
                                        new FieldValueInfo(boolFieldInfoBasic, true),
                                        new FieldValueInfo(boolFieldInfoDefaultTrue, false),
                                        new FieldValueInfo(boolFieldInfoDefaultFalse, true),
                                        new FieldValueInfo(dateTimeFieldInfoFormula, new DateTime(1977, 7, 7)),
                                        new FieldValueInfo(dateTimeFieldInfoDefault, new DateTime(1977, 7, 7)),
                                        new FieldValueInfo(textFieldInfo, "Text value"),
                                        new FieldValueInfo(noteFieldInfo, "Note value"),
                                        new FieldValueInfo(htmlFieldInfo, "<p class=\"some-css-class\">HTML value</p>"),
                                        new FieldValueInfo(
                                            imageFieldInfo,
                                            new ImageValue()
                                            {
                                                Hyperlink = "http://github.com/GSoft-SharePoint/",
                                                ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                                            }),
                                        new FieldValueInfo(
                                            urlFieldInfo,
                                            new UrlValue()
                                            {
                                                Url = "http://github.com/GSoft-SharePoint/",
                                                Description = "patate!"
                                            }),
                                        new FieldValueInfo(
                                            urlFieldInfoImage,
                                            new UrlValue()
                                            {
                                                Url = "http://github.com/GSoft-SharePoint/",
                                                Description = "patate!"
                                            }),
                                        new FieldValueInfo(lookupFieldInfo, new LookupValue(1, "Test Item 1")),
                                        new FieldValueInfo(lookupFieldInfoAlt, new LookupValue(2, "2")),
                                        new FieldValueInfo(lookupMultiFieldInfo, new LookupValueCollection() { new LookupValue(1, "Test Item 1"), new LookupValue(2, "Test Item 2") }),
                                        new FieldValueInfo(userFieldInfo, new UserValue(ensuredUser1)),
                                        new FieldValueInfo(userMultiFieldInfo, new UserValueCollection(new[] { new UserValue(ensuredUser1), new UserValue(ensuredUser2) })),
                                        new FieldValueInfo(
                                            mediaFieldInfo,
                                            new MediaValue()
                                            {
                                                Title = "Some media file title",
                                                Url = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                                                IsAutoPlay = true,
                                                IsLoop = true,
                                                PreviewImageUrl = "/_layouts/15/Images/logo.png"
                                            }),
                                        new FieldValueInfo(taxoFieldInfo, new TaxonomyValue(createdTermB)),
                                        new FieldValueInfo(taxoMultiFieldInfo, new TaxonomyValueCollection(new[] { createdTermAA, createdTermAB }))
                                    }
                                }
                            }
                            });

                    var pages = publishingWeb.GetPublishingPages();
                    var testPage = pages.Single(p => p.Title == "MyTestPage");

                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entityMappedFromSingleItem = new TestItemEntityWithLookups();
                    var entityMappedFromItemVersion = new TestItemEntityWithLookups();

                    // Act

                    // Map from SPListItem
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromSingleItem, testPage.ListItem);

                    // Map from SPListItemVersion
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromItemVersion, testPage.ListItem.Versions[0]);

                    // Map from DataRow/SPListItemCollection
                    var pagesLibrary = testScope.SiteCollection.RootWeb.GetPagesLibrary();
                    var caml = injectionScope.Resolve<ICamlBuilder>();
                    var items = pagesLibrary.GetItems(new SPQuery() { Query = caml.Where(caml.Equal(caml.FieldRef("Title"), caml.Value("MyTestPage"))) });
                    var entitiesMappedFromItemCollection = entityBinder.Get<TestItemEntity>(items);

                    // Assert
                    // #1 Validate straight single list item to entity mappings
                    Assert.AreEqual(555, entityMappedFromSingleItem.IntegerProperty);
                    Assert.AreEqual(5.5, entityMappedFromSingleItem.DoubleProperty);
                    Assert.AreEqual(500.95, entityMappedFromSingleItem.CurrencyProperty);
                    Assert.IsTrue(entityMappedFromSingleItem.BoolProperty.Value);
                    Assert.IsFalse(entityMappedFromSingleItem.BoolDefaultTrueProperty);
                    Assert.IsTrue(entityMappedFromSingleItem.BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromSingleItem.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromSingleItem.DateTimeProperty);
                    Assert.AreEqual("Text value", entityMappedFromSingleItem.TextProperty);
                    Assert.AreEqual("Note value", entityMappedFromSingleItem.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", entityMappedFromSingleItem.HtmlProperty);

                    Assert.IsNotNull(entityMappedFromSingleItem.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entityMappedFromSingleItem.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.UrlProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromSingleItem.UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromSingleItem.UrlImageProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromSingleItem.UrlProperty.Description);

                    Assert.AreEqual(1, entityMappedFromSingleItem.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromSingleItem.LookupProperty.Value);

                    Assert.AreEqual(2, entityMappedFromSingleItem.LookupAltProperty.Id);
                    Assert.AreEqual("2", entityMappedFromSingleItem.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, entityMappedFromSingleItem.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromSingleItem.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, entityMappedFromSingleItem.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", entityMappedFromSingleItem.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromSingleItem.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromSingleItem.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", entityMappedFromSingleItem.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", entityMappedFromSingleItem.MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entityMappedFromSingleItem.MediaProperty.Url);
                    Assert.IsTrue(entityMappedFromSingleItem.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entityMappedFromSingleItem.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entityMappedFromSingleItem.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entityMappedFromSingleItem.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entityMappedFromSingleItem.TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entityMappedFromSingleItem.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entityMappedFromSingleItem.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entityMappedFromSingleItem.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entityMappedFromSingleItem.TaxonomyMultiProperty[1].Label);

                    // #2 Validate list item version mappings
                    Assert.AreEqual(555, entityMappedFromItemVersion.IntegerProperty);
                    Assert.AreEqual(5.5, entityMappedFromItemVersion.DoubleProperty);
                    Assert.AreEqual(500.95, entityMappedFromItemVersion.CurrencyProperty);
                    Assert.IsTrue(entityMappedFromItemVersion.BoolProperty.Value);
                    Assert.IsFalse(entityMappedFromItemVersion.BoolDefaultTrueProperty);
                    Assert.IsTrue(entityMappedFromItemVersion.BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromItemVersion.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entityMappedFromItemVersion.DateTimeProperty);
                    Assert.AreEqual("Text value", entityMappedFromItemVersion.TextProperty);
                    Assert.AreEqual("Note value", entityMappedFromItemVersion.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", entityMappedFromItemVersion.HtmlProperty);

                    Assert.IsNotNull(entityMappedFromItemVersion.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entityMappedFromItemVersion.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.UrlProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromItemVersion.UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entityMappedFromItemVersion.UrlImageProperty.Url);
                    Assert.AreEqual("patate!", entityMappedFromItemVersion.UrlProperty.Description);

                    Assert.AreEqual(1, entityMappedFromItemVersion.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromItemVersion.LookupProperty.Value);

                    Assert.AreEqual(2, entityMappedFromItemVersion.LookupAltProperty.Id);
                    Assert.AreEqual("2", entityMappedFromItemVersion.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, entityMappedFromItemVersion.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", entityMappedFromItemVersion.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, entityMappedFromItemVersion.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", entityMappedFromItemVersion.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromItemVersion.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, entityMappedFromItemVersion.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", entityMappedFromItemVersion.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", entityMappedFromItemVersion.MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entityMappedFromItemVersion.MediaProperty.Url);
                    Assert.IsTrue(entityMappedFromItemVersion.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entityMappedFromItemVersion.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entityMappedFromItemVersion.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entityMappedFromItemVersion.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entityMappedFromItemVersion.TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entityMappedFromItemVersion.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entityMappedFromItemVersion.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entityMappedFromItemVersion.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entityMappedFromItemVersion.TaxonomyMultiProperty[1].Label);

                    // #3 Validate straight list item collection to entity mappings
                    Assert.AreEqual(555, entitiesMappedFromItemCollection[0].IntegerProperty);
                    Assert.AreEqual(5.5, entitiesMappedFromItemCollection[0].DoubleProperty);
                    Assert.AreEqual(500.95, entitiesMappedFromItemCollection[0].CurrencyProperty);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].BoolProperty.Value);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].BoolDefaultTrueProperty);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entitiesMappedFromItemCollection[0].DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), entitiesMappedFromItemCollection[0].DateTimeProperty);
                    Assert.AreEqual("Text value", entitiesMappedFromItemCollection[0].TextProperty);
                    Assert.AreEqual("Note value", entitiesMappedFromItemCollection[0].NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", entitiesMappedFromItemCollection[0].HtmlProperty);

                    Assert.IsNotNull(entitiesMappedFromItemCollection[0].ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", entitiesMappedFromItemCollection[0].ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].UrlProperty.Url);
                    Assert.AreEqual("patate!", entitiesMappedFromItemCollection[0].UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", entitiesMappedFromItemCollection[0].UrlImageProperty.Url);
                    Assert.AreEqual("patate!", entitiesMappedFromItemCollection[0].UrlImageProperty.Description);

                    // No lookups or User fields because DataRow formatting screws up lookup values (we lose the lookup IDs)
                    Assert.AreEqual("Some media file title", entitiesMappedFromItemCollection[0].MediaProperty.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), entitiesMappedFromItemCollection[0].MediaProperty.Url);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].MediaProperty.IsAutoPlay);
                    Assert.IsTrue(entitiesMappedFromItemCollection[0].MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", entitiesMappedFromItemCollection[0].MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, entitiesMappedFromItemCollection[0].TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, entitiesMappedFromItemCollection[0].TaxonomyProperty.Label);

                    Assert.AreEqual(levelTwoTermAA.Id, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, entitiesMappedFromItemCollection[0].TaxonomyMultiProperty[1].Label);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that entities with non-nullable primitive and struct-based properties
        /// get their properly initialized from NULL SPListItem values
        /// </summary>
        [TestMethod]
        public void ToEntity_WhenAllValuesInListItemAreNull_AndEntityTypeHasNonNullablePrimitiveValues_ShouldInitializeEntityWithPrimitiveTypeDefaults()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey");

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                DateTimeFieldInfo dateTimeFieldInfo = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey");

                GuidFieldInfo guidFieldInfo = new GuidFieldInfo(
                    "TestInternalNameGuid",
                    new Guid("{308C0899-DDBE-44EE-A8A8-226F7A6A7C89}"),
                    "NameKeyGuid",
                    "DescriptionKeyGuid",
                    "GroupKey");

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        dateTimeFieldInfo,
                        guidFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Create the  test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    list.EnableVersioning = true;
                    list.Update();

                    // Create an empty item
                    var itemOnList = list.AddItem();
                    itemOnList.Update();

                    // Act
                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entityMappedFromSingleItem = new TestItemEntityWithNonNullablePrimitivesOnly();
                    var entityMappedFromItemVersion = new TestItemEntityWithNonNullablePrimitivesOnly();

                    // Act

                    // Map from SPListItem
                    entityBinder.ToEntity<TestItemEntityWithNonNullablePrimitivesOnly>(entityMappedFromSingleItem, itemOnList);

                    // Map from SPListItemVersion
                    entityBinder.ToEntity<TestItemEntityWithNonNullablePrimitivesOnly>(entityMappedFromItemVersion, itemOnList.Versions[0]);

                    // Map from DataRow/SPListItemCollection
                    var entitiesMappedFromItemCollection = entityBinder.Get<TestItemEntityWithNonNullablePrimitivesOnly>(list.Items);

                    // Assert

                    // #1 Check SPListItem-mapped values
                    Assert.AreEqual(0, entityMappedFromSingleItem.IntegerProperty);
                    Assert.AreEqual(0.0, entityMappedFromSingleItem.DoubleProperty);
                    Assert.AreEqual(0.0, entityMappedFromSingleItem.CurrencyProperty);
                    Assert.IsFalse(entityMappedFromSingleItem.BoolProperty);
                    Assert.AreEqual(DateTime.MinValue, entityMappedFromSingleItem.DateTimeProperty);
                    Assert.AreEqual(Guid.Empty, entityMappedFromSingleItem.GuidProperty);

                    // #2 Check SPListItemVersion-mapped values
                    Assert.AreEqual(0, entityMappedFromItemVersion.IntegerProperty);
                    Assert.AreEqual(0.0, entityMappedFromItemVersion.DoubleProperty);
                    Assert.AreEqual(0.0, entityMappedFromItemVersion.CurrencyProperty);
                    Assert.IsFalse(entityMappedFromItemVersion.BoolProperty);
                    Assert.AreEqual(DateTime.MinValue, entityMappedFromItemVersion.DateTimeProperty);
                    Assert.AreEqual(Guid.Empty, entityMappedFromItemVersion.GuidProperty);

                    // #1 Check SPListItemCollection/DataRow-mapped values
                    Assert.AreEqual(0, entitiesMappedFromItemCollection[0].IntegerProperty);
                    Assert.AreEqual(0.0, entitiesMappedFromItemCollection[0].DoubleProperty);
                    Assert.AreEqual(0.0, entitiesMappedFromItemCollection[0].CurrencyProperty);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].BoolProperty);
                    Assert.AreEqual(DateTime.MinValue, entitiesMappedFromItemCollection[0].DateTimeProperty);
                    Assert.AreEqual(Guid.Empty, entitiesMappedFromItemCollection[0].GuidProperty);
                }
            }
        }

        /// <summary>
        /// Validates that entities with nullable primitive and struct-based properties
        /// get their properly initialized from NULL SPListItem values
        /// </summary>
        [TestMethod]
        public void ToEntity_WhenAllValuesInListItemAreNull_AndEntityTypeHasNullablePrimitiveValues_ShouldInitializeEntityWithEmptyNullable()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey");

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                DateTimeFieldInfo dateTimeFieldInfo = new DateTimeFieldInfo(
                    "TestInternalNameDate",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey");

                GuidFieldInfo guidFieldInfo = new GuidFieldInfo(
                    "TestInternalNameGuid",
                    new Guid("{308C0899-DDBE-44EE-A8A8-226F7A6A7C89}"),
                    "NameKeyGuid",
                    "DescriptionKeyGuid",
                    "GroupKey");

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        dateTimeFieldInfo,
                        guidFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Create the  test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    list.EnableVersioning = true;
                    list.Update();

                    // Create an empty item
                    var itemOnList = list.AddItem();
                    itemOnList.Update();

                    // Act
                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entityMappedFromSingleItem = new TestItemEntityWithNullablePrimitivesOnly();
                    var entityMappedFromItemVersion = new TestItemEntityWithNullablePrimitivesOnly();

                    // Act

                    // Map from SPListItem
                    entityBinder.ToEntity<TestItemEntityWithNullablePrimitivesOnly>(entityMappedFromSingleItem, itemOnList);

                    // Map from SPListItemVersion
                    entityBinder.ToEntity<TestItemEntityWithNullablePrimitivesOnly>(entityMappedFromItemVersion, itemOnList.Versions[0]);

                    // Map from DataRow/SPListItemCollection
                    var entitiesMappedFromItemCollection = entityBinder.Get<TestItemEntityWithNullablePrimitivesOnly>(list.Items);

                    // Assert

                    // #1 Check SPListItem-mapped values
                    Assert.IsFalse(entityMappedFromSingleItem.IntegerProperty.HasValue);
                    Assert.IsFalse(entityMappedFromSingleItem.DoubleProperty.HasValue);
                    Assert.IsFalse(entityMappedFromSingleItem.CurrencyProperty.HasValue);
                    Assert.IsFalse(entityMappedFromSingleItem.BoolProperty.HasValue);
                    Assert.IsFalse(entityMappedFromSingleItem.DateTimeProperty.HasValue);
                    Assert.IsFalse(entityMappedFromSingleItem.GuidProperty.HasValue);

                    // #2 Check SPListItemVersion-mapped values
                    Assert.IsFalse(entityMappedFromItemVersion.IntegerProperty.HasValue);
                    Assert.IsFalse(entityMappedFromItemVersion.DoubleProperty.HasValue);
                    Assert.IsFalse(entityMappedFromItemVersion.CurrencyProperty.HasValue);
                    Assert.IsFalse(entityMappedFromItemVersion.BoolProperty.HasValue);
                    Assert.IsFalse(entityMappedFromItemVersion.DateTimeProperty.HasValue);
                    Assert.IsFalse(entityMappedFromItemVersion.GuidProperty.HasValue);

                    // #1 Check SPListItemCollection/DataRow-mapped values
                    // Note that the DataRow item conversion prevents us from distinguishing 
                    // empty integer value from 0.
                    Assert.AreEqual(0, entitiesMappedFromItemCollection[0].IntegerProperty);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].DoubleProperty.HasValue);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].CurrencyProperty.HasValue);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].BoolProperty.HasValue);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].DateTimeProperty.HasValue);
                    Assert.IsFalse(entitiesMappedFromItemCollection[0].GuidProperty.HasValue);
                }
            }
        }

        /// <summary>
        /// Serialization test: validates that all value types serialize to/from JSON properly
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void ToEntity_AllValueTypesShouldBeEasyToSerializeAndDeserialize()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey");

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                    "TestInternalNameBoolTrue",
                    new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                    "NameKeyBoolTrue",
                    "DescriptionKeyBoolTrue",
                    "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfoDefault = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey");

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey");

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey");

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey");

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(WindowsIdentity.GetCurrent().Name);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey");

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey");

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultTrue,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfoDefault,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo,
                        userFieldInfo,
                        userMultiFieldInfo,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;
                    lookupFieldInfoAlt.ListId = lookupList.ID;
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    list.EnableVersioning = true;
                    list.Update();

                    // Create item on list (Upload an empty file in root folder)
                    var fileInLib = list.RootFolder.Files.Add("SomeRootFile.txt", new byte[0]);
                    fileInLib.Update();
                    var itemOnList = fileInLib.Item;

                    // Update with the field values through the SharePoint API
                    itemOnList["Title"] = "Item under test";
                    itemOnList["TestInternalNameInteger"] = 555;
                    itemOnList["TestInternalNameNumber"] = 5.5;
                    itemOnList["TestInternalNameCurrency"] = 500.95;
                    itemOnList["TestInternalNameBool"] = true;
                    itemOnList["TestInternalNameBoolTrue"] = false;
                    itemOnList["TestInternalNameBoolFalse"] = true;
                    itemOnList["TestInternalNameDateFormula"] = new DateTime(1977, 7, 7);
                    itemOnList["TestInternalNameDateDefault"] = new DateTime(1977, 7, 7);
                    itemOnList["TestInternalNameText"] = "Text value";
                    itemOnList["TestInternalNameNote"] = "Note value";
                    itemOnList["TestInternalNameHtml"] = "<p class=\"some-css-class\">HTML value</p>";
                    itemOnList["TestInternalNameImage"] = new ImageFieldValue()
                    {
                        Hyperlink = "http://github.com/GSoft-SharePoint/",
                        ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                    };
                    itemOnList["TestInternalNameUrl"] = new SPFieldUrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    };
                    itemOnList["TestInternalNameUrlImg"] = new SPFieldUrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    };

                    itemOnList["TestInternalNameLookup"] = new SPFieldLookupValue(1, "Test Item 1");
                    itemOnList["TestInternalNameLookupAlt"] = new SPFieldLookupValue(2, "2");
                    itemOnList["TestInternalNameLookupM"] = new SPFieldLookupValueCollection() { new SPFieldLookupValue(1, "Test Item 1"), new SPFieldLookupValue(2, "Test Item 2") };
                    itemOnList["TestInternalNameUser"] = new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name);
                    itemOnList["TestInternalNameUserMulti"] = new SPFieldUserValueCollection()
                        {
                            new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name),
                            new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser2.ID, ensuredUser2.Name)
                        };
                    itemOnList["TestInternalNameMedia"] = new MediaFieldValue()
                    {
                        Title = "Some media file title",
                        MediaSource = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                        AutoPlay = true,
                        Loop = true,
                        PreviewImageSource = "/_layouts/15/Images/logo.png"
                    };

                    var taxonomyField = (TaxonomyField)itemOnList.Fields.GetFieldByInternalName("TestInternalNameTaxo");
                    taxonomyField.SetFieldValue(itemOnList, createdTermB);

                    var taxonomyMultiField = (TaxonomyField)itemOnList.Fields.GetFieldByInternalName("TestInternalNameTaxoMulti");
                    taxonomyMultiField.SetFieldValue(itemOnList, new[] { createdTermAA, createdTermAB });

                    itemOnList.Update();

                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var serializer = injectionScope.Resolve<ISerializer>();

                    // Act

                    // Map from SPListItem then JSON serialize/deserialize
                    var entityMapped = entityBinder.Get<TestItemEntityWithLookups>(itemOnList);
                    string serializedRepresentation = serializer.Serialize(entityMapped);
                    var deserializedObject = serializer.Deserialize<TestItemEntityWithLookups>(serializedRepresentation);

                    // Assert
                    // #1 Validate that deserialized object contains all field values
                    Assert.AreEqual("Item under test", deserializedObject.Title);
                    Assert.AreEqual(entityMapped.IntegerProperty, deserializedObject.IntegerProperty);
                    Assert.AreEqual(5.5, deserializedObject.DoubleProperty);
                    Assert.AreEqual(500.95, deserializedObject.CurrencyProperty);
                    Assert.IsTrue(deserializedObject.BoolProperty.Value);
                    Assert.IsFalse(deserializedObject.BoolDefaultTrueProperty);
                    Assert.IsTrue(deserializedObject.BoolDefaultFalseProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), deserializedObject.DateTimeFormulaProperty);
                    Assert.AreEqual(new DateTime(1977, 7, 7), deserializedObject.DateTimeProperty);
                    Assert.AreEqual("Text value", deserializedObject.TextProperty);
                    Assert.AreEqual("Note value", deserializedObject.NoteProperty);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", deserializedObject.HtmlProperty);

                    Assert.IsNotNull(deserializedObject.ImageProperty);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", deserializedObject.ImageProperty.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", deserializedObject.ImageProperty.ImageUrl);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", deserializedObject.UrlProperty.Url);
                    Assert.AreEqual("patate!", deserializedObject.UrlProperty.Description);

                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", deserializedObject.UrlImageProperty.Url);
                    Assert.AreEqual("patate!", deserializedObject.UrlProperty.Description);

                    Assert.AreEqual(1, deserializedObject.LookupProperty.Id);
                    Assert.AreEqual("Test Item 1", deserializedObject.LookupProperty.Value);

                    Assert.AreEqual(2, deserializedObject.LookupAltProperty.Id);
                    Assert.AreEqual("2", deserializedObject.LookupAltProperty.Value); // ShowField/LookupField is ID

                    Assert.AreEqual(1, deserializedObject.LookupMultiProperty[0].Id);
                    Assert.AreEqual("Test Item 1", deserializedObject.LookupMultiProperty[0].Value);
                    Assert.AreEqual(2, deserializedObject.LookupMultiProperty[1].Id);
                    Assert.AreEqual("Test Item 2", deserializedObject.LookupMultiProperty[1].Value);

                    Assert.AreEqual(ensuredUser1.Name, deserializedObject.UserProperty.DisplayName);

                    Assert.AreEqual(ensuredUser1.Name, deserializedObject.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual("Maxime Boissonneault", deserializedObject.UserMultiProperty[1].DisplayName);

                    Assert.AreEqual("Some media file title", deserializedObject.MediaProperty.Title);
                    Assert.AreEqual(
                        HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"),
                        deserializedObject.MediaProperty.Url);
                    Assert.IsTrue(deserializedObject.MediaProperty.IsAutoPlay);
                    Assert.IsTrue(deserializedObject.MediaProperty.IsLoop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", deserializedObject.MediaProperty.PreviewImageUrl);

                    Assert.AreEqual(levelOneTermB.Id, deserializedObject.TaxonomyProperty.Id);
                    Assert.AreEqual(levelOneTermB.Label, deserializedObject.TaxonomyProperty.Label);

                    // TODO: Figure out the best way to get the TermSet context bound during entity binding
                    ////Assert.AreEqual(createdTermB.TermSet.Id, deserializedObject.TaxonomyProperty.Term.TermSet.Id);
                    ////Assert.AreEqual(createdTermB.TermSet.Name, deserializedObject.TaxonomyProperty.Term.TermSet.Label);

                    ////Assert.AreEqual(createdTermB.TermSet.Group.Id, deserializedObject.TaxonomyProperty.Term.TermSet.Group.Id);
                    ////Assert.AreEqual(createdTermB.TermSet.Group.Name, deserializedObject.TaxonomyProperty.Term.TermSet.Group.Name);

                    ////Assert.AreEqual(createdTermB.TermSet.Group.TermStore.Id, deserializedObject.TaxonomyProperty.Term.TermSet.Group.TermStore.Id);
                    ////Assert.AreEqual(createdTermB.TermSet.Group.TermStore.Name, deserializedObject.TaxonomyProperty.Term.TermSet.Group.TermStore.Name);

                    Assert.AreEqual(levelTwoTermAA.Id, deserializedObject.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(levelTwoTermAA.Label, deserializedObject.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(levelTwoTermAB.Id, deserializedObject.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(levelTwoTermAB.Label, deserializedObject.TaxonomyMultiProperty[1].Label);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that when you create an entity from a list item then use that entity
        /// to create another list item, then both list items end up with identical properties
        /// </summary>
        [TestMethod]
        public void ToEntityFromEntityRoundTrip_ShouldEndUpWithIdenticalListItems()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey");

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                    "TestInternalNameBoolTrue",
                    new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                    "NameKeyBoolTrue",
                    "DescriptionKeyBoolTrue",
                    "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfoDefault = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey");

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey");

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey");

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey");

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(WindowsIdentity.GetCurrent().Name);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey");

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey");

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultTrue,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfoDefault,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo,
                        userFieldInfo,
                        userMultiFieldInfo,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;
                    lookupFieldInfoAlt.ListId = lookupList.ID;
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    list.EnableVersioning = true;
                    list.Update();

                    // Create item on list
                    var itemOnList = list.AddItem();

                    // Update with the field values through the SharePoint API
                    itemOnList["Title"] = "Item under test";
                    itemOnList["TestInternalNameInteger"] = 555;
                    itemOnList["TestInternalNameNumber"] = 5.5;
                    itemOnList["TestInternalNameCurrency"] = 500.95;
                    itemOnList["TestInternalNameBool"] = true;
                    itemOnList["TestInternalNameBoolTrue"] = false;
                    itemOnList["TestInternalNameBoolFalse"] = true;
                    itemOnList["TestInternalNameDateFormula"] = new DateTime(1977, 7, 7);
                    itemOnList["TestInternalNameDateDefault"] = new DateTime(1977, 7, 7);
                    itemOnList["TestInternalNameText"] = "Text value";
                    itemOnList["TestInternalNameNote"] = "Note value";
                    itemOnList["TestInternalNameHtml"] = "<p class=\"some-css-class\">HTML value</p>";
                    itemOnList["TestInternalNameImage"] = new ImageFieldValue()
                    {
                        Hyperlink = "http://github.com/GSoft-SharePoint/",
                        ImageUrl = "/_layouts/15/MyFolder/MyImage.png",
                        BorderWidth = 0
                    };
                    itemOnList["TestInternalNameUrl"] = new SPFieldUrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    };
                    itemOnList["TestInternalNameUrlImg"] = new SPFieldUrlValue()
                    {
                        Url = "http://github.com/GSoft-SharePoint/",
                        Description = "patate!"
                    };

                    itemOnList["TestInternalNameLookup"] = new SPFieldLookupValue(1, "Test Item 1");
                    itemOnList["TestInternalNameLookupAlt"] = new SPFieldLookupValue(2, "2");
                    itemOnList["TestInternalNameLookupM"] = new SPFieldLookupValueCollection() { new SPFieldLookupValue(1, "Test Item 1"), new SPFieldLookupValue(2, "Test Item 2") };
                    itemOnList["TestInternalNameUser"] = new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name);
                    itemOnList["TestInternalNameUserMulti"] = new SPFieldUserValueCollection()
                        {
                            new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser1.ID, ensuredUser1.Name),
                            new SPFieldUserValue(testScope.SiteCollection.RootWeb, ensuredUser2.ID, ensuredUser2.Name)
                        };
                    itemOnList["TestInternalNameMedia"] = new MediaFieldValue()
                    {
                        Title = "Some media file title",
                        MediaSource = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                        AutoPlay = true,
                        Loop = true,
                        PreviewImageSource = "/_layouts/15/Images/logo.png"
                    };

                    var taxonomyField = (TaxonomyField)itemOnList.Fields.GetFieldByInternalName("TestInternalNameTaxo");
                    taxonomyField.SetFieldValue(itemOnList, createdTermB);

                    var taxonomyMultiField = (TaxonomyField)itemOnList.Fields.GetFieldByInternalName("TestInternalNameTaxoMulti");
                    taxonomyMultiField.SetFieldValue(itemOnList, new[] { createdTermAA, createdTermAB });

                    itemOnList.Update();

                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entityMappedFromSingleItem = new TestItemEntityWithLookups();
                    var entityMappedFromItemVersion = new TestItemEntityWithLookups();

                    // Act

                    // Map from SPListItem
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromSingleItem, itemOnList);

                    // Map from SPListItemVersion
                    entityBinder.ToEntity<TestItemEntityWithLookups>(entityMappedFromItemVersion, itemOnList.Versions[0]);

                    // Map from DataRow/SPListItemCollection
                    var entitiesMappedFromItemCollection = entityBinder.Get<TestItemEntity>(list.Items);

                    // Create 1 more list items
                    var copyItem = list.AddItem();

                    // Initialize the new item their corresponding entity to fill up their values with the original list item's values
                    entityBinder.FromEntity<TestItemEntityWithLookups>(entityMappedFromSingleItem, copyItem);

                    copyItem.Update();

                    // Assert
                    Assert.AreEqual(itemOnList["Title"], copyItem["Title"]);
                    Assert.AreEqual(itemOnList["TestInternalNameInteger"], copyItem["TestInternalNameInteger"]);
                    Assert.AreEqual(itemOnList["TestInternalNameNumber"], copyItem["TestInternalNameNumber"]);
                    Assert.AreEqual(itemOnList["TestInternalNameCurrency"], copyItem["TestInternalNameCurrency"]);
                    Assert.AreEqual(itemOnList["TestInternalNameBool"], copyItem["TestInternalNameBool"]);
                    Assert.AreEqual(itemOnList["TestInternalNameBoolTrue"], copyItem["TestInternalNameBoolTrue"]);
                    Assert.AreEqual(itemOnList["TestInternalNameBoolFalse"], copyItem["TestInternalNameBoolFalse"]);
                    Assert.AreEqual(itemOnList["TestInternalNameDateFormula"], copyItem["TestInternalNameDateFormula"]);
                    Assert.AreEqual(itemOnList["TestInternalNameDateDefault"], copyItem["TestInternalNameDateDefault"]);
                    Assert.AreEqual(itemOnList["TestInternalNameText"], copyItem["TestInternalNameText"]);
                    Assert.AreEqual(itemOnList["TestInternalNameNote"], copyItem["TestInternalNameNote"]);
                    Assert.AreEqual(itemOnList["TestInternalNameHtml"], copyItem["TestInternalNameHtml"]);
                    Assert.AreEqual(itemOnList["TestInternalNameImage"].ToString(), copyItem["TestInternalNameImage"].ToString());
                    Assert.AreEqual(itemOnList["TestInternalNameUrl"], copyItem["TestInternalNameUrl"]);
                    Assert.AreEqual(itemOnList["TestInternalNameUrlImg"], copyItem["TestInternalNameUrlImg"]);
                    Assert.AreEqual(itemOnList["TestInternalNameLookup"], copyItem["TestInternalNameLookup"]);
                    Assert.AreEqual(itemOnList["TestInternalNameLookupAlt"], copyItem["TestInternalNameLookupAlt"]);
                    Assert.AreEqual(itemOnList["TestInternalNameLookupM"].ToString(), copyItem["TestInternalNameLookupM"].ToString());
                    Assert.AreEqual(itemOnList["TestInternalNameUser"], copyItem["TestInternalNameUser"]);
                    Assert.AreEqual(itemOnList["TestInternalNameUserMulti"].ToString(), copyItem["TestInternalNameUserMulti"].ToString());
                    Assert.AreEqual(itemOnList["TestInternalNameMedia"].ToString(), copyItem["TestInternalNameMedia"].ToString());
                    Assert.AreEqual(itemOnList["TestInternalNameTaxo"].ToString(), copyItem["TestInternalNameTaxo"].ToString());
                    Assert.AreEqual(itemOnList["TestInternalNameTaxoMulti"].ToString(), copyItem["TestInternalNameTaxoMulti"].ToString());
                }
            }
        }

        /// <summary>
        /// Validates that using the ISharePointEntityBinder to map an entity's properties to
        /// a list item which was created in a list works.
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void FromEntity_WhenMappingToNewListItem_ShouldInitializeListItemFieldValues()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey");

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                    "TestInternalNameBoolTrue",
                    new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                    "NameKeyBoolTrue",
                    "DescriptionKeyBoolTrue",
                    "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfo = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey");

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey");

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey");

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey");

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(WindowsIdentity.GetCurrent().Name);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey");

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey");

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultTrue,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfo,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo,
                        userFieldInfo,
                        userMultiFieldInfo,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;
                    lookupFieldInfoAlt.ListId = lookupList.ID;
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    // Initialize the entity object with all the property values we want to apply on the new list item
                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entity = new TestItemEntityWithLookups()
                    {
                        IntegerProperty = 555,
                        DoubleProperty = 5.5,
                        CurrencyProperty = 500.95,
                        BoolProperty = true,
                        BoolDefaultTrueProperty = false,
                        BoolDefaultFalseProperty = true,
                        DateTimeFormulaProperty = new DateTime(2005, 10, 21),
                        DateTimeProperty = new DateTime(2005, 10, 21),
                        TextProperty = "Text value",
                        NoteProperty = "Note value",
                        HtmlProperty = "<p class=\"some-css-class\">HTML value</p>",
                        ImageProperty = new ImageValue()
                        {
                            Hyperlink = "http://github.com/GSoft-SharePoint/",
                            ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                        },
                        UrlProperty = new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        },
                        UrlImageProperty = new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        },
                        LookupProperty = new LookupValue(1, "Test Item 1"),
                        LookupAltProperty = new LookupValue(2, "2"),
                        LookupMultiProperty = new LookupValueCollection() { new LookupValue(1, "Test Item 1"), new LookupValue(2, "Test Item 2") },
                        UserProperty = new UserValue(ensuredUser1),
                        UserMultiProperty = new UserValueCollection() { new UserValue(ensuredUser1), new UserValue(ensuredUser2) },
                        MediaProperty = new MediaValue()
                        {
                            Title = "Some media file title",
                            Url = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                            IsAutoPlay = true,
                            IsLoop = true,
                            PreviewImageUrl = "/_layouts/15/Images/logo.png"
                        },
                        TaxonomyProperty = new TaxonomyValue(createdTermB),
                        TaxonomyMultiProperty = new TaxonomyValueCollection(
                        new List<TaxonomyValue>()
                            {
                                new TaxonomyValue(createdTermAA),
                                new TaxonomyValue(createdTermAB)
                            })
                    };

                    // Act (create the list item and bind the Entity's values to it)
                    var itemOnList = list.AddItem();
                    entityBinder.FromEntity<TestItemEntityWithLookups>(entity, itemOnList);
                    itemOnList.Update();

                    // Assert
                    // #1: validate ListItem field values on the mapped item object
                    Assert.AreEqual(555, itemOnList["TestInternalNameInteger"]);
                    Assert.AreEqual(5.5, itemOnList["TestInternalNameNumber"]);
                    Assert.AreEqual(500.95, itemOnList["TestInternalNameCurrency"]);
                    Assert.IsTrue((bool)itemOnList["TestInternalNameBool"]);
                    Assert.IsFalse((bool)itemOnList["TestInternalNameBoolTrue"]);
                    Assert.IsTrue((bool)itemOnList["TestInternalNameBoolFalse"]);
                    Assert.AreEqual(new DateTime(2005, 10, 21), itemOnList["TestInternalNameDateFormula"]);
                    Assert.AreEqual(new DateTime(2005, 10, 21), itemOnList["TestInternalNameDateDefault"]);
                    Assert.AreEqual("Text value", itemOnList["TestInternalNameText"]);
                    Assert.AreEqual("Note value", itemOnList["TestInternalNameNote"]);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", itemOnList["TestInternalNameHtml"]);

                    var imageFieldVal = (ImageFieldValue)itemOnList["TestInternalNameImage"];
                    Assert.IsNotNull(imageFieldVal);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", imageFieldVal.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", imageFieldVal.ImageUrl);

                    var urlFieldVal = new SPFieldUrlValue(itemOnList["TestInternalNameUrl"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlFieldVal.Url);
                    Assert.AreEqual("patate!", urlFieldVal.Description);

                    var urlImageFieldVal = new SPFieldUrlValue(itemOnList["TestInternalNameUrlImg"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlImageFieldVal.Url);
                    Assert.AreEqual("patate!", urlImageFieldVal.Description);

                    var lookupFieldVal = new SPFieldLookupValue(itemOnList["TestInternalNameLookup"].ToString());
                    Assert.AreEqual(1, lookupFieldVal.LookupId);
                    Assert.AreEqual("Test Item 1", lookupFieldVal.LookupValue);

                    var lookupAltFieldVal = new SPFieldLookupValue(itemOnList["TestInternalNameLookupAlt"].ToString());
                    Assert.AreEqual(2, lookupAltFieldVal.LookupId);
                    Assert.AreEqual("2", lookupAltFieldVal.LookupValue); // ShowField/LookupField is ID

                    var lookupMultiFieldVal = new SPFieldLookupValueCollection(itemOnList["TestInternalNameLookupM"].ToString());
                    Assert.AreEqual(1, lookupMultiFieldVal[0].LookupId);
                    Assert.AreEqual("Test Item 1", lookupMultiFieldVal[0].LookupValue);
                    Assert.AreEqual(2, lookupMultiFieldVal[1].LookupId);
                    Assert.AreEqual("Test Item 2", lookupMultiFieldVal[1].LookupValue);

                    var userFieldVal = new SPFieldUserValue(testScope.SiteCollection.RootWeb, itemOnList["TestInternalNameUser"].ToString());
                    Assert.AreEqual(ensuredUser1.Name, userFieldVal.User.Name);

                    // TODO: Make this work with ListItem converters
                    var userMultiFieldVal = new SPFieldUserValueCollection(testScope.SiteCollection.RootWeb, itemOnList["TestInternalNameUserMulti"].ToString());
                    Assert.AreEqual(ensuredUser1.Name, userMultiFieldVal[0].User.Name);
                    Assert.AreEqual("Maxime Boissonneault", userMultiFieldVal[1].User.Name);

                    var mediaFieldVal = MediaFieldValue.FromString(itemOnList["TestInternalNameMedia"].ToString());
                    Assert.AreEqual("Some media file title", mediaFieldVal.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), mediaFieldVal.MediaSource);
                    Assert.IsTrue(mediaFieldVal.AutoPlay);
                    Assert.IsTrue(mediaFieldVal.Loop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", mediaFieldVal.PreviewImageSource);

                    var taxoFieldValue = (TaxonomyFieldValue)itemOnList["TestInternalNameTaxo"];
                    Assert.AreNotEqual(-1, taxoFieldValue.WssId);
                    Assert.AreEqual(levelOneTermB.Id, new Guid(taxoFieldValue.TermGuid));
                    Assert.AreEqual(levelOneTermB.Label, taxoFieldValue.Label);

                    var taxoFieldValueMulti = (TaxonomyFieldValueCollection)itemOnList["TestInternalNameTaxoMulti"];
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[0].WssId);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(taxoFieldValueMulti[0].TermGuid));
                    Assert.AreEqual(levelTwoTermAA.Label, taxoFieldValueMulti[0].Label);
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[1].WssId);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(taxoFieldValueMulti[1].TermGuid));
                    Assert.AreEqual(levelTwoTermAB.Label, taxoFieldValueMulti[1].Label);

                    // #2: validate ListItem field values on the re-fetched list item
                    var refetchedItemOnList = list.GetItemById(itemOnList.ID);

                    Assert.AreEqual(555, refetchedItemOnList["TestInternalNameInteger"]);
                    Assert.AreEqual(5.5, refetchedItemOnList["TestInternalNameNumber"]);
                    Assert.AreEqual(500.95, refetchedItemOnList["TestInternalNameCurrency"]);
                    Assert.IsTrue((bool)refetchedItemOnList["TestInternalNameBool"]);
                    Assert.IsFalse((bool)refetchedItemOnList["TestInternalNameBoolTrue"]);
                    Assert.IsTrue((bool)refetchedItemOnList["TestInternalNameBoolFalse"]);
                    Assert.AreEqual(new DateTime(2005, 10, 21), refetchedItemOnList["TestInternalNameDateFormula"]);
                    Assert.AreEqual(new DateTime(2005, 10, 21), refetchedItemOnList["TestInternalNameDateDefault"]);
                    Assert.AreEqual("Text value", refetchedItemOnList["TestInternalNameText"]);
                    Assert.AreEqual("Note value", refetchedItemOnList["TestInternalNameNote"]);
                    Assert.AreEqual("<p class=\"some-css-class\">HTML value</p>", refetchedItemOnList["TestInternalNameHtml"]);

                    imageFieldVal = (ImageFieldValue)refetchedItemOnList["TestInternalNameImage"];
                    Assert.IsNotNull(imageFieldVal);
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", imageFieldVal.Hyperlink);
                    Assert.AreEqual("/_layouts/15/MyFolder/MyImage.png", imageFieldVal.ImageUrl);

                    urlFieldVal = new SPFieldUrlValue(refetchedItemOnList["TestInternalNameUrl"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlFieldVal.Url);
                    Assert.AreEqual("patate!", urlFieldVal.Description);     // proper Url description will never be set for Format=Hyperlink

                    urlImageFieldVal = new SPFieldUrlValue(refetchedItemOnList["TestInternalNameUrlImg"].ToString());
                    Assert.AreEqual("http://github.com/GSoft-SharePoint/", urlImageFieldVal.Url);
                    Assert.AreEqual("patate!", urlImageFieldVal.Description);     // proper Url description will never be set for Format=Image either

                    lookupFieldVal = new SPFieldLookupValue(refetchedItemOnList["TestInternalNameLookup"].ToString());
                    Assert.AreEqual(1, lookupFieldVal.LookupId);
                    Assert.AreEqual("Test Item 1", lookupFieldVal.LookupValue);

                    lookupAltFieldVal = new SPFieldLookupValue(refetchedItemOnList["TestInternalNameLookupAlt"].ToString());
                    Assert.AreEqual(2, lookupAltFieldVal.LookupId);
                    Assert.AreEqual("2", lookupAltFieldVal.LookupValue); // ShowField/LookupField is ID

                    lookupMultiFieldVal = new SPFieldLookupValueCollection(refetchedItemOnList["TestInternalNameLookupM"].ToString());
                    Assert.AreEqual(1, lookupMultiFieldVal[0].LookupId);
                    Assert.AreEqual("Test Item 1", lookupMultiFieldVal[0].LookupValue);
                    Assert.AreEqual(2, lookupMultiFieldVal[1].LookupId);
                    Assert.AreEqual("Test Item 2", lookupMultiFieldVal[1].LookupValue);

                    userFieldVal = new SPFieldUserValue(testScope.SiteCollection.RootWeb, refetchedItemOnList["TestInternalNameUser"].ToString());
                    Assert.AreEqual(ensuredUser1.Name, userFieldVal.User.Name);

                    userMultiFieldVal = new SPFieldUserValueCollection(testScope.SiteCollection.RootWeb, refetchedItemOnList["TestInternalNameUserMulti"].ToString());
                    Assert.AreEqual(ensuredUser1.Name, userMultiFieldVal[0].User.Name);
                    Assert.AreEqual("Maxime Boissonneault", userMultiFieldVal[1].User.Name);

                    mediaFieldVal = MediaFieldValue.FromString(refetchedItemOnList["TestInternalNameMedia"].ToString());
                    Assert.AreEqual("Some media file title", mediaFieldVal.Title);
                    Assert.AreEqual(HttpUtility.UrlDecode("/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf"), mediaFieldVal.MediaSource);
                    Assert.IsTrue(mediaFieldVal.AutoPlay);
                    Assert.IsTrue(mediaFieldVal.Loop);
                    Assert.AreEqual("/_layouts/15/Images/logo.png", mediaFieldVal.PreviewImageSource);

                    taxoFieldValue = (TaxonomyFieldValue)refetchedItemOnList["TestInternalNameTaxo"];
                    Assert.AreNotEqual(-1, taxoFieldValue.WssId);
                    Assert.AreEqual(levelOneTermB.Id, new Guid(taxoFieldValue.TermGuid));
                    Assert.AreEqual(levelOneTermB.Label, taxoFieldValue.Label);

                    taxoFieldValueMulti = (TaxonomyFieldValueCollection)refetchedItemOnList["TestInternalNameTaxoMulti"];
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[0].WssId);
                    Assert.AreEqual(levelTwoTermAA.Id, new Guid(taxoFieldValueMulti[0].TermGuid));
                    Assert.AreEqual(levelTwoTermAA.Label, taxoFieldValueMulti[0].Label);
                    Assert.AreNotEqual(-1, taxoFieldValueMulti[1].WssId);
                    Assert.AreEqual(levelTwoTermAB.Id, new Guid(taxoFieldValueMulti[1].TermGuid));
                    Assert.AreEqual(levelTwoTermAB.Label, taxoFieldValueMulti[1].Label);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        [TestMethod]
        public void FromEntityToEntityRoundTrip_ShouldEndUpWithIdenticalUserValuesInEntities()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                UserFieldInfo TestInternalNameUser = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{84E76E42-EE77-4718-9E8C-0657D03A8A31}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey");

                UserFieldInfo TestInternalNameUser2 = new UserFieldInfo(
                    "TestInternalNameUser2",
                    new Guid("{2A948D63-5240-4569-A839-62CC2EB6E673}"),
                    "NameKeyUser2",
                    "DescriptionKeyUser",
                    "GroupKey");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = new List<BaseFieldInfo>()
                    {
                        TestInternalNameUser,
                        TestInternalNameUser2
                    }
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    // Initialize the entity object with all the property values we want to apply on the new list item
                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entity = new TestItemEntityWithUserValuesOnly()
                    {
                        Title = "Some Test Title",

                        // User value with a non existing user using only the login name.
                        UserProperty = new UserValue("OFFICE\\maxime.boissonneault"),

                        // User value with an existing user using only its principal id.
                        UserProperty2 = new UserValue(testScope.SiteCollection.RootWeb.EnsureUser(WindowsIdentity.GetCurrent().Name).ID)
                    };

                    // Act 

                    // Create the list item and bind the Entity's values to it
                    var itemOnList = list.AddItem();
                    entityBinder.FromEntity<TestItemEntityWithUserValuesOnly>(entity, itemOnList);
                    itemOnList.Update();

                    // Then use the list item as data source for a brand new entity
                    var copyEntity = entityBinder.Get<TestItemEntityWithUserValuesOnly>(itemOnList);

                    // Assert
                    // #1: validate ListItem field values on the mapped item object
                    Assert.AreEqual(entity.Title, copyEntity.Title);
                    Assert.IsTrue(copyEntity.UserProperty.LoginName.IndexOf("maxime.boissonneault", StringComparison.OrdinalIgnoreCase) != -1);
                    Assert.AreEqual(entity.UserProperty2.Id, copyEntity.UserProperty2.Id);

                    // #2: Validate the values in the copied Entity
                    Assert.AreNotEqual(default(int), copyEntity.UserProperty.Id);
                    Assert.AreEqual(WindowsIdentity.GetCurrent().Name, copyEntity.UserProperty2.LoginName);
                }
            }
        }

        /// <summary>
        /// Validates that when you create a list item from an entity and then use that list item
        /// to create another entity, then both entities and up with identical properties
        /// </summary>
        [TestMethod]
        public void FromEntityToEntityRoundTrip_ShouldEndUpWithIdenticalEntities()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                IntegerFieldInfo integerFieldInfo = new IntegerFieldInfo(
                    "TestInternalNameInteger",
                    new Guid("{12E262D0-C7C4-4671-A266-064CDBD3905A}"),
                    "NameKeyInt",
                    "DescriptionKeyInt",
                    "GroupKey");

                NumberFieldInfo numberFieldInfo = new NumberFieldInfo(
                    "TestInternalNameNumber",
                    new Guid("{5DD4EE0F-8498-4033-97D0-317A24988786}"),
                    "NameKeyNumber",
                    "DescriptionKeyNumber",
                    "GroupKey");

                CurrencyFieldInfo currencyFieldInfo = new CurrencyFieldInfo(
                    "TestInternalNameCurrency",
                    new Guid("{9E9963F6-1EE6-46FB-9599-783BBF4D6249}"),
                    "NameKeyCurrency",
                    "DescriptionKeyCurrency",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoBasic = new BooleanFieldInfo(
                    "TestInternalNameBool",
                    new Guid("{F556AB6B-9E51-43E2-99C9-4A4E551A4BEF}"),
                    "NameKeyBool",
                    "DescriptionKeyBool",
                    "GroupKey");

                BooleanFieldInfo boolFieldInfoDefaultTrue = new BooleanFieldInfo(
                    "TestInternalNameBoolTrue",
                    new Guid("{0D0289AD-C5FB-495B-96C6-48CC46737D08}"),
                    "NameKeyBoolTrue",
                    "DescriptionKeyBoolTrue",
                    "GroupKey")
                {
                    DefaultValue = true
                };

                BooleanFieldInfo boolFieldInfoDefaultFalse = new BooleanFieldInfo(
                    "TestInternalNameBoolFalse",
                    new Guid("{628181BD-9B0B-4B7E-934F-1CF1796EA4E4}"),
                    "NameKeyBoolFalse",
                    "DescriptionKeyBoolFalse",
                    "GroupKey")
                {
                    DefaultValue = false
                };

                DateTimeFieldInfo dateTimeFieldInfoFormula = new DateTimeFieldInfo(
                    "TestInternalNameDateFormula",
                    new Guid("{D23EAD73-9E18-46DB-A426-41B2D47F696C}"),
                    "NameKeyDateTimeFormula",
                    "DescriptionKeyDateTimeFormula",
                    "GroupKey")
                {
                    DefaultFormula = "=[Today]"
                };

                DateTimeFieldInfo dateTimeFieldInfo = new DateTimeFieldInfo(
                    "TestInternalNameDateDefault",
                    new Guid("{016BF8D9-CEDC-4BF4-BA21-AC6A8F174AD5}"),
                    "NameKeyDateTimeDefault",
                    "DescriptionKeyDateTimeDefault",
                    "GroupKey");

                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalNameText",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                NoteFieldInfo noteFieldInfo = new NoteFieldInfo(
                    "TestInternalNameNote",
                    new Guid("{E315BB24-19C3-4F2E-AABC-9DE5EFC3D5C2}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                HtmlFieldInfo htmlFieldInfo = new HtmlFieldInfo(
                    "TestInternalNameHtml",
                    new Guid("{D16958E7-CF9A-4C38-A8BB-99FC03BFD913}"),
                    "NameKeyAlt",
                    "DescriptionKeyAlt",
                    "GroupKey");

                ImageFieldInfo imageFieldInfo = new ImageFieldInfo(
                    "TestInternalNameImage",
                    new Guid("{6C5B9E77-B621-43AA-BFBF-B333093EFCAE}"),
                    "NameKeyImage",
                    "DescriptionKeyImage",
                    "GroupKey");

                UrlFieldInfo urlFieldInfo = new UrlFieldInfo(
                    "TestInternalNameUrl",
                    new Guid("{208F904C-5A1C-4E22-9A79-70B294FABFDA}"),
                    "NameKeyUrl",
                    "DescriptionKeyUrl",
                    "GroupKey");

                UrlFieldInfo urlFieldInfoImage = new UrlFieldInfo(
                    "TestInternalNameUrlImg",
                    new Guid("{96D22CFF-5B40-4675-B632-28567792E11B}"),
                    "NameKeyUrlImg",
                    "DescriptionKeyUrlImg",
                    "GroupKey")
                {
                    Format = UrlFieldFormat.Image
                };

                LookupFieldInfo lookupFieldInfo = new LookupFieldInfo(
                    "TestInternalNameLookup",
                    new Guid("{62F8127C-4A8C-4217-8BD8-C6712753AFCE}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey");

                LookupFieldInfo lookupFieldInfoAlt = new LookupFieldInfo(
                    "TestInternalNameLookupAlt",
                    new Guid("{1F05DFFA-6396-4AEF-AD23-72217206D35E}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    ShowField = "ID"
                };

                LookupMultiFieldInfo lookupMultiFieldInfo = new LookupMultiFieldInfo(
                    "TestInternalNameLookupM",
                    new Guid("{2C9D4C0E-21EB-4742-8C6C-4C30DCD08A05}"),
                    "NameKeyMulti",
                    "DescriptionKeyMulti",
                    "GroupKey");

                var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(WindowsIdentity.GetCurrent().Name);
                var ensuredUser2 = testScope.SiteCollection.RootWeb.EnsureUser("OFFICE\\maxime.boissonneault");

                UserFieldInfo userFieldInfo = new UserFieldInfo(
                    "TestInternalNameUser",
                    new Guid("{5B74DD50-0D2D-4D24-95AF-0C4B8AA3F68A}"),
                    "NameKeyUser",
                    "DescriptionKeyUser",
                    "GroupKey");

                UserMultiFieldInfo userMultiFieldInfo = new UserMultiFieldInfo(
                    "TestInternalNameUserMulti",
                    new Guid("{8C662588-D54E-4905-B232-856C2239B036}"),
                    "NameKeyUserMulti",
                    "DescriptionKeyUserMulti",
                    "GroupKey");

                MediaFieldInfo mediaFieldInfo = new MediaFieldInfo(
                    "TestInternalNameMedia",
                    new Guid("{A2F070FE-FE33-44FC-9FDF-D18E74ED4D67}"),
                    "NameKeyMedia",
                    "DescriptionKeyMEdia",
                    "GroupKey");

                var testTermSet = new TermSetInfo(Guid.NewGuid(), "Test Term Set"); // keep Ids random because, if this test fails midway, the term
                // set will not be cleaned up and upon next test run we will
                // run into a term set and term ID conflicts.
                var levelOneTermA = new TermInfo(Guid.NewGuid(), "Term A", testTermSet);
                var levelOneTermB = new TermInfo(Guid.NewGuid(), "Term B", testTermSet);
                var levelTwoTermAA = new TermInfo(Guid.NewGuid(), "Term A-A", testTermSet);
                var levelTwoTermAB = new TermInfo(Guid.NewGuid(), "Term A-B", testTermSet);

                TaxonomySession session = new TaxonomySession(testScope.SiteCollection);
                TermStore defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                Group defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                TermSet newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSet.Label, testTermSet.Id);
                Term createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                Term createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                Term createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                Term createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                defaultSiteCollectionTermStore.CommitAll();

                TaxonomyFieldInfo taxoFieldInfo = new TaxonomyFieldInfo(
                    "TestInternalNameTaxo",
                    new Guid("{18CC105F-16C9-43E2-9933-37F98452C038}"),
                    "NameKey",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(testTermSet)     // choices limited to all terms in test term set
                };

                TaxonomyMultiFieldInfo taxoMultiFieldInfo = new TaxonomyMultiFieldInfo(
                    "TestInternalNameTaxoMulti",
                    new Guid("{2F49D362-B014-41BB-9959-1000C9A7FFA0}"),
                    "NameKeyMulti",
                    "DescriptionKey",
                    "GroupKey")
                {
                    TermStoreMapping = new TaxonomyContext(levelOneTermA)   // choices limited to children of a specific term, instead of having full term set choices
                };

                var fieldsToEnsure = new List<BaseFieldInfo>()
                    {
                        integerFieldInfo,
                        numberFieldInfo,
                        currencyFieldInfo,
                        boolFieldInfoBasic,
                        boolFieldInfoDefaultTrue,
                        boolFieldInfoDefaultFalse,
                        dateTimeFieldInfoFormula,
                        dateTimeFieldInfo,
                        textFieldInfo,
                        noteFieldInfo,
                        htmlFieldInfo,
                        imageFieldInfo,
                        urlFieldInfo,
                        urlFieldInfoImage,
                        lookupFieldInfo,
                        lookupFieldInfoAlt,
                        lookupMultiFieldInfo,
                        userFieldInfo,
                        userMultiFieldInfo,
                        mediaFieldInfo,
                        taxoFieldInfo,
                        taxoMultiFieldInfo
                    };

                ListInfo lookupListInfo = new ListInfo("sometestlistpathlookup", "DynamiteTestListNameKeyLookup", "DynamiteTestListDescriptionKeyLookup");

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey")
                {
                    FieldDefinitions = fieldsToEnsure
                };

                // Note how we need to specify SPSite for injection context - ISharePointEntityBinder's implementation
                // is lifetime-scoped to InstancePerSite.
                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Lookup field ListId setup
                    SPList lookupList = listHelper.EnsureList(testScope.SiteCollection.RootWeb, lookupListInfo);
                    lookupFieldInfo.ListId = lookupList.ID;
                    lookupFieldInfoAlt.ListId = lookupList.ID;
                    lookupMultiFieldInfo.ListId = lookupList.ID;

                    // Create the looked-up items
                    var lookupItem1 = lookupList.Items.Add();
                    lookupItem1["Title"] = "Test Item 1";
                    lookupItem1.Update();

                    var lookupItem2 = lookupList.Items.Add();
                    lookupItem2["Title"] = "Test Item 2";
                    lookupItem2.Update();

                    // Create the first test list
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);

                    // Initialize the entity object with all the property values we want to apply on the new list item
                    var entityBinder = injectionScope.Resolve<ISharePointEntityBinder>();
                    var entity = new TestItemEntityWithLookups()
                    {
                        Title = "Test entity",
                        IntegerProperty = 555,
                        DoubleProperty = 5.5,
                        CurrencyProperty = 500.95,
                        BoolProperty = true,
                        BoolDefaultTrueProperty = false,
                        BoolDefaultFalseProperty = true,
                        DateTimeFormulaProperty = new DateTime(2005, 10, 21),
                        DateTimeProperty = new DateTime(2005, 10, 21),
                        TextProperty = "Text value",
                        NoteProperty = "Note value",
                        HtmlProperty = "<p class=\"some-css-class\">HTML value</p>",
                        ImageProperty = new ImageValue()
                        {
                            Hyperlink = "http://github.com/GSoft-SharePoint/",
                            ImageUrl = "/_layouts/15/MyFolder/MyImage.png"
                        },
                        UrlProperty = new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        },
                        UrlImageProperty = new UrlValue()
                        {
                            Url = "http://github.com/GSoft-SharePoint/",
                            Description = "patate!"
                        },
                        LookupProperty = new LookupValue(1, "Test Item 1"),
                        LookupAltProperty = new LookupValue(2, "2"),
                        LookupMultiProperty = new LookupValueCollection() { new LookupValue(1, "Test Item 1"), new LookupValue(2, "Test Item 2") },
                        UserProperty = new UserValue(ensuredUser1),
                        UserMultiProperty = new UserValueCollection() { new UserValue(ensuredUser1), new UserValue(ensuredUser2) },
                        MediaProperty = new MediaValue()
                        {
                            Title = "Some media file title",
                            Url = "/sites/test/SiteAssets/01_01_ASP.NET%20MVC%203%20Fundamentals%20Intro%20-%20Overview.asf",
                            IsAutoPlay = true,
                            IsLoop = true,
                            PreviewImageUrl = "/_layouts/15/Images/logo.png"
                        },
                        TaxonomyProperty = new TaxonomyValue(createdTermB),
                        TaxonomyMultiProperty = new TaxonomyValueCollection(
                        new List<TaxonomyValue>()
                            {
                                new TaxonomyValue(createdTermAA),
                                new TaxonomyValue(createdTermAB)
                            })
                    };

                    // Act 

                    // Create the list item and bind the Entity's values to it
                    var itemOnList = list.AddItem();
                    entityBinder.FromEntity<TestItemEntityWithLookups>(entity, itemOnList);
                    itemOnList.Update();

                    // Then use the list item as data source for a brand new entity
                    var copyEntity = entityBinder.Get<TestItemEntityWithLookups>(itemOnList);

                    // Assert
                    // #1: validate ListItem field values on the mapped item object
                    Assert.AreEqual(entity.Title, copyEntity.Title);
                    Assert.AreEqual(entity.IntegerProperty, copyEntity.IntegerProperty);
                    Assert.AreEqual(entity.DoubleProperty, copyEntity.DoubleProperty);
                    Assert.AreEqual(entity.CurrencyProperty, copyEntity.CurrencyProperty);
                    Assert.AreEqual(entity.BoolProperty, copyEntity.BoolProperty);
                    Assert.AreEqual(entity.BoolDefaultTrueProperty, copyEntity.BoolDefaultTrueProperty);
                    Assert.AreEqual(entity.BoolDefaultFalseProperty, copyEntity.BoolDefaultFalseProperty);
                    Assert.AreEqual(entity.DateTimeFormulaProperty, copyEntity.DateTimeFormulaProperty);
                    Assert.AreEqual(entity.DateTimeProperty, copyEntity.DateTimeProperty);
                    Assert.AreEqual(entity.TextProperty, copyEntity.TextProperty);
                    Assert.AreEqual(entity.NoteProperty, copyEntity.NoteProperty);
                    Assert.AreEqual(entity.HtmlProperty, copyEntity.HtmlProperty);
                    Assert.AreEqual(entity.ImageProperty.ImageUrl, copyEntity.ImageProperty.ImageUrl);
                    Assert.AreEqual(entity.ImageProperty.Hyperlink, copyEntity.ImageProperty.Hyperlink);
                    Assert.AreEqual(entity.UrlProperty.Url, copyEntity.UrlProperty.Url);
                    Assert.AreEqual(entity.UrlProperty.Description, copyEntity.UrlProperty.Description);
                    Assert.AreEqual(entity.UrlImageProperty.Url, copyEntity.UrlImageProperty.Url);
                    Assert.AreEqual(entity.UrlImageProperty.Description, copyEntity.UrlImageProperty.Description);
                    Assert.AreEqual(entity.LookupProperty.Id, copyEntity.LookupProperty.Id);
                    Assert.AreEqual(entity.LookupProperty.Value, copyEntity.LookupProperty.Value);
                    Assert.AreEqual(entity.LookupAltProperty.Id, copyEntity.LookupAltProperty.Id);
                    Assert.AreEqual(entity.LookupAltProperty.Value, copyEntity.LookupAltProperty.Value);
                    Assert.AreEqual(entity.LookupMultiProperty[0].Id, copyEntity.LookupMultiProperty[0].Id);
                    Assert.AreEqual(entity.LookupMultiProperty[0].Value, copyEntity.LookupMultiProperty[0].Value);
                    Assert.AreEqual(entity.LookupMultiProperty[1].Id, copyEntity.LookupMultiProperty[1].Id);
                    Assert.AreEqual(entity.LookupMultiProperty[1].Value, copyEntity.LookupMultiProperty[1].Value);
                    Assert.AreEqual(entity.UserProperty.Id, copyEntity.UserProperty.Id);
                    Assert.AreEqual(entity.UserProperty.DisplayName, copyEntity.UserProperty.DisplayName);
                    Assert.AreEqual(entity.UserMultiProperty[0].Id, copyEntity.UserMultiProperty[0].Id);
                    Assert.AreEqual(entity.UserMultiProperty[0].DisplayName, copyEntity.UserMultiProperty[0].DisplayName);
                    Assert.AreEqual(entity.UserMultiProperty[1].Id, copyEntity.UserMultiProperty[1].Id);
                    Assert.AreEqual(entity.UserMultiProperty[1].DisplayName, copyEntity.UserMultiProperty[1].DisplayName);
                    Assert.AreEqual(entity.MediaProperty.Title, copyEntity.MediaProperty.Title);
                    Assert.AreEqual(entity.MediaProperty.Url, copyEntity.MediaProperty.Url);
                    Assert.AreEqual(entity.TaxonomyProperty.Id, copyEntity.TaxonomyProperty.Id);
                    Assert.AreEqual(entity.TaxonomyProperty.Label, copyEntity.TaxonomyProperty.Label);
                    Assert.AreEqual(entity.TaxonomyMultiProperty[0].Id, copyEntity.TaxonomyMultiProperty[0].Id);
                    Assert.AreEqual(entity.TaxonomyMultiProperty[0].Label, copyEntity.TaxonomyMultiProperty[0].Label);
                    Assert.AreEqual(entity.TaxonomyMultiProperty[1].Id, copyEntity.TaxonomyMultiProperty[1].Id);
                    Assert.AreEqual(entity.TaxonomyMultiProperty[1].Label, copyEntity.TaxonomyMultiProperty[1].Label);
                }
            }
        }

        // MORE TEST CASE Suggestions:
        // - Non SiteCollection-specific term group bindings

        /// <summary>
        /// A test class
        /// </summary>
        public class TestItemEntity : BaseEntity
        {
            /// <summary>
            /// Test int property
            /// </summary>
            [Property("TestInternalNameInteger")]
            public int IntegerProperty { get; set; }

            /// <summary>
            /// Test double property
            /// </summary>
            [Property("TestInternalNameNumber")]
            public double DoubleProperty { get; set; }

            /// <summary>
            /// Test currency property
            /// </summary>
            [Property("TestInternalNameCurrency")]
            public double CurrencyProperty { get; set; }   // TODO: right now currency (which, ideally, would be mapped to type decimal) will use the DoubleValueWriter, which may map wrong to decimal

            /// <summary>
            /// Test bool property
            /// </summary>
            [Property("TestInternalNameBool")]
            public bool? BoolProperty { get; set; }

            /// <summary>
            /// Test bool property with True default value on field
            /// </summary>
            [Property("TestInternalNameBoolTrue")]
            public bool BoolDefaultTrueProperty { get; set; }

            /// <summary>
            /// Test bool property with False default value on field
            /// </summary>
            [Property("TestInternalNameBoolFalse")]
            public bool BoolDefaultFalseProperty { get; set; }

            /// <summary>
            /// Test datetime property with formula on field
            /// </summary>
            [Property("TestInternalNameDateFormula")]
            public DateTime DateTimeFormulaProperty { get; set; }

            /// <summary>
            /// Test datetime property
            /// </summary>
            [Property("TestInternalNameDateDefault")]
            public DateTime? DateTimeProperty { get; set; }

            /// <summary>
            /// Test text property
            /// </summary>
            [Property("TestInternalNameText")]
            public string TextProperty { get; set; }

            /// <summary>
            /// Test note property
            /// </summary>
            [Property("TestInternalNameNote")]
            public string NoteProperty { get; set; }

            /// <summary>
            /// Test HTML property
            /// </summary>
            [Property("TestInternalNameHtml")]
            public string HtmlProperty { get; set; }

            /// <summary>
            /// Test Image property
            /// </summary>
            [Property("TestInternalNameImage")]
            public ImageValue ImageProperty { get; set; }

            /// <summary>
            /// Test URL property
            /// </summary>
            [Property("TestInternalNameUrl")]
            public UrlValue UrlProperty { get; set; }

            /// <summary>
            /// Test URL-as-Image property
            /// </summary>
            [Property("TestInternalNameUrlImg")]
            public UrlValue UrlImageProperty { get; set; }

            /// <summary>
            /// Test media (audio-video) property
            /// </summary>
            [Property("TestInternalNameMedia")]
            public MediaValue MediaProperty { get; set; }

            /// <summary>
            /// Test taxonomy property
            /// </summary>
            [Property("TestInternalNameTaxo")]
            public TaxonomyValue TaxonomyProperty { get; set; }

            /// <summary>
            /// Test taxonomy multi property
            /// </summary>
            [Property("TestInternalNameTaxoMulti")]
            public TaxonomyValueCollection TaxonomyMultiProperty { get; set; }
        }

        /// <summary>
        /// Another test class
        /// </summary>
        public class TestItemEntityWithLookups : TestItemEntity
        {
            /// <summary>
            /// Test lookup property
            /// </summary>
            [Property("TestInternalNameLookup")]
            public LookupValue LookupProperty { get; set; }

            /// <summary>
            /// Alternate test lookup property
            /// </summary>
            [Property("TestInternalNameLookupAlt")]
            public LookupValue LookupAltProperty { get; set; }

            /// <summary>
            /// Test lookup multi property
            /// </summary>
            [Property("TestInternalNameLookupM")]
            public LookupValueCollection LookupMultiProperty { get; set; }

            /// <summary>
            /// Test User property
            /// </summary>
            [Property("TestInternalNameUser")]
            public UserValue UserProperty { get; set; }

            /// <summary>
            /// Test user multi property
            /// </summary>
            [Property("TestInternalNameUserMulti")]
            public UserValueCollection UserMultiProperty { get; set; }
        }

        /// <summary>
        /// A test class with primitive-based or struct value types
        /// </summary>
        public class TestItemEntityWithNonNullablePrimitivesOnly : BaseEntity
        {
            /// <summary>
            /// Test int property
            /// </summary>
            [Property("TestInternalNameInteger")]
            public int IntegerProperty { get; set; }

            /// <summary>
            /// Test double property
            /// </summary>
            [Property("TestInternalNameNumber")]
            public double DoubleProperty { get; set; }

            /// <summary>
            /// Test currency property
            /// </summary>
            [Property("TestInternalNameCurrency")]
            public double CurrencyProperty { get; set; }

            /// <summary>
            /// Test bool property
            /// </summary>
            [Property("TestInternalNameBool")]
            public bool BoolProperty { get; set; }

            /// <summary>
            /// Test datetime property
            /// </summary>
            [Property("TestInternalNameDate")]
            public DateTime DateTimeProperty { get; set; }

            /// <summary>
            /// Test Guid property
            /// </summary>
            [Property("TestInternalNameGuid")]
            public Guid GuidProperty { get; set; }
        }

        /// <summary>
        /// A test class with primitive-based or struct value types
        /// </summary>
        public class TestItemEntityWithNullablePrimitivesOnly : BaseEntity
        {
            /// <summary>
            /// Test int property
            /// </summary>
            [Property("TestInternalNameInteger")]
            public int? IntegerProperty { get; set; }

            /// <summary>
            /// Test double property
            /// </summary>
            [Property("TestInternalNameNumber")]
            public double? DoubleProperty { get; set; }

            /// <summary>
            /// Test currency property
            /// </summary>
            [Property("TestInternalNameCurrency")]
            public double? CurrencyProperty { get; set; }

            /// <summary>
            /// Test bool property
            /// </summary>
            [Property("TestInternalNameBool")]
            public bool? BoolProperty { get; set; }

            /// <summary>
            /// Test datetime property
            /// </summary>
            [Property("TestInternalNameDate")]
            public DateTime? DateTimeProperty { get; set; }

            /// <summary>
            /// Test Guid property
            /// </summary>
            [Property("TestInternalNameGuid")]
            public Guid? GuidProperty { get; set; }
        }

        public class TestItemEntityWithUserValuesOnly : BaseEntity
        {

            /// <summary>
            /// Test User property
            /// </summary>
            [Property("TestInternalNameUser")]
            public UserValue UserProperty { get; set; }

            /// <summary>
            /// Test User property
            /// </summary>
            [Property("TestInternalNameUser2")]
            public UserValue UserProperty2 { get; set; }
        }
    }
}