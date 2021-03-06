﻿using System;
using System.Collections.ObjectModel;
using System.Web;

/******************************************************
 * 
 * Copyright (c) 2012-2020 MyFlightbook LLC
 * Contact myflightbook-at-gmail.com for more information
 *
*******************************************************/

namespace MyFlightbook
{
    /// <summary>
    /// The various brands known to the system.
    /// </summary>
    public enum BrandID
    {
        brandMyFlightbook, brandMyFlightbookStaging
    };

    public class Brand
    {
        #region properties
        /// <summary>
        /// The ID for the brand
        /// </summary>
        public BrandID BrandID { get; private set; }

        /// <summary>
        /// The name of the app (e.g., "MyFlightbook")
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        /// The host name for the app (e.g., "myflightbook.com")
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// The root for the app (e.g., "/logbook" on the live site).  The equivalent to "~" in relative URLs
        /// </summary>
        public string Root { get; private set; }

        /// <summary>
        /// The URL for the logo for the app (upper corner)
        /// </summary>
        public string LogoHRef { get; private set; }

        /// <summary>
        /// The Email address used for mail that gets sent from the app
        /// </summary>
        public string EmailAddress { get; private set; }

        /// <summary>
        /// Link to Facebook feed
        /// </summary>
        public string FacebookFeed { get; private set; }

        /// <summary>
        /// Link to Twitter feed
        /// </summary>
        public string TwitterFeed { get; private set; }

        /// <summary>
        /// Link to Blog
        /// </summary>
        public string BlogAddress { get; private set; }

        /// <summary>
        /// Link to stylesheet path.
        /// </summary>
        public string StyleSheet { get; private set; }

        /// <summary>
        /// Link to any video/tutorial channel
        /// </summary>
        public string VideoRef { get; private set; }

        /// <summary>
        /// which AWS bucket to use for this brand?
        /// </summary>
        public string AWSBucket { get; private set; }

        /// <summary>
        /// Which LocalConfig key retrieves the pipeline config for this brand
        /// </summary>
        public string AWSETSPipelineConfigKey { get; private set; }
        #endregion

        public Brand(BrandID brandID)
        {
            BrandID = brandID;
            AppName = HostName = Root = LogoHRef = EmailAddress = FacebookFeed = TwitterFeed = StyleSheet = BlogAddress = VideoRef = AWSBucket = AWSETSPipelineConfigKey = string.Empty;
        }

        private const string szPrefixToIgnore = "www.";

        /// <summary>
        /// Determines if the specified host name matches to this brand
        /// Ignores szPrefixToIgnore ("www.") as a prefix.
        /// </summary>
        /// <param name="szHost">The hostname (e.g., "Myflightbook.com")</param>
        /// <returns>True if it matches the host</returns>
        public bool MatchesHost(string szHost)
        {
            if (szHost == null)
                throw new ArgumentNullException(nameof(szHost));
            if (szHost.StartsWith(szPrefixToIgnore, StringComparison.OrdinalIgnoreCase))
                szHost = szHost.Substring(szPrefixToIgnore.Length);
            return String.Compare(HostName, szHost, StringComparison.OrdinalIgnoreCase) == 0;
        }

        static private Collection<Brand> _knownBrands;

        static public Collection<Brand> KnownBrands
        {
            get
            {
                if (_knownBrands == null)
                {
                    _knownBrands = new Collection<Brand> {
                        new Brand(BrandID.brandMyFlightbook)
                        {
                            AppName = "MyFlightbook",
                            HostName = "myflightbook.com",
                            Root = "/logbook",
                            LogoHRef = "~/Public/mfblogonew.png",
                            StyleSheet = string.Empty,
                            EmailAddress = "noreply@mg.myflightbook.com",
                            FacebookFeed = "http://www.facebook.com/MyFlightbook",
                            TwitterFeed = "http://twitter.com/MyFlightbook",
                            BlogAddress = "https://myflightbookblog.blogspot.com/",
                            VideoRef = "https://www.youtube.com/channel/UC6oqJL-aLMEagSyV0AKkIoQ?view_as=subscriber",
                            AWSBucket = "mfbimages",
                            AWSETSPipelineConfigKey = "ETSPipelineID"
                        },
                        new Brand(BrandID.brandMyFlightbookStaging)
                        {
                            AppName = "MFBStaging",
                            HostName = "staging.myflightbook.com",
                            Root = "/logbook",
                            LogoHRef = "~/Public/myflightbooknewstaging.png",
                            StyleSheet = "~/Public/CSS/staging.css",
                            EmailAddress = "noreply@mg.myflightbook.com",
                            AWSBucket = "mfb-staging",
                            AWSETSPipelineConfigKey = "ETSPipelineIDStaging"
                        }
                    };
                }
                return _knownBrands;
            }
        }
    }

