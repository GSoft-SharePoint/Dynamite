﻿using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Definitions;
using Microsoft.SharePoint;
using GSoft.Dynamite.FieldTypes;

namespace GSoft.Dynamite
{
    /// <summary>
    /// Site columns constants for built-in (OOTB) content types
    /// </summary>
    public static class BuiltInFields
    {
        #region Name

        /// <summary>
        /// Title field internal name
        /// </summary>
        public const string TitleName = "Title";

        /// <summary>
        /// Linked to item with edit menu
        /// </summary>
        public const string TitleLinkName = "LinkTitle";

        /// <summary>
        /// Linked to item with no edit menu
        /// </summary>
        public const string TitleLinkNoMenuName = "LinkTitleNoMenu";

        /// <summary>
        /// FileRef (i.e. File Url) field internal name
        /// </summary>
        public const string FileRefName = "FileRef";

        /// <summary>
        /// FileLeafRef (i.e. DocumentName) field internal name
        /// </summary>
        public const string FileLeafRefName = "FileLeafRef";

        /// <summary>
        /// The URL field internal name
        /// </summary>
        public const string UrlName = "URL";

        /// <summary>
        /// ContentType field internal name
        /// </summary>
        public const string ContentTypeName = "ContentType";

        /// <summary>
        /// ContentTypeId field internal name
        /// </summary>
        public const string ContentTypeIdName = "ContentTypeId";

        /// <summary>
        /// TaxCatchAll field name.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "Mean Catch All, not Catchall")]
        public const string TaxCatchAllName = "TaxCatchAll";

        /// <summary>
        /// TaxCatchAllLabel field name.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "Mean Catch All, not Catchall")]
        public const string TaxCatchAllLabelName = "TaxCatchAllLabel";

        /// <summary>
        /// The assigned to field name
        /// </summary>
        public const string AssignedToName = "AssignedTo";

        /// <summary>
        /// The percent complete field name
        /// </summary>
        public const string PercentCompleteName = "PercentComplete";

        /// <summary>
        /// The predecessors field name
        /// </summary>
        public const string PredecessorsName = "Predecessors";

        /// <summary>
        /// The priority field name
        /// </summary>
        public const string PriorityName = "Priority";

        /// <summary>
        /// The task status field name
        /// </summary>
        public const string TaskStatusName = "TaskStatus";

        /// <summary>
        /// The home phone field name
        /// </summary>
        public const string HomePhoneName = "HomePhone";

        /// <summary>
        /// The work fax field name
        /// </summary>
        public const string WorkFaxName = "WorkFax";

        /// <summary>
        /// The work address field name
        /// </summary>
        public const string WorkAddressName = "WorkAddress";

        /// <summary>
        /// The work country field name
        /// </summary>
        public const string WorkCountryName = "WorkCountry";

        /// <summary>
        /// The work city field name
        /// </summary>
        public const string WorkCityName = "WorkCity";

        /// <summary>
        /// The work state field name
        /// </summary>
        public const string WorkStateName = "WorkState";

        /// <summary>
        /// The work zip field name
        /// </summary>
        public const string WorkZipName = "WorkZip";

        /// <summary>
        /// The web page field name
        /// </summary>
        public const string WebpageName = "WebPage";

        /// <summary>
        /// The comments field name
        /// </summary>
        public const string CommentsName = "Comments";

        /// <summary>
        /// The department field name
        /// </summary>
        public const string DepartmentName = "Department";

        /// <summary>
        /// The role field name
        /// </summary>
        public const string RoleName = "Role";

        /// <summary>
        /// The related items field name
        /// </summary>
        public const string RelatedItemsName = "RelatedItems";

        /// <summary>
        /// The display order field name
        /// </summary>
        public const string DisplayOrderName = "DisplayOrder";

        /// <summary>
        /// The master page URL field name
        /// </summary>
        public const string MasterPageUrlName = "MasterPageUrl";

        /// <summary>
        /// The theme URL field name
        /// </summary>
        public const string ThemeUrlName = "ThemeUrl";

        /// <summary>
        /// The image URL field name
        /// </summary>
        public const string ImageUrlName = "ImageUrl";

        /// <summary>
        /// The font scheme URL field name
        /// </summary>
        public const string FontSchemeUrlName = "FontSchemeUrl";

        /// <summary>
        /// The enterprise keywords field name
        /// </summary>
        public const string EnterpriseKeywordsName = "TaxKeyword";

        /// <summary>
        /// The cell phone field name
        /// </summary>
        public const string CellPhoneName = "CellPhone";

        /// <summary>
        /// The full name field name
        /// </summary>
        public const string FullNameName = "FullName";

        /// <summary>
        /// The email field name
        /// </summary>
        public const string EmailName = "EMail";

        /// <summary>
        /// The first name field name
        /// </summary>
        public const string FirstNameName = "FirstName";

