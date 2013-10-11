namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// An image value entity.
    /// </summary>
    public class ImageValue
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlValue"/> class.
        /// </summary>
        public ImageValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageValue"/> class.
        /// </summary>
        /// <param name="fieldImageValue">The field image value.</param>
        public ImageValue(ImageFieldValue fieldImageValue)
        {
            this.Alignment = fieldImageValue.Alignment;
            this.AlternateText = fieldImageValue.AlternateText;
            this.BorderWidth = fieldImageValue.BorderWidth;
            this.Height = fieldImageValue.Height;
            this.HorizontalSpacing = fieldImageValue.HorizontalSpacing;
            this.Hyperlink = fieldImageValue.Hyperlink;
            this.ImageUrl = fieldImageValue.ImageUrl;
            this.OpenHyperlinkInNewWindow = fieldImageValue.OpenHyperlinkInNewWindow;
            this.VerticalSpacing = fieldImageValue.VerticalSpacing;
            this.Width = fieldImageValue.Width;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the alignment
        /// </summary>
        public string Alignment { get; set; }
        
        /// <summary>
        /// Gets or sets alt text
        /// </summary>
        public string AlternateText { get; set; }
        
        /// <summary>
        /// Gets or sets border width
        /// </summary>
        public int BorderWidth { get; set; }
        
        /// <summary>
        /// Gets or sets height
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// Gets or sets horizontal spacing
        /// </summary>
        public int HorizontalSpacing { get; set; }
        
        /// <summary>
        /// Gets or sets image hyperlink
        /// </summary>
        public string Hyperlink { get; set; }
        
        /// <summary>
        /// Gets or sets image url
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Meant for direct mapping from pre-validated ImageFieldValue in ListItem.")]
        public string ImageUrl { get; set; }
        
        /// <summary>
        /// Gets or sets whether hyperlink should open in new window
        /// </summary>
        public bool OpenHyperlinkInNewWindow { get; set; }
        
        /// <summary>
        /// Gets or sets vertical spacing
        /// </summary>
        public int VerticalSpacing { get; set; }

        /// <summary>
        /// Gets or sets width
        /// </summary>
        public int Width { get; set; }

        #endregion
    }
}