    public static class Branding
    {
        private const string brandStateKey = "_brandid";

        /// <summary>
        /// The ID of the brand for the current request.  Use session if available.
        /// </summary>
        static public BrandID CurrentBrandID
        {
            get
            {
                if (HttpContext.Current == null)
                    return BrandID.brandMyFlightbook;

                // use a session object, if available, else key off of the hostname
                if (HttpContext.Current.Session != null)
                {
                    object o = HttpContext.Current.Session[brandStateKey];
                    if (o != null)
                        return (BrandID)o;
                }

                BrandID result = BrandID.brandMyFlightbook;

                string szHost = HttpContext.Current.Request.Url.Host;
                foreach (Brand b in Brand.KnownBrands)
                    if (String.Compare(szHost, b.HostName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        result = b.BrandID;
                        break;
                    }

                if (HttpContext.Current.Session != null)
                    HttpContext.Current.Session[brandStateKey] = result;

                return result;
            }
            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                    HttpContext.Current.Session[brandStateKey] = value;
            }
        }

        /// <summary>
        /// The active brand, as defined by the current brandID.  Defaults to MyFlightbook.
        /// </summary>
        static public Brand CurrentBrand
        {
            get { return Brand.KnownBrands[(int)CurrentBrandID]; }
        }

        /// <summary>
        /// Rebrands a template with appropriate brand substitutions:
        /// Current valid placeholders are:
        ///  %APP_NAME%: the name of the app
        ///  %SHORT_DATE%: Current date format (short) - date pattern
        ///  %DATE_TIME%: Current time format (long) - sample in long format
        ///  %APP_URL%: the URL (host) for the current request.
        ///  %APP_ROOT%: The root (analogous to "~") for the app brand.
        ///  %APP_LOGO%: the URL for the app logo
        /// </summary>
        /// <param name="szTemplate">The template</param>
        /// <param name="brand">The brand to use (omit for current brand)</param>
        /// <returns>The template with the appropriate substitutions</returns>
        static public string ReBrand(string szTemplate, Brand brand)
        {
            if (szTemplate == null)
                throw new ArgumentNullException(nameof(szTemplate));
            if (brand == null)
                throw new ArgumentNullException(nameof(brand));
            string szNew = szTemplate.Replace("%APP_NAME%", brand.AppName).
                Replace("%SHORT_DATE%", System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern).
                Replace("%DATE_TIME%", DateTime.UtcNow.UTCDateFormatString()).
                Replace("%APP_URL%", brand.HostName).
                Replace("%APP_LOGO%", VirtualPathUtility.ToAbsolute(brand.LogoHRef)).
                Replace("%APP_ROOT%", brand.Root);

            return szNew;
        }

        /// <summary>
        /// Rebrands a template with appropriate brand substitutions using the CURRENT BRAND
        /// </summary>
        /// <param name="szTemplate">The template</param>
        /// <returns>The template with the appropriate substitutions</returns>
        static public string ReBrand(string szTemplate)
        {
            return ReBrand(szTemplate, CurrentBrand);
        }

        static public Uri PublicFlightURL(int idFlight)
        {
            return new Uri(String.Format(System.Globalization.CultureInfo.InvariantCulture, "http://{0}/{1}public/ViewPublicFlight.aspx/{2}", CurrentBrand.HostName, CurrentBrand.Root, idFlight));
        }
    }
}