        /// <summary>
        /// The work phone field name
        /// </summary>
        public const string WorkPhoneName = "WorkPhone";

        /// <summary>
        /// The job title field name
        /// </summary>
        public const string JobTitleName = "JobTitle";

        /// <summary>
        /// The company field name
        /// </summary>
        public const string CompanyName = "Company";

        /// <summary>
        /// PublishingLinks list : the url of the publishing link
        /// </summary>
        public const string PublishedLinksUrlName = "PublishedLinksURL";

        #endregion

        #region FieldInfo

        /// <summary>
        /// Title field info
        /// </summary>
        public static readonly IFieldInfo Title = new MinimalFieldInfo(TitleName, SPBuiltInFieldId.Title);

        /// <summary>
        /// Title linked to item with edit menu
        /// </summary>
        public static readonly IFieldInfo TitleLink = new MinimalFieldInfo(TitleLinkName, SPBuiltInFieldId.LinkTitle);

        /// <summary>
        /// Title linked to item with no edit menu
        /// </summary>
        public static readonly IFieldInfo TitleLinkNoMenu = new MinimalFieldInfo(TitleLinkNoMenuName, SPBuiltInFieldId.LinkTitleNoMenu);

        /// <summary>
        /// FileRef (i.e. File Url) field info
        /// </summary>
        public static readonly IFieldInfo FileRef = new MinimalFieldInfo(FileRefName, SPBuiltInFieldId.FileRef);

        /// <summary>
        /// FileLeafRef (i.e. Document Name) field info
        /// </summary>
        public static readonly IFieldInfo FileLeafRef = new MinimalFieldInfo(FileLeafRefName, SPBuiltInFieldId.FileLeafRef);

        /// <summary>
        /// ContentType field info
        /// </summary>
        public static readonly IFieldInfo ContentType = new MinimalFieldInfo(ContentTypeName, SPBuiltInFieldId.ContentType);

        /// <summary>
        /// ContentTypeId field info
        /// </summary>
        public static readonly IFieldInfo ContentTypeId = new MinimalFieldInfo(ContentTypeIdName, SPBuiltInFieldId.ContentTypeId);

        /// <summary>
        /// URL field info
        /// </summary>
        public static readonly IFieldInfo Url = new MinimalFieldInfo(UrlName, SPBuiltInFieldId.URL);   // TODO: turn into UrlFieldValue

        /// <summary>
        /// TaxCatchAll field info.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "This is the actual SharePoint field name")]
        public static readonly IFieldInfo TaxCatchAll = new MinimalFieldInfo(TaxCatchAllName, new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611"));

        /// <summary>
        /// TaxCatchAllLabel field info.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "This is the actual SharePoint field name")]
        public static readonly IFieldInfo TaxCatchAllLabel = new MinimalFieldInfo(TaxCatchAllLabelName, new Guid("8f6b6dd8-9357-4019-8172-966fcd502ed2"));

        /// <summary>
        /// The assigned to field info
        /// </summary>
        public static readonly IFieldInfo AssignedTo = new MinimalFieldInfo(AssignedToName, SPBuiltInFieldId.AssignedTo);  // TODO: turn into UserFieldInfo

        /// <summary>
        /// The percent complete field info
        /// </summary>
        public static readonly IFieldInfo PercentComplete = new MinimalFieldInfo(PercentCompleteName, SPBuiltInFieldId.PercentComplete);

        /// <summary>
        /// The predecessors field info
        /// </summary>
        public static readonly IFieldInfo Predecessors = new MinimalFieldInfo(PredecessorsName, SPBuiltInFieldId.Predecessors);

        /// <summary>
        /// The priority field info
        /// </summary>
        public static readonly IFieldInfo Priority = new MinimalFieldInfo(PriorityName, SPBuiltInFieldId.Priority);    // TODO: turn into ChoiceFieldInfo

        /// <summary>
        /// The task status field info
        /// </summary>
        public static readonly IFieldInfo TaskStatus = new MinimalFieldInfo(TaskStatusName, SPBuiltInFieldId.TaskStatus);      // TODO: turn into ChoiceFieldInfo

        /// <summary>
        /// The home phone field info
        /// </summary>
        public static readonly IFieldInfo HomePhone = new MinimalFieldInfo(HomePhoneName, SPBuiltInFieldId.HomePhone);

        /// <summary>
        /// The work fax field info
        /// </summary>
        public static readonly IFieldInfo WorkFax = new MinimalFieldInfo(WorkFaxName, SPBuiltInFieldId.WorkFax);

        /// <summary>
        /// The work address field info
        /// </summary>
        public static readonly IFieldInfo WorkAddress = new MinimalFieldInfo(WorkAddressName, SPBuiltInFieldId.WorkAddress);

        /// <summary>
        /// The work country field info
        /// </summary>
        public static readonly IFieldInfo WorkCountry = new MinimalFieldInfo(WorkCountryName, SPBuiltInFieldId.WorkCountry);

        /// <summary>
        /// The work city field info
        /// </summary>
        public static readonly IFieldInfo WorkCity = new MinimalFieldInfo(WorkCityName, SPBuiltInFieldId.WorkCity);

        /// <summary>
        /// The work state field info
        /// </summary>
        public static readonly IFieldInfo WorkState = new MinimalFieldInfo(WorkStateName, SPBuiltInFieldId.WorkState);

        /// <summary>
        /// The work zip field info
        /// </summary>
        public static readonly IFieldInfo WorkZip = new MinimalFieldInfo(WorkZipName, SPBuiltInFieldId.WorkZip);

        /// <summary>
        /// The web page field info
        /// </summary>
        public static readonly IFieldInfo Webpage = new MinimalFieldInfo(WebpageName, SPBuiltInFieldId.WebPage);       // TODO: turn into UrlFieldInfo

        /// <summary>
        /// The comments field info
        /// </summary>
        public static readonly IFieldInfo Comments = new MinimalFieldInfo(CommentsName, SPBuiltInFieldId.Comments);

        /// <summary>
        /// The department field info
        /// </summary>
        public static readonly IFieldInfo Department = new MinimalFieldInfo(DepartmentName, SPBuiltInFieldId.Department);

        /// <summary>
        /// The role field info
        /// </summary>
        public static readonly IFieldInfo Role = new MinimalFieldInfo(RoleName, SPBuiltInFieldId.Role);

        /// <summary>
        /// The related items field info
        /// </summary>
        public static readonly IFieldInfo RelatedItems = new MinimalFieldInfo(RelatedItemsName, SPBuiltInFieldId.RelatedItems);

        /// <summary>
        /// The display order field info
        /// </summary>
        public static readonly IFieldInfo DisplayOrder = new MinimalFieldInfo(DisplayOrderName, new Guid("2cc33755-5880-44c7-925c-fd41fd76cefb"));

        /// <summary>
        /// The master page URL field info
        /// </summary>
        public static readonly IFieldInfo MasterPageUrl = new MinimalFieldInfo(MasterPageUrlName, new Guid("b65d5645-28c4-44b5-8f87-c49250c5c98c"));   // TODO: Turn into UrlFieldInfo

        /// <summary>
        /// The theme URL field info
        /// </summary>
        public static readonly IFieldInfo ThemeUrl = new MinimalFieldInfo(ThemeUrlName, new Guid("f0490cd6-93e0-42bd-8de3-1be68e3045f1")); // TODO turn into UrlFieldValue

        /// <summary>
        /// The image URL field info
        /// </summary>
        public static readonly IFieldInfo ImageUrl = new MinimalFieldInfo(ImageUrlName, new Guid("833cb87d-835f-4fa7-8927-e781c890f023")); // TODO turn into UrlFieldValue

        /// <summary>
        /// The font scheme URL field info
        /// </summary>
        public static readonly IFieldInfo FontSchemeUrl = new MinimalFieldInfo(ImageUrlName, new Guid("b5dfc328-900e-4306-93e1-43c74a847320")); // TODO turn into UrlFieldValue

        /// <summary>
        /// The enterprise keywords field info
        /// </summary>
        public static readonly IFieldInfo EnterpriseKeywords = new MinimalFieldInfo(EnterpriseKeywordsName, new Guid("23f27201-bee3-471e-b2e7-b64fd8b7ca38")); // TODO: turn into TaxonomyMultiFieldInfo

        /// <summary>
        /// The cell phone
        /// </summary>
        public static readonly IFieldInfo CellPhone = new MinimalFieldInfo(CellPhoneName, new Guid("2a464df1-44c1-4851-949d-fcd270f0ccf2"));

        /// <summary>
        /// The full name
        /// </summary>
        public static readonly IFieldInfo FullName = new MinimalFieldInfo(FullNameName, new Guid("475c2610-c157-4b91-9e2d-6855031b3538"));

        /// <summary>
        /// The email
        /// </summary>
        public static readonly IFieldInfo Email = new MinimalFieldInfo(EmailName, SPBuiltInFieldId.EMail);

        /// <summary>
        /// The first name
        /// </summary>
        public static readonly IFieldInfo FirstName = new MinimalFieldInfo(FirstNameName, SPBuiltInFieldId.FirstName);

        /// <summary>
        /// The work phone
        /// </summary>
        public static readonly IFieldInfo WorkPhone = new MinimalFieldInfo(WorkPhoneName, SPBuiltInFieldId.WorkPhone);

        /// <summary>
        /// The job title
        /// </summary>
        public static readonly IFieldInfo JobTitle = new MinimalFieldInfo(JobTitleName, SPBuiltInFieldId.JobTitle);

        /// <summary>
        /// The company
        /// </summary>
        public static readonly IFieldInfo Company = new MinimalFieldInfo(CompanyName, SPBuiltInFieldId.Company);

        #endregion
    }
